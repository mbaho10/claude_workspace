import client from './client'
import type { BudgetTemplate, BudgetGridData, BudgetCategory } from '@/types'

export const budgetApi = {
  getTemplates: (year?: number) =>
    client.get<BudgetTemplate[]>('/budget/templates', { params: { year } }),

  createTemplate: (data: { name: string; year: number; periodType: string }) =>
    client.post<BudgetTemplate>('/budget/templates', data),

  getTemplate: (id: number) =>
    client.get<BudgetTemplate>(`/budget/templates/${id}`),

  lockTemplate: (id: number, locked: boolean) =>
    client.put(`/budget/templates/${id}/lock`, null, { params: { locked } }),

  getGridData: (templateId: number, departmentId?: number) =>
    client.get<BudgetGridData>(`/budget/templates/${templateId}/data`, {
      params: { departmentId }
    }),

  saveCells: (templateId: number, cells: Array<{
    categoryId: number
    departmentId: number
    periodIndex: number
    amount: number
    note?: string | null
  }>) => client.put(`/budget/templates/${templateId}/cells`, { cells }),

  createCategory: (templateId: number, data: {
    name: string; parentId?: number | null; sortOrder: number; isCalculated: boolean; categoryCode?: string | null
  }) => client.post<BudgetCategory>(`/budget/templates/${templateId}/categories`, data),

  updateCategory: (templateId: number, categoryId: number, data: {
    name: string; parentId?: number | null; sortOrder: number; isCalculated: boolean; categoryCode?: string | null
  }) => client.put<BudgetCategory>(`/budget/templates/${templateId}/categories/${categoryId}`, data),

  deleteCategory: (templateId: number, categoryId: number) =>
    client.delete(`/budget/templates/${templateId}/categories/${categoryId}`),

  exportExcel: (templateId: number) =>
    client.get(`/budget/templates/${templateId}/export`, { responseType: 'blob' })
}
