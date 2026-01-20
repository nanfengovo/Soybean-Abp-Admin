<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import { fetchAddRole, fetchGetPermissions, fetchUpdatePermissions, fetchUpdateRole } from '@/service/api';
import { useFormRules, useNaiveForm } from '@/hooks/common/form';
import { $t } from '@/locales';

defineOptions({
  name: 'RoleOperateDrawer'
});

interface Props {
  operateType: NaiveUI.TableOperateType;
  rowData?: Api.SystemManage.Role | null;
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
    add: '新增角色',
    edit: '编辑角色'
  };
  return titles[props.operateType];
});

type Model = Pick<Api.SystemManage.RoleEdit, 'name' | 'isDefault' | 'isPublic'>;

const model: Model = reactive(createDefaultModel());

function createDefaultModel(): Model {
  return {
    name: '',
    isDefault: false,
    isPublic: false
  };
}

type RuleKey = Extract<keyof Model, 'name'>;

const rules = computed<Record<RuleKey, App.Global.FormRule>>(() => {
  const { defaultRequiredRule } = useFormRules();

  return {
    name: defaultRequiredRule
  };
});

// 权限相关
const loadingPermissions = ref(false);
const permissionGroups = ref<Api.SystemManage.PermissionGroup[]>([]);
const checkedPermissions = ref<string[]>([]);

// 将权限组转换为树形结构
const permissionTreeData = computed(() => {
  return permissionGroups.value.map(group => ({
    key: group.name,
    label: group.displayName,
    children: group.permissions.map(p => ({
      key: p.name,
      label: p.displayName
    }))
  }));
});

// 保存原始权限用于比较
const originalPermissions = ref<string[]>([]);

// 获取角色权限
async function fetchRolePermissions(roleName: string) {
  loadingPermissions.value = true;
  try {
    const { data, error } = await fetchGetPermissions('R', roleName);
    if (!error && data) {
      permissionGroups.value = data.groups;
      // 提取已授权的权限
      const granted: string[] = [];
      data.groups.forEach(group => {
        group.permissions.forEach(p => {
          if (p.isGranted) {
            granted.push(p.name);
          }
        });
      });
      checkedPermissions.value = granted;
      originalPermissions.value = [...granted];
    }
  } finally {
    loadingPermissions.value = false;
  }
}

// 获取所有可用权限（用于新增角色）
async function fetchAllPermissions() {
  loadingPermissions.value = true;
  try {
    // 使用空的 providerKey 获取所有权限定义
    const { data, error } = await fetchGetPermissions('R', '');
    if (!error && data) {
      permissionGroups.value = data.groups;
      checkedPermissions.value = [];
    }
  } finally {
    loadingPermissions.value = false;
  }
}

function handleUpdateModelWhenEdit() {
  if (props.operateType === 'add') {
    Object.assign(model, createDefaultModel());
    checkedPermissions.value = [];
    fetchAllPermissions();
    return;
  }

  if (props.operateType === 'edit' && props.rowData) {
    Object.assign(model, {
      name: props.rowData.name,
      isDefault: props.rowData.isDefault,
      isPublic: props.rowData.isPublic
    });
    fetchRolePermissions(props.rowData.name);
  }
}

function closeDrawer() {
  visible.value = false;
}

async function handleSubmit() {
  await validate();

  // 构建提交数据
  const submitData: Api.SystemManage.RoleEdit = {
    name: model.name,
    isDefault: model.isDefault,
    isPublic: model.isPublic
  };

  // 更新角色时需要 concurrencyStamp
  if (props.operateType === 'edit' && props.rowData) {
    submitData.concurrencyStamp = props.rowData.concurrencyStamp;
  }

  let error: any = null;
  let roleName = model.name;

  // 调用对应的 API
  if (props.operateType === 'add') {
    const result = await fetchAddRole(submitData);
    error = result.error;
    if (!error && result.data) {
      roleName = result.data.name;
    }
  } else if (props.operateType === 'edit' && props.rowData) {
    const result = await fetchUpdateRole(props.rowData.id, submitData);
    error = result.error;
  }

  if (error) {
    return;
  }

  // 保存权限 - 构建所有权限的更新数据
  const allPermissions: Array<{ name: string; isGranted: boolean }> = [];
  permissionGroups.value.forEach(group => {
    group.permissions.forEach(p => {
      allPermissions.push({
        name: p.name,
        isGranted: checkedPermissions.value.includes(p.name)
      });
    });
  });

  // 调用更新权限接口
  const permResult = await fetchUpdatePermissions('R', roleName, {
    permissions: allPermissions
  });

  if (permResult.error) {
    window.$message?.error('权限保存失败');
    return;
  }

  window.$message?.success($t('common.updateSuccess'));
  closeDrawer();
  emit('submitted');
}

watch(visible, () => {
  if (visible.value) {
    handleUpdateModelWhenEdit();
    restoreValidation();
  }
});
</script>

<template>
  <NDrawer v-model:show="visible" display-directive="show" :width="500">
    <NDrawerContent :title="title" :native-scrollbar="false" closable>
      <NForm ref="formRef" :model="model" :rules="rules" label-placement="left" :label-width="100">
        <NFormItem label="角色名称" path="name">
          <NInput v-model:value="model.name" placeholder="请输入角色名称" />
        </NFormItem>
        <NFormItem label="默认角色" path="isDefault">
          <NSwitch v-model:value="model.isDefault">
            <template #checked>是</template>
            <template #unchecked>否</template>
          </NSwitch>
          <span class="ml-8px text-12px text-#999">新用户注册时自动分配此角色</span>
        </NFormItem>
        <NFormItem label="公开角色" path="isPublic">
          <NSwitch v-model:value="model.isPublic">
            <template #checked>是</template>
            <template #unchecked>否</template>
          </NSwitch>
          <span class="ml-8px text-12px text-#999">所有用户都可以查看此角色</span>
        </NFormItem>
        <NFormItem label="权限分配">
          <NSpin :show="loadingPermissions" class="w-full">
            <NTree
              v-model:checked-keys="checkedPermissions"
              :data="permissionTreeData"
              checkable
              cascade
              selectable
              block-line
              expand-on-click
              default-expand-all
              class="max-h-400px w-full overflow-auto border-1 border-gray-200 rounded-4px border-solid p-8px"
            />
          </NSpin>
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
