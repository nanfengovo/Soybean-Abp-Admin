<script setup lang="ts">
import type { XAMPRoute } from '@/typings/agv-map';

interface Props {
  routes: XAMPRoute[];
  isDarkMode?: boolean;
}

defineProps<Props>();

const emit = defineEmits<{
  add: [];
  select: [route: XAMPRoute];
}>();
</script>

<template>
  <div class="route-panel" :class="{ 'light-mode': !isDarkMode }">
    <div class="panel-header">
      <div class="header-title">
        <span class="title-icon">—</span>
        <span class="title-text">路径管理</span>
      </div>
      <NButton type="primary" size="small" ghost @click="emit('add')">+ 添加</NButton>
    </div>

    <div class="route-list">
      <div v-for="route in routes" :key="route.name" class="route-item" @click="emit('select', route)">
        <div class="route-color" :style="{ background: route.color }" />
        <div class="route-info">
          <div class="route-name">{{ route.name }}</div>
          <div class="route-nodes">{{ route.nodes }} 节点</div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.route-panel {
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

.route-panel.light-mode {
  background: rgba(255, 255, 255, 0.9);
  border: 1px solid rgba(0, 0, 0, 0.15);
  color: #1a1a1a;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.header-title {
  display: flex;
  align-items: center;
  gap: 8px;
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

.route-list {
  flex: 1;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.route-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  background: rgba(0, 255, 255, 0.05);
  border: 1px solid rgba(0, 255, 255, 0.2);
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.3s;
}

.light-mode .route-item {
  background: rgba(24, 144, 255, 0.05);
  border: 1px solid rgba(24, 144, 255, 0.2);
}

.route-item:hover {
  background: rgba(0, 255, 255, 0.1);
  border-color: rgba(0, 255, 255, 0.4);
}

.light-mode .route-item:hover {
  background: rgba(24, 144, 255, 0.1);
  border-color: rgba(24, 144, 255, 0.4);
}

.route-color {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  flex-shrink: 0;
}

.route-info {
  flex: 1;
}

.route-name {
  font-size: 14px;
  font-weight: 500;
  margin-bottom: 4px;
}

.route-nodes {
  font-size: 12px;
  color: rgba(255, 255, 255, 0.6);
}

.light-mode .route-nodes {
  color: rgba(0, 0, 0, 0.6);
}
</style>
