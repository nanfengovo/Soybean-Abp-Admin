<script setup lang="ts">
import { onMounted, onUnmounted, ref, watch } from 'vue';
import type { AGVStatus, XAMPMapData } from '@/typings/agv-map';

interface Props {
  mapData: XAMPMapData;
  agvList?: AGVStatus[];
  zoom?: number;
  isDarkMode?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  agvList: () => [],
  zoom: 1,
  isDarkMode: true
});

const canvasRef = ref<HTMLCanvasElement>();
const ctx = ref<CanvasRenderingContext2D | null>(null);
const scale = ref(1);
const offset = ref({ x: 0, y: 0 });
const isDragging = ref(false);
const lastMousePos = ref({ x: 0, y: 0 });
const dpr = ref(window.devicePixelRatio || 1);

// Cache for performance
let animationFrameId: number | null = null;
let needsRedraw = true;

onMounted(() => {
  if (canvasRef.value) {
    ctx.value = canvasRef.value.getContext('2d', {
      alpha: false, // Disable alpha for better performance
      desynchronized: true // Allow async rendering
    });
    initCanvas();
    scheduleRedraw();

    window.addEventListener('resize', handleResize);
  }
});

onUnmounted(() => {
  window.removeEventListener('resize', handleResize);
  if (animationFrameId) {
    cancelAnimationFrame(animationFrameId);
  }
});

watch(
  () => [props.mapData, props.agvList, props.isDarkMode],
  () => {
    scheduleRedraw();
  },
  { deep: true }
);

watch(
  () => props.zoom,
  () => {
    initCanvas();
    scheduleRedraw();
  }
);

function scheduleRedraw() {
  needsRedraw = true;
  if (!animationFrameId) {
    animationFrameId = requestAnimationFrame(drawLoop);
  }
}

function drawLoop() {
  if (needsRedraw) {
    drawMap();
    needsRedraw = false;
  }
  animationFrameId = requestAnimationFrame(drawLoop);
}

function handleResize() {
  initCanvas();
  scheduleRedraw();
}

function initCanvas() {
  if (!canvasRef.value) return;

  const container = canvasRef.value.parentElement;
  if (!container) return;

  const displayWidth = container.clientWidth;
  const displayHeight = container.clientHeight;

  canvasRef.value.width = displayWidth * dpr.value;
  canvasRef.value.height = displayHeight * dpr.value;

  canvasRef.value.style.width = `${displayWidth}px`;
  canvasRef.value.style.height = `${displayHeight}px`;

  if (ctx.value) {
    ctx.value.scale(dpr.value, dpr.value);
  }

  const scaleX = (displayWidth - 100) / props.mapData.width;
  const scaleY = (displayHeight - 100) / props.mapData.height;
  scale.value = Math.min(scaleX, scaleY, 1) * props.zoom;

  offset.value = {
    x: (displayWidth - props.mapData.width * scale.value) / 2,
    y: (displayHeight - props.mapData.height * scale.value) / 2
  };
}

function drawMap() {
  if (!ctx.value || !canvasRef.value) return;

  const context = ctx.value;
  const displayWidth = canvasRef.value.clientWidth;
  const displayHeight = canvasRef.value.clientHeight;

  // Clear with theme-appropriate color
  context.fillStyle = props.isDarkMode ? '#0a0a0f' : '#f5f5f5';
  context.fillRect(0, 0, displayWidth, displayHeight);

  // Draw grid
  drawGrid(context, displayWidth, displayHeight);

  context.save();
  context.translate(offset.value.x, offset.value.y);
  context.scale(scale.value, scale.value);

  // Disable anti-aliasing for better performance on large datasets
  context.imageSmoothingEnabled = false;

  // Draw all edges in one path for better performance
  drawAllEdges(context);

  // Draw all nodes in one path
  drawAllNodes(context);

  // Draw AGVs
  props.agvList.forEach(agv => {
    drawAGV(context, agv);
  });

  context.restore();
}

function drawGrid(context: CanvasRenderingContext2D, width: number, height: number) {
  context.strokeStyle = props.isDarkMode ? 'rgba(26, 26, 46, 0.3)' : 'rgba(200, 200, 200, 0.3)';
  context.lineWidth = 0.5;

  const gridSize = 50 * scale.value;

  context.beginPath();
  for (let x = offset.value.x % gridSize; x < width; x += gridSize) {
    context.moveTo(x, 0);
    context.lineTo(x, height);
  }
  for (let y = offset.value.y % gridSize; y < height; y += gridSize) {
    context.moveTo(0, y);
    context.lineTo(width, y);
  }
  context.stroke();
}

function drawAllEdges(context: CanvasRenderingContext2D) {
  if (props.mapData.edges.length === 0) return;

  context.lineCap = 'round';

  // Group edges by color for better performance
  const edgesByColor = new Map<string, typeof props.mapData.edges>();
  props.mapData.edges.forEach(edge => {
    const color = edge.color || '#00ffff';
    if (!edgesByColor.has(color)) {
      edgesByColor.set(color, []);
    }
    edgesByColor.get(color)!.push(edge);
  });

  // Draw each color group
  edgesByColor.forEach((edges, color) => {
    context.strokeStyle = color;
    context.lineWidth = (edges[0]?.width || 1) / scale.value;

    context.beginPath();
    edges.forEach(edge => {
      const fromNode = props.mapData.nodes.find(n => n.id === edge.from);
      const toNode = props.mapData.nodes.find(n => n.id === edge.to);

      if (fromNode && toNode) {
        context.moveTo(fromNode.x, fromNode.y);
        context.lineTo(toNode.x, toNode.y);
      }
    });
    context.stroke();
  });
}

function drawAllNodes(context: CanvasRenderingContext2D) {
  if (props.mapData.nodes.length === 0) return;

  const nodeSize = 2 / scale.value;

  // Draw all nodes as small dots
  context.fillStyle = '#00ffff';
  context.beginPath();
  props.mapData.nodes.forEach(node => {
    context.moveTo(node.x + nodeSize, node.y);
    context.arc(node.x, node.y, nodeSize, 0, Math.PI * 2);
  });
  context.fill();
}

function drawAGV(context: CanvasRenderingContext2D, agv: AGVStatus) {
  const { x, y } = agv.position;

  const statusColors = {
    running: '#00ff00',
    charging: '#ffff00',
    idle: '#808080',
    paused: '#ff9900'
  };

  const width = 40 / scale.value;
  const height = 28 / scale.value;
  const radius = 6 / scale.value;

  // Draw shadow
  context.shadowBlur = 10;
  context.shadowColor = 'rgba(0, 0, 0, 0.5)';
  context.shadowOffsetY = 2;

  // Draw AGV body
  context.fillStyle = statusColors[agv.status];
  context.strokeStyle = '#ffffff';
  context.lineWidth = 2 / scale.value;

  context.beginPath();
  context.moveTo(x - width / 2 + radius, y - height / 2);
  context.lineTo(x + width / 2 - radius, y - height / 2);
  context.quadraticCurveTo(x + width / 2, y - height / 2, x + width / 2, y - height / 2 + radius);
  context.lineTo(x + width / 2, y + height / 2 - radius);
  context.quadraticCurveTo(x + width / 2, y + height / 2, x + width / 2 - radius, y + height / 2);
  context.lineTo(x - width / 2 + radius, y + height / 2);
  context.quadraticCurveTo(x - width / 2, y + height / 2, x - width / 2, y + height / 2 - radius);
  context.lineTo(x - width / 2, y - height / 2 + radius);
  context.quadraticCurveTo(x - width / 2, y - height / 2, x - width / 2 + radius, y - height / 2);
  context.closePath();
  context.fill();
  context.stroke();

  context.shadowBlur = 0;
  context.shadowOffsetY = 0;

  // Draw AGV ID
  const fontSize = 11 / scale.value;
  context.fillStyle = '#000000';
  context.font = `bold ${fontSize}px Arial`;
  context.textAlign = 'center';
  context.textBaseline = 'middle';
  context.fillText(agv.id, x, y);

  // Draw name below
  const labelFontSize = 9 / scale.value;
  context.fillStyle = '#ffffff';
  context.font = `${labelFontSize}px Arial`;
  context.fillText(agv.name, x, y + height / 2 + labelFontSize + 3 / scale.value);
}

function handleWheel(event: WheelEvent) {
  event.preventDefault();
  const delta = event.deltaY > 0 ? 0.9 : 1.1;
  const newScale = Math.max(0.1, Math.min(5, scale.value * delta));

  const rect = canvasRef.value?.getBoundingClientRect();
  if (rect) {
    const mouseX = event.clientX - rect.left;
    const mouseY = event.clientY - rect.top;

    const scaleRatio = newScale / scale.value;
    offset.value.x = mouseX - (mouseX - offset.value.x) * scaleRatio;
    offset.value.y = mouseY - (mouseY - offset.value.y) * scaleRatio;
  }

  scale.value = newScale;
  scheduleRedraw();
}

function handleMouseDown(event: MouseEvent) {
  isDragging.value = true;
  lastMousePos.value = { x: event.clientX, y: event.clientY };
  if (canvasRef.value) {
    canvasRef.value.style.cursor = 'grabbing';
  }
}

function handleMouseMove(event: MouseEvent) {
  if (!isDragging.value) return;

  const dx = event.clientX - lastMousePos.value.x;
  const dy = event.clientY - lastMousePos.value.y;

  offset.value.x += dx;
  offset.value.y += dy;

  lastMousePos.value = { x: event.clientX, y: event.clientY };
  scheduleRedraw();
}

function handleMouseUp() {
  isDragging.value = false;
  if (canvasRef.value) {
    canvasRef.value.style.cursor = 'grab';
  }
}
</script>

<template>
  <div class="agv-map-canvas-container">
    <canvas
      ref="canvasRef"
      class="agv-map-canvas"
      @wheel="handleWheel"
      @mousedown="handleMouseDown"
      @mousemove="handleMouseMove"
      @mouseup="handleMouseUp"
      @mouseleave="handleMouseUp"
    />
  </div>
</template>

<style scoped>
.agv-map-canvas-container {
  width: 100%;
  height: 100%;
  background: #0a0a0f;
  position: relative;
  overflow: hidden;
  transition: background 0.3s ease;
}

.agv-map-canvas {
  width: 100%;
  height: 100%;
  cursor: grab;
  display: block;
}

.agv-map-canvas:active {
  cursor: grabbing;
}
</style>
