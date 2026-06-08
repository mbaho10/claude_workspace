<template>
  <div class="relative group min-h-[28px]">
    <template v-if="editable">
      <input
        v-if="editing || type === 'number'"
        ref="inputRef"
        :type="type === 'number' ? 'number' : 'text'"
        :value="localValue"
        @input="localValue = ($event.target as HTMLInputElement).value"
        @blur="commit"
        @keydown.enter.prevent="commit"
        @keydown.escape="cancel"
        class="w-full text-sm border-0 bg-white border border-primary-300 rounded px-1.5 py-0.5 focus:outline-none focus:ring-1 focus:ring-primary-400"
        :class="type === 'number' ? 'text-right' : 'text-left'"
        v-focus
      />
      <div v-else @click="startEdit" @dblclick="startEdit"
        class="px-1.5 py-0.5 rounded cursor-text hover:bg-gray-100 text-sm min-h-[28px] flex items-center"
        :class="type === 'number' ? 'justify-end' : 'justify-start'">
        {{ value || '—' }}
      </div>
    </template>
    <template v-else>
      <div class="px-1.5 py-0.5 text-sm text-gray-600 min-h-[28px] flex items-center"
        :class="type === 'number' ? 'justify-end' : 'justify-start'">
        {{ value || '' }}
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

const props = defineProps<{
  value: string
  editable?: boolean
  type?: 'text' | 'number'
}>()

const emit = defineEmits<{
  update: [value: string]
}>()

const editing = ref(false)
const localValue = ref(props.value)
const inputRef = ref<HTMLInputElement | null>(null)

watch(() => props.value, v => { localValue.value = v })

const vFocus = { mounted: (el: HTMLElement) => el.focus() }

function startEdit() {
  localValue.value = props.value
  editing.value = true
}

function commit() {
  editing.value = false
  if (localValue.value !== props.value) emit('update', localValue.value)
}

function cancel() {
  localValue.value = props.value
  editing.value = false
}
</script>
