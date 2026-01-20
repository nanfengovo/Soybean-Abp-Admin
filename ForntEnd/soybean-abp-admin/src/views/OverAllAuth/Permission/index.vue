<script setup lang="tsx">
import { reactive, ref } from 'vue';
import { NButton, NCard, NDataTable, NPopconfirm, NSpace, NTag } from 'naive-ui';
import { useBoolean } from '@sa/hooks';
import { fetchDeleteMenu, fetchGetMenuList, fetchSyncPermissions } from '@/service/api';
import { useAppStore } from '@/store/modules/app';
import { useSvgIcon } from '@/hooks/common/icon';
import { useNaiveTable } from '@/hooks/common/table';
import { $t } from '@/locales';
import TableHeaderOperation from '@/components/advanced/table-header-operation.vue';
import MenuSearch from './modules/menu-search.vue';
import MenuOperateDrawer from './modules/menu-operate-drawer.vue';

const { SvgIconVNode } = useSvgIcon();

const appStore = useAppStore();

const searchParams: Api.SystemManage.MenuSearchParams = reactive({
  Filter: '',
  Sorting: ''
});

// 选中的行
const checkedRowKeys = ref<string[]>([]);

// 菜单类型标签映射
const menuTypeMap: Record<Api.SystemManage.MenuType, { label: string; type: NaiveUI.ThemeColor }> = {
  0: { label: '目录', type: 'default' },
  1: { label: '菜单', type: 'info' },
  2: { label: '按钮', type: 'warning' }
};

// 构建树形结构
function buildTree(items: Api.SystemManage.Menu[]): Api.SystemManage.Menu[] {
  const map = new Map<string, Api.SystemManage.Menu>();
  const roots: Api.SystemManage.Menu[] = [];

  // 第一遍：创建映射
  items.forEach(item => {
    map.set(item.id, { ...item, children: [] });
  });

  // 第二遍：建立父子关系
  items.forEach(item => {
    const node = map.get(item.id)!;
    if (item.parentId && map.has(item.parentId)) {
      const parent = map.get(item.parentId)!;
      if (!parent.children) {
        parent.children = [];
      }
      parent.children.push(node);
    } else {
      roots.push(node);
    }
  });

  return roots;
}

// 使用 useNaiveTable hook 来支持列设置功能
const { columns, columnChecks, data, getData, loading } = useNaiveTable({
  api: () => fetchGetMenuList(searchParams),
  transform: response => {
    if (!response.error && response.data) {
      return buildTree(response.data);
    }
    return [];
  },
  columns: () => [
    {
      type: 'selection' as const,
      width: 48
    },
    {
      key: 'name',
      title: '菜单名称',
      align: 'left' as const,
      minWidth: 150
    },
    {
      key: 'permissionName',
      title: '权限码',
      align: 'center' as const,
      width: 200
    },
    {
      key: 'menuType',
      title: '类型',
      align: 'center' as const,
      width: 80,
      render: (row: Api.SystemManage.Menu) => {
        const config = menuTypeMap[row.menuType];
        return <NTag type={config.type}>{config.label}</NTag>;
      }
    },
    {
      key: 'path',
      title: '路由路径',
      align: 'center' as const,
      width: 150,
      render: (row: Api.SystemManage.Menu) => row.path || '-'
    },
    {
      key: 'icon',
      title: '图标',
      align: 'center' as const,
      width: 150,
      render: (row: Api.SystemManage.Menu) => {
        if (!row.icon) return '-';

        // If icon contains ":", treat as Iconify icon (e.g., "mdi:account")
        // Otherwise, treat as localIcon (e.g., "logo")
        const iconConfig = row.icon.includes(':')
          ? { icon: row.icon, fontSize: 20 }
          : { localIcon: row.icon, fontSize: 20 };

        const IconVNode = SvgIconVNode(iconConfig);

        return (
          <NSpace align="center" justify="center" size={8}>
            {IconVNode && <IconVNode />}
            <span class="text-12px text-#999">{row.icon}</span>
          </NSpace>
        );
      }
    },
    {
      key: 'sortOrder',
      title: '排序',
      align: 'center' as const,
      width: 80
    },
    {
      key: 'isHidden',
      title: '可见',
      align: 'center' as const,
      width: 80,
      render: (row: Api.SystemManage.Menu) => {
        // isHidden: true 表示隐藏，false 表示可见
        return <NTag type={!row.isHidden ? 'success' : 'default'}>{!row.isHidden ? '是' : '否'}</NTag>;
      }
    },
    {
      key: 'isEnabled',
      title: '启用',
      align: 'center' as const,
      width: 80,
      render: (row: Api.SystemManage.Menu) => {
        return <NTag type={row.isEnabled ? 'success' : 'error'}>{row.isEnabled ? '是' : '否'}</NTag>;
      }
    },
    {
      key: 'operate',
      title: $t('common.operate'),
      align: 'center' as const,
      width: 200,
      fixed: 'right' as const,
      render: (row: Api.SystemManage.Menu) => (
        <NSpace justify="center" size={8}>
          <NButton type="primary" ghost size="tiny" onClick={() => handleEdit(row.id)}>
            {$t('common.edit')}
          </NButton>
          <NButton type="info" ghost size="tiny" onClick={() => handleAddChild(row.id)}>
            新增子项
          </NButton>
          <NPopconfirm onPositiveClick={() => handleDelete(row.id)}>
            {{
              default: () => $t('common.confirmDelete'),
              trigger: () => (
                <NButton type="error" ghost size="tiny">
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

// 抽屉相关
const { bool: drawerVisible, setTrue: openDrawer } = useBoolean();
const operateType = ref<NaiveUI.TableOperateType>('add');
const editingData = ref<Api.SystemManage.Menu | null>(null);

function handleAdd() {
  operateType.value = 'add';
  editingData.value = null;
  openDrawer();
}

function handleAddChild(parentId: string) {
  operateType.value = 'add';
  // 创建一个临时的编辑数据，设置 parentId
  editingData.value = { parentId } as Api.SystemManage.Menu;
  openDrawer();
}

function handleEdit(id: string) {
  operateType.value = 'edit';
  // 从树形结构中找到对应的节点
  function findNode(nodes: Api.SystemManage.Menu[], targetId: string): Api.SystemManage.Menu | null {
    for (const node of nodes) {
      if (node.id === targetId) {
        return node;
      }
      if (node.children && node.children.length > 0) {
        const found = findNode(node.children, targetId);
        if (found) return found;
      }
    }
    return null;
  }
  editingData.value = findNode(data.value, id);
  openDrawer();
}

async function handleDelete(id: string) {
  const { error } = await fetchDeleteMenu(id);
  if (!error) {
    window.$message?.success($t('common.deleteSuccess'));
    await getData();
  }
}

// 批量删除
async function handleBatchDelete() {
  if (checkedRowKeys.value.length === 0) return;

  window.$dialog?.warning({
    title: '确认删除',
    content: `确认删除选中的 ${checkedRowKeys.value.length} 个菜单吗？`,
    positiveText: '确认',
    negativeText: '取消',
    onPositiveClick: async () => {
      // 同时发起请求并等待全部完成
      const deletePromises = checkedRowKeys.value.map(id => fetchDeleteMenu(id));
      const results = await Promise.allSettled(deletePromises);

      // 检查是否有失败的请求
      const failedCount = results.filter(r => r.status === 'rejected').length;

      if (failedCount > 0) {
        window.$message?.warning(`成功删除 ${results.length - failedCount} 个菜单，失败 ${failedCount} 个`);
      } else {
        window.$message?.success('批量删除成功');
      }

      // 清空选中项并刷新列表
      checkedRowKeys.value = [];
      await getData();
    }
  });
}

// 同步权限功能
const syncing = ref(false);

async function handleSyncPermissions() {
  window.$dialog?.warning({
    title: '同步权限',
    content: '确认将所有菜单同步为系统权限吗？此操作会创建或更新权限定义。',
    positiveText: '确认',
    negativeText: '取消',
    onPositiveClick: async () => {
      syncing.value = true;
      try {
        // 收集所有菜单ID
        const menuIds: string[] = [];
        function collectIds(nodes: Api.SystemManage.Menu[]) {
          nodes.forEach(node => {
            menuIds.push(node.id);
            if (node.children && node.children.length > 0) {
              collectIds(node.children);
            }
          });
        }
        collectIds(data.value);

        const { error } = await fetchSyncPermissions({ menuIds });
        if (!error) {
          window.$message?.success('权限同步成功');
        }
      } finally {
        syncing.value = false;
      }
    }
  });
}
</script>

<template>
  <div class="min-h-500px flex-col-stretch gap-16px overflow-hidden lt-sm:overflow-auto">
    <MenuSearch v-model:model="searchParams" @search="getData" />
    <NCard title="权限管理" :bordered="false" size="small" class="card-wrapper sm:flex-1-hidden">
      <template #header-extra>
        <NSpace>
          <TableHeaderOperation
            v-model:columns="columnChecks"
            :disabled-delete="checkedRowKeys.length === 0"
            :loading="loading"
            @add="handleAdd"
            @delete="handleBatchDelete"
            @refresh="getData"
          >
            <template #prefix>
              <NButton type="success" size="small" ghost :loading="syncing" @click="handleSyncPermissions">
                <template #icon>
                  <icon-ic-round-sync class="text-icon" />
                </template>
                同步权限
              </NButton>
            </template>
          </TableHeaderOperation>
        </NSpace>
      </template>
      <NDataTable
        v-model:checked-row-keys="checkedRowKeys"
        :columns="columns"
        :data="data"
        size="small"
        :flex-height="!appStore.isMobile"
        :scroll-x="1100"
        :loading="loading"
        remote
        :row-key="row => row.id"
        :default-expand-all="true"
        class="sm:h-full"
      />
      <MenuOperateDrawer
        v-model:visible="drawerVisible"
        :operate-type="operateType"
        :row-data="editingData"
        @submitted="getData"
      />
    </NCard>
  </div>
</template>

<style scoped></style>
