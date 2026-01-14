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
export function fetchGetMenuList(params?: Api.SystemManage.MenuSearchParams) {
  return request<Api.SystemManage.MenuList>({
    url: 'api/app/menu',
    method: 'get',
    params
  });
}

/** get my menus (current user's accessible menus) */
export function fetchGetMyMenus() {
  return request<Api.SystemManage.MenuList>({
    url: 'api/app/menu/my-menus',
    method: 'get'
  });
}

/** get menu by id */
export function fetchGetMenu(id: string) {
  return request<Api.SystemManage.Menu>({
    url: `api/app/menu/${id}`,
    method: 'get'
  });
}

/** create menu */
export function fetchAddMenu(data: Api.SystemManage.MenuEdit) {
  return request<Api.SystemManage.Menu>({
    url: 'api/app/menu',
    method: 'post',
    data
  });
}

/** update menu */
export function fetchUpdateMenu(id: string, data: Api.SystemManage.MenuEdit) {
  return request<Api.SystemManage.Menu>({
    url: `api/app/menu/${id}`,
    method: 'put',
    data
  });
}

/** delete menu */
export function fetchDeleteMenu(id: string) {
  return request({
    url: `api/app/menu/${id}`,
    method: 'delete'
  });
}

/** sync permissions from menus */
export function fetchSyncPermissions(data: Api.SystemManage.SyncPermissionsRequest) {
  return request({
    url: 'api/app/menu/sync-permissions',
    method: 'post',
    data
  });
}
