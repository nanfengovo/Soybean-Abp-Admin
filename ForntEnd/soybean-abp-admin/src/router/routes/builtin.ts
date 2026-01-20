import type { CustomRoute } from '@elegant-router/types';
import { layouts, views } from '../elegant/imports';
import { getRoutePath, transformElegantRoutesToVueRoutes } from '../elegant/transform';

/**
 * Get the home route redirect path
 * In dynamic mode, the path might not exist in static routeMap,
 * so we construct it from the route name
 */
function getHomeRedirectPath(): string {
  const routeHome = import.meta.env.VITE_ROUTE_HOME;
  const staticPath = getRoutePath(routeHome);

  if (staticPath) {
    return staticPath;
  }

  // For dynamic routes, construct path from route name
  // e.g., 'dashboard' -> '/dashboard', 'system_menus' -> '/system/menus'
  const pathSegments = routeHome.split('_');
  return '/' + pathSegments.join('/');
}

export const ROOT_ROUTE: CustomRoute = {
  name: 'root',
  path: '/',
  redirect: getHomeRedirectPath(),
  meta: {
    title: 'root',
    constant: true
  }
};

const NOT_FOUND_ROUTE: CustomRoute = {
  name: 'not-found',
  path: '/:pathMatch(.*)*',
  component: 'layout.blank$view.404',
  meta: {
    title: 'not-found',
    constant: true
  }
};

/** builtin routes, it must be constant and setup in vue-router */
const builtinRoutes: CustomRoute[] = [ROOT_ROUTE, NOT_FOUND_ROUTE];

/** create builtin vue routes */
export function createBuiltinVueRoutes() {
  return transformElegantRoutesToVueRoutes(builtinRoutes, layouts, views);
}
