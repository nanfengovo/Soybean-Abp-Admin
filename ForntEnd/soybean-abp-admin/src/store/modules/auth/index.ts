import { computed, reactive, ref } from 'vue';
import { useRoute } from 'vue-router';
import { defineStore } from 'pinia';
import { useLoading } from '@sa/hooks';
import { fetchGetCurrentUser, fetchGetPermissions, fetchGetRoleInfo, fetchGetUserInfo, fetchLogin } from '@/service/api';
import { useRouterPush } from '@/hooks/common/router';
import { localStg } from '@/utils/storage';
import { SetupStoreId } from '@/enum';
import { $t } from '@/locales';
import { useRouteStore } from '../route';
import { useTabStore } from '../tab';
import { clearAuthStorage, getToken } from './shared';

export const useAuthStore = defineStore(SetupStoreId.Auth, () => {
  const route = useRoute();
  const authStore = useAuthStore();
  const routeStore = useRouteStore();
  const tabStore = useTabStore();
  const { toLogin, redirectFromLogin } = useRouterPush(false);
  const { loading: loginLoading, startLoading, endLoading } = useLoading();

  const token = ref(getToken());

  const userInfo: Api.Auth.UserInfo = reactive({
    userId: '',
    userName: '',
    roles: [],
    buttons: []
  });

  /** 权限是否已加载完成 */
  const isPermissionsLoaded = ref(false);

  /** is super role in static route */
  const isStaticSuper = computed(() => {
    const { VITE_AUTH_ROUTE_MODE, VITE_STATIC_SUPER_ROLE } = import.meta.env;

    return VITE_AUTH_ROUTE_MODE === 'static' && userInfo.roles.includes(VITE_STATIC_SUPER_ROLE);
  });

  /** Is login */
  const isLogin = computed(() => Boolean(token.value));

  /** Reset auth store */
  async function resetStore() {
    recordUserId();

    clearAuthStorage();

    authStore.$reset();
    isPermissionsLoaded.value = false;

    if (!route.meta.constant) {
      await toLogin();
    }

    tabStore.cacheTabs();
    routeStore.resetStore();
  }

  /** Record the user ID of the previous login session Used to compare with the current user ID on next login */
  function recordUserId() {
    if (!userInfo.userId) {
      return;
    }

    // Store current user ID locally for next login comparison
    localStg.set('lastLoginUserId', userInfo.userId);
  }

  /**
   * Check if current login user is different from previous login user If different, clear all tabs
   *
   * @returns {boolean} Whether to clear all tabs
   */
  function checkTabClear(): boolean {
    if (!userInfo.userId) {
      return false;
    }

    const lastLoginUserId = localStg.get('lastLoginUserId');

    // Clear all tabs if current user is different from previous user
    if (!lastLoginUserId || lastLoginUserId !== userInfo.userId) {
      localStg.remove('globalTabs');
      tabStore.clearTabs();

      localStg.remove('lastLoginUserId');
      return true;
    }

    localStg.remove('lastLoginUserId');
    return false;
  }

  /**
   * Login
   *
   * @param userName User name
   * @param password Password
   * @param [redirect=true] Whether to redirect after login. Default is `true`
   */
  async function login(userName: string, password: string, redirect = true) {
    startLoading();
    const { data: loginToken, error } = await fetchLogin(userName, password);
    if (!error && loginToken) {
      const pass = await loginByToken(loginToken);
      const user = await getUserInfo(userName);
      if (pass && user) {
        // Check if the tab needs to be cleared
        const isClear = checkTabClear();
        let needRedirect = redirect;

        if (isClear) {
          // If the tab needs to be cleared,it means we don't need to redirect.
          needRedirect = false;
        }

        // Initialize auth routes before redirecting to ensure menus are properly loaded
        await routeStore.initAuthRoute();

        await redirectFromLogin(needRedirect);

        window.$notification?.success({
          title: $t('page.login.common.loginSuccess'),
          content: $t('page.login.common.welcomeBack', { userName }),
          duration: 4500
        });
      }
    } else {
      resetStore();
    }

    endLoading();
  }

  async function loginByToken(loginToken: Api.Auth.LoginToken) {
    // 1. stored in the localStorage, the later requests need it in headers
    localStg.set('token', loginToken.access_token);
    localStg.set('refreshToken', loginToken.refresh_token);

    token.value = loginToken.access_token;

    return true;
  }

  async function getUserInfo(userName: string) {
    const { data: info, error } = await fetchGetUserInfo(userName);
    if (!error && info) {
      // === ABP 数据到 Soybean 数据的适配逻辑 ===
      const adapterUserInfo: Api.Auth.User = {
        userId: info.id || '',
        userName: info.currentUser?.userName || ''
      };
      const { data: role } = await fetchGetRoleInfo(info.id);
      // 更新 Pinia 状态
      userInfo.userId = adapterUserInfo.userId;
      userInfo.userName = adapterUserInfo.userName;
      userInfo.roles = role?.items ? role.items.map((r: any) => r.name) : [];

      // 获取用户权限（通过角色获取）
      await getUserPermissions(userInfo.roles);

      return true;
    }

    return false;
  }

  /** 获取用户已授权的权限列表（通过角色获取） */
  async function getUserPermissions(roles: string[]) {
    try {
      const allPermissions = new Set<string>();

      // 遍历用户所有角色，获取每个角色的权限
      const promises = roles.map(async roleName => {
        const { data, error } = await fetchGetPermissions('R', roleName);
        if (!error && data) {
          data.groups.forEach(group => {
            group.permissions.forEach(p => {
              if (p.isGranted) {
                allPermissions.add(p.name);
              }
            });
          });
        }
      });

      await Promise.allSettled(promises);
      userInfo.buttons = Array.from(allPermissions);
      isPermissionsLoaded.value = true;
      // 调试：打印获取到的权限列表
      console.log('用户权限列表:', userInfo.buttons);
    } catch {
      userInfo.buttons = [];
      isPermissionsLoaded.value = true;
    }
  }

  /** 初始化用户信息（用于页面刷新时恢复用户状态） */
  async function initUserInfo() {
    const hasToken = getToken();
    console.log('initUserInfo - hasToken:', hasToken);

    if (hasToken) {
      try {
        // 使用 ABP 的 application-configuration 接口获取当前用户信息
        const { data, error } = await fetchGetCurrentUser();
        console.log('initUserInfo - fetchGetCurrentUser response:', { data, error });

        if (!error && data?.currentUser?.isAuthenticated) {
          const currentUser = data.currentUser;
          userInfo.userId = currentUser.id || '';
          userInfo.userName = currentUser.userName || '';
          userInfo.roles = currentUser.roles || [];

          console.log('initUserInfo - userInfo after update:', {
            userId: userInfo.userId,
            userName: userInfo.userName,
            roles: userInfo.roles
          });

          // 获取用户权限（通过角色获取）
          await getUserPermissions(userInfo.roles);

          console.log('initUserInfo - buttons after getUserPermissions:', userInfo.buttons);
        } else {
          console.log('initUserInfo - user not authenticated, clearing auth storage');
          clearAuthStorage();
          token.value = '';
          // 重置路由状态，以便重新登录后能重新初始化路由
          routeStore.setIsInitAuthRoute(false);
        }
      } catch (err) {
        console.error('initUserInfo - error:', err);
        clearAuthStorage();
        token.value = '';
        // 重置路由状态，以便重新登录后能重新初始化路由
        routeStore.setIsInitAuthRoute(false);
      }
    }
  }

  return {
    token,
    userInfo,
    isStaticSuper,
    isLogin,
    loginLoading,
    isPermissionsLoaded,
    resetStore,
    login,
    initUserInfo
  };
});
