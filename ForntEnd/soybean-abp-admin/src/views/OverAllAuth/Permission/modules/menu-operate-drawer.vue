<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import type { TreeSelectOption } from 'naive-ui';
import { fetchAddMenu, fetchGetMenuList, fetchUpdateMenu } from '@/service/api';
import { useFormRules, useNaiveForm } from '@/hooks/common/form';
import { $t } from '@/locales';

defineOptions({
  name: 'MenuOperateDrawer'
});

interface Props {
  operateType: NaiveUI.TableOperateType;
  rowData?: Api.SystemManage.Menu | null;
}

const props = defineProps<Props>();

interface Emits {
  (e: 'submitted'): void;
}

const emit = defineEmits<Emits>();

const visible = defineModel<boolean>('visible', {
  default: false
});

const { formRef, validate, restoreValidation } = useNaiveForm();

const title = computed(() => {
  const titles: Record<NaiveUI.TableOperateType, string> = {
    add: '新增菜单',
    edit: '编辑菜单'
  };
  return titles[props.operateType];
});

type Model = Pick<
  Api.SystemManage.MenuEdit,
  | 'parentId'
  | 'name'
  | 'permissionName'
  | 'menuType'
  | 'path'
  | 'component'
  | 'icon'
  | 'sortOrder'
  | 'isHidden'
  | 'isEnabled'
  | 'isExternal'
  | 'externalUrl'
  | 'remark'
>;

const model: Model = reactive(createDefaultModel());

function createDefaultModel(): Model {
  return {
    parentId: null,
    name: '',
    permissionName: '',
    menuType: 0,
    path: null,
    component: null,
    icon: null,
    sortOrder: 0,
    isHidden: false,
    isEnabled: true,
    isExternal: false,
    externalUrl: null,
    remark: null
  };
}

type RuleKey = Extract<keyof Model, 'name' | 'permissionName'>;

const rules = computed<Record<RuleKey, App.Global.FormRule>>(() => {
  const { defaultRequiredRule } = useFormRules();

  return {
    name: defaultRequiredRule,
    permissionName: defaultRequiredRule
  };
});

// 菜单类型选项
const menuTypeOptions = [
  { label: '目录', value: 0 },
  { label: '菜单', value: 1 },
  { label: '按钮', value: 2 }
];

// 父菜单树形选项
const parentMenuOptions = ref<TreeSelectOption[]>([]);

// 加载父菜单列表
async function loadParentMenus() {
  const { data: menuData, error } = await fetchGetMenuList();
  if (!error && menuData) {
    parentMenuOptions.value = buildTreeOptions(menuData);
  }
}

// 构建树形选项
function buildTreeOptions(menus: Api.SystemManage.Menu[]): TreeSelectOption[] {
  const options: TreeSelectOption[] = [
    {
      label: '根目录',
      value: null,
      key: 'root'
    }
  ];

  function buildChildren(items: Api.SystemManage.Menu[]): TreeSelectOption[] {
    return items.map(item => {
      const option: TreeSelectOption = {
        label: item.name,
        value: item.id,
        key: item.id
      };

      // 只有目录和菜单可以作为父节点
      if ((item.menuType === 0 || item.menuType === 1) && item.children && item.children.length > 0) {
        option.children = buildChildren(item.children);
      }

      return option;
    });
  }

  options.push(...buildChildren(menus));
  return options;
}

function handleUpdateModelWhenEdit() {
  if (props.operateType === 'add') {
    Object.assign(model, createDefaultModel());
    return;
  }

  if (props.operateType === 'edit' && props.rowData) {
    Object.assign(model, {
      parentId: props.rowData.parentId,
      name: props.rowData.name,
      permissionName: props.rowData.permissionName,
      menuType: props.rowData.menuType,
      path: props.rowData.path,
      component: props.rowData.component,
      icon: props.rowData.icon,
      sortOrder: props.rowData.sortOrder,
      isHidden: props.rowData.isHidden,
      isEnabled: props.rowData.isEnabled,
      isExternal: props.rowData.isExternal,
      externalUrl: props.rowData.externalUrl,
      remark: props.rowData.remark
    });
  }
}

function closeDrawer() {
  visible.value = false;
}

async function handleSubmit() {
  await validate();

  // 构建提交数据
  const submitData: Api.SystemManage.MenuEdit = {
    parentId: model.parentId,
    name: model.name,
    permissionName: model.permissionName,
    menuType: model.menuType,
    path: model.path,
    component: model.component,
    icon: model.icon,
    sortOrder: model.sortOrder,
    isHidden: model.isHidden,
    isEnabled: model.isEnabled,
    isExternal: model.isExternal,
    externalUrl: model.externalUrl,
    remark: model.remark
  };

  let error: any = null;

  // 调用对应的 API
  if (props.operateType === 'add') {
    const result = await fetchAddMenu(submitData);
    error = result.error;
  } else if (props.operateType === 'edit' && props.rowData) {
    const result = await fetchUpdateMenu(props.rowData.id, submitData);
    error = result.error;
  }

  if (!error) {
    window.$message?.success($t('common.updateSuccess'));
    closeDrawer();
    emit('submitted');
  }
}

watch(visible, () => {
  if (visible.value) {
    handleUpdateModelWhenEdit();
    restoreValidation();
    loadParentMenus();
  }
});
</script>

<template>
  <NDrawer v-model:show="visible" display-directive="show" :width="480">
    <NDrawerContent :title="title" :native-scrollbar="false" closable>
      <NForm ref="formRef" :model="model" :rules="rules" label-placement="left" :label-width="100">
        <NFormItem label="父菜单" path="parentId">
          <NTreeSelect
            v-model:value="model.parentId"
            :options="parentMenuOptions"
            placeholder="请选择父菜单"
            clearable
            default-expand-all
          />
        </NFormItem>
        <NFormItem label="菜单类型" path="menuType">
          <NRadioGroup v-model:value="model.menuType">
            <NSpace>
              <NRadio v-for="item in menuTypeOptions" :key="item.value" :value="item.value">
                {{ item.label }}
              </NRadio>
            </NSpace>
          </NRadioGroup>
        </NFormItem>
        <NFormItem label="菜单名称" path="name">
          <NInput v-model:value="model.name" placeholder="请输入菜单名称" />
        </NFormItem>
        <NFormItem label="权限码" path="permissionName">
          <NInput v-model:value="model.permissionName" placeholder="例如: menu-overallauth-users" />
        </NFormItem>
        <NFormItem v-if="model.menuType !== 2" label="路由路径" path="path">
          <NInput v-model:value="model.path" placeholder="例如: /system/user" />
        </NFormItem>
        <NFormItem v-if="model.menuType === 1" label="组件路径" path="component">
          <NInput v-model:value="model.component" placeholder="例如: User" />
        </NFormItem>
        <NFormItem label="图标" path="icon">
          <NInput v-model:value="model.icon" placeholder="请输入图标名称" />
        </NFormItem>
        <NFormItem label="排序" path="sortOrder">
          <NInputNumber v-model:value="model.sortOrder" :min="0" placeholder="数字越小越靠前" class="w-full" />
        </NFormItem>
        <NFormItem label="是否隐藏" path="isHidden">
          <NSwitch v-model:value="model.isHidden">
            <template #checked>隐藏</template>
            <template #unchecked>显示</template>
          </NSwitch>
        </NFormItem>
        <NFormItem label="是否启用" path="isEnabled">
          <NSwitch v-model:value="model.isEnabled">
            <template #checked>启用</template>
            <template #unchecked>禁用</template>
          </NSwitch>
        </NFormItem>
        <NFormItem label="外部链接" path="isExternal">
          <NSwitch v-model:value="model.isExternal">
            <template #checked>是</template>
            <template #unchecked>否</template>
          </NSwitch>
        </NFormItem>
        <NFormItem v-if="model.isExternal" label="外部URL" path="externalUrl">
          <NInput v-model:value="model.externalUrl" placeholder="请输入外部链接URL" />
        </NFormItem>
        <NFormItem label="备注" path="remark">
          <NInput
            v-model:value="model.remark"
            type="textarea"
            placeholder="请输入备注"
            :autosize="{ minRows: 3, maxRows: 5 }"
          />
        </NFormItem>
      </NForm>
      <template #footer>
        <NSpace :size="16">
          <NButton @click="closeDrawer">{{ $t('common.cancel') }}</NButton>
          <NButton type="primary" @click="handleSubmit">{{ $t('common.confirm') }}</NButton>
        </NSpace>
      </template>
    </NDrawerContent>
  </NDrawer>
</template>

<style scoped></style>
