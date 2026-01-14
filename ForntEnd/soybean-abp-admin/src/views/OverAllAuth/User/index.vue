<script setup lang="tsx">
import { reactive } from 'vue';
import { NButton, NCard, NDataTable, NPopconfirm, NTag } from 'naive-ui';
import type { FlatResponseData } from '@sa/axios';
import type { PaginationData } from '@sa/hooks';
import { enableStatusRecord } from '@/constants/business';
import { fetchDeleteUser, fetchGetUserList } from '@/service/api';
import { useAppStore } from '@/store/modules/app';
import { useNaivePaginatedTable, useTableOperate } from '@/hooks/common/table';
import { $t } from '@/locales';
import TableHeaderOperation from '@/components/advanced/table-header-operation.vue';
import UserSearch from './modules/user-search.vue';
import UserOperateDrawer from './modules/user-operate-drawer.vue';

const appStore = useAppStore();

const searchParams: Api.SystemManage.UserSearchParams = reactive({
  Filter: '',
  Sorting: '',
  SkipCount: 0,
  MaxResultCount: 10
});

// ABP 数据转换函数
function abpTransform(
  response: FlatResponseData<any, Api.SystemManage.UserList>
): PaginationData<Api.SystemManage.User> {
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
  api: () => fetchGetUserList(searchParams),
  transform: (response: FlatResponseData<any, Api.SystemManage.UserList>) => abpTransform(response),
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
      key: 'userName',
      title: $t('page.manage.user.userName'),
      align: 'center',
      minWidth: 100
    },
    {
      key: 'name',
      title: $t('page.manage.user.nickName'),
      align: 'center',
      minWidth: 100
    },
    {
      key: 'phoneNumber',
      title: $t('page.manage.user.userPhone'),
      align: 'center',
      width: 120
    },
    {
      key: 'email',
      title: $t('page.manage.user.userEmail'),
      align: 'center',
      minWidth: 200
    },
    {
      key: 'isActive',
      title: $t('page.manage.user.userStatus'),
      align: 'center',
      width: 100,
      render: row => {
        // ABP 使用 isActive 字段，转换为对应的状态
        const status: Api.Common.EnableStatus = row.isActive ? '1' : '2';

        const tagMap: Record<string, NaiveUI.ThemeColor> = {
          '1': 'success',
          '2': 'warning'
        };

        const label = $t(enableStatusRecord[status]);

        return <NTag type={tagMap[status]}>{label}</NTag>;
      }
    },
    {
      key: 'operate',
      title: $t('common.operate'),
      align: 'center',
      width: 130,
      render: row => (
        <div class="flex-center gap-8px">
          <NButton type="primary" ghost size="small" onClick={() => edit(row.id)}>
            {$t('common.edit')}
          </NButton>
          <NPopconfirm onPositiveClick={() => handleDelete(row.id)}>
            {{
              default: () => $t('common.confirmDelete'),
              trigger: () => (
                <NButton type="error" ghost size="small">
                  {$t('common.delete')}
                </NButton>
              )
            }}
          </NPopconfirm>
        </div>
      )
    }
  ]
});

const {
  drawerVisible,
  operateType,
  editingData,
  handleAdd,
  handleEdit,
  checkedRowKeys,
  onBatchDeleted,
  onDeleted
  // closeDrawer
} = useTableOperate(data, 'id', getData);

async function handleBatchDelete() {
  if (checkedRowKeys.value.length === 0) return;

  window.$dialog?.warning({
    title: '确认删除',
    content: `确认删除选中的 ${checkedRowKeys.value.length} 条数据吗？`,
    positiveText: '确认',
    onPositiveClick: async () => {
      // 1. 同时发起请求并等待全部完成
      const deletePromises = checkedRowKeys.value.map(id => fetchDeleteUser(id));
      await Promise.all(deletePromises);

      window.$message?.success('批量删除成功');

      // 2. 全部删除完成后再刷新列表并清空选中项
      onBatchDeleted();
      checkedRowKeys.value = [];
    }
  });
}

async function handleDelete(id: string) {
  const { error } = await fetchDeleteUser(id);
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
    <UserSearch v-model:model="searchParams" @search="getDataByPage" />
    <NCard :title="$t('page.manage.user.title')" :bordered="false" size="small" class="card-wrapper sm:flex-1-hidden">
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
      <UserOperateDrawer
        v-model:visible="drawerVisible"
        :operate-type="operateType"
        :row-data="editingData"
        @submitted="getDataByPage"
      />
    </NCard>
  </div>
</template>
