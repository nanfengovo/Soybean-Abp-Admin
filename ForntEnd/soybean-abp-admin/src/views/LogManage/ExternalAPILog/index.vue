<script setup lang="tsx">
import { reactive, ref } from 'vue';
import { NButton, NCard, NDataTable, NEllipsis, NPopconfirm, NSpace, NTag } from 'naive-ui';
import type { FlatResponseData } from '@sa/axios';
import type { PaginationData } from '@sa/hooks';
import { fetchDeleteExternalAPILog, fetchGetExternalAPILogList } from '@/service/api';
import { useAppStore } from '@/store/modules/app';
import { useNaivePaginatedTable } from '@/hooks/common/table';
import { useAuth } from '@/hooks/business/auth';
import { $t } from '@/locales';
import TableHeaderOperation from '@/components/advanced/table-header-operation.vue';
import ExternalAPILogSearch from './modules/ExternalAPILog-search.vue';
import ExternalAPILogDetailDrawer from './modules/ExternalAPILog-detail-drawer.vue';

const appStore = useAppStore();
const { hasAuth } = useAuth();

// 权限码定义
const permissions = {
  delete: 'AbpOverallAuth.APILog.Delete'
};

const searchParams: Api.SystemManage.ExAPILogSearchParams = reactive({
  Filter: '',
  Sorting: 'creationTime desc',
  SkipCount: 0,
  MaxResultCount: 10
});

// ABP 数据转换函数
function abpTransform(
  response: FlatResponseData<any, Api.SystemManage.ExternalAPILogList>
): PaginationData<Api.SystemManage.ExternalAPILog> {
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
  api: () => fetchGetExternalAPILogList(searchParams),
  transform: (response: FlatResponseData<any, Api.SystemManage.ExternalAPILogList>) => abpTransform(response),
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
      key: 'sysName',
      title: '系统名称',
      align: 'center',
      width: 100,
      render: row => <NTag type="info">{row.sysName}</NTag>
    },
    {
      key: 'url',
      title: '请求地址',
      align: 'center',
      minWidth: 200,
      ellipsis: {
        tooltip: true
      }
    },
    {
      key: 'httpMethod',
      title: '请求方法',
      align: 'center',
      width: 100,
      render: row => {
        const typeMap: Record<string, any> = {
          GET: 'success',
          POST: 'info',
          PUT: 'warning',
          DELETE: 'error'
        };
        return <NTag type={typeMap[row.httpMethod] || 'default'}>{row.httpMethod}</NTag>;
      }
    },
    {
      key: 'statusCode',
      title: '状态码',
      align: 'center',
      width: 100,
      render: row => {
        let type: any = 'default';
        if (row.statusCode >= 200 && row.statusCode < 300) type = 'success';
        else if (row.statusCode >= 400 && row.statusCode < 500) type = 'warning';
        else if (row.statusCode >= 500) type = 'error';
        return <NTag type={type}>{row.statusCode}</NTag>;
      }
    },
    {
      key: 'isSuccess',
      title: '是否成功',
      align: 'center',
      width: 100,
      render: row => <NTag type={row.isSuccess ? 'success' : 'error'}>{row.isSuccess ? '成功' : '失败'}</NTag>
    },
    {
      key: 'duration',
      title: '耗时(ms)',
      align: 'center',
      width: 100,
      render: row => {
        let type: any = 'success';
        if (row.duration > 1000) type = 'warning';
        if (row.duration > 3000) type = 'error';
        return <NTag type={type}>{row.duration}</NTag>;
      }
    },
    {
      key: 'errorMessage',
      title: '错误信息',
      align: 'center',
      width: 150,
      render: row => {
        if (!row.errorMessage) return '-';
        return (
          <NEllipsis style="max-width: 150px">
            <NTag type="error">{row.errorMessage}</NTag>
          </NEllipsis>
        );
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
      width: 150,
      fixed: 'right',
      render: row => (
        <NSpace justify="center">
          <NButton type="primary" ghost size="small" onClick={() => viewDetail(row)}>
            查看详情
          </NButton>
          {/* <NPopconfirm onPositiveClick={() => handleDelete(row.id)}>
            {{
              default: () => $t('common.confirmDelete'),
              trigger: () => (
                <NButton type="error" ghost size="small" disabled={!hasAuth(permissions.delete)}>
                  {$t('common.delete')}
                </NButton>
              )
            }}
          </NPopconfirm> */}
        </NSpace>
      )
    }
  ]
});

const checkedRowKeys = ref<string[]>([]);
const detailDrawerVisible = ref(false);
const selectedLog = ref<Api.SystemManage.ExternalAPILog | null>(null);

function viewDetail(row: Api.SystemManage.ExternalAPILog) {
  selectedLog.value = row;
  detailDrawerVisible.value = true;
}

async function handleBatchDelete() {
  if (checkedRowKeys.value.length === 0) return;

  window.$dialog?.warning({
    title: '确认删除',
    content: `确认删除选中的 ${checkedRowKeys.value.length} 条数据吗？`,
    positiveText: '确认',
    negativeText: '取消',
    onPositiveClick: async () => {
      const deletePromises = checkedRowKeys.value.map(id => fetchDeleteExternalAPILog(id));
      const results = await Promise.allSettled(deletePromises);

      const failedCount = results.filter(r => r.status === 'rejected').length;

      if (failedCount > 0) {
        window.$message?.warning(`成功删除 ${results.length - failedCount} 条，失败 ${failedCount} 条`);
      } else {
        window.$message?.success('批量删除成功');
      }

      checkedRowKeys.value = [];
      getData();
    }
  });
}

async function handleDelete(id: string) {
  const { error } = await fetchDeleteExternalAPILog(id);
  if (!error) {
    window.$message?.success('删除成功');
    getData();
  }
}
</script>

<template>
  <div class="min-h-500px flex-col-stretch gap-16px overflow-hidden lt-sm:overflow-auto">
    <ExternalAPILogSearch v-model:model="searchParams" @search="getDataByPage" />
    <NCard title="外部API日志" :bordered="false" size="small" class="card-wrapper sm:flex-1-hidden">
      <template #header-extra>
        <TableHeaderOperation
          v-model:columns="columnChecks"
          :disabled-add="true"
          :disabled-delete="checkedRowKeys.length === 0 || !hasAuth(permissions.delete)"
          :loading="loading"
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
        :scroll-x="1400"
        :loading="loading"
        remote
        :row-key="row => row.id"
        :pagination="mobilePagination"
        class="sm:h-full"
      />
      <ExternalAPILogDetailDrawer v-model:visible="detailDrawerVisible" :log-data="selectedLog" />
    </NCard>
  </div>
</template>
