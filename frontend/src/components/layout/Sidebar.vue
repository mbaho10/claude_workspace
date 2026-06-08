<template>
  <aside class="w-60 bg-primary-900 text-white flex flex-col shrink-0">
    <div class="p-5 border-b border-primary-800">
      <h1 class="text-lg font-bold">Budget Planner</h1>
      <p class="text-xs text-primary-300 mt-0.5">ระบบวางแผนงบประมาณ</p>
    </div>

    <nav class="flex-1 p-4 space-y-1">
      <router-link
        v-for="item in navItems" :key="item.path"
        :to="item.path"
        class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors"
        :class="$route.path.startsWith(item.path) ? 'bg-primary-700 text-white' : 'text-primary-200 hover:bg-primary-800 hover:text-white'"
      >
        <span class="text-lg">{{ item.icon }}</span>
        {{ item.label }}
      </router-link>
    </nav>

    <div class="p-4 border-t border-primary-800">
      <div class="flex items-center gap-3 mb-3">
        <div class="w-8 h-8 rounded-full bg-primary-600 flex items-center justify-center text-sm font-bold">
          {{ auth.user?.fullName?.charAt(0) ?? 'U' }}
        </div>
        <div class="min-w-0">
          <p class="text-sm font-medium truncate">{{ auth.user?.fullName }}</p>
          <p class="text-xs text-primary-300 truncate">{{ roleLabel }}</p>
        </div>
      </div>
      <button @click="handleLogout" class="w-full text-xs text-primary-300 hover:text-white transition-colors text-left px-3 py-1.5 rounded hover:bg-primary-800">
        ออกจากระบบ
      </button>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const router = useRouter()

const navItems = computed(() => {
  const items = [
    { path: '/dashboard', icon: '📊', label: 'ภาพรวม' },
    { path: '/budget', icon: '💰', label: 'Budget Planning' },
    { path: '/action-plan', icon: '📋', label: 'Action Plan' }
  ]
  if (auth.isAdmin) items.push({ path: '/admin', icon: '⚙️', label: 'ตั้งค่าระบบ' })
  return items
})

const roleLabel = computed(() => {
  const map: Record<string, string> = { Admin: 'ผู้ดูแลระบบ', DeptManager: 'ผู้จัดการแผนก', User: 'ผู้ใช้ทั่วไป' }
  return map[auth.user?.role ?? ''] ?? ''
})

async function handleLogout() {
  await auth.logout()
  router.push('/login')
}
</script>
