import client from './client'
import type { ActionPlan, ActionPlanItem } from '@/types'

export const actionPlanApi = {
  getAll: (templateId?: number) =>
    client.get<ActionPlan[]>('/action-plans', { params: { templateId } }),

  create: (data: { templateId: number; name: string; description?: string }) =>
    client.post<ActionPlan>('/action-plans', data),

  getItems: (planId: number) =>
    client.get<ActionPlanItem[]>(`/action-plans/${planId}/items`),

  createItem: (planId: number, data: Partial<ActionPlanItem>) =>
    client.post<ActionPlanItem>(`/action-plans/${planId}/items`, data),

  updateItem: (planId: number, itemId: number, data: Partial<ActionPlanItem>) =>
    client.put<ActionPlanItem>(`/action-plans/${planId}/items/${itemId}`, data),

  deleteItem: (planId: number, itemId: number) =>
    client.delete(`/action-plans/${planId}/items/${itemId}`),

  reorderItems: (planId: number, orderedIds: number[]) =>
    client.put(`/action-plans/${planId}/items/reorder`, { orderedIds })
}
