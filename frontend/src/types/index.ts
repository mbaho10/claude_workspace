export interface UserProfile {
  id: number
  email: string
  fullName: string
  role: 'Admin' | 'DeptManager' | 'User'
  departmentId: number | null
  departmentName: string | null
  editableDepartmentIds: number[]
}

export interface Department {
  id: number
  name: string
  code: string
}

export interface BudgetTemplate {
  id: number
  name: string
  year: number
  periodType: 'Monthly' | 'Quarterly'
  periodCount: number
  isLocked: boolean
  createdAt: string
}

export interface BudgetCategory {
  id: number
  name: string
  parentId: number | null
  sortOrder: number
  isCalculated: boolean
  categoryCode: string | null
}

export interface BudgetCell {
  categoryId: number
  departmentId: number
  periodIndex: number
  amount: number
  note: string | null
}

export interface BudgetGridData {
  template: BudgetTemplate
  categories: BudgetCategory[]
  departments: Department[]
  periodLabels: string[]
  cells: BudgetCell[]
  editableDepartmentIds: number[]
}

export interface ActionPlan {
  id: number
  templateId: number
  name: string
  description: string | null
  isLocked: boolean
  createdAt: string
}

export interface ActionPlanItem {
  id: number
  actionPlanId: number
  departmentId: number
  departmentName: string
  initiativeName: string
  responsiblePerson: string | null
  startDate: string | null
  endDate: string | null
  budgetAllocated: number | null
  budgetSpent: number | null
  status: ActionPlanStatus
  notes: string | null
  sortOrder: number
  lastModifiedAt: string
}

export type ActionPlanStatus = 'NotStarted' | 'InProgress' | 'Completed' | 'OnHold' | 'Cancelled'

export interface UserDto {
  id: number
  email: string
  fullName: string
  role: string
  departmentId: number | null
  departmentName: string | null
  isActive: boolean
  lastLoginAt: string | null
  createdAt: string
}

export interface UserPermission {
  departmentId: number
  departmentName: string
  permissionLevel: 'ReadOnly' | 'Edit'
}
