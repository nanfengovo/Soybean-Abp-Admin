<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import { fetchAddUser, fetchGetAssignableRoles, fetchGetUserRoles, fetchUpdateUser } from '@/service/api';
import { useFormRules, useNaiveForm } from '@/hooks/common/form';
import { $t } from '@/locales';

defineOptions({
  name: 'UserOperateDrawer'
});

interface Props {
  operateType: NaiveUI.TableOperateType;
  rowData?: Api.SystemManage.User | null;
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
    add: $t('page.manage.user.addUser'),
    edit: $t('page.manage.user.editUser')
  };
  return titles[props.operateType];
});

type Model = Pick<Api.SystemManage.UserEdit, 'userName' | 'name' | 'email' | 'isActive' | 'roleNames'> & {
  password?: string;
  confirmPassword?: string;
};

const model: Model = reactive(createDefaultModel());

function createDefaultModel(): Model {
  return {
    userName: '',
    name: '',
    email: '',
    isActive: true,
    roleNames: [],
    password: '',
    confirmPassword: ''
  };
}

type RuleKey = Extract<keyof Model, 'userName' | 'name' | 'email' | 'password' | 'confirmPassword'>;

const rules = computed<Record<RuleKey, App.Global.FormRule | App.Global.FormRule[]>>(() => {
  const { formRules, defaultRequiredRule, createConfirmPwdRule } = useFormRules();

  const baseRules: Record<string, App.Global.FormRule | App.Global.FormRule[]> = {
    userName: defaultRequiredRule,
    name: defaultRequiredRule,
    email: formRules.email
  };

  // 添加用户时密码必填
  if (props.operateType === 'add') {
    baseRules.password = formRules.pwd;
    baseRules.confirmPassword = createConfirmPwdRule(model.password || '');
  }

  return baseRules as Record<RuleKey, App.Global.FormRule | App.Global.FormRule[]>;
});

// 可分配的角色列表
const assignableRoles = ref<Api.SystemManage.Role[]>([]);
const loadingRoles = ref(false);

// 获取可分配的角色列表
async function fetchAssignableRoles() {
  loadingRoles.value = true;
  try {
    const { data, error } = await fetchGetAssignableRoles();
    if (!error && data) {
      assignableRoles.value = data.items || [];
    }
  } finally {
    loadingRoles.value = false;
  }
}

// 获取用户的角色
async function fetchUserRoles(userId: string) {
  loadingRoles.value = true;
  try {
    const { data, error } = await fetchGetUserRoles(userId);
    if (!error && data) {
      // 从后端获取的角色列表中提取角色名称
      model.roleNames = (data.items || []).map(role => role.name);
    }
  } finally {
    loadingRoles.value = false;
  }
}

// 角色选项
const roleOptions = computed(() => {
  return assignableRoles.value.map(role => ({
    label: role.name,
    value: role.name
  }));
});

async function handleUpdateModelWhenEdit() {
  if (props.operateType === 'add') {
    Object.assign(model, createDefaultModel());
    return;
  }

  if (props.operateType === 'edit' && props.rowData) {
    Object.assign(model, {
      userName: props.rowData.userName,
      name: props.rowData.name,
      email: props.rowData.email,
      isActive: props.rowData.isActive,
      roleNames: [] // 先清空，等待从后端加载
    });

    // 根据用户ID获取实际拥有的角色
    await fetchUserRoles(props.rowData.id);
  }
}

function closeDrawer() {
  visible.value = false;
}

async function handleSubmit() {
  await validate();

  // 构建提交数据
  const submitData: Api.SystemManage.UserEdit = {
    userName: model.userName,
    name: model.name,
    email: model.email,
    isActive: model.isActive,
    roleNames: model.roleNames || []
  };

  // 添加用户时需要密码
  if (props.operateType === 'add') {
    submitData.password = model.password;
  }

  // 更新用户时需要 concurrencyStamp
  if (props.operateType === 'edit' && props.rowData) {
    submitData.concurrencyStamp = props.rowData.concurrencyStamp;
  }

  let error: any = null;

  // 调用对应的 API
  if (props.operateType === 'add') {
    const result = await fetchAddUser(submitData);
    error = result.error;
  } else if (props.operateType === 'edit' && props.rowData) {
    const result = await fetchUpdateUser(props.rowData.id, submitData);
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
    fetchAssignableRoles();
  }
});
</script>

<template>
  <NDrawer v-model:show="visible" display-directive="show" :width="420">
    <NDrawerContent :title="title" :native-scrollbar="false" closable>
      <NForm ref="formRef" :model="model" :rules="rules" label-placement="left" :label-width="100">
        <NFormItem :label="$t('page.manage.user.userName')" path="userName">
          <NInput v-model:value="model.userName" :placeholder="$t('page.manage.user.form.userName')" />
        </NFormItem>
        <NFormItem label="姓名" path="name">
          <NInput v-model:value="model.name" placeholder="请输入姓名" />
        </NFormItem>
        <NFormItem :label="$t('page.manage.user.userEmail')" path="email">
          <NInput v-model:value="model.email" :placeholder="$t('page.manage.user.form.userEmail')" />
        </NFormItem>
        <NFormItem label="角色" path="roleNames">
          <NSelect
            v-model:value="model.roleNames"
            multiple
            :options="roleOptions"
            :loading="loadingRoles"
            placeholder="请选择角色"
            clearable
          />
        </NFormItem>
        <NFormItem v-if="operateType === 'add'" label="密码" path="password">
          <NInput v-model:value="model.password" type="password" show-password-on="click" placeholder="请输入密码" />
        </NFormItem>
        <NFormItem v-if="operateType === 'add'" label="确认密码" path="confirmPassword">
          <NInput
            v-model:value="model.confirmPassword"
            type="password"
            show-password-on="click"
            placeholder="请再次输入密码"
          />
        </NFormItem>
        <NFormItem :label="$t('page.manage.user.userStatus')" path="isActive">
          <NSwitch v-model:value="model.isActive">
            <template #checked>启用</template>
            <template #unchecked>禁用</template>
          </NSwitch>
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
