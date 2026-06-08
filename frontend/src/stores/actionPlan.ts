import { defineStore } from 'pinia'
import { ref } from 'vue'
import { actionPlanApi } from '@/api/actionPlan'
import type { ActionPlan, ActionPlanItem } from '@/types'

export const useActionPlanStore = defineStore('actionPlan', () => {
  const plans = ref<ActionPlan[]>([])
  const currentPlan = ref<ActionPlan | null>(null)
  const items = ref<ActionPlanItem[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function loadPlans(templateId?: number) {
    const { data } = await actionPlanApi.getAll(templateId)
    plans.value = data
  }

  async function loadItems(planId: number) {
    loading.value = true
    error.value = null
    try {
      const plan = plans.value.find(p => p.id === planId)
      if (plan) currentPlan.value = plan
      const { data } = await actionPlanApi.getItems(planId)
      items.value = data
    } catch (e: any) {
      error.value = e.response?.data?.message ?? 'เกิดข้อผิดพลาด'
    } finally {
      loading.value = false
    }
  }

  async function addItem(planId: number, data: Partial<ActionPlanItem>) {
    const { data: item } = await actionPlanApi.createItem(planId, data)
    items.value.push(item)
    return item
  }

  async function saveItem(planId: number, itemId: number, data: Partial<ActionPlanItem>) {
    const { data: updated } = await actionPlanApi.updateItem(planId, itemId, data)
    const idx = items.value.findIndex(i => i.id === itemId)
    if (idx >= 0) items.value[idx] = updated
    return updated
  }

  async function removeItem(planId: number, itemId: number) {
    await actionPlanApi.deleteItem(planId, itemId)
    items.value = items.value.filter(i => i.id !== itemId)
  }

  return { plans, currentPlan, items, loading, error, loadPlans, loadItems, addItem, saveItem, removeItem }
})
