<template>
  <div class="space-y-6">
    <!-- KPI Cards -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div class="card p-5" v-for="card in kpiCards" :key="card.label">
        <div class="flex items-start justify-between">
          <div>
            <p class="text-sm text-gray-500 font-medium">{{ card.label }}</p>
            <p class="text-2xl font-bold text-gray-900 mt-1">{{ card.value }}</p>
          </div>
          <span class="text-3xl">{{ card.icon }}</span>
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <!-- Budget by Department -->
      <div class="card p-5">
        <h3 class="text-base font-semibold text-gray-800 mb-4">งบประมาณตามแผนก</h3>
        <div class="space-y-3">
          <div v-for="dept in deptSummary" :key="dept.name" class="flex items-center gap-3">
            <div class="w-28 text-sm text-gray-600 truncate">{{ dept.name }}</div>
            <div class="flex-1 bg-gray-100 rounded-full h-2.5 overflow-hidden">
              <div class="h-full bg-primary-500 rounded-full transition-all" :style="`width: ${dept.pct}%`"></div>
            </div>
            <div class="text-xs text-gray-500 w-28 text-right">{{ dept.total.toLocaleString() }} บาท</div>
          </div>
          <p v-if="!deptSummary.length" class="text-sm text-gray-400 text-center py-4">
            ยังไม่มีข้อมูล กรุณาเลือก Template และกรอกข้อมูลงบประมาณ
          </p>
        </div>
      </div>

      <!-- Action Plan Status -->
      <div class="card p-5">
        <h3 class="text-base font-semibold text-gray-800 mb-4">สถานะ Action Plan</h3>
        <div class="space-y-3">
          <div v-for="s in statusSummary" :key="s.label" class="flex items-center gap-3">
            <span class="text-xs px-2 py-1 rounded-full w-32 text-center" :class="s.class">{{ s.label }}</span>
            <div class="flex-1 bg-gray-100 rounded-full h-2.5 overflow-hidden">
              <div class="h-full rounded-full transition-all" :class="s.barClass" :style="`width: ${s.pct}%`"></div>
            </div>
            <span class="text-sm font-semibold text-gray-700 w-8 text-right">{{ s.count }}</span>
          </div>
          <p v-if="!totalItems" class="text-sm text-gray-400 text-center py-4">
            ยังไม่มีข้อมูล Action Plan
          </p>
        </div>
      </div>
    </div>

    <!-- Quick Links -->
    <div class="card p-5">
      <h3 class="text-base font-semibold text-gray-800 mb-4">ทางลัด</h3>
      <div class="grid grid-cols-2 sm:grid-cols-3 gap-3">
        <router-link to="/budget" class="flex items-center gap-3 p-4 rounded-xl border border-gray-200 hover:border-primary-300 hover:bg-primary-50 transition-colors">
          <span class="text-2xl">💰</span>
          <div>
            <p class="font-medium text-sm">Budget Planning</p>
            <p class="text-xs text-gray-500">กรอกงบประมาณ</p>
          </div>
        </router-link>
        <router-link to="/action-plan" class="flex items-center gap-3 p-4 rounded-xl border border-gray-200 hover:border-primary-300 hover:bg-primary-50 transition-colors">
          <span class="text-2xl">📋</span>
          <div>
            <p class="font-medium text-sm">Action Plan</p>
            <p class="text-xs text-gray-500">จัดการโครงการ</p>
          </div>
        </router-link>
        <router-link v-if="auth.isAdmin" to="/admin" class="flex items-center gap-3 p-4 rounded-xl border border-gray-200 hover:border-primary-300 hover:bg-primary-50 transition-colors">
          <span class="text-2xl">⚙️</span>
          <div>
            <p class="font-medium text-sm">ตั้งค่าระบบ</p>
            <p class="text-xs text-gray-500">จัดการผู้ใช้/แผนก</p>
          </div>
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useBudgetStore } from '@/stores/budget'
import { useActionPlanStore } from '@/stores/actionPlan'
import { useAuthStore } from '@/stores/auth'
import { budgetApi } from '@/api/budget'
import { actionPlanApi } from '@/api/actionPlan'
import type { ActionPlanStatus } from '@/types'

const budget = useBudgetStore()
const planStore = useActionPlanStore()
const auth = useAuthStore()

const deptSummary = ref<Array<{ name: string; total: number; pct: number }>>([])
const actionItems = ref<any[]>([])

const totalItems = computed(() => actionItems.value.length)

const kpiCards = computed(() => {
  const totalBudget = deptSummary.value.reduce((s, d) => s + d.total, 0)
  const completed = actionItems.value.filter(i => i.status === 'Completed').length
  const inProgress = actionItems.value.filter(i => i.status === 'InProgress').length
  return [
    { label: 'งบประมาณรวม', value: totalBudget.toLocaleString() + ' ฿', icon: '💰' },
    { label: 'จำนวนโครงการ', value: totalItems.value.toString(), icon: '📋' },
    { label: 'กำลังดำเนินการ', value: inProgress.toString(), icon: '🔄' },
    { label: 'เสร็จสิ้น', value: completed.toString(), icon: '✅' }
  ]
})

const statusSummary = computed(() => {
  const statuses: Array<{ key: ActionPlanStatus; label: string; class: string; barClass: string }> = [
    { key: 'NotStarted', label: 'ยังไม่เริ่ม', class: 'bg-gray-100 text-gray-700', barClass: 'bg-gray-400' },
    { key: 'InProgress', label: 'กำลังดำเนินการ', class: 'bg-blue-100 text-blue-700', barClass: 'bg-blue-500' },
    { key: 'Completed', label: 'เสร็จสิ้น', class: 'bg-green-100 text-green-700', barClass: 'bg-green-500' },
    { key: 'OnHold', label: 'หยุดชั่วคราว', class: 'bg-yellow-100 text-yellow-700', barClass: 'bg-yellow-400' },
    { key: 'Cancelled', label: 'ยกเลิก', class: 'bg-red-100 text-red-700', barClass: 'bg-red-400' }
  ]
  return statuses.map(s => ({
    ...s,
    count: actionItems.value.filter(i => i.status === s.key).length,
    pct: totalItems.value ? (actionItems.value.filter(i => i.status === s.key).length / totalItems.value) * 100 : 0
  }))
})

onMounted(async () => {
  try {
    const templates = await budgetApi.getTemplates(new Date().getFullYear()).then(r => r.data)
    if (templates.length) {
      const data = await budgetApi.getGridData(templates[0].id).then(r => r.data)
      const max = Math.max(...data.departments.map(d =>
        data.categories.filter(c => !c.isCalculated).reduce((s, cat) =>
          s + data.cells.filter(cell => cell.categoryId === cat.id && cell.departmentId === d.id)
            .reduce((cs, c) => cs + c.amount, 0), 0)
      ), 1)
      deptSummary.value = data.departments.map(d => {
        const total = data.categories.filter(c => !c.isCalculated)
          .reduce((s, cat) => s + data.cells
            .filter(cell => cell.categoryId === cat.id && cell.departmentId === d.id)
            .reduce((cs, c) => cs + c.amount, 0), 0)
        return { name: d.name, total, pct: Math.round((total / max) * 100) }
      })
    }

    await planStore.loadPlans()
    if (planStore.plans.length) {
      actionItems.value = await actionPlanApi.getItems(planStore.plans[0].id).then(r => r.data)
    }
  } catch { /* ignore */ }
})
</script>
