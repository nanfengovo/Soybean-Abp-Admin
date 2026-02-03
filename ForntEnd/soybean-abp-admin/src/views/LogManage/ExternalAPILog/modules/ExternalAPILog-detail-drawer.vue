<script setup lang="ts">
import { computed } from 'vue';

defineOptions({
  name: 'ExternalAPILogDetailDrawer'
});

interface Props {
  /** drawer visible */
  visible: boolean;
  /** log data */
  logData?: Api.SystemManage.ExternalAPILog | null;
}

const props = defineProps<Props>();

interface Emits {
  (e: 'update:visible', visible: boolean): void;
}

const emit = defineEmits<Emits>();

const drawerVisible = computed({
  get() {
    return props.visible;
  },
  set(visible) {
    emit('update:visible', visible);
  }
});

const title = computed(() => '日志详情');

function formatJson(jsonString: string | null) {
  if (!jsonString) return '-';
  try {
    const obj = JSON.parse(jsonString);
    return JSON.stringify(obj, null, 2);
  } catch {
    return jsonString;
  }
}

function formatDateTime(dateTime: string | null) {
  if (!dateTime) return '-';
  const date = new Date(dateTime);
  return date.toLocaleString('zh-CN');
}

function getStatusTag(statusCode: number) {
  if (statusCode >= 200 && statusCode < 300) {
    return 'success';
  }
  if (statusCode >= 400 && statusCode < 500) {
    return 'warning';
  }
  if (statusCode >= 500) {
    return 'error';
  }
  return 'default';
}
</script>

<template>
  <NDrawer v-model:show="drawerVisible" :width="800" display-directive="show">
    <NDrawerContent :title="title" :native-scrollbar="false" closable>
      <NDescriptions v-if="logData" label-placement="left" :column="1" bordered>
        <NDescriptionsItem label="系统名称">
          <NTag type="info">{{ logData.sysName }}</NTag>
        </NDescriptionsItem>
        <NDescriptionsItem label="请求URL">
          <NText code>{{ logData.url }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="请求路径">
          <NText code>{{ logData.path }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="HTTP方法">
          <NTag :type="logData.httpMethod === 'GET' ? 'success' : 'info'">{{ logData.httpMethod }}</NTag>
        </NDescriptionsItem>
        <NDescriptionsItem label="状态码">
          <NTag :type="getStatusTag(logData.statusCode)">{{ logData.statusCode }}</NTag>
        </NDescriptionsItem>
        <NDescriptionsItem label="是否成功">
          <NTag :type="logData.isSuccess ? 'success' : 'error'">
            {{ logData.isSuccess ? '成功' : '失败' }}
          </NTag>
        </NDescriptionsItem>
        <NDescriptionsItem label="耗时">
          <NText>{{ logData.duration }} 毫秒</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="追踪ID">
          <NText code>{{ logData.traceId }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="业务ID">
          <NText>{{ logData.businessId || '-' }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="业务类型">
          <NText>{{ logData.businessType || '-' }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="客户端IP">
          <NText>{{ logData.clientIpAddress || '-' }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="用户ID">
          <NText code>{{ logData.userId || '-' }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="租户ID">
          <NText>{{ logData.tenantId || '-' }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="创建时间">
          <NText>{{ formatDateTime(logData.creationTime) }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="请求头">
          <NCode v-if="logData.requestHeaders" :code="formatJson(logData.requestHeaders)" language="json" />
          <NText v-else>-</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="请求体">
          <NCode v-if="logData.requestBody" :code="formatJson(logData.requestBody)" language="json" />
          <NText v-else>-</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="响应头">
          <NCode v-if="logData.responseHeaders" :code="formatJson(logData.responseHeaders)" language="json" />
          <NText v-else>-</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="响应体">
          <NCode v-if="logData.responseBody" :code="formatJson(logData.responseBody)" language="json" />
          <NText v-else>-</NText>
        </NDescriptionsItem>
        <NDescriptionsItem v-if="logData.errorMessage" label="错误信息">
          <NText type="error">{{ logData.errorMessage }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem v-if="logData.errorStackTrace" label="错误堆栈">
          <NCode :code="logData.errorStackTrace" language="text" />
        </NDescriptionsItem>
        <NDescriptionsItem v-if="logData.extraData" label="额外数据">
          <NCode :code="formatJson(logData.extraData)" language="json" />
        </NDescriptionsItem>
      </NDescriptions>
    </NDrawerContent>
  </NDrawer>
</template>

<style scoped></style>
