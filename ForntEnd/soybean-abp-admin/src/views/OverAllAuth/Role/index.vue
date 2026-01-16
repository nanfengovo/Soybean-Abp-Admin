<script setup lang="tsx">
import { reactive } from 'vue';
import { NButton, NCard, NDataTable, NPopconfirm, NSpace, NTag } from 'naive-ui';
import type { FlatResponseData } from '@sa/axios';
import type { PaginationData } from '@sa/hooks';
import { fetchDeleteRole, fetchGetPermissions, fetchGetRoleList } from '@/service/api';
import { useAppStore } from '@/store/modules/app';
import { useNaivePaginatedTable, useTableOperate } from '@/hooks/common/table';
import { $t } from '@/locales';
import TableHeaderOperation from '@/components/advanced/table-header-operation.vue';
import RoleSearch from './modules/role-search.vue';
import RoleOperateDrawer from './modules/role-operate-drawer.vue';

const appStore = useAppStore();

const searchParams: Api.SystemManage.RoleSearchParams = reactive({
  Filter: '',
  Sorting: '',
  SkipCount: 0,
  MaxResultCount: 10
});

// 权限到菜单名称的映射
const permissionToMenuMap = reactive<Record<string, string>>({});
// 角色权限数据
const rolePermissionsMap = reactive<Record<string, { menus: string[]; others: string[] }>>({});

// 保存初始化 Promise
// let initPermissionMapPromise: Promise<void> | null = null;

// 初始化权限映射
// async function initPermissionMap() {
//   // 获取所有菜单（假设不超过1000个）
//   const { data, error } = await fetchGetMenuList({ MaxResultCount: 1000 });
//   if (!error && data) {
//     const buildMap = (menus: Api.SystemManage.Menu[]) => {
//       menus.forEach(menu => {
//         if (menu.permissionName) {
//           permissionToMenuMap[menu.permissionName] = menu.name;
//         }
//         if (menu.children && menu.children.length > 0) {
//           buildMap(menu.children);
//         }
//       });
//     };
//     buildMap(data.items || []);
//   }
// }

// 初始化时调用
// initPermissionMapPromise = initPermissionMap();

// 批量获取角色权限
async function fetchRolesPermissions(roles: Api.SystemManage.Role[]) {
  // 确保映射表加载完成
  // if (initPermissionMapPromise) {
  //   await initPermissionMapPromise;
  // }

  const promises = roles.map(async role => {
    try {
      // providerName='R' 代表 Role, providerKey 为角色名称
      const { data, error } = await fetchGetPermissions('R', role.name);
      if (!error && data) {
        const menus: string[] = [];
        const others: string[] = [];

        data.groups.forEach((group: Api.SystemManage.PermissionGroup) => {
          group.permissions.forEach((p: Api.SystemManage.PermissionGrantInfo) => {
            if (p.isGranted) {
              if (permissionToMenuMap[p.name]) {
                menus.push(permissionToMenuMap[p.name]);
              } else {
                others.push(p.displayName || p.name);
              }
            }
          });
        });

        rolePermissionsMap[role.id] = { menus, others };
      }
    } catch {
      // ignore
    }
  });

  await Promise.allSettled(promises);
}

async function getRolesWithPermissions() {
  const response = await fetchGetRoleList(searchParams);
  if (response.data && response.data.items) {
    fetchRolesPermissions(response.data.items);
  }
  return response;
}

// ABP 数据转换函数
function abpTransform(
  response: FlatResponseData<any, Api.SystemManage.RoleList>
): PaginationData<Api.SystemManage.Role> {
  const { data, error } = response;

  if (!error && data) {
    return {
      data: data.items || [],
      pageNum: (searchParams.SkipCount || 0) / (searchParams.MaxResultCount || 10) + 1,
      pageSize: searchParams.MaxResultCount || 10,
      total: data.totalCount || 0
    };
  }

  return {
    data: [],
    pageNum: 1,
    pageSize: 10,
    total: 0
  };
}

const { columns, columnChecks, data, getData, getDataByPage, loading, mobilePagination } = useNaivePaginatedTable({
  api: getRolesWithPermissions,
  transform: (response: FlatResponseData<any, Api.SystemManage.RoleList>) => abpTransform(response),
  onPaginationParamsChange: params => {
    // ABP 使用 SkipCount 和 MaxResultCount
    searchParams.SkipCount = ((params.page ?? 1) - 1) * (params.pageSize ?? 10);
    searchParams.MaxResultCount = params.pageSize ?? 10;
  },
  columns: () => [
    {
      type: 'selection',
      align: 'center',
      width: 48
    },
    {
      key: 'index',
      title: $t('common.index'),
      align: 'center',
      width: 64,
      render: (_, index) => index + 1
    },
    {
      key: 'name',
      title: '角色名称',
      align: 'center',
      minWidth: 120
    },
    {
      key: 'permissions',
      title: '拥有权限',
      align: 'center',
      minWidth: 300,
      render: row => {
        const permissions = rolePermissionsMap[row.id];
        if (!permissions || (permissions.menus.length === 0 && permissions.others.length === 0)) {
          return '-';
        }
        return (
          <div class="flex-center flex-wrap gap-4px">
            {permissions.menus.map(menu => (
              <NTag key={menu} type="success" size="small">
                {menu}
              </NTag>
            ))}
            {permissions.others.map(other => (
              <NTag key={other} type="default" size="small">
                {other}
              </NTag>
            ))}
          </div>
        );
      }
    },
    {
      key: 'isDefault',
      title: '默认角色',
      align: 'center',
      width: 100,
      render: row => {
        const tagMap: Record<string, NaiveUI.ThemeColor> = {
          true: 'success',
          false: 'default'
        };

        const label = row.isDefault ? '是' : '否';

        return <NTag type={tagMap[String(row.isDefault)]}>{label}</NTag>;
      }
    },
    {
      key: 'isPublic',
      title: '公开角色',
      align: 'center',
      width: 100,
      render: row => {
        const tagMap: Record<string, NaiveUI.ThemeColor> = {
          true: 'info',
          false: 'default'
        };

        const label = row.isPublic ? '是' : '否';

        return <NTag type={tagMap[String(row.isPublic)]}>{label}</NTag>;
      }
    },
    {
      key: 'isStatic',
      title: '系统角色',
      align: 'center',
      width: 100,
      render: row => {
        const tagMap: Record<string, NaiveUI.ThemeColor> = {
          true: 'warning',
          false: 'default'
        };

        const label = row.isStatic ? '是' : '否';

        return <NTag type={tagMap[String(row.isStatic)]}>{label}</NTag>;
      }
    },
    {
      key: 'creationTime',
      title: '创建时间',
      align: 'center',
      width: 180,
      render: row => {
        if (!row.creationTime) return '-';
        const date = new Date(row.creationTime);
        return date.toLocaleString('zh-CN');
      }
    },
    {
      key: 'operate',
      title: $t('common.operate'),
      align: 'center',
      width: 130,
      render: row => (
        <NSpace justify="center">
          <NButton type="primary" ghost size="small" onClick={() => edit(row.id)}>
            {$t('common.edit')}
          </NButton>
          <NPopconfirm onPositiveClick={() => handleDelete(row.id)}>
            {{
              default: () => $t('common.confirmDelete'),
              trigger: () => (
                <NButton type="error" ghost size="small" disabled={row.isStatic}>
                  {$t('common.delete')}
                </NButton>
              )
            }}
          </NPopconfirm>
        </NSpace>
      )
    }
  ]
});

const { drawerVisible, operateType, editingData, handleAdd, handleEdit, checkedRowKeys, onBatchDeleted, onDeleted } =
  useTableOperate(data, 'id', getData);

async function handleBatchDelete() {
  if (checkedRowKeys.value.length === 0) return;

  window.$dialog?.warning({
    title: '确认删除',
    content: `确认删除选中的 ${checkedRowKeys.value.length} 个角色吗？`,
    positiveText: '确认',
    negativeText: '取消',
    onPositiveClick: async () => {
      // 1. 同时发起请求并等待全部完成
      const deletePromises = checkedRowKeys.value.map(id => fetchDeleteRole(id));
      const results = await Promise.allSettled(deletePromises);

      // 2. 检查是否有失败的请求
      const failedCount = results.filter(r => r.status === 'rejected').length;

      if (failedCount > 0) {
        window.$message?.warning(`成功删除 ${results.length - failedCount} 个角色，失败 ${failedCount} 个`);
      } else {
        window.$message?.success('批量删除成功');
      }

      // 3. 全部删除完成后再刷新列表并清空选中项
      onBatchDeleted();
    }
  });
}

async function handleDelete(id: string) {
  const { error } = await fetchDeleteRole(id);
  if (!error) {
    onDeleted();
  }
}

function edit(id: string) {
  handleEdit(id);
}
</script>

<template>
  <div class="min-h-500px flex-col-stretch gap-16px overflow-hidden lt-sm:overflow-auto">
    <RoleSearch v-model:model="searchParams" @search="getDataByPage" />
    <NCard title="角色管理" :bordered="false" size="small" class="card-wrapper sm:flex-1-hidden">
      <template #header-extra>
        <TableHeaderOperation
          v-model:columns="columnChecks"
          :disabled-delete="checkedRowKeys.length === 0"
          :loading="loading"
          @add="handleAdd"
          @delete="handleBatchDelete"
          @refresh="getData"
        />
      </template>
      <NDataTable
        v-model:checked-row-keys="checkedRowKeys"
        :columns="columns"
        :data="data"
        size="small"
        :flex-height="!appStore.isMobile"
        :scroll-x="962"
        :loading="loading"
        remote
        :row-key="row => row.id"
        :pagination="mobilePagination"
        class="sm:h-full"
      />
      <RoleOperateDrawer
        v-model:visible="drawerVisible"
        :operate-type="operateType"
        :row-data="editingData"
        @submitted="getDataByPage"
      />
    </NCard>
  </div>
</template>
