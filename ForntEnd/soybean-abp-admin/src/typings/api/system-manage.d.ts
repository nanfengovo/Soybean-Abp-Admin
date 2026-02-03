declare namespace Api {
  /**
   * namespace SystemManage
   *
   * backend api module: "systemManage"
   */
  namespace SystemManage {
    type CommonSearchParams = Pick<Common.PaginatingCommonParams, 'current' | 'size'>;
    /** user search params (ABP standard) */
    type UserSearchParams = {
      /** 模糊查询字段 (ABP Filter) */
      Filter?: string;
      /** 排序字段 (ABP Sorting) */
      Sorting?: string;
      /** 跳过记录数 (ABP SkipCount) */
      SkipCount?: number;
      /** 最大结果数 (ABP MaxResultCount) */
      MaxResultCount?: number;
      /** 额外属性 (ABP ExtraProperties) */
      ExtraProperties?: Record<string, any>;
    };
    /**
     * user gender
     *
     * - "1": "male"
     * - "2": "female"
     */
    type UserGender = '1' | '2';

    /** user */
    type User = {
      /** user id */
      id: string;
      /** tenant id */
      tenantId: string | null;
      /** user name */
      userName: string;
      /** name */
      name: string;
      /** surname */
      surname: string | null;
      /** email */
      email: string;
      /** email confirmed */
      emailConfirmed: boolean;
      /** phone number */
      phoneNumber: string | null;
      /** phone number confirmed */
      phoneNumberConfirmed: boolean;
      /** is active */
      isActive: boolean;
      /** lockout enabled */
      lockoutEnabled: boolean;
      /** access failed count */
      accessFailedCount: number;
      /** lockout end */
      lockoutEnd: string | null;
      /** concurrency stamp */
      concurrencyStamp: string;
      /** entity version */
      entityVersion: number;
      /** last password change time */
      lastPasswordChangeTime: string;
      /** is deleted */
      isDeleted: boolean;
      /** deleter id */
      deleterId: string | null;
      /** deletion time */
      deletionTime: string | null;
      /** last modification time */
      lastModificationTime: string | null;
      /** last modifier id */
      lastModifierId: string | null;
      /** creation time */
      creationTime: string;
      /** creator id */
      creatorId: string | null;
      /** extra properties */
      extraProperties: Record<string, any>;
      /** user gender (custom field) */
      userGender?: UserGender | null;
      /** user nick name (custom field) */
      nickName?: string;
      /** user phone (custom field) */
      userPhone?: string;
      /** user email (custom field) */
      userEmail?: string;
      /** user role code collection (custom field) */
      userRoles?: string[];
      /** status (custom field) */
      status?: Common.EnableStatus | null;
    };

    /** user list */
    type UserList = Common.AbpPaginatingQueryRecord<User>;

    /** user edit (for create/update) */
    type UserEdit = {
      /** user name */
      userName: string;
      /** name */
      name: string;
      /** surname */
      surname?: string | null;
      /** email */
      email: string;
      /** phone number */
      phoneNumber?: string | null;
      /** password (required for create) */
      password?: string;
      /** is active */
      isActive: boolean;
      /** lockout enabled */
      lockoutEnabled?: boolean;
      /** role names */
      roleNames?: string[];
      /** extra properties */
      extraProperties?: Record<string, any>;
      /** concurrency stamp (required for update) */
      concurrencyStamp?: string;
    };

    /** role search params (ABP standard) */
    type RoleSearchParams = {
      /** 模糊查询字段 (ABP Filter) */
      Filter?: string;
      /** 排序字段 (ABP Sorting) */
      Sorting?: string;
      /** 跳过记录数 (ABP SkipCount) */
      SkipCount?: number;
      /** 最大结果数 (ABP MaxResultCount) */
      MaxResultCount?: number;
    };

    /** role */
    type Role = {
      /** role id */
      id: string;
      /** tenant id */
      tenantId: string | null;
      /** role name */
      name: string;
      /** is default */
      isDefault: boolean;
      /** is static */
      isStatic: boolean;
      /** is public */
      isPublic: boolean;
      /** concurrency stamp */
      concurrencyStamp: string;
      /** extra properties */
      extraProperties: Record<string, any>;
      /** creation time */
      creationTime: string;
      /** creator id */
      creatorId: string | null;
      /** last modification time */
      lastModificationTime: string | null;
      /** last modifier id */
      lastModifierId: string | null;
    };

    /** role list */
    type RoleList = Common.AbpPaginatingQueryRecord<Role>;

    /** role edit (for create/update) */
    type RoleEdit = {
      /** role name */
      name: string;
      /** is default */
      isDefault: boolean;
      /** is public */
      isPublic: boolean;
      /** concurrency stamp (required for update) */
      concurrencyStamp?: string;
      /** extra properties */
      extraProperties?: Record<string, any>;
    };

    /** menu search params */
    type MenuSearchParams = {
      /** 模糊查询字段 */
      Filter?: string;
      /** 排序字段 */
      Sorting?: string;
      /** 跳过记录数 */
      SkipCount?: number;
      /** 最大结果数 */
      MaxResultCount?: number;
    };

    /**
     * menu type
     *
     * - 0: "directory" (目录)
     * - 1: "menu" (菜单)
     * - 2: "button" (按钮/权限)
     */
    type MenuType = 0 | 1 | 2;

    /** menu */
    type Menu = {
      /** menu id */
      id: string;
      /** parent id */
      parentId: string | null;
      /** menu name */
      name: string;
      /** icon */
      icon: string | null;
      /** route path */
      path: string | null;
      /** component path */
      component: string | null;
      /** sort order */
      sortOrder: number;
      /** is hidden (true = 隐藏, false = 可见) */
      isHidden: boolean;
      /** permission name (permission code) */
      permissionName: string;
      /** children menu */
      children?: Menu[];
    };

    /** menu list */
    type MenuList = Menu[];

    /** menu edit (for create/update) */
    type MenuEdit = {
      /** parent id */
      parentId?: string | null;
      /** menu name */
      name: string;
      /** permission name */
      permissionName: string;
      /** menu type */
      menuType: MenuType;
      /** route path */
      path?: string | null;
      /** component path */
      component?: string | null;
      /** icon */
      icon?: string | null;
      /** sort order */
      sortOrder?: number;
      /** is hidden */
      isHidden?: boolean;
      /** is enabled */
      isEnabled?: boolean;
      /** is external */
      isExternal?: boolean;
      /** external url */
      externalUrl?: string | null;
      /** remark */
      remark?: string | null;
    };

    /** sync permissions request */
    type SyncPermissionsRequest = {
      /** menu ids to sync */
      menuIds: string[];
    };

    /** permission grant info */
    type PermissionGrantInfo = {
      name: string;
      displayName: string;
      parentName: string | null;
      isGranted: boolean;
      allowedProviders: string[];
      grantedProviders: { providerName: string; providerKey: string }[];
    };

    /** permission group */
    type PermissionGroup = {
      name: string;
      displayName: string;
      displayNameKey: string;
      displayNameResource: string;
      permissions: PermissionGrantInfo[];
    };

    /** permission list */
    type PermissionList = {
      entityDisplayName: string;
      groups: PermissionGroup[];
    };

    /** permission update item */
    type PermissionUpdateItem = {
      name: string;
      isGranted: boolean;
    };

    /** update permissions request */
    type UpdatePermissionsRequest = {
      permissions: PermissionUpdateItem[];
    };

    /** location map search params */
    type LocationMapSearchParams = {
      /** 模糊查询字段 */
      Filter?: string;
      /** 排序字段 */
      Sorting?: string;
      /** 跳过记录数 */
      SkipCount?: number;
      /** 最大结果数 */
      MaxResultCount?: number;
    };

    /** location map */
    type LocationMap = {
      /** id */
      id: string;
      /** 名称 */
      name: string;
      /** 机台点位 */
      machinePoint: string;
      /** AGV点位 */
      agvPoint: string;
      /** 描述 */
      description: string;
      /** 创建时间 */
      creationTime?: string;
      /** 最后修改时间 */
      lastModificationTime?: string;
    };

    /** location map list */
    type LocationMapList = Common.AbpPaginatingQueryRecord<LocationMap>;

    /** location map edit */
    type LocationMapEdit = {
      /** 名称 */
      name: string;
      /** 机台点位 */
      machinePoint: string;
      /** AGV点位 */
      agvPoint: string;
      /** 描述 */
      description?: string;
    };

    type ExAPILogSearchParams = {
      /** 模糊查询字段 */
      Filter?: string;
      /** 排序字段 */
      Sorting?: string;
      /** 跳过记录数 */
      SkipCount?: number;
      /** 最大结果数 */
      MaxResultCount?: number;
    };

    /** external API log */
    type ExternalAPILog = {
      /** id */
      id: string;
      /** 系统名称 */
      sysName: string;
      /** 业务ID */
      businessId: string | null;
      /** 业务类型 */
      businessType: string | null;
      /** 请求URL */
      url: string;
      /** 请求路径 */
      path: string;
      /** HTTP方法 */
      httpMethod: string;
      /** 请求头 */
      requestHeaders: string | null;
      /** 请求体 */
      requestBody: string | null;
      /** 状态码 */
      statusCode: number;
      /** 响应体 */
      responseBody: string | null;
      /** 响应头 */
      responseHeaders: string | null;
      /** 耗时(毫秒) */
      duration: number;
      /** 客户端IP地址 */
      clientIpAddress: string | null;
      /** 追踪ID */
      traceId: string;
      /** 是否成功 */
      isSuccess: boolean;
      /** 错误信息 */
      errorMessage: string | null;
      /** 错误堆栈 */
      errorStackTrace: string | null;
      /** 用户ID */
      userId: string | null;
      /** 租户ID */
      tenantId: string | null;
      /** 额外数据 */
      extraData: string | null;
      /** 创建时间 */
      creationTime: string;
    };

    /** external API log list */
    type ExternalAPILogList = Common.AbpPaginatingQueryRecord<ExternalAPILog>;

    /** internal API log search params */
    type InternalAPILogSearchParams = {
      /** 模糊查询字段 (可搜索URL、用户名、客户端IP等) */
      Filter?: string;
      /** 开始时间 */
      StartTime?: string;
      /** 结束时间 */
      EndTime?: string;
      /** HTTP方法 */
      HttpMethod?: string;
      /** 用户ID */
      UserId?: string;
      /** 最小执行时间 (毫秒) */
      MinExecutionDuration?: number;
      /** 最大执行时间 (毫秒) */
      MaxExecutionDuration?: number;
      /** 是否有异常 */
      HasException?: boolean;
      /** HTTP状态码 */
      HttpStatusCode?: number;
      /** 排序字段 */
      Sorting?: string;
      /** 跳过记录数 */
      SkipCount?: number;
      /** 最大结果数 */
      MaxResultCount?: number;
    };

    /** internal API log */
    type InternalAPILog = {
      /** id */
      id: string;
      /** 应用名称 */
      applicationName: string;
      /** 用户ID */
      userId: string | null;
      /** 用户名 */
      userName: string | null;
      /** 租户ID */
      tenantId: string | null;
      /** 租户名称 */
      tenantName: string | null;
      /** 模拟用户ID */
      impersonatorUserId: string | null;
      /** 模拟租户ID */
      impersonatorTenantId: string | null;
      /** 执行时间 */
      executionTime: string;
      /** 执行耗时(毫秒) */
      executionDuration: number;
      /** 客户端IP地址 */
      clientIpAddress: string | null;
      /** 客户端名称 */
      clientName: string | null;
      /** 客户端ID */
      clientId: string | null;
      /** 关联ID */
      correlationId: string | null;
      /** 浏览器信息 */
      browserInfo: string | null;
      /** HTTP方法 */
      httpMethod: string;
      /** 请求URL */
      url: string;
      /** 异常信息 */
      exceptions: string | null;
      /** HTTP状态码 */
      httpStatusCode: number;
      /** 备注 */
      comments: string | null;
    };

    /** internal API log list */
    type InternalAPILogList = Common.AbpPaginatingQueryRecord<InternalAPILog>;
  }
}
