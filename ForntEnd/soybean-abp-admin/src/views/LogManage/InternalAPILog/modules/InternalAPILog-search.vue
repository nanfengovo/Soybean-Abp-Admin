<script setup lang="ts">
import { useNaiveForm } from '@/hooks/common/form';
import { $t } from '@/locales';

defineOptions({
  name: 'InternalAPILogSearch'
});

interface Emits {
  (e: 'search'): void;
}

const emit = defineEmits<Emits>();

const { formRef, validate, restoreValidation } = useNaiveForm();

const model = defineModel<Api.SystemManage.InternalAPILogSearchParams>('model', { required: true });

const httpMethodOptions = [
  { label: 'GET', value: 'GET' },
  { label: 'POST', value: 'POST' },
  { label: 'PUT', value: 'PUT' },
  { label: 'DELETE', value: 'DELETE' },
  { label: 'PATCH', value: 'PATCH' }
];

const hasExceptionOptions = [
  { label: '全部', value: undefined },
  { label: '有异常', value: true },
  { label: '无异常', value: false }
];

async function reset() {
  await restoreValidation();
  model.value.Filter = '';
  model.value.StartTime = undefined;
  model.value.EndTime = undefined;
  model.value.HttpMethod = undefined;
  model.value.UserId = undefined;
  model.value.MinExecutionDuration = undefined;
  model.value.MaxExecutionDuration = undefined;
  model.value.HasException = undefined;
  model.value.HttpStatusCode = undefined;
  model.value.Sorting = '';
}

async function search() {
  await validate();
  emit('search');
}
</script>

<template>
  <NCard :bordered="false" size="small" class="card-wrapper">
    <NCollapse>
      <NCollapseItem :title="$t('common.search')" name="internal-api-log-search">
        <NForm ref="formRef" :model="model" label-placement="left" :label-width="120">
          <NGrid responsive="screen" item-responsive>
            <NFormItemGi span="24 s:12 m:6" label="模糊查询" path="Filter" class="pr-24px">
              <NInput v-model:value="model.Filter" placeholder="请输入URL/用户名/IP" />
            </NFormItemGi>
            <NFormItemGi span="24 s:12 m:6" label="HTTP方法" path="HttpMethod" class="pr-24px">
              <NSelect
                v-model:value="model.HttpMethod"
                :options="httpMethodOptions"
                placeholder="请选择HTTP方法"
                clearable
              />
            </NFormItemGi>
            <NFormItemGi span="24 s:12 m:6" label="HTTP状态码" path="HttpStatusCode" class="pr-24px">
              <NInputNumber
                v-model:value="model.HttpStatusCode"
                placeholder="请输入状态码"
                :show-button="false"
                clearable
                class="w-full"
              />
            </NFormItemGi>
            <NFormItemGi span="24 s:12 m:6" label="是否有异常" path="HasException" class="pr-24px">
              <NSelect
                v-model:value="model.HasException"
                :options="hasExceptionOptions"
                placeholder="请选择"
                clearable
              />
            </NFormItemGi>
            <NFormItemGi span="24 s:12 m:6" label="开始时间" path="StartTime" class="pr-24px">
              <NDatePicker
                v-model:formatted-value="model.StartTime"
                type="datetime"
                placeholder="请选择开始时间"
                value-format="yyyy-MM-dd'T'HH:mm:ss"
                clearable
                class="w-full"
              />
            </NFormItemGi>
            <NFormItemGi span="24 s:12 m:6" label="结束时间" path="EndTime" class="pr-24px">
              <NDatePicker
                v-model:formatted-value="model.EndTime"
                type="datetime"
                placeholder="请选择结束时间"
                value-format="yyyy-MM-dd'T'HH:mm:ss"
                clearable
                class="w-full"
              />
            </NFormItemGi>
            <NFormItemGi span="24 s:12 m:6" label="最小执行时间(ms)" path="MinExecutionDuration" class="pr-24px">
              <NInputNumber
                v-model:value="model.MinExecutionDuration"
                placeholder="请输入最小执行时间"
                :min="0"
                :show-button="false"
                clearable
                class="w-full"
              />
            </NFormItemGi>
            <NFormItemGi span="24 s:12 m:6" label="最大执行时间(ms)" path="MaxExecutionDuration" class="pr-24px">
              <NInputNumber
                v-model:value="model.MaxExecutionDuration"
                placeholder="请输入最大执行时间"
                :min="0"
                :show-button="false"
                clearable
                class="w-full"
              />
            </NFormItemGi>
            <NFormItemGi span="24 s:12 m:6" label="用户ID" path="UserId" class="pr-24px">
              <NInput v-model:value="model.UserId" placeholder="请输入用户ID" clearable />
            </NFormItemGi>
            <NFormItemGi span="24 s:12 m:6" label="排序字段" path="Sorting" class="pr-24px">
              <NInput v-model:value="model.Sorting" placeholder="例如: executionTime desc" />
            </NFormItemGi>
            <NFormItemGi span="24 m:12" class="pr-24px">
              <NSpace class="w-full" justify="end">
                <NButton @click="reset">
                  <template #icon>
                    <icon-ic-round-refresh class="text-icon" />
                  </template>
                  {{ $t('common.reset') }}
                </NButton>
                <NButton type="primary" ghost @click="search">
                  <template #icon>
                    <icon-ic-round-search class="text-icon" />
                  </template>
                  {{ $t('common.search') }}
                </NButton>
              </NSpace>
            </NFormItemGi>
          </NGrid>
        </NForm>
      </NCollapseItem>
    </NCollapse>
  </NCard>
</template>

<style scoped></style>
