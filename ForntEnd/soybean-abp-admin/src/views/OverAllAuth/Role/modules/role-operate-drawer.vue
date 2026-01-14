<script setup lang="ts">
import { computed, reactive, watch } from 'vue';
import { fetchAddRole, fetchUpdateRole } from '@/service/api';
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

function handleUpdateModelWhenEdit() {
  if (props.operateType === 'add') {
    Object.assign(model, createDefaultModel());
    return;
  }

  if (props.operateType === 'edit' && props.rowData) {
    Object.assign(model, {
      name: props.rowData.name,
      isDefault: props.rowData.isDefault,
      isPublic: props.rowData.isPublic
    });
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

  // 调用对应的 API
  if (props.operateType === 'add') {
    const result = await fetchAddRole(submitData);
    error = result.error;
  } else if (props.operateType === 'edit' && props.rowData) {
    const result = await fetchUpdateRole(props.rowData.id, submitData);
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
  }
});
</script>

<template>
  <NDrawer v-model:show="visible" display-directive="show" :width="360">
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
