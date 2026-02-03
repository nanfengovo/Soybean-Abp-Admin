<script setup lang="ts">
import { computed, ref } from 'vue';
import type { UploadFileInfo } from 'naive-ui';
import { generateSampleMapData, parseXAMPFile } from '@/utils/agv-map-parser';
import type { AGVStatus, XAMPMapData } from '@/typings/agv-map';
import MapCanvas from './components/MapCanvas.vue';
import RoutePanel from './components/RoutePanel.vue';
import AGVStatusPanel from './components/AGVStatusPanel.vue';

const mapData = ref<XAMPMapData>(generateSampleMapData());
const zoom = ref(1);
const isDarkMode = ref(true);

// Computed AGV positions based on map dimensions
const agvList = computed<AGVStatus[]>(() => {
  // Find AGV nodes from the map data
  const agvNodes = mapData.value.nodes.filter(node => node.id.startsWith('AGV-'));

  // Only return AGVs if they exist in the map data
  if (agvNodes.length > 0) {
    return agvNodes.map((node, index) => ({
      id: node.id,
      name: `搬运车${index + 1}号`,
      position: { x: node.x, y: node.y },
      status: index === 0 ? 'running' : index === 1 ? 'running' : 'charging',
      route: index === 0 ? '主运输线' : index === 1 ? '充电路线' : '出货路线',
      battery: index === 0 ? 85 : index === 1 ? 45 : 15
    })) as AGVStatus[];
  }

  // Return empty array if no AGV nodes found
  return [];
});

const onlineCount = computed(() => agvList.value.filter(agv => agv.status !== 'idle').length);
const totalCount = computed(() => agvList.value.length);

function handleFileUpload(options: { file: UploadFileInfo }) {
  const file = options.file.file;
  if (!file) return;

  const reader = new FileReader();
  reader.onload = e => {
    try {
      const content = e.target?.result as string;
      const parsedData = parseXAMPFile(content);
      mapData.value = parsedData;
      window.$message?.success('地图文件导入成功');
    } catch (error) {
      window.$message?.error('地图文件解析失败，请检查文件格式');
      // eslint-disable-next-line no-console
      console.error(error);
    }
  };
  reader.readAsText(file);
}

function handleZoomIn() {
  zoom.value = Math.min(zoom.value + 0.1, 3);
}

function handleZoomOut() {
  zoom.value = Math.max(zoom.value - 0.1, 0.3);
}

function handleReset() {
  zoom.value = 1;
}

function toggleDarkMode() {
  isDarkMode.value = !isDarkMode.value;
}

function handleAGVPause(id: string) {
  const agv = agvList.value.find(a => a.id === id);
  if (agv) {
    agv.status = 'paused';
    window.$message?.info(`${agv.name} 已暂停`);
  }
}

function handleAGVResume(id: string) {
  const agv = agvList.value.find(a => a.id === id);
  if (agv) {
    agv.status = 'running';
    window.$message?.success(`${agv.name} 已启动`);
  }
}

function handleRouteAdd() {
  window.$message?.info('添加路径功能开发中');
}

function handleRouteSelect(route: any) {
  window.$message?.info(`选中路径: ${route.name}`);
}
</script>

<template>
  <div class="agv-map-container" :class="{ 'light-mode': !isDarkMode }">
    <!-- Header -->
    <div class="map-header">
      <div class="header-left">
        <span class="header-icon">●</span>
        <span class="header-title">AGV 调度监控系统</span>
      </div>
      <div class="header-right">
        <NSpace>
          <NUpload :show-file-list="false" accept=".xml,.xmap" :custom-request="handleFileUpload">
            <NButton type="primary" ghost size="small">
              <template #icon>
                <icon-mdi-upload />
              </template>
              导入地图
            </NButton>
          </NUpload>
          <NButton size="small" ghost @click="handleZoomOut">
            <template #icon>
              <icon-mdi-minus />
            </template>
          </NButton>
          <NButton size="small" ghost @click="handleReset">重置</NButton>
          <NButton size="small" ghost @click="handleZoomIn">
            <template #icon>
              <icon-mdi-plus />
            </template>
          </NButton>
          <NButton size="small" ghost @click="toggleDarkMode">
            <template #icon>
              <icon-mdi-theme-light-dark v-if="isDarkMode" />
              <icon-mdi-white-balance-sunny v-else />
            </template>
            {{ isDarkMode ? '浅色模式' : '深色模式' }}
          </NButton>
        </NSpace>
      </div>
    </div>

    <!-- Main Content -->
    <div class="map-content">
      <!-- Left Panel - Route Management -->
      <div class="left-panel">
        <RoutePanel :routes="mapData.routes" :is-dark-mode="isDarkMode" @add="handleRouteAdd" @select="handleRouteSelect" />
      </div>

      <!-- Center - Map Canvas -->
      <div class="map-canvas-wrapper">
        <MapCanvas :map-data="mapData" :agv-list="agvList" :zoom="zoom" :is-dark-mode="isDarkMode" />

        <!-- Zoom Controls -->
        <div class="zoom-controls">
          <NButton circle size="small" @click="handleZoomOut">
            <template #icon>
              <icon-mdi-minus />
            </template>
          </NButton>
          <div class="zoom-display">{{ Math.round(zoom * 100) }}%</div>
          <NButton circle size="small" @click="handleZoomIn">
            <template #icon>
              <icon-mdi-plus />
            </template>
          </NButton>
        </div>

        <!-- Instructions -->
        <div class="map-instructions">
          <span>Shift+拖拽 平移 | 滚轮 缩放</span>
        </div>
      </div>

      <!-- Right Panel - AGV Status -->
      <div class="right-panel">
        <AGVStatusPanel
          :agv-list="agvList"
          :online-count="onlineCount"
          :total-count="totalCount"
          :shrinkage="zoom"
          :is-dark-mode="isDarkMode"
          @pause="handleAGVPause"
          @resume="handleAGVResume"
        />
      </div>
    </div>
  </div>
</template>

<style scoped>
.agv-map-container {
  width: 100%;
  height: 100vh;
  background: #0a0a0f;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  transition: background 0.3s ease;
}

.agv-map-container.light-mode {
  background: #f5f5f5;
}

.map-header {
  height: 60px;
  background: rgba(10, 10, 15, 0.95);
  border-bottom: 1px solid rgba(0, 255, 255, 0.3);
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 24px;
  color: #00ffff;
  transition: all 0.3s ease;
}

.light-mode .map-header {
  background: rgba(255, 255, 255, 0.95);
  border-bottom: 1px solid rgba(0, 0, 0, 0.1);
  color: #1a1a1a;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.header-icon {
  color: #00ffff;
  font-size: 24px;
}

.light-mode .header-icon {
  color: #1890ff;
}

.header-title {
  font-size: 18px;
  font-weight: 500;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.map-content {
  flex: 1;
  display: flex;
  gap: 16px;
  padding: 16px;
  overflow: hidden;
}

.left-panel {
  width: 280px;
  flex-shrink: 0;
}

.map-canvas-wrapper {
  flex: 1;
  position: relative;
  border: 1px solid rgba(0, 255, 255, 0.3);
  border-radius: 8px;
  overflow: hidden;
  transition: border-color 0.3s ease;
}

.light-mode .map-canvas-wrapper {
  border: 1px solid rgba(0, 0, 0, 0.15);
}

.right-panel {
  width: 320px;
  flex-shrink: 0;
}

.zoom-controls {
  position: absolute;
  bottom: 80px;
  right: 20px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  background: rgba(10, 10, 15, 0.9);
  border: 1px solid rgba(0, 255, 255, 0.3);
  border-radius: 8px;
  padding: 8px;
  transition: all 0.3s ease;
}

.light-mode .zoom-controls {
  background: rgba(255, 255, 255, 0.9);
  border: 1px solid rgba(0, 0, 0, 0.15);
}

.zoom-display {
  text-align: center;
  color: #00ffff;
  font-size: 12px;
  padding: 4px 0;
  transition: color 0.3s ease;
}

.light-mode .zoom-display {
  color: #1a1a1a;
}

.map-instructions {
  position: absolute;
  bottom: 20px;
  left: 50%;
  transform: translateX(-50%);
  background: rgba(10, 10, 15, 0.9);
  border: 1px solid rgba(0, 255, 255, 0.3);
  border-radius: 6px;
  padding: 8px 16px;
  color: rgba(255, 255, 255, 0.7);
  font-size: 12px;
  transition: all 0.3s ease;
}

.light-mode .map-instructions {
  background: rgba(255, 255, 255, 0.9);
  border: 1px solid rgba(0, 0, 0, 0.15);
  color: rgba(0, 0, 0, 0.7);
}
</style>
