<template>
  <header class="h-14 bg-white border-b border-gray-200 flex items-center justify-between px-6 shrink-0">
    <h2 class="text-base font-semibold text-gray-800">{{ pageTitle }}</h2>
    <div class="flex items-center gap-3">
      <span v-if="budget.saving" class="text-xs text-amber-600 flex items-center gap-1.5">
        <span class="w-1.5 h-1.5 rounded-full bg-amber-400 animate-pulse"></span>
        กำลังบันทึก...
      </span>
      <span class="text-sm text-gray-500">{{ currentDate }}</span>
    </div>
  </header>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useBudgetStore } from '@/stores/budget'
import { format } from 'date-fns'

const route = useRoute()
const budget = useBudgetStore()

const pageTitle = computed(() => {
  const map: Record<string, string> = {
    '/dashboard': 'ภาพรวม',
    '/budget': 'Budget Planning',
    '/action-plan': 'Action Plan',
    '/admin': 'ตั้งค่าระบบ'
  }
  return map[route.path] ?? ''
})

const currentDate = computed(() =>
  format(new Date(), 'dd/MM/yyyy')
)
</script>
