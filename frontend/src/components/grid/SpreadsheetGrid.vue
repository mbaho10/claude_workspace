<template>
  <div class="flex flex-col h-full">
    <!-- Toolbar -->
    <div class="flex items-center justify-between mb-3 px-1">
      <div class="flex items-center gap-3">
        <span class="text-sm text-gray-600">
          {{ gridData?.categories.length ?? 0 }} หมวดหมู่ |
          {{ gridData?.departments.length ?? 0 }} แผนก
        </span>
        <span v-if="budget.saving" class="text-xs text-amber-600 flex items-center gap-1">
          <span class="w-1.5 h-1.5 rounded-full bg-amber-400 animate-pulse"></span>
          บันทึกอัตโนมัติ...
        </span>
        <span v-else-if="lastSaved" class="text-xs text-green-600">✓ บันทึกแล้ว</span>
      </div>
      <div class="flex gap-2">
        <slot name="toolbar-actions" />
      </div>
    </div>

    <!-- Grid -->
    <div class="flex-1 overflow-hidden rounded-lg border border-gray-200">
      <ag-grid-vue
        class="ag-theme-alpine h-full w-full"
        :columnDefs="columnDefs"
        :rowData="rowData"
        :defaultColDef="defaultColDef"
        :pinnedBottomRowData="pinnedBottomRow"
        :suppressMovableColumns="true"
        :enableRangeSelection="true"
        :stopEditingWhenCellsLoseFocus="true"
        :enterNavigatesVertically="true"
        :enterNavigatesVerticallyAfterEdit="true"
        :tabToNextCell="tabToNextEditableCell"
        @cell-value-changed="onCellValueChanged"
        @grid-ready="onGridReady"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { AgGridVue } from 'ag-grid-vue3'
import type { ColDef, GridApi, CellValueChangedEvent, TabToNextCellParams } from 'ag-grid-community'
import { useBudgetStore } from '@/stores/budget'
import { useAuthStore } from '@/stores/auth'

const props = defineProps<{
  departmentFilter?: number | null
}>()

const budget = useBudgetStore()
const auth = useAuthStore()
const gridApi = ref<GridApi | null>(null)
const lastSaved = ref(false)
const gridData = computed(() => budget.gridData)

watch(() => budget.saving, (val, prev) => {
  if (!val && prev) {
    lastSaved.value = true
    setTimeout(() => { lastSaved.value = false }, 3000)
  }
})

function onGridReady(params: { api: GridApi }) {
  gridApi.value = params.api
}

const defaultColDef: ColDef = {
  resizable: true,
  sortable: false,
  suppressMenu: true,
  cellClass: 'text-right',
}

const columnDefs = computed((): ColDef[] => {
  if (!gridData.value) return []
  const { departments, periodLabels } = gridData.value

  const cols: ColDef[] = [
    {
      field: 'categoryName',
      headerName: 'หมวดหมู่งบประมาณ',
      pinned: 'left',
      width: 220,
      cellClass: (params) => params.data?.isCalculated ? 'font-semibold bg-blue-50 text-left px-3' : 'text-left px-3',
      editable: false,
      cellRenderer: (params: any) => {
        const indent = params.data?.parentId ? 'ml-4' : ''
        return `<span class="${indent}">${params.data?.isCalculated ? '∑ ' : ''}${params.value}</span>`
      }
    }
  ]

  const visibleDepts = props.departmentFilter
    ? departments.filter(d => d.id === props.departmentFilter)
    : departments

  visibleDepts.forEach(dept => {
    const canEdit = auth.canEditDepartment(dept.id)

    if (visibleDepts.length > 1) {
      // Admin view: group by dept with period sub-columns
      const children: ColDef[] = periodLabels.map((label, pIdx) => ({
        field: `${dept.id}_${pIdx}`,
        headerName: label,
        width: 90,
        type: 'numericColumn',
        editable: (p: any) => canEdit && !p.data?.isCalculated && !budget.currentTemplate?.isLocked,
        cellClass: (p: any) => getCellClass(p, dept.id, canEdit),
        valueFormatter: (p: any) => formatAmount(p.value),
        valueSetter: (p: any) => {
          const val = parseFloat(String(p.newValue).replace(/,/g, '')) || 0
          p.data[p.colDef.field!] = val
          return true
        }
      }))
      children.push({
        field: `total_${dept.id}`,
        headerName: 'รวม',
        width: 110,
        editable: false,
        cellClass: 'font-semibold bg-gray-50 text-right',
        valueFormatter: (p: any) => formatAmount(p.value)
      })
      cols.push({
        headerName: dept.name,
        children,
        marryChildren: true
      } as any)
    } else {
      // Single dept view: flat period columns
      periodLabels.forEach((label, pIdx) => {
        cols.push({
          field: `${dept.id}_${pIdx}`,
          headerName: label,
          width: 100,
          type: 'numericColumn',
          editable: (p: any) => canEdit && !p.data?.isCalculated && !budget.currentTemplate?.isLocked,
          cellClass: (p: any) => getCellClass(p, dept.id, canEdit),
          valueFormatter: (p: any) => formatAmount(p.value),
          valueSetter: (p: any) => {
            const val = parseFloat(String(p.newValue).replace(/,/g, '')) || 0
            p.data[p.colDef.field!] = val
            return true
          }
        })
      })
      cols.push({
        headerName: 'รวม',
        field: `total_${dept.id}`,
        width: 120,
        editable: false,
        cellClass: 'font-semibold bg-gray-50 text-right',
        valueFormatter: (p: any) => formatAmount(p.value)
      })
    }
  })

  cols.push({
    headerName: 'รวมทั้งหมด',
    field: 'grandTotal',
    width: 130,
    pinned: 'right',
    editable: false,
    cellClass: 'font-bold bg-primary-50 text-right',
    valueFormatter: (p: any) => formatAmount(p.value)
  })

  return cols
})

const rowData = computed(() => {
  if (!gridData.value) return []
  const { categories, departments, periodLabels } = gridData.value
  const visibleDepts = props.departmentFilter
    ? departments.filter(d => d.id === props.departmentFilter)
    : departments

  return categories.map(cat => {
    const row: Record<string, any> = {
      categoryId: cat.id,
      categoryName: cat.name,
      parentId: cat.parentId,
      isCalculated: cat.isCalculated
    }

    let grandTotal = 0
    visibleDepts.forEach(dept => {
      let deptTotal = 0
      for (let p = 0; p < periodLabels.length; p++) {
        const amount = budget.getCellAmount(cat.id, dept.id, p)
        row[`${dept.id}_${p}`] = amount
        deptTotal += amount
      }
      row[`total_${dept.id}`] = deptTotal
      grandTotal += deptTotal
    })
    row.grandTotal = grandTotal
    return row
  })
})

const pinnedBottomRow = computed(() => {
  if (!gridData.value) return []
  const { departments, periodLabels } = gridData.value
  const visibleDepts = props.departmentFilter
    ? departments.filter(d => d.id === props.departmentFilter)
    : departments

  const row: Record<string, any> = { categoryName: 'รวมทั้งหมด', isCalculated: true }
  let grand = 0
  visibleDepts.forEach(dept => {
    let deptTotal = 0
    for (let p = 0; p < periodLabels.length; p++) {
      const colTotal = budget.getPeriodTotal(p, dept.id)
      row[`${dept.id}_${p}`] = colTotal
      deptTotal += colTotal
    }
    row[`total_${dept.id}`] = deptTotal
    grand += deptTotal
  })
  row.grandTotal = grand
  return [row]
})

function getCellClass(params: any, deptId: number, canEdit: boolean): string {
  const isCalc = params.data?.isCalculated
  const isLocked = budget.currentTemplate?.isLocked
  const classes = ['text-right', 'px-2']
  if (isCalc || !canEdit || isLocked) classes.push('cell-readonly')
  else classes.push('cell-editable')
  return classes.join(' ')
}

function formatAmount(val: any): string {
  if (val === null || val === undefined || val === '') return ''
  const num = parseFloat(String(val))
  if (isNaN(num) || num === 0) return '-'
  return num.toLocaleString('th-TH', { minimumFractionDigits: 0, maximumFractionDigits: 2 })
}

function onCellValueChanged(event: CellValueChangedEvent) {
  const field = event.colDef.field!
  const match = field.match(/^(\d+)_(\d+)$/)
  if (!match) return

  const deptId = parseInt(match[1])
  const periodIndex = parseInt(match[2])
  const categoryId = event.data.categoryId
  const amount = parseFloat(event.newValue) || 0

  budget.updateCell(categoryId, deptId, periodIndex, amount)

  // Recalculate totals in the row
  const data = event.data
  if (!gridData.value) return
  const depts = gridData.value.departments
  let grand = 0
  depts.forEach(d => {
    let deptTotal = 0
    for (let p = 0; p < gridData.value!.periodLabels.length; p++) {
      deptTotal += parseFloat(data[`${d.id}_${p}`]) || 0
    }
    data[`total_${d.id}`] = deptTotal
    grand += deptTotal
  })
  data.grandTotal = grand
  event.api.refreshCells({ rowNodes: [event.node!], force: true })
}

function tabToNextEditableCell(params: TabToNextCellParams) {
  return params.nextCellPosition
}
</script>
