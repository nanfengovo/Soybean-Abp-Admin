<script setup lang="tsx">
import { reactive } from 'vue';
import { NButton, NCard, NDataTable, NPopconfirm, NSpace } from 'naive-ui';
import type { FlatResponseData } from '@sa/axios';
import type { PaginationData } from '@sa/hooks';
import { fetchDeleteLocationMap, fetchGetLocationMapList } from '@/service/api';
import { useAppStore } from '@/store/modules/app';
import { useNaivePaginatedTable, useTableOperate } from '@/hooks/common/table';
import { $t } from '@/locales';
import TableHeaderOperation from '@/components/advanced/table-header-operation.vue';
import LocationMapSearch from './modules/location-map-search.vue';
import LocationMapOperateDrawer from './modules/location-map-operate-drawer.vue';

const appStore = useAppStore();

const searchParams: Api.SystemManage.LocationMapSearchParams = reactive({
  Filter: '',
  Sorting: '',
  SkipCount: 0,
  MaxResultCount: 10
});

// ABP 数据转换函数
function abpTransform(
  response: FlatResponseData<any, Api.SystemManage.LocationMapList>
): PaginationData<Api.SystemManage.LocationMap> {
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
  api: () => fetchGetLocationMapList(searchParams),
  transform: (response: FlatResponseData<any, Api.SystemManage.LocationMapList>) => abpTransform(response),
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
      title: '名称',
      align: 'center',
      minWidth: 120
    },
    {
      key: 'machinePoint',
      title: '机台点位',
      align: 'center',
      minWidth: 120
    },
    {
      key: 'agvPoint',
      title: 'AGV点位',
      align: 'center',
      minWidth: 120
    },
    {
      key: 'description',
      title: '描述',
      align: 'center',
      minWidth: 200,
      ellipsis: {
        tooltip: true
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
                <NButton type="error" ghost size="small">
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
    content: `确认删除选中的 ${checkedRowKeys.value.length} 条数据吗？`,
    positiveText: '确认',
    negativeText: '取消',
    onPositiveClick: async () => {
      const deletePromises = checkedRowKeys.value.map(id => fetchDeleteLocationMap(id));
      const results = await Promise.allSettled(deletePromises);

      const failedCount = results.filter(r => r.status === 'rejected').length;

      if (failedCount > 0) {
        window.$message?.warning(`成功删除 ${results.length - failedCount} 条，失败 ${failedCount} 条`);
      } else {
        window.$message?.success('批量删除成功');
      }

      onBatchDeleted();
    }
  });
}

async function handleDelete(id: string) {
  const { error } = await fetchDeleteLocationMap(id);
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
    <LocationMapSearch v-model:model="searchParams" @search="getDataByPage" />
    <NCard title="机台点位映射" :bordered="false" size="small" class="card-wrapper sm:flex-1-hidden">
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
      <LocationMapOperateDrawer
        v-model:visible="drawerVisible"
        :operate-type="operateType"
        :row-data="editingData"
        @submitted="getDataByPage"
      />
    </NCard>
  </div>
</template>
