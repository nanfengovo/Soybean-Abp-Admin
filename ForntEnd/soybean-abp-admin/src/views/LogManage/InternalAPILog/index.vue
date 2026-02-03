<script setup lang="tsx">
import { reactive, ref } from 'vue';
import { NButton, NCard, NDataTable, NEllipsis, NPopconfirm, NSpace, NTag } from 'naive-ui';
import type { FlatResponseData } from '@sa/axios';
import type { PaginationData } from '@sa/hooks';
import { fetchDeleteInternalAPILog, fetchGetInternalAPILogList } from '@/service/api';
import { useAppStore } from '@/store/modules/app';
import { useNaivePaginatedTable } from '@/hooks/common/table';
import { useAuth } from '@/hooks/business/auth';
import { $t } from '@/locales';
import TableHeaderOperation from '@/components/advanced/table-header-operation.vue';
import InternalAPILogSearch from './modules/InternalAPILog-search.vue';
import InternalAPILogDetailDrawer from './modules/InternalAPILog-detail-drawer.vue';

const appStore = useAppStore();
const { hasAuth } = useAuth();

// 权限码定义
const permissions = {
  delete: 'AbpOverallAuth.InternalAPILog.Delete'
};

const searchParams: Api.SystemManage.InternalAPILogSearchParams = reactive({
  Filter: '',
  StartTime: undefined,
  EndTime: undefined,
  HttpMethod: undefined,
  UserId: undefined,
  MinExecutionDuration: undefined,
  MaxExecutionDuration: undefined,
  HasException: undefined,
  HttpStatusCode: undefined,
  Sorting: 'executionTime desc',
  SkipCount: 0,
  MaxResultCount: 10
});

// ABP 数据转换函数
function abpTransform(
  response: FlatResponseData<any, Api.SystemManage.InternalAPILogList>
): PaginationData<Api.SystemManage.InternalAPILog> {
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
  api: () => fetchGetInternalAPILogList(searchParams),
  transform: (response: FlatResponseData<any, Api.SystemManage.InternalAPILogList>) => abpTransform(response),
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
      key: 'applicationName',
      title: '应用名称',
      align: 'center',
      width: 180,
      ellipsis: {
        tooltip: true
      },
      render: row => <NTag type="info">{row.applicationName}</NTag>
    },
    {
      key: 'url',
      title: '请求URL',
      align: 'center',
      minWidth: 200,
      ellipsis: {
        tooltip: true
      }
    },
    {
      key: 'httpMethod',
      title: 'HTTP方法',
      align: 'center',
      width: 100,
      render: row => {
        const typeMap: Record<string, any> = {
          GET: 'success',
          POST: 'info',
          PUT: 'warning',
          DELETE: 'error',
          PATCH: 'default'
        };
        return <NTag type={typeMap[row.httpMethod] || 'default'}>{row.httpMethod}</NTag>;
      }
    },
    {
      key: 'httpStatusCode',
      title: 'HTTP状态码',
      align: 'center',
      width: 110,
      render: row => {
        let type: any = 'default';
        if (row.httpStatusCode >= 200 && row.httpStatusCode < 300) type = 'success';
        else if (row.httpStatusCode >= 400 && row.httpStatusCode < 500) type = 'warning';
        else if (row.httpStatusCode >= 500) type = 'error';
        return <NTag type={type}>{row.httpStatusCode}</NTag>;
      }
    },
    {
      key: 'userName',
      title: '用户名',
      align: 'center',
      width: 120,
      render: row => row.userName || '-'
    },
    {
      key: 'executionDuration',
      title: '执行耗时(ms)',
      align: 'center',
      width: 120,
      render: row => {
        let type: any = 'success';
        if (row.executionDuration > 1000) type = 'warning';
        if (row.executionDuration > 3000) type = 'error';
        return <NTag type={type}>{row.executionDuration}</NTag>;
      }
    },
    {
      key: 'clientIpAddress',
      title: '客户端IP',
      align: 'center',
      width: 130,
      render: row => row.clientIpAddress || '-'
    },
    {
      key: 'exceptions',
      title: '是否有异常',
      align: 'center',
      width: 110,
      render: row => {
        if (row.exceptions) {
          return (
            <NEllipsis style="max-width: 110px">
              <NTag type="error">有异常</NTag>
            </NEllipsis>
          );
        }
        return <NTag type="success">无异常</NTag>;
      }
    },
    {
      key: 'executionTime',
      title: '执行时间',
      align: 'center',
      width: 180,
      render: row => {
        if (!row.executionTime) return '-';
        const date = new Date(row.executionTime);
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
          <NPopconfirm onPositiveClick={() => handleDelete(row.id)}>
            {{
              default: () => $t('common.confirmDelete'),
              trigger: () => (
                <NButton type="error" ghost size="small" disabled={!hasAuth(permissions.delete)}>
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

const checkedRowKeys = ref<string[]>([]);
const detailDrawerVisible = ref(false);
const selectedLog = ref<Api.SystemManage.InternalAPILog | null>(null);

function viewDetail(row: Api.SystemManage.InternalAPILog) {
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
      const deletePromises = checkedRowKeys.value.map(id => fetchDeleteInternalAPILog(id));
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
  const { error } = await fetchDeleteInternalAPILog(id);
  if (!error) {
    window.$message?.success('删除成功');
    getData();
  }
}
</script>

<template>
  <div class="min-h-500px flex-col-stretch gap-16px overflow-hidden lt-sm:overflow-auto">
    <InternalAPILogSearch v-model:model="searchParams" @search="getDataByPage" />
    <NCard title="内部API日志" :bordered="false" size="small" class="card-wrapper sm:flex-1-hidden">
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
        :scroll-x="1600"
        :loading="loading"
        remote
        :row-key="row => row.id"
        :pagination="mobilePagination"
        class="sm:h-full"
      />
      <InternalAPILogDetailDrawer v-model:visible="detailDrawerVisible" :log-data="selectedLog" />
    </NCard>
  </div>
</template>
