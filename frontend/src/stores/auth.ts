import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '@/api/auth'
import type { UserProfile } from '@/types'

export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref<string | null>(null)
  const user = ref<UserProfile | null>(null)
  const loading = ref(false)

  const isAuthenticated = computed(() => !!accessToken.value && !!user.value)
  const isAdmin = computed(() => user.value?.role === 'Admin')
  const editableDeptIds = computed(() => user.value?.editableDepartmentIds ?? [])

  function setAccessToken(token: string) {
    accessToken.value = token
  }

  function canEditDepartment(deptId: number) {
    if (isAdmin.value) return true
    return editableDeptIds.value.includes(deptId)
  }

  async function login(email: string, password: string) {
    loading.value = true
    try {
      const { data } = await authApi.login(email, password)
      accessToken.value = data.accessToken
      user.value = data.user
    } finally {
      loading.value = false
    }
  }

  async function logout() {
    try { await authApi.logout() } catch { /* ignore */ }
    accessToken.value = null
    user.value = null
  }

  async function fetchMe() {
    try {
      const { data } = await authApi.me()
      user.value = data
    } catch {
      accessToken.value = null
      user.value = null
    }
  }

  return { accessToken, user, loading, isAuthenticated, isAdmin, editableDeptIds, setAccessToken, canEditDepartment, login, logout, fetchMe }
})
