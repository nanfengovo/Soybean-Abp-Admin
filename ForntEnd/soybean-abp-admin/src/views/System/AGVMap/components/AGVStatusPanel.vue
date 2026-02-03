<script setup lang="ts">
import type { AGVStatus } from '@/typings/agv-map';

interface Props {
  agvList: AGVStatus[];
  onlineCount: number;
  totalCount: number;
  shrinkage: number;
  isDarkMode?: boolean;
}

defineProps<Props>();

const emit = defineEmits<{
  pause: [id: string];
  resume: [id: string];
}>();

function getStatusText(status: AGVStatus['status']) {
  const statusMap = {
    running: '运行中',
    charging: '充电中',
    idle: '空闲',
    paused: '暂停'
  };
  return statusMap[status];
}

function getStatusColor(status: AGVStatus['status']): 'success' | 'warning' | 'default' | 'error' {
  const colorMap: Record<AGVStatus['status'], 'success' | 'warning' | 'default' | 'error'> = {
    running: 'success',
    charging: 'warning',
    idle: 'default',
    paused: 'error'
  };
  return colorMap[status];
}

function handlePause(agv: AGVStatus) {
  if (agv.status === 'running') {
    emit('pause', agv.id);
  } else if (agv.status === 'paused') {
    emit('resume', agv.id);
  }
}
</script>

<template>
  <div class="agv-status-panel" :class="{ 'light-mode': !isDarkMode }">
    <div class="panel-header">
      <div class="header-title">
        <span class="title-icon">—</span>
        <span class="title-text">AGV 车辆状态</span>
      </div>
      <div class="header-stats">
        <span class="stat-item">在线设备: {{ onlineCount }}/{{ totalCount }}</span>
        <span class="stat-item">缩放: {{ Math.round(shrinkage * 100) }}%</span>
      </div>
    </div>

    <div class="agv-list">
      <div v-for="agv in agvList" :key="agv.id" class="agv-item">
        <div class="agv-header">
          <div class="agv-id">
            <NTag :type="getStatusColor(agv.status)" size="small" round>
              <template #icon>
                <span class="status-dot" />
              </template>
              {{ getStatusText(agv.status) }}
            </NTag>
            <span class="agv-name">{{ agv.id }}</span>
          </div>
        </div>

        <div class="agv-info">
          <div class="info-row">
            <span class="info-label">名称:</span>
            <span class="info-value">{{ agv.name }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">位置:</span>
            <span class="info-value">({{ Math.round(agv.position.x) }}, {{ Math.round(agv.position.y) }})</span>
          </div>
          <div class="info-row">
            <span class="info-label">路径:</span>
            <span class="info-value">{{ agv.route }}</span>
          </div>
          <div v-if="agv.battery !== undefined" class="info-row">
            <span class="info-label">电量:</span>
            <NProgress
              type="line"
              :percentage="agv.battery"
              :color="agv.battery > 50 ? '#00ff00' : agv.battery > 20 ? '#ffff00' : '#ff0000'"
              :height="8"
              :border-radius="4"
              :fill-border-radius="4"
            />
          </div>
        </div>

        <div class="agv-actions">
          <NButton v-if="agv.status === 'running'" type="warning" size="small" ghost @click="handlePause(agv)">
            暂停
          </NButton>
          <NButton v-else-if="agv.status === 'paused'" type="success" size="small" ghost @click="handlePause(agv)">
            启动
          </NButton>
          <NButton v-else type="default" size="small" ghost disabled>
            {{ getStatusText(agv.status) }}
          </NButton>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.agv-status-panel {
  background: rgba(10, 10, 15, 0.9);
  border: 1px solid rgba(0, 255, 255, 0.3);
  border-radius: 8px;
  padding: 16px;
  color: #00ffff;
  height: 100%;
  display: flex;
  flex-direction: column;
  transition: all 0.3s ease;
}

.agv-status-panel.light-mode {
  background: rgba(255, 255, 255, 0.9);
  border: 1px solid rgba(0, 0, 0, 0.15);
  color: #1a1a1a;
}

.panel-header {
  margin-bottom: 16px;
}

.header-title {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
}

.title-icon {
  color: #00ffff;
  font-size: 20px;
  font-weight: bold;
}

.light-mode .title-icon {
  color: #1890ff;
}

.title-text {
  font-size: 16px;
  font-weight: 500;
}

.header-stats {
  display: flex;
  gap: 16px;
  font-size: 12px;
  color: rgba(255, 255, 255, 0.7);
}

.light-mode .header-stats {
  color: rgba(0, 0, 0, 0.7);
}

.stat-item {
  display: flex;
  align-items: center;
}

.agv-list {
  flex: 1;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.agv-item {
  background: rgba(0, 255, 255, 0.05);
  border: 1px solid rgba(0, 255, 255, 0.2);
  border-radius: 6px;
  padding: 12px;
  transition: all 0.3s;
}

.light-mode .agv-item {
  background: rgba(24, 144, 255, 0.05);
  border: 1px solid rgba(24, 144, 255, 0.2);
}

.agv-item:hover {
  background: rgba(0, 255, 255, 0.1);
  border-color: rgba(0, 255, 255, 0.4);
}

.light-mode .agv-item:hover {
  background: rgba(24, 144, 255, 0.1);
  border-color: rgba(24, 144, 255, 0.4);
}

.agv-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.agv-id {
  display: flex;
  align-items: center;
  gap: 8px;
}

.agv-name {
  font-weight: 500;
  font-size: 14px;
}

.status-dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: currentColor;
  display: inline-block;
}

.agv-info {
  display: flex;
  flex-direction: column;
  gap: 6px;
  margin-bottom: 8px;
}

.info-row {
  display: flex;
  align-items: center;
  font-size: 12px;
}

.info-label {
  color: rgba(255, 255, 255, 0.6);
  min-width: 50px;
}

.light-mode .info-label {
  color: rgba(0, 0, 0, 0.6);
}

.info-value {
  color: rgba(255, 255, 255, 0.9);
  flex: 1;
}

.light-mode .info-value {
  color: rgba(0, 0, 0, 0.9);
}

.agv-actions {
  display: flex;
  justify-content: flex-end;
}
</style>
