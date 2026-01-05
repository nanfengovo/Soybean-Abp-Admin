import { request } from '../request';

const { VITE_CLIENT_ID, VITE_SCOPE } = import.meta.env;

/**
 * Login
 *
 * @param userName User name
 * @param password Password
 */
export function fetchLogin(userName: string, password: string) {
  // 使用 URLSearchParams 来构建 x-www-form-urlencoded 格式的数据
  const params = new URLSearchParams();
  params.append('grant_type', 'password');
  params.append('username', userName);
  params.append('password', password);
  params.append('client_id', VITE_CLIENT_ID); // 必须匹配你后端配置的名称
  params.append('scope', `${VITE_SCOPE} offline_access`); // 加上 offline_access 才能拿到 refresh_token
  return request<Api.Auth.LoginToken>({
    url: '/connect/token',
    method: 'post',
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded'
    },
    data: params
  });
}

/** Get user info */
// export function fetchGetUserInfo() {
//   return request<Api.Auth.UserInfo>({ url: '/auth/getUserInfo' });
// }

/** 获取 ABP 应用配置（包含用户信息和权限） */
export function fetchGetUserInfo(userName: string) {
  // 这里的 url 指向 ABP 的根据用户名获取用户信息的接口
  return request<any>({ url: `/api/identity/users/by-username/${userName}` });
}

export function fetchGetRoleInfo(userId: string) {
  // 这里的 url 指向 ABP 的获取用户角色信息的接口
  return request<any>({ url: `/api/identity/users/${userId}/roles` });
}

/**
 * Refresh token
 *
 * @param refreshToken Refresh token
 */
export function fetchRefreshToken(refreshToken: string) {
  return request<Api.Auth.LoginToken>({
    url: '/auth/refreshToken',
    method: 'post',
    data: {
      refreshToken
    }
  });
}

/**
 * return custom backend error
 *
 * @param code error code
 * @param msg error message
 */
export function fetchCustomBackendError(code: string, msg: string) {
  return request({ url: '/auth/error', params: { code, msg } });
}
