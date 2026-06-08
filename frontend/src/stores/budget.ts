import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { budgetApi } from '@/api/budget'
import type { BudgetTemplate, BudgetGridData, BudgetCell } from '@/types'
import { useDebounceFn } from '@vueuse/core'

export const useBudgetStore = defineStore('budget', () => {
  const templates = ref<BudgetTemplate[]>([])
  const currentTemplate = ref<BudgetTemplate | null>(null)
  const gridData = ref<BudgetGridData | null>(null)
  const loading = ref(false)
  const saving = ref(false)
  const error = ref<string | null>(null)

  // Pending changes queue: key = "catId_deptId_periodIdx"
  const pendingChanges = ref<Map<string, BudgetCell>>(new Map())

  const cellMap = computed(() => {
    const map = new Map<string, number>()
    gridData.value?.cells.forEach(c => {
      map.set(`${c.categoryId}_${c.departmentId}_${c.periodIndex}`, c.amount)
    })
    return map
  })

  function getCellAmount(categoryId: number, departmentId: number, periodIndex: number): number {
    const key = `${categoryId}_${departmentId}_${periodIndex}`
    if (pendingChanges.value.has(key)) return pendingChanges.value.get(key)!.amount
    return cellMap.value.get(key) ?? 0
  }

  function getCategoryTotal(categoryId: number, departmentId: number, periodCount: number): number {
    let total = 0
    for (let i = 0; i < periodCount; i++) total += getCellAmount(categoryId, departmentId, i)
    return total
  }

  function getPeriodTotal(periodIndex: number, departmentId: number): number {
    if (!gridData.value) return 0
    return gridData.value.categories
      .filter(c => !c.isCalculated)
      .reduce((sum, cat) => sum + getCellAmount(cat.id, departmentId, periodIndex), 0)
  }

  async function loadTemplates(year?: number) {
    const { data } = await budgetApi.getTemplates(year)
    templates.value = data
  }

  async function loadGridData(templateId: number, departmentId?: number) {
    loading.value = true
    error.value = null
    try {
      const { data } = await budgetApi.getGridData(templateId, departmentId)
      gridData.value = data
      currentTemplate.value = data.template
      pendingChanges.value.clear()
    } catch (e: any) {
      error.value = e.response?.data?.message ?? 'เกิดข้อผิดพลาดในการโหลดข้อมูล'
    } finally {
      loading.value = false
    }
  }

  const flushChanges = useDebounceFn(async () => {
    if (!currentTemplate.value || pendingChanges.value.size === 0) return
    saving.value = true
    const cells = [...pendingChanges.value.values()]
    pendingChanges.value.clear()
    try {
      await budgetApi.saveCells(currentTemplate.value.id, cells)
      // Update local cell data
      if (gridData.value) {
        cells.forEach(cell => {
          const idx = gridData.value!.cells.findIndex(
            c => c.categoryId === cell.categoryId && c.departmentId === cell.departmentId && c.periodIndex === cell.periodIndex
          )
          if (idx >= 0) gridData.value!.cells[idx].amount = cell.amount
          else gridData.value!.cells.push(cell)
        })
      }
    } catch (e: any) {
      error.value = e.response?.data?.message ?? 'เกิดข้อผิดพลาดในการบันทึก'
    } finally {
      saving.value = false
    }
  }, 800)

  function updateCell(categoryId: number, departmentId: number, periodIndex: number, amount: number, note?: string | null) {
    const key = `${categoryId}_${departmentId}_${periodIndex}`
    pendingChanges.value.set(key, { categoryId, departmentId, periodIndex, amount, note: note ?? null })
    flushChanges()
  }

  return {
    templates, currentTemplate, gridData, loading, saving, error,
    cellMap, pendingChanges,
    getCellAmount, getCategoryTotal, getPeriodTotal,
    loadTemplates, loadGridData, updateCell
  }
})
