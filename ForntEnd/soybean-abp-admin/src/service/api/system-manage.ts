import { request } from '../request';

/** get user list */
export function fetchGetUserList(params?: Api.SystemManage.UserSearchParams) {
  return request<Api.SystemManage.UserList>({
    url: 'api/identity/users',
    method: 'get',
    params
  });
}

/** create user */
export function fetchAddUser(data: Api.SystemManage.UserEdit) {
  return request<Api.SystemManage.User>({
    url: 'api/identity/users',
    method: 'post',
    data
  });
}

/** update user */
export function fetchUpdateUser(id: string, data: Api.SystemManage.UserEdit) {
  return request<Api.SystemManage.User>({
    url: `api/identity/users/${id}`,
    method: 'put',
    data
  });
}

/** delete user */
export function fetchDeleteUser(id: string) {
  return request({
    url: `api/identity/users/${id}`,
    method: 'delete'
  });
}

/** get user roles */
export function fetchGetUserRoles(id: string) {
  return request<{ items: Api.SystemManage.Role[] }>({
    url: `api/identity/users/${id}/roles`,
    method: 'get'
  });
}

/** get assignable roles */
export function fetchGetAssignableRoles() {
  return request<{ items: Api.SystemManage.Role[] }>({
    url: 'api/identity/users/assignable-roles',
    method: 'get'
  });
}

/** assign roles to user */
export function fetchAssignRolesToUser(id: string, data: { roleNames: string[] }) {
  return request({
    url: `api/identity/users/${id}/roles`,
    method: 'put',
    data
  });
}

/** get role list */
export function fetchGetRoleList(params?: Api.SystemManage.RoleSearchParams) {
  return request<Api.SystemManage.RoleList>({
    url: 'api/identity/roles',
    method: 'get',
    params
  });
}

/** get all roles (without pagination) */
export function fetchGetAllRoles() {
  return request<Api.SystemManage.RoleList>({
    url: 'api/identity/roles/all',
    method: 'get'
  });
}

/** get role by id */
export function fetchGetRole(id: string) {
  return request<Api.SystemManage.Role>({
    url: `api/identity/roles/${id}`,
    method: 'get'
  });
}

/** create role */
export function fetchAddRole(data: Api.SystemManage.RoleEdit) {
  return request<Api.SystemManage.Role>({
    url: 'api/identity/roles',
    method: 'post',
    data
  });
}

/** update role */
export function fetchUpdateRole(id: string, data: Api.SystemManage.RoleEdit) {
  return request<Api.SystemManage.Role>({
    url: `api/identity/roles/${id}`,
    method: 'put',
    data
  });
}

/** delete role */
export function fetchDeleteRole(id: string) {
  return request({
    url: `api/identity/roles/${id}`,
    method: 'delete'
  });
}

/** get menu list */
// export function fetchGetMenuList(params?: Api.SystemManage.MenuSearchParams) {
//   return request<Api.SystemManage.MenuList>({
//     url: 'api/app/menu',
//     method: 'get',
//     params
//   });
// }

// /** get my menus (current user's accessible menus) */
// export function fetchGetMyMenus() {
//   return request<Api.SystemManage.MenuList>({
//     url: 'api/app/menu/my-menus',
//     method: 'get'
//   });
// }

// /** get menu by id */
// export function fetchGetMenu(id: string) {
//   return request<Api.SystemManage.Menu>({
//     url: `api/app/menu/${id}`,
//     method: 'get'
//   });
// }

// /** create menu */
// export function fetchAddMenu(data: Api.SystemManage.MenuEdit) {
//   return request<Api.SystemManage.Menu>({
//     url: 'api/app/menu',
//     method: 'post',
//     data
//   });
// }

// /** update menu */
// export function fetchUpdateMenu(id: string, data: Api.SystemManage.MenuEdit) {
//   return request<Api.SystemManage.Menu>({
//     url: `api/app/menu/${id}`,
//     method: 'put',
//     data
//   });
// }

// /** delete menu */
// export function fetchDeleteMenu(id: string) {
//   return request({
//     url: `api/app/menu/${id}`,
//     method: 'delete'
//   });
// }

// /** sync permissions from menus */
export function fetchSyncPermissions(data: Api.SystemManage.SyncPermissionsRequest) {
  return request({
    url: 'api/app/menu/sync-permissions',
    method: 'post',
    data
  });
}

/** get permissions */
export function fetchGetPermissions(providerName: string, providerKey: string) {
  return request<Api.SystemManage.PermissionList>({
    url: 'api/permission-management/permissions',
    method: 'get',
    params: { providerName, providerKey }
  });
}

/** update permissions */
export function fetchUpdatePermissions(
  providerName: string,
  providerKey: string,
  data: Api.SystemManage.UpdatePermissionsRequest
) {
  return request({
    url: 'api/permission-management/permissions',
    method: 'put',
    params: { providerName, providerKey },
    data
  });
}

/** get location map list */
export function fetchGetLocationMapList(params?: Api.SystemManage.LocationMapSearchParams) {
  return request<Api.SystemManage.LocationMapList>({
    url: 'api/app/location-map',
    method: 'get',
    params
  });
}

/** get location map by id */
export function fetchGetLocationMap(id: string) {
  return request<Api.SystemManage.LocationMap>({
    url: `api/app/location-map/${id}`,
    method: 'get'
  });
}

/** create location map */
export function fetchAddLocationMap(data: Api.SystemManage.LocationMapEdit) {
  return request<Api.SystemManage.LocationMap>({
    url: 'api/app/location-map',
    method: 'post',
    data
  });
}

/** update location map */
export function fetchUpdateLocationMap(id: string, data: Api.SystemManage.LocationMapEdit) {
  return request<Api.SystemManage.LocationMap>({
    url: `api/app/location-map/${id}`,
    method: 'put',
    data
  });
}

/** delete location map */
export function fetchDeleteLocationMap(id: string) {
  return request({
    url: `api/app/location-map/${id}`,
    method: 'delete'
  });
}

/** get external API log list */
export function fetchGetExternalAPILogList(params?: Api.SystemManage.ExAPILogSearchParams) {
  return request<Api.SystemManage.ExternalAPILogList>({
    url: 'api/app/a-piLog',
    method: 'get',
    params
  });
}

/** get external API log by id */
export function fetchGetExternalAPILog(id: string) {
  return request<Api.SystemManage.ExternalAPILog>({
    url: `api/app/a-piLog/${id}`,
    method: 'get'
  });
}

/** delete external API log */
export function fetchDeleteExternalAPILog(id: string) {
  return request({
    url: `api/app/a-piLog/${id}`,
    method: 'delete'
  });
}

/** get internal API log list */
export function fetchGetInternalAPILogList(params?: Api.SystemManage.InternalAPILogSearchParams) {
  return request<Api.SystemManage.InternalAPILogList>({
    url: 'api/app/internal-api-log',
    method: 'get',
    params
  });
}

/** get internal API log by id */
export function fetchGetInternalAPILog(id: string) {
  return request<Api.SystemManage.InternalAPILog>({
    url: `api/app/internal-api-log/${id}`,
    method: 'get'
  });
}

/** delete internal API log */
export function fetchDeleteInternalAPILog(id: string) {
  return request({
    url: `api/app/internal-api-log/${id}`,
    method: 'delete'
  });
}
