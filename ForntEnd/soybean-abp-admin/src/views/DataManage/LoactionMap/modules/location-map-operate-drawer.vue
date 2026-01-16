<script setup lang="ts">
import { computed, reactive, watch } from 'vue';
import { fetchAddLocationMap, fetchUpdateLocationMap } from '@/service/api';
import { useFormRules, useNaiveForm } from '@/hooks/common/form';
import { $t } from '@/locales';

defineOptions({
  name: 'LocationMapOperateDrawer'
});

interface Props {
  operateType: NaiveUI.TableOperateType;
  rowData?: Api.SystemManage.LocationMap | null;
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
    add: '新增机台点位映射',
    edit: '编辑机台点位映射'
  };
  return titles[props.operateType];
});

type Model = Api.SystemManage.LocationMapEdit;

const model: Model = reactive(createDefaultModel());

function createDefaultModel(): Model {
  return {
    name: '',
    machinePoint: '',
    agvPoint: '',
    description: ''
  };
}

type RuleKey = Extract<keyof Model, 'name' | 'machinePoint' | 'agvPoint'>;

const rules = computed<Record<RuleKey, App.Global.FormRule>>(() => {
  const { defaultRequiredRule } = useFormRules();

  return {
    name: defaultRequiredRule,
    machinePoint: defaultRequiredRule,
    agvPoint: defaultRequiredRule
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
      machinePoint: props.rowData.machinePoint,
      agvPoint: props.rowData.agvPoint,
      description: props.rowData.description || ''
    });
  }
}

function closeDrawer() {
  visible.value = false;
}

async function handleSubmit() {
  await validate();

  const submitData: Api.SystemManage.LocationMapEdit = {
    name: model.name,
    machinePoint: model.machinePoint,
    agvPoint: model.agvPoint,
    description: model.description
  };

  let error: any = null;

  if (props.operateType === 'add') {
    const result = await fetchAddLocationMap(submitData);
    error = result.error;
  } else if (props.operateType === 'edit' && props.rowData) {
    const result = await fetchUpdateLocationMap(props.rowData.id, submitData);
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
  <NDrawer v-model:show="visible" display-directive="show" :width="400">
    <NDrawerContent :title="title" :native-scrollbar="false" closable>
      <NForm ref="formRef" :model="model" :rules="rules" label-placement="left" :label-width="100">
        <NFormItem label="名称" path="name">
          <NInput v-model:value="model.name" placeholder="请输入名称" />
        </NFormItem>
        <NFormItem label="机台点位" path="machinePoint">
          <NInput v-model:value="model.machinePoint" placeholder="请输入机台点位" />
        </NFormItem>
        <NFormItem label="AGV点位" path="agvPoint">
          <NInput v-model:value="model.agvPoint" placeholder="请输入AGV点位" />
        </NFormItem>
        <NFormItem label="描述" path="description">
          <NInput
            v-model:value="model.description"
            type="textarea"
            placeholder="请输入描述"
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
