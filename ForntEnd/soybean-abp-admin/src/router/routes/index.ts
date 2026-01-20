import type { CustomRoute, ElegantConstRoute, ElegantRoute } from '@elegant-router/types';
import type { RouteComponent } from 'vue-router';
import { generatedRoutes } from '../elegant/routes';
import { layouts, views } from '../elegant/imports';
import { transformElegantRoutesToVueRoutes } from '../elegant/transform';

/**
 * Dynamic views mapping for backend menu routes
 * Maps route names to view components
 */
const dynamicViews: Record<string, RouteComponent | (() => Promise<RouteComponent>)> = {
  // Dashboard
  dashboard: () => import('@/views/System/Dashboard/index.vue'),

  // System management routes (from backend menu paths)
  system_menus: () => import('@/views/OverAllAuth/Permission/index.vue'),
  system_users: () => import('@/views/OverAllAuth/User/index.vue'),
  system_roles: () => import('@/views/OverAllAuth/Role/index.vue')
};

/**
 * Get extended views including dynamic routes
 */
function getExtendedViews() {
  return {
    ...views,
    ...dynamicViews
  };
}

/**
 * custom routes
 *
 * @link https://github.com/soybeanjs/elegant-router?tab=readme-ov-file#custom-route
 */
const customRoutes: CustomRoute[] = [];

/** create routes when the auth route mode is static */
export function createStaticRoutes() {
  const constantRoutes: ElegantRoute[] = [];

  const authRoutes: ElegantRoute[] = [];

  [...customRoutes, ...generatedRoutes].forEach(item => {
    if (item.meta?.constant) {
      constantRoutes.push(item);
    } else {
      authRoutes.push(item);
    }
  });

  return {
    constantRoutes,
    authRoutes
  };
}

/**
 * Get auth vue routes
 *
 * @param routes Elegant routes
 */
export function getAuthVueRoutes(routes: ElegantConstRoute[]) {
  return transformElegantRoutesToVueRoutes(routes, layouts, getExtendedViews());
}
