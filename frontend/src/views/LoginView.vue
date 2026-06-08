<template>
  <div class="min-h-screen bg-gradient-to-br from-primary-800 to-primary-900 flex items-center justify-center p-4">
    <div class="bg-white rounded-2xl shadow-2xl w-full max-w-sm p-8">
      <div class="text-center mb-8">
        <div class="text-4xl mb-3">💰</div>
        <h1 class="text-2xl font-bold text-gray-900">Budget Planner</h1>
        <p class="text-sm text-gray-500 mt-1">ระบบวางแผนงบประมาณ</p>
      </div>

      <form @submit.prevent="handleLogin" class="space-y-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">อีเมล</label>
          <input
            v-model="email" type="email" required autocomplete="email"
            class="input-field" placeholder="user@company.com"
          />
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">รหัสผ่าน</label>
          <input
            v-model="password" type="password" required autocomplete="current-password"
            class="input-field" placeholder="••••••••"
          />
        </div>

        <div v-if="errorMsg" class="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg p-3">
          {{ errorMsg }}
        </div>

        <button type="submit" :disabled="loading" class="btn-primary w-full justify-center flex items-center gap-2 py-2.5">
          <svg v-if="loading" class="animate-spin w-4 h-4" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
          </svg>
          {{ loading ? 'กำลังเข้าสู่ระบบ...' : 'เข้าสู่ระบบ' }}
        </button>
      </form>

      <p class="text-xs text-center text-gray-400 mt-6">
        ค่าเริ่มต้น: admin@company.com / Admin@1234
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const router = useRouter()
const email = ref('')
const password = ref('')
const loading = ref(false)
const errorMsg = ref('')

async function handleLogin() {
  loading.value = true
  errorMsg.value = ''
  try {
    await auth.login(email.value, password.value)
    router.push('/dashboard')
  } catch (e: any) {
    errorMsg.value = e.response?.data?.message ?? 'เกิดข้อผิดพลาด กรุณาลองใหม่'
  } finally {
    loading.value = false
  }
}
</script>
