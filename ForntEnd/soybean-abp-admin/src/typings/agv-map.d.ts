export interface XAMPNode {
  id: string;
  x: number;
  y: number;
  type?: string;
}

export interface XAMPEdge {
  id: string;
  from: string;
  to: string;
  color?: string;
  width?: number;
}

export interface XAMPRoute {
  name: string;
  color: string;
  nodes: number;
  edges: XAMPEdge[];
}

export interface XAMPMapData {
  nodes: XAMPNode[];
  edges: XAMPEdge[];
  routes: XAMPRoute[];
  width: number;
  height: number;
}

export interface AGVStatus {
  id: string;
  name: string;
  position: { x: number; y: number };
  status: 'running' | 'charging' | 'idle' | 'paused';
  route: string;
  battery?: number;
}
