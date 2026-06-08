<template>
  <div class="flex flex-col h-full gap-4">
    <!-- Controls -->
    <div class="card p-4 flex flex-wrap items-center gap-3">
      <div class="flex items-center gap-2">
        <label class="text-sm font-medium text-gray-700">ปีงบประมาณ:</label>
        <select v-model="selectedYear" @change="loadTemplates" class="input-field w-28">
          <option v-for="y in years" :key="y" :value="y">{{ y + 543 }}</option>
        </select>
      </div>

      <div class="flex items-center gap-2">
        <label class="text-sm font-medium text-gray-700">Template:</label>
        <select v-model="selectedTemplateId" @change="loadData" class="input-field w-56"
          :disabled="!budget.templates.length">
          <option value="">-- เลือก Template --</option>
          <option v-for="t in budget.templates" :key="t.id" :value="t.id">{{ t.name }}</option>
        </select>
      </div>

      <div v-if="auth.isAdmin" class="flex items-center gap-2">
        <label class="text-sm font-medium text-gray-700">แผนก:</label>
        <select v-model="selectedDeptId" @change="loadData" class="input-field w-48">
          <option :value="null">ทุกแผนก</option>
          <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
        </select>
      </div>

      <div class="ml-auto flex items-center gap-2">
        <span v-if="budget.currentTemplate?.isLocked" class="text-xs text-red-600 bg-red-50 border border-red-200 px-3 py-1.5 rounded-full flex items-center gap-1">
          🔒 Template ถูกล็อค
        </span>
        <button v-if="selectedTemplateId && auth.isAdmin"
          @click="toggleLock"
          class="btn-secondary text-xs">
          {{ budget.currentTemplate?.isLocked ? '🔓 ปลดล็อค' : '🔒 ล็อค Template' }}
        </button>
        <button v-if="selectedTemplateId" @click="exportExcel" class="btn-secondary text-xs">
          📥 Export Excel
        </button>
        <button v-if="auth.isAdmin" @click="showCreateTemplate = true" class="btn-primary text-xs">
          + สร้าง Template
        </button>
      </div>
    </div>

    <!-- Grid -->
    <div class="card flex-1 overflow-hidden p-4 flex flex-col">
      <div v-if="!selectedTemplateId" class="flex-1 flex items-center justify-center text-gray-400">
        <div class="text-center">
          <div class="text-5xl mb-3">📊</div>
          <p class="text-base font-medium">เลือก Template เพื่อเริ่มต้น</p>
          <p class="text-sm mt-1">กรุณาเลือกปีงบประมาณและ Template ด้านบน</p>
        </div>
      </div>
      <div v-else-if="budget.loading" class="flex-1 flex items-center justify-center">
        <div class="text-center text-gray-500">
          <svg class="animate-spin w-8 h-8 mx-auto mb-2 text-primary-500" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
          </svg>
          กำลังโหลดข้อมูล...
        </div>
      </div>
      <SpreadsheetGrid v-else-if="budget.gridData" :department-filter="selectedDeptId">
        <template #toolbar-actions>
          <button v-if="auth.isAdmin" @click="showAddCategory = true" class="btn-secondary text-xs">
            + เพิ่มหมวดหมู่
          </button>
        </template>
      </SpreadsheetGrid>
    </div>

    <!-- Create Template Modal -->
    <div v-if="showCreateTemplate" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div class="bg-white rounded-xl shadow-xl p-6 w-full max-w-md">
        <h3 class="text-lg font-bold mb-4">สร้าง Template ใหม่</h3>
        <div class="space-y-3">
          <div>
            <label class="text-sm font-medium text-gray-700">ชื่อ Template</label>
            <input v-model="newTemplate.name" class="input-field mt-1" placeholder="Budget 2025" />
          </div>
          <div>
            <label class="text-sm font-medium text-gray-700">ปีงบประมาณ (ค.ศ.)</label>
            <input v-model.number="newTemplate.year" type="number" class="input-field mt-1" />
          </div>
          <div>
            <label class="text-sm font-medium text-gray-700">ประเภทช่วงเวลา</label>
            <select v-model="newTemplate.periodType" class="input-field mt-1">
              <option value="Monthly">รายเดือน (12 คอลัมน์)</option>
              <option value="Quarterly">รายไตรมาส (4 คอลัมน์)</option>
            </select>
          </div>
        </div>
        <div class="flex gap-2 mt-5 justify-end">
          <button @click="showCreateTemplate = false" class="btn-secondary">ยกเลิก</button>
          <button @click="createTemplate" class="btn-primary">สร้าง</button>
        </div>
      </div>
    </div>

    <!-- Add Category Modal -->
    <div v-if="showAddCategory" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div class="bg-white rounded-xl shadow-xl p-6 w-full max-w-md">
        <h3 class="text-lg font-bold mb-4">เพิ่มหมวดหมู่งบประมาณ</h3>
        <div class="space-y-3">
          <div>
            <label class="text-sm font-medium text-gray-700">ชื่อหมวดหมู่</label>
            <input v-model="newCategory.name" class="input-field mt-1" placeholder="เช่น เงินเดือน" />
          </div>
          <div>
            <label class="text-sm font-medium text-gray-700">หมวดหมู่หลัก (ถ้ามี)</label>
            <select v-model="newCategory.parentId" class="input-field mt-1">
              <option :value="null">ไม่มี (หมวดหมู่หลัก)</option>
              <option v-for="cat in budget.gridData?.categories.filter(c => !c.parentId)" :key="cat.id" :value="cat.id">
                {{ cat.name }}
              </option>
            </select>
          </div>
          <div class="flex items-center gap-2">
            <input type="checkbox" v-model="newCategory.isCalculated" id="isCalc" class="rounded" />
            <label for="isCalc" class="text-sm text-gray-700">เป็นแถวยอดรวม (คำนวณอัตโนมัติ)</label>
          </div>
        </div>
        <div class="flex gap-2 mt-5 justify-end">
          <button @click="showAddCategory = false" class="btn-secondary">ยกเลิก</button>
          <button @click="addCategory" class="btn-primary">เพิ่ม</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import SpreadsheetGrid from '@/components/grid/SpreadsheetGrid.vue'
import { useBudgetStore } from '@/stores/budget'
import { useAuthStore } from '@/stores/auth'
import { budgetApi } from '@/api/budget'
import { usersApi } from '@/api/users'
import type { Department } from '@/types'

const budget = useBudgetStore()
const auth = useAuthStore()

const currentYear = new Date().getFullYear()
const years = Array.from({ length: 5 }, (_, i) => currentYear - 2 + i)
const selectedYear = ref(currentYear)
const selectedTemplateId = ref<number | ''>('')
const selectedDeptId = ref<number | null>(null)
const departments = ref<Department[]>([])

const showCreateTemplate = ref(false)
const showAddCategory = ref(false)

const newTemplate = ref({ name: '', year: currentYear, periodType: 'Monthly' })
const newCategory = ref({ name: '', parentId: null as number | null, isCalculated: false })

async function loadTemplates() {
  await budget.loadTemplates(selectedYear.value)
  selectedTemplateId.value = budget.templates[0]?.id ?? ''
  if (selectedTemplateId.value) await loadData()
}

async function loadData() {
  if (!selectedTemplateId.value) return
  await budget.loadGridData(Number(selectedTemplateId.value), selectedDeptId.value ?? undefined)
}

async function createTemplate() {
  if (!newTemplate.value.name) return
  await budgetApi.createTemplate(newTemplate.value)
  showCreateTemplate.value = false
  newTemplate.value = { name: '', year: currentYear, periodType: 'Monthly' }
  await loadTemplates()
}

async function addCategory() {
  if (!newCategory.value.name || !selectedTemplateId.value) return
  await budgetApi.createCategory(Number(selectedTemplateId.value), {
    ...newCategory.value, sortOrder: budget.gridData?.categories.length ?? 0
  })
  showAddCategory.value = false
  newCategory.value = { name: '', parentId: null, isCalculated: false }
  await loadData()
}

async function toggleLock() {
  if (!budget.currentTemplate) return
  await budgetApi.lockTemplate(budget.currentTemplate.id, !budget.currentTemplate.isLocked)
  await loadData()
}

async function exportExcel() {
  if (!selectedTemplateId.value) return
  const { data } = await budgetApi.exportExcel(Number(selectedTemplateId.value))
  const url = URL.createObjectURL(new Blob([data]))
  const a = document.createElement('a')
  a.href = url
  a.download = `budget_${selectedYear.value}.xlsx`
  a.click()
  URL.revokeObjectURL(url)
}

onMounted(async () => {
  departments.value = await usersApi.getDepartments().then(r => r.data)
  await loadTemplates()
})
</script>
