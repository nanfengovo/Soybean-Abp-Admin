import type { ElegantConstRoute } from '@elegant-router/types';
import { request } from '../request';

/** get constant routes */
export function fetchGetConstantRoutes() {
  return request<Api.Route.MenuRoute[]>({ url: '/route/getConstantRoutes' });
}

/**
 * Icon mapping from backend simple names to Iconify format
 * You can customize this mapping based on your backend icon names
 */
const iconMap: Record<string, string> = {
  // Common icons
  dashboard: 'mdi:view-dashboard',
  home: 'mdi:home',
  setting: 'mdi:cog',
  settings: 'mdi:cog',
  system: 'mdi:cog',
  user: 'mdi:account',
  users: 'mdi:account-group',
  role: 'mdi:account-key',
  roles: 'mdi:account-key',
  team: 'mdi:account-multiple',
  menu: 'mdi:menu',
  menus: 'mdi:menu',
  permission: 'mdi:shield-key',
  permissions: 'mdi:shield-key',
  // Add more mappings as needed
  list: 'mdi:format-list-bulleted',
  table: 'mdi:table',
  form: 'mdi:form-select',
  chart: 'mdi:chart-bar',
  file: 'mdi:file',
  folder: 'mdi:folder',
  link: 'mdi:link',
  search: 'mdi:magnify',
  edit: 'mdi:pencil',
  delete: 'mdi:delete',
  add: 'mdi:plus',
  config: 'mdi:cog-outline',
  log: 'mdi:file-document-outline',
  audit: 'mdi:clipboard-check-outline'
};

/**
 * Transform backend icon name to Iconify format
 * @param icon Backend icon name
 * @returns Iconify icon name
 */
function transformIcon(icon: string | null): string | undefined {
  if (!icon) return undefined;

  // If already in Iconify format (contains ':'), return as is
  if (icon.includes(':')) {
    return icon;
  }

  // Look up in icon map
  const mappedIcon = iconMap[icon.toLowerCase()];
  if (mappedIcon) {
    return mappedIcon;
  }

  // Default: try to use with mdi prefix
  return `mdi:${icon}`;
}

/**
 * Transform backend menu to elegant route format
 *
 * @param menu Backend menu item
 * @param parentRouteName Parent route name for building child route names
 */
function transformMenuToRoute(
  menu: Api.SystemManage.Menu,
  parentRouteName?: string
): ElegantConstRoute | null {
  // Skip button type menus (menuType === 2) and disabled menus
  if (menu.menuType === 2 || !menu.isEnabled) {
    return null;
  }

  // Generate route name: parent_child format (e.g., system_menus)
  const pathSegments = menu.path?.split('/').filter(Boolean) || [];
  const routeName = pathSegments.join('_') || menu.name;

  // Determine component based on menu type
  let component: string;
  if (menu.menuType === 0) {
    // Directory: use layout
    component = 'layout.base';
  } else if (menu.isExternal && menu.externalUrl) {
    // External link: use iframe or external handling
    component = `layout.base$view.iframe-page`;
  } else {
    // Menu page: determine if it's a single-level route or child route
    if (!parentRouteName) {
      // First-level menu with page: layout + view
      component = `layout.base$view.${routeName}`;
    } else {
      // Child menu: just view
      component = `view.${routeName}`;
    }
  }

  const route: ElegantConstRoute = {
    name: routeName,
    path: menu.path || `/${routeName}`,
    component,
    meta: {
      title: menu.name,
      icon: transformIcon(menu.icon),
      order: menu.sortOrder,
      hideInMenu: menu.isHidden,
      keepAlive: true
    }
  };

  // Handle external links
  if (menu.isExternal && menu.externalUrl) {
    route.meta!.href = menu.externalUrl;
  }

  // Process children recursively
  if (menu.children && menu.children.length > 0) {
    const childRoutes = menu.children
      .map(child => transformMenuToRoute(child, routeName))
      .filter((r): r is ElegantConstRoute => r !== null);

    if (childRoutes.length > 0) {
      route.children = childRoutes;
    }
  }

  return route;
}

/**
 * Transform backend menus array to elegant routes
 *
 * @param menus Backend menus array
 */
export function transformMenusToRoutes(menus: Api.SystemManage.Menu[]): ElegantConstRoute[] {
  return menus
    .map(menu => transformMenuToRoute(menu))
    .filter((r): r is ElegantConstRoute => r !== null);
}

/**
 * Get user routes from backend menus
 * Fetches my-menus API and transforms to elegant route format
 */
export async function fetchGetUserRoutes(): Promise<{
  data: Api.Route.UserRoute | null;
  error: any;
}> {
  const response = await request<Api.SystemManage.Menu[]>({
    url: 'api/app/menu/my-menus',
    method: 'get'
  });

  if (response.error || !response.data) {
    return { data: null, error: response.error || new Error('Failed to fetch menus') };
  }

  const routes = transformMenusToRoutes(response.data);

  // Find the first menu page as home route
  const findFirstMenuPage = (menus: Api.SystemManage.Menu[]): string | null => {
    for (const menu of menus) {
      if (menu.menuType === 1 && menu.path && menu.isEnabled && !menu.isHidden) {
        const pathSegments = menu.path.split('/').filter(Boolean);
        return pathSegments.join('_');
      }
      if (menu.children && menu.children.length > 0) {
        const found = findFirstMenuPage(menu.children);
        if (found) return found;
      }
    }
    return null;
  };

  const home = findFirstMenuPage(response.data) || 'home';

  return {
    data: {
      routes: routes as Api.Route.MenuRoute[],
      home: home as import('@elegant-router/types').LastLevelRouteKey
    },
    error: null
  };
}

/**
 * whether the route is exist
 *
 * @param routeName route name
 */
export function fetchIsRouteExist(routeName: string) {
  return request<boolean>({ url: '/route/isRouteExist', params: { routeName } });
}
