<template>
  <div class="flex flex-col h-full gap-4">
    <!-- Controls -->
    <div class="card p-4 flex flex-wrap items-center gap-3">
      <div class="flex items-center gap-2">
        <label class="text-sm font-medium text-gray-700">Action Plan:</label>
        <select v-model="selectedPlanId" @change="loadItems" class="input-field w-64">
          <option value="">-- เลือก Action Plan --</option>
          <option v-for="p in planStore.plans" :key="p.id" :value="p.id">{{ p.name }}</option>
        </select>
      </div>
      <div v-if="auth.isAdmin" class="flex items-center gap-2">
        <label class="text-sm font-medium text-gray-700">แผนก:</label>
        <select v-model="filterDeptId" class="input-field w-48">
          <option :value="null">ทุกแผนก</option>
          <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
        </select>
      </div>
      <div class="ml-auto flex gap-2">
        <button v-if="auth.isAdmin" @click="showCreatePlan = true" class="btn-primary text-xs">+ สร้าง Action Plan</button>
        <button v-if="selectedPlanId && canAddItem" @click="addNewRow" class="btn-secondary text-xs">+ เพิ่มรายการ</button>
      </div>
    </div>

    <!-- Table -->
    <div class="card flex-1 overflow-hidden">
      <div v-if="!selectedPlanId" class="h-full flex items-center justify-center text-gray-400">
        <div class="text-center">
          <div class="text-5xl mb-3">📋</div>
          <p>เลือก Action Plan เพื่อดูรายการ</p>
        </div>
      </div>
      <div v-else-if="planStore.loading" class="h-full flex items-center justify-center text-gray-500">
        กำลังโหลด...
      </div>
      <div v-else class="overflow-auto h-full">
        <table class="w-full text-sm border-collapse">
          <thead class="sticky top-0 bg-gray-50 z-10">
            <tr>
              <th class="px-3 py-3 text-left border-b border-gray-200 font-semibold text-gray-700 w-8">#</th>
              <th class="px-3 py-3 text-left border-b border-gray-200 font-semibold text-gray-700 min-w-48">โครงการ/กิจกรรม</th>
              <th class="px-3 py-3 text-left border-b border-gray-200 font-semibold text-gray-700 w-40">แผนก</th>
              <th class="px-3 py-3 text-left border-b border-gray-200 font-semibold text-gray-700 w-36">ผู้รับผิดชอบ</th>
              <th class="px-3 py-3 text-left border-b border-gray-200 font-semibold text-gray-700 w-28">วันเริ่ม</th>
              <th class="px-3 py-3 text-left border-b border-gray-200 font-semibold text-gray-700 w-28">วันสิ้นสุด</th>
              <th class="px-3 py-3 text-right border-b border-gray-200 font-semibold text-gray-700 w-32">งบประมาณ</th>
              <th class="px-3 py-3 text-right border-b border-gray-200 font-semibold text-gray-700 w-32">ใช้จ่าย</th>
              <th class="px-3 py-3 text-center border-b border-gray-200 font-semibold text-gray-700 w-28">สถานะ</th>
              <th class="px-3 py-3 text-left border-b border-gray-200 font-semibold text-gray-700">หมายเหตุ</th>
              <th class="px-3 py-3 border-b border-gray-200 w-16"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(item, idx) in filteredItems" :key="item.id"
              class="border-b border-gray-100 hover:bg-blue-50/30 transition-colors"
              :class="{ 'bg-gray-50/50': idx % 2 === 1 }">
              <td class="px-3 py-2 text-gray-400 text-xs">{{ idx + 1 }}</td>
              <td class="px-2 py-1">
                <EditableCell :value="item.initiativeName" :editable="canEditItem(item)"
                  @update="val => updateField(item, 'initiativeName', val)" />
              </td>
              <td class="px-2 py-1">
                <template v-if="canEditItem(item)">
                  <select :value="item.departmentId" @change="updateField(item, 'departmentId', parseInt(($event.target as HTMLSelectElement).value))"
                    class="w-full text-sm border-0 bg-transparent focus:bg-white focus:border focus:border-primary-300 rounded px-1 py-0.5">
                    <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
                  </select>
                </template>
                <template v-else>
                  <span class="text-gray-700">{{ item.departmentName }}</span>
                </template>
              </td>
              <td class="px-2 py-1">
                <EditableCell :value="item.responsiblePerson ?? ''" :editable="canEditItem(item)"
                  @update="val => updateField(item, 'responsiblePerson', val)" />
              </td>
              <td class="px-2 py-1">
                <input v-if="canEditItem(item)" type="date" :value="item.startDate ?? ''"
                  @change="updateField(item, 'startDate', ($event.target as HTMLInputElement).value)"
                  class="w-full text-xs border-0 bg-transparent focus:bg-white focus:border focus:border-primary-300 rounded px-1 py-0.5" />
                <span v-else class="text-xs text-gray-600">{{ item.startDate ?? '-' }}</span>
              </td>
              <td class="px-2 py-1">
                <input v-if="canEditItem(item)" type="date" :value="item.endDate ?? ''"
                  @change="updateField(item, 'endDate', ($event.target as HTMLInputElement).value)"
                  class="w-full text-xs border-0 bg-transparent focus:bg-white focus:border focus:border-primary-300 rounded px-1 py-0.5" />
                <span v-else class="text-xs text-gray-600">{{ item.endDate ?? '-' }}</span>
              </td>
              <td class="px-2 py-1 text-right">
                <EditableCell :value="item.budgetAllocated?.toLocaleString() ?? ''" :editable="canEditItem(item)"
                  type="number" @update="val => updateField(item, 'budgetAllocated', parseFloat(val) || null)" />
              </td>
              <td class="px-2 py-1 text-right">
                <EditableCell :value="item.budgetSpent?.toLocaleString() ?? ''" :editable="canEditItem(item)"
                  type="number" @update="val => updateField(item, 'budgetSpent', parseFloat(val) || null)" />
              </td>
              <td class="px-2 py-1 text-center">
                <select v-if="canEditItem(item)" :value="item.status"
                  @change="updateField(item, 'status', ($event.target as HTMLSelectElement).value)"
                  class="text-xs px-2 py-1 rounded-full border border-gray-200 focus:outline-none"
                  :class="statusClass(item.status)">
                  <option value="NotStarted">ยังไม่เริ่ม</option>
                  <option value="InProgress">กำลังดำเนินการ</option>
                  <option value="Completed">เสร็จสิ้น</option>
                  <option value="OnHold">หยุดชั่วคราว</option>
                  <option value="Cancelled">ยกเลิก</option>
                </select>
                <span v-else class="text-xs px-2 py-1 rounded-full" :class="statusClass(item.status)">
                  {{ statusLabel(item.status) }}
                </span>
              </td>
              <td class="px-2 py-1">
                <EditableCell :value="item.notes ?? ''" :editable="canEditItem(item)"
                  @update="val => updateField(item, 'notes', val)" />
              </td>
              <td class="px-2 py-1 text-center">
                <button v-if="canEditItem(item)" @click="deleteItem(item)"
                  class="text-red-400 hover:text-red-600 transition-colors text-base">
                  🗑
                </button>
              </td>
            </tr>
          </tbody>
          <tfoot v-if="filteredItems.length > 0" class="sticky bottom-0 bg-gray-50">
            <tr class="border-t-2 border-gray-300">
              <td colspan="6" class="px-3 py-2 font-semibold text-sm">รวม</td>
              <td class="px-2 py-2 text-right font-semibold text-sm">
                {{ filteredItems.reduce((s, i) => s + (i.budgetAllocated ?? 0), 0).toLocaleString() }}
              </td>
              <td class="px-2 py-2 text-right font-semibold text-sm">
                {{ filteredItems.reduce((s, i) => s + (i.budgetSpent ?? 0), 0).toLocaleString() }}
              </td>
              <td colspan="3"></td>
            </tr>
          </tfoot>
        </table>
        <div v-if="filteredItems.length === 0" class="text-center py-12 text-gray-400">
          ยังไม่มีรายการ กดปุ่ม "+ เพิ่มรายการ" เพื่อเริ่มต้น
        </div>
      </div>
    </div>

    <!-- Create Plan Modal -->
    <div v-if="showCreatePlan" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div class="bg-white rounded-xl shadow-xl p-6 w-full max-w-md">
        <h3 class="text-lg font-bold mb-4">สร้าง Action Plan ใหม่</h3>
        <div class="space-y-3">
          <div>
            <label class="text-sm font-medium text-gray-700">ชื่อ</label>
            <input v-model="newPlan.name" class="input-field mt-1" placeholder="Action Plan 2025" />
          </div>
          <div>
            <label class="text-sm font-medium text-gray-700">รายละเอียด</label>
            <textarea v-model="newPlan.description" class="input-field mt-1 resize-none h-20" />
          </div>
          <div>
            <label class="text-sm font-medium text-gray-700">Template (ปีงบประมาณ)</label>
            <select v-model.number="newPlan.templateId" class="input-field mt-1">
              <option v-for="t in budgetTemplates" :key="t.id" :value="t.id">{{ t.name }} ({{ t.year }})</option>
            </select>
          </div>
        </div>
        <div class="flex gap-2 mt-5 justify-end">
          <button @click="showCreatePlan = false" class="btn-secondary">ยกเลิก</button>
          <button @click="createPlan" class="btn-primary">สร้าง</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useActionPlanStore } from '@/stores/actionPlan'
import { useAuthStore } from '@/stores/auth'
import { budgetApi } from '@/api/budget'
import { actionPlanApi } from '@/api/actionPlan'
import { usersApi } from '@/api/users'
import type { ActionPlanItem, ActionPlanStatus, BudgetTemplate, Department } from '@/types'
import EditableCell from '@/components/shared/EditableCell.vue'

const planStore = useActionPlanStore()
const auth = useAuthStore()

const selectedPlanId = ref<number | ''>('')
const filterDeptId = ref<number | null>(null)
const departments = ref<Department[]>([])
const budgetTemplates = ref<BudgetTemplate[]>([])
const showCreatePlan = ref(false)
const newPlan = ref({ name: '', description: '', templateId: 0 })

const filteredItems = computed(() => {
  if (!filterDeptId.value) return planStore.items
  return planStore.items.filter(i => i.departmentId === filterDeptId.value)
})

const currentPlan = computed(() => planStore.plans.find(p => p.id === selectedPlanId.value))

const canAddItem = computed(() => {
  if (!currentPlan.value || currentPlan.value.isLocked) return false
  return auth.isAdmin || auth.editableDeptIds.length > 0
})

function canEditItem(item: ActionPlanItem) {
  if (!currentPlan.value || currentPlan.value.isLocked) return false
  return auth.canEditDepartment(item.departmentId)
}

async function loadItems() {
  if (!selectedPlanId.value) return
  await planStore.loadItems(Number(selectedPlanId.value))
}

async function addNewRow() {
  if (!selectedPlanId.value) return
  const deptId = auth.isAdmin ? departments.value[0]?.id : auth.editableDeptIds[0]
  if (!deptId) return
  await planStore.addItem(Number(selectedPlanId.value), {
    departmentId: deptId,
    initiativeName: 'โครงการใหม่',
    status: 'NotStarted'
  })
}

async function updateField(item: ActionPlanItem, field: keyof ActionPlanItem, value: any) {
  const updated = { ...item, [field]: value }
  await planStore.saveItem(Number(selectedPlanId.value), item.id, updated)
}

async function deleteItem(item: ActionPlanItem) {
  if (!confirm('ต้องการลบรายการนี้?')) return
  await planStore.removeItem(Number(selectedPlanId.value), item.id)
}

async function createPlan() {
  if (!newPlan.value.name || !newPlan.value.templateId) return
  await actionPlanApi.create(newPlan.value)
  showCreatePlan.value = false
  await planStore.loadPlans()
}

const statusLabel = (s: ActionPlanStatus) => ({
  NotStarted: 'ยังไม่เริ่ม', InProgress: 'กำลังดำเนินการ',
  Completed: 'เสร็จสิ้น', OnHold: 'หยุดชั่วคราว', Cancelled: 'ยกเลิก'
}[s] ?? s)

const statusClass = (s: ActionPlanStatus) => ({
  NotStarted: 'bg-gray-100 text-gray-700',
  InProgress: 'bg-blue-100 text-blue-700',
  Completed: 'bg-green-100 text-green-700',
  OnHold: 'bg-yellow-100 text-yellow-700',
  Cancelled: 'bg-red-100 text-red-700'
}[s] ?? '')

onMounted(async () => {
  [departments.value, budgetTemplates.value] = await Promise.all([
    usersApi.getDepartments().then(r => r.data),
    budgetApi.getTemplates().then(r => r.data)
  ])
  await planStore.loadPlans()
  if (planStore.plans.length) {
    selectedPlanId.value = planStore.plans[0].id
    await loadItems()
  }
  newPlan.value.templateId = budgetTemplates.value[0]?.id ?? 0
})
</script>
