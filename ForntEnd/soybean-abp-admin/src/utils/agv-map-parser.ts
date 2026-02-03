import type { XAMPMapData } from '@/typings/agv-map';

/**
 * Parse XMAP file content to map data
 * Supports XMAP format (laser scan point cloud data)
 */
export function parseXAMPFile(content: string): XAMPMapData {
  try {
    const parser = new DOMParser();
    const xmlDoc = parser.parseFromString(content, 'text/xml');

    // Check for parsing errors
    const parserError = xmlDoc.querySelector('parsererror');
    if (parserError) {
      throw new Error('XML parsing error');
    }

    const nodes: XAMPMapData['nodes'] = [];
    const edges: XAMPMapData['edges'] = [];
    const routes: XAMPMapData['routes'] = [];

    // Parse point cloud data
    const pointElements = xmlDoc.querySelectorAll('point');
    const points: Array<{ x: number; y: number; index: number }> = [];

    pointElements.forEach((point, index) => {
      const x = parseFloat(point.getAttribute('x') || '0');
      const y = parseFloat(point.getAttribute('y') || '0');
      points.push({ x, y, index });
    });

    if (points.length === 0) {
      throw new Error('No points found in XMAP file');
    }

    // Calculate actual bounds from point cloud data instead of using header
    let actualMinX = points[0].x;
    let actualMaxX = points[0].x;
    let actualMinY = points[0].y;
    let actualMaxY = points[0].y;

    points.forEach(point => {
      actualMinX = Math.min(actualMinX, point.x);
      actualMaxX = Math.max(actualMaxX, point.x);
      actualMinY = Math.min(actualMinY, point.y);
      actualMaxY = Math.max(actualMaxY, point.y);
    });

    // Normalize coordinates based on actual data bounds
    const offsetX = -actualMinX;
    const offsetY = -actualMinY;

    points.forEach(point => {
      point.x += offsetX;
      point.y += offsetY;
    });

    const mapWidth = actualMaxX - actualMinX;
    const mapHeight = actualMaxY - actualMinY;

    // Scale to reasonable canvas size
    const targetWidth = 1000;
    const targetHeight = 800;
    const scaleX = targetWidth / mapWidth;
    const scaleY = targetHeight / mapHeight;
    const scale = Math.min(scaleX, scaleY);

    points.forEach(point => {
      point.x *= scale;
      point.y *= scale;
    });

    // Aggressive sampling for performance - keep ~500 points
    const targetNodeCount = 500;
    const sampleRate = Math.max(1, Math.floor(points.length / targetNodeCount));

    // Sample points while preserving order
    const sampledPoints: Array<{ x: number; y: number; index: number }> = [];
    for (let i = 0; i < points.length; i += sampleRate) {
      sampledPoints.push(points[i]);
    }

    // Convert to nodes
    sampledPoints.forEach((point, idx) => {
      nodes.push({
        id: `P${idx}`,
        x: point.x,
        y: point.y,
        type: 'scan'
      });
    });

    // Connect consecutive points to form the scan path
    // XMAP files contain sequential laser scan data, so consecutive points should be connected
    for (let i = 0; i < nodes.length - 1; i++) {
      const node1 = nodes[i];
      const node2 = nodes[i + 1];

      const dx = node2.x - node1.x;
      const dy = node2.y - node1.y;
      const distance = Math.sqrt(dx * dx + dy * dy);

      // Connect if distance is reasonable (not a scan jump)
      // Adjust threshold based on map size
      const threshold = Math.max(targetWidth, targetHeight) * 0.05; // 5% of map size

      if (distance < threshold) {
        edges.push({
          id: `E${i}`,
          from: node1.id,
          to: node2.id,
          color: '#00ffff',
          width: 1
        });
      }
    }

    // Create route
    routes.push({
      name: '扫描轮廓',
      color: '#00ffff',
      nodes: nodes.length,
      edges: []
    });

    // eslint-disable-next-line no-console
    console.log(
      `Parsed XMAP: ${nodes.length} nodes, ${edges.length} edges from ${points.length} points (${Math.round((edges.length / (nodes.length - 1)) * 100)}% connected)`
    );

    return {
      nodes,
      edges,
      routes,
      width: targetWidth + 50,
      height: targetHeight + 50
    };
  } catch (error) {
    // eslint-disable-next-line no-console
    console.error('Failed to parse XMAP file:', error);
    throw new Error('Invalid XMAP file format');
  }
}

/**
 * Generate sample map data for testing
 */
export function generateSampleMapData(): XAMPMapData {
  const nodes = [
    { id: '入口A', x: 100, y: 100 },
    { id: '入口B', x: 100, y: 400 },
    { id: '工位1', x: 400, y: 100 },
    { id: '工位2', x: 400, y: 400 },
    { id: '出口', x: 700, y: 100 },
    { id: '仓库', x: 700, y: 250 },
    { id: '出1', x: 700, y: 400 }
  ];

  const edges = [
    { id: 'e1', from: '入口A', to: '工位1', color: '#ffa500', width: 3 },
    { id: 'e2', from: '工位1', to: '出口', color: '#ff0000', width: 2 },
    { id: 'e4', from: '入口B', to: '工位2', color: '#ffa500', width: 3 },
    { id: 'e5', from: '工位2', to: '仓库', color: '#00ff00', width: 2 },
    { id: 'e7', from: '工位1', to: '出1', color: '#ff00ff', width: 2 },
    { id: 'e9', from: '入口A', to: '入口B', color: '#ff0000', width: 2 },
    { id: 'e10', from: '工位1', to: '工位2', color: '#ff0000', width: 2 },
    { id: 'e11', from: '出口', to: '仓库', color: '#00ffff', width: 2 },
    { id: 'e12', from: '仓库', to: '出1', color: '#00ffff', width: 2 }
  ];

  const routes = [
    { name: '主运输线', color: '#ffa500', nodes: 7, edges: edges.filter(e => e.color === '#ffa500') },
    { name: '充电路线', color: '#00ff00', nodes: 5, edges: edges.filter(e => e.color === '#00ff00') },
    { name: '出货路线', color: '#ff00ff', nodes: 4, edges: edges.filter(e => e.color === '#ff00ff') },
    { name: '巡检路线', color: '#ffa500', nodes: 6, edges: [] },
    { name: '快速通道', color: '#ff0000', nodes: 8, edges: edges.filter(e => e.color === '#ff0000') }
  ];

  return {
    nodes,
    edges,
    routes,
    width: 900,
    height: 600
  };
}
