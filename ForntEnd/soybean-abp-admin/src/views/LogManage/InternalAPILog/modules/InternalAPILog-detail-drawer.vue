<script setup lang="ts">
import { computed } from 'vue';

defineOptions({
  name: 'InternalAPILogDetailDrawer'
});

interface Props {
  /** drawer visible */
  visible: boolean;
  /** log data */
  logData?: Api.SystemManage.InternalAPILog | null;
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

function formatJson(jsonString: string | null) {
  if (!jsonString) return '-';
  try {
    const obj = JSON.parse(jsonString);
    return JSON.stringify(obj, null, 2);
  } catch {
    return jsonString;
  }
}
</script>

<template>
  <NDrawer v-model:show="drawerVisible" :width="800" display-directive="show">
    <NDrawerContent :title="title" :native-scrollbar="false" closable>
      <NDescriptions v-if="logData" label-placement="left" :column="1" bordered>
        <NDescriptionsItem label="应用名称">
          <NTag type="info">{{ logData.applicationName }}</NTag>
        </NDescriptionsItem>
        <NDescriptionsItem label="请求URL">
          <NText code>{{ logData.url }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="HTTP方法">
          <NTag :type="logData.httpMethod === 'GET' ? 'success' : 'info'">{{ logData.httpMethod }}</NTag>
        </NDescriptionsItem>
        <NDescriptionsItem label="HTTP状态码">
          <NTag :type="getStatusTag(logData.httpStatusCode)">{{ logData.httpStatusCode }}</NTag>
        </NDescriptionsItem>
        <NDescriptionsItem label="执行时间">
          <NText>{{ formatDateTime(logData.executionTime) }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="执行耗时">
          <NTag :type="logData.executionDuration > 1000 ? 'warning' : 'success'">
            {{ logData.executionDuration }} 毫秒
          </NTag>
        </NDescriptionsItem>
        <NDescriptionsItem label="是否有异常">
          <NTag :type="logData.exceptions ? 'error' : 'success'">
            {{ logData.exceptions ? '有异常' : '无异常' }}
          </NTag>
        </NDescriptionsItem>
        <NDescriptionsItem label="用户名">
          <NText>{{ logData.userName || '-' }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="用户ID">
          <NText code>{{ logData.userId || '-' }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="租户名称">
          <NText>{{ logData.tenantName || '-' }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="租户ID">
          <NText code>{{ logData.tenantId || '-' }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="模拟用户ID">
          <NText code>{{ logData.impersonatorUserId || '-' }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="模拟租户ID">
          <NText code>{{ logData.impersonatorTenantId || '-' }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="客户端IP地址">
          <NText>{{ logData.clientIpAddress || '-' }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="客户端名称">
          <NText>{{ logData.clientName || '-' }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="客户端ID">
          <NText code>{{ logData.clientId || '-' }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="关联ID">
          <NText code>{{ logData.correlationId || '-' }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem label="浏览器信息">
          <NText>{{ logData.browserInfo || '-' }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem v-if="logData.comments" label="备注">
          <NText>{{ logData.comments }}</NText>
        </NDescriptionsItem>
        <NDescriptionsItem v-if="logData.exceptions" label="异常信息">
          <NCode :code="formatJson(logData.exceptions)" language="json" />
        </NDescriptionsItem>
      </NDescriptions>
    </NDrawerContent>
  </NDrawer>
</template>

<style scoped></style>
