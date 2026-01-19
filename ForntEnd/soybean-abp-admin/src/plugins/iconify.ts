import { addAPIProvider, addCollection } from '@iconify/vue';

// 导入本地图标集（离线使用）
// 根据项目实际使用的图标集添加或删除
import iconsAntDesign from '@iconify/json/json/ant-design.json';
import iconsMdi from '@iconify/json/json/mdi.json';
import iconsCarbon from '@iconify/json/json/carbon.json';
import iconsIc from '@iconify/json/json/ic.json';
import iconsMaterial from '@iconify/json/json/material-symbols.json';

/** Setup the iconify offline */
export function setupIconifyOffline() {
  const { VITE_ICONIFY_URL } = import.meta.env;

  if (VITE_ICONIFY_URL) {
    addAPIProvider('', { resources: [VITE_ICONIFY_URL] });
  }

  // 注册本地图标集，使图标在离线状态下可用
  addCollection(iconsAntDesign);
  addCollection(iconsMdi);
  addCollection(iconsMaterial);
  addCollection(iconsCarbon);
  addCollection(iconsIc);
}
