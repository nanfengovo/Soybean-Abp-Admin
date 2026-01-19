import type { AxiosResponse } from 'axios';
import type { FlatRequestInstance } from '@sa/axios';
import { BACKEND_ERROR_CODE, createFlatRequest, createRequest } from '@sa/axios';
import { useAuthStore } from '@/store/modules/auth';
import { localStg } from '@/utils/storage';
import { getServiceBaseURL } from '@/utils/service';
import { $t } from '@/locales';
import { getAuthorization, handleExpiredRequest, showErrorMsg } from './shared';
import type { RequestInstanceState } from './type';

const isHttpProxy = import.meta.env.DEV && import.meta.env.VITE_HTTP_PROXY === 'Y';
const { baseURL, otherBaseURL } = getServiceBaseURL(import.meta.env, isHttpProxy);

export const request: FlatRequestInstance<any, any, RequestInstanceState> = createFlatRequest(
  {
    baseURL,
    headers: {
      apifoxToken: 'XL299LiMEDZ0H5h3A29PxwQXdMJqWyY2'
    },
    // 让 401 也进入成功拦截器，这样才能在 onBackendFail 中处理 token 刷新
    validateStatus: status => status >= 200 && status < 500
  },
  {
    defaultState: {
      errMsgStack: [],
      refreshTokenPromise: null
    } as RequestInstanceState,
    transform(response: AxiosResponse<App.Service.Response<any>>) {
      return response.data?.data ?? response.data;
    },
    async onRequest(config) {
      const Authorization = getAuthorization();
      Object.assign(config.headers, { Authorization });

      return config;
    },
    isBackendSuccess(response) {
      // when the backend response code is "0000"(default), it means the request is success
      // to change this logic by yourself, you can modify the `VITE_SERVICE_SUCCESS_CODE` in `.env` file
      return response.status >= 200 && response.status < 300;
    },
    async onBackendFail(response, instance) {
      const authStore = useAuthStore();

      // ABP 处理：当 HTTP 状态码为 401 时，尝试刷新 token 并重试
      if (response.status === 401) {
        const refreshToken = localStg.get('refreshToken');
        if (refreshToken) {
          const success = await handleExpiredRequest(request.state);
          if (success) {
            // 刷新成功，重新设置 Authorization header 并重试请求
            const Authorization = getAuthorization();
            Object.assign(response.config.headers, { Authorization });
            return instance.request(response.config) as Promise<AxiosResponse>;
          }
          // 刷新失败，清除登录状态
          authStore.resetStore();
          return null;
        }
        // 没有 refresh token，直接登出
        authStore.resetStore();
        return null;
      }

      // ABP 错误处理：对于 4xx 错误（如 403），显示后端返回的详细错误信息
      if (response.status >= 400 && response.status < 500) {
        const responseData = response.data as any;
        let errorMessage = '';

        if (responseData?.error) {
          // ABP 标准错误格式: { error: { message: '...', details: '...' } }
          errorMessage = responseData.error.details || responseData.error.message || '';
        } else if (responseData?.message) {
          errorMessage = responseData.message;
        }

        // 如果没有错误信息，根据状态码提供默认信息
        if (!errorMessage) {
          const statusMessages: Record<number, string> = {
            400: '请求参数错误',
            401: '未授权，请重新登录',
            403: '没有权限访问该资源',
            404: '请求的资源不存在',
            405: '请求方法不允许',
            408: '请求超时',
            422: '请求参数验证失败',
            429: '请求过于频繁，请稍后再试'
          };
          errorMessage = statusMessages[response.status] || `请求失败 (${response.status})`;
        }

        showErrorMsg(request.state, errorMessage);
        return null;
      }

      const responseCode = String(response.data?.code || '');

      function handleLogout() {
        authStore.resetStore();
      }

      function logoutAndCleanup() {
        handleLogout();
        window.removeEventListener('beforeunload', handleLogout);

        request.state.errMsgStack = request.state.errMsgStack.filter(msg => msg !== response.data?.msg);
      }

      // when the backend response code is in `logoutCodes`, it means the user will be logged out and redirected to login page
      const logoutCodes = import.meta.env.VITE_SERVICE_LOGOUT_CODES?.split(',') || [];
      if (logoutCodes.includes(responseCode)) {
        handleLogout();
        return null;
      }

      // when the backend response code is in `modalLogoutCodes`, it means the user will be logged out by displaying a modal
      const modalLogoutCodes = import.meta.env.VITE_SERVICE_MODAL_LOGOUT_CODES?.split(',') || [];
      if (modalLogoutCodes.includes(responseCode) && !request.state.errMsgStack?.includes(response.data?.msg)) {
        request.state.errMsgStack = [...(request.state.errMsgStack || []), response.data?.msg];

        // prevent the user from refreshing the page
        window.addEventListener('beforeunload', handleLogout);

        window.$dialog?.error({
          title: $t('common.error'),
          content: response.data?.msg,
          positiveText: $t('common.confirm'),
          maskClosable: false,
          closeOnEsc: false,
          onPositiveClick() {
            logoutAndCleanup();
          },
          onClose() {
            logoutAndCleanup();
          }
        });

        return null;
      }

      // when the backend response code is in `expiredTokenCodes`, it means the token is expired, and refresh token
      // the api `refreshToken` can not return error code in `expiredTokenCodes`, otherwise it will be a dead loop, should return `logoutCodes` or `modalLogoutCodes`
      const expiredTokenCodes = import.meta.env.VITE_SERVICE_EXPIRED_TOKEN_CODES?.split(',') || [];
      if (expiredTokenCodes.includes(responseCode)) {
        const success = await handleExpiredRequest(request.state);
        if (success) {
          const Authorization = getAuthorization();
          Object.assign(response.config.headers, { Authorization });

          return instance.request(response.config) as Promise<AxiosResponse>;
        }
      }

      return null;
    },
    onError(error) {
      // when the request is fail, you can show error message

      let message = error.message;
      let backendErrorCode = '';

      // get backend error message and code
      if (error.code === BACKEND_ERROR_CODE) {
        // ABP 错误响应格式: { error: { message: '...', details: '...', code: '...' } }
        const responseData = error.response?.data as any;
        if (responseData?.error) {
          // ABP 标准错误格式
          message = responseData.error.details || responseData.error.message || message;
          backendErrorCode = String(responseData.error.code || '');
        } else {
          // 兼容其他格式
          message = responseData?.message || responseData?.msg || message;
          backendErrorCode = String(responseData?.code || '');
        }

        // ABP 处理：当 HTTP 状态码为 401 时，不显示错误消息（会由拦截器自动刷新 token）
        if (error.response?.status === 401) {
          return;
        }
      }

      // the error message is displayed in the modal
      const modalLogoutCodes = import.meta.env.VITE_SERVICE_MODAL_LOGOUT_CODES?.split(',') || [];
      if (modalLogoutCodes.includes(backendErrorCode)) {
        return;
      }

      // when the token is expired, refresh token and retry request, so no need to show error message
      const expiredTokenCodes = import.meta.env.VITE_SERVICE_EXPIRED_TOKEN_CODES?.split(',') || [];
      if (expiredTokenCodes.includes(backendErrorCode)) {
        return;
      }

      showErrorMsg(request.state, message);
    }
  }
);

export const demoRequest = createRequest(
  {
    baseURL: otherBaseURL.demo
  },
  {
    // 1. 修改 transform，兼容 ABP 扁平的数据结构
    transform(response: AxiosResponse) {
      // 如果后端直接返回的是对象本体，则直接返回 response.data
      return response.data?.data ?? response.data;
    },
    async onRequest(config) {
      const { headers } = config;

      // set token
      const token = localStg.get('token');
      const Authorization = token ? `Bearer ${token}` : null;
      Object.assign(headers, { Authorization });

      return config;
    },
    // 2. 修改成功判断逻辑
    isBackendSuccess(response) {
      // 如果是 ABP 接口且 HTTP 状态码在 2xx 范围内，即视为成功
      return response.status >= 200 && response.status < 300;
    },
    async onBackendFail(_response) {
      // when the backend response code is not "200", it means the request is fail
      // for example: the token is expired, refresh token and retry request
    },
    onError(error) {
      // when the request is fail, you can show error message

      let message = error.message;

      // show backend error message
      if (error.code === BACKEND_ERROR_CODE) {
        message = error.response?.data?.message || message;
      }

      window.$message?.error(message);
    }
  }
);
