<template>
  <div class="space-y-4">
    <!-- Tabs -->
    <div class="card">
      <div class="flex border-b border-gray-200">
        <button v-for="tab in tabs" :key="tab.id" @click="activeTab = tab.id"
          class="px-5 py-3.5 text-sm font-medium border-b-2 -mb-px transition-colors"
          :class="activeTab === tab.id
            ? 'border-primary-600 text-primary-600'
            : 'border-transparent text-gray-500 hover:text-gray-700'">
          {{ tab.label }}
        </button>
      </div>
    </div>

    <!-- Users Tab -->
    <div v-if="activeTab === 'users'" class="card p-6">
      <div class="flex items-center justify-between mb-5">
        <h3 class="text-base font-semibold">จัดการผู้ใช้ ({{ users.length }} คน)</h3>
        <button @click="openCreateUser" class="btn-primary text-sm">+ เพิ่มผู้ใช้</button>
      </div>
      <div class="overflow-auto">
        <table class="w-full text-sm border-collapse">
          <thead>
            <tr class="bg-gray-50">
              <th class="px-3 py-3 text-left border-b border-gray-200 font-semibold">ชื่อ</th>
              <th class="px-3 py-3 text-left border-b border-gray-200 font-semibold">อีเมล</th>
              <th class="px-3 py-3 text-left border-b border-gray-200 font-semibold">สิทธิ์</th>
              <th class="px-3 py-3 text-left border-b border-gray-200 font-semibold">แผนก</th>
              <th class="px-3 py-3 text-center border-b border-gray-200 font-semibold">สถานะ</th>
              <th class="px-3 py-3 text-center border-b border-gray-200 font-semibold">จัดการสิทธิ์</th>
              <th class="px-3 py-3 border-b border-gray-200"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="u in users" :key="u.id" class="border-b border-gray-100 hover:bg-gray-50">
              <td class="px-3 py-2 font-medium">{{ u.fullName }}</td>
              <td class="px-3 py-2 text-gray-600">{{ u.email }}</td>
              <td class="px-3 py-2">
                <span class="text-xs px-2 py-1 rounded-full" :class="roleClass(u.role)">{{ roleLabel(u.role) }}</span>
              </td>
              <td class="px-3 py-2 text-gray-600">{{ u.departmentName ?? '-' }}</td>
              <td class="px-3 py-2 text-center">
                <span class="text-xs px-2 py-1 rounded-full" :class="u.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'">
                  {{ u.isActive ? 'ใช้งาน' : 'ปิดใช้งาน' }}
                </span>
              </td>
              <td class="px-3 py-2 text-center">
                <button @click="openPermissions(u)" class="text-xs text-primary-600 hover:underline">ตั้งค่าสิทธิ์</button>
              </td>
              <td class="px-3 py-2 text-center">
                <button @click="openEditUser(u)" class="text-xs text-gray-600 hover:text-gray-900">แก้ไข</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Departments Tab -->
    <div v-if="activeTab === 'departments'" class="card p-6">
      <div class="flex items-center justify-between mb-5">
        <h3 class="text-base font-semibold">จัดการแผนก ({{ departments.length }} แผนก)</h3>
        <button @click="openCreateDept" class="btn-primary text-sm">+ เพิ่มแผนก</button>
      </div>
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
        <div v-for="d in departments" :key="d.id" class="border border-gray-200 rounded-lg p-4 flex items-center justify-between hover:border-primary-300 transition-colors">
          <div>
            <p class="font-medium">{{ d.name }}</p>
            <p class="text-xs text-gray-500 mt-0.5">{{ d.code }}</p>
          </div>
          <button @click="openEditDept(d)" class="text-xs text-gray-500 hover:text-gray-800">แก้ไข</button>
        </div>
      </div>
    </div>

    <!-- User Form Modal -->
    <div v-if="showUserForm" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div class="bg-white rounded-xl shadow-xl p-6 w-full max-w-md max-h-[90vh] overflow-y-auto">
        <h3 class="text-lg font-bold mb-4">{{ editingUser ? 'แก้ไขผู้ใช้' : 'เพิ่มผู้ใช้ใหม่' }}</h3>
        <div class="space-y-3">
          <div>
            <label class="text-sm font-medium text-gray-700">ชื่อ-นามสกุล</label>
            <input v-model="userForm.fullName" class="input-field mt-1" />
          </div>
          <div v-if="!editingUser">
            <label class="text-sm font-medium text-gray-700">อีเมล</label>
            <input v-model="userForm.email" type="email" class="input-field mt-1" />
          </div>
          <div v-if="!editingUser">
            <label class="text-sm font-medium text-gray-700">รหัสผ่าน</label>
            <input v-model="userForm.password" type="password" class="input-field mt-1" />
          </div>
          <div>
            <label class="text-sm font-medium text-gray-700">สิทธิ์</label>
            <select v-model="userForm.role" class="input-field mt-1">
              <option value="Admin">ผู้ดูแลระบบ</option>
              <option value="DeptManager">ผู้จัดการแผนก</option>
              <option value="User">ผู้ใช้ทั่วไป</option>
            </select>
          </div>
          <div>
            <label class="text-sm font-medium text-gray-700">แผนกหลัก</label>
            <select v-model.number="userForm.departmentId" class="input-field mt-1">
              <option :value="null">ไม่ระบุ</option>
              <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
            </select>
          </div>
          <div v-if="editingUser" class="flex items-center gap-2">
            <input type="checkbox" v-model="userForm.isActive" id="isActive" class="rounded" />
            <label for="isActive" class="text-sm text-gray-700">เปิดใช้งาน</label>
          </div>
        </div>
        <div class="flex gap-2 mt-5 justify-end">
          <button @click="showUserForm = false" class="btn-secondary">ยกเลิก</button>
          <button @click="saveUser" class="btn-primary">บันทึก</button>
        </div>
      </div>
    </div>

    <!-- Permissions Modal -->
    <div v-if="showPermissions && permUser" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div class="bg-white rounded-xl shadow-xl p-6 w-full max-w-lg">
        <h3 class="text-lg font-bold mb-1">สิทธิ์การเข้าถึง</h3>
        <p class="text-sm text-gray-500 mb-4">{{ permUser.fullName }}</p>
        <div class="space-y-2 max-h-80 overflow-y-auto">
          <div v-for="dept in departments" :key="dept.id" class="flex items-center justify-between py-2 border-b border-gray-100">
            <span class="text-sm font-medium">{{ dept.name }}</span>
            <select :value="getPermLevel(dept.id)" @change="setPermLevel(dept.id, ($event.target as HTMLSelectElement).value)"
              class="text-sm border border-gray-200 rounded-lg px-2 py-1 focus:outline-none focus:ring-1 focus:ring-primary-400">
              <option value="">ไม่มีสิทธิ์</option>
              <option value="ReadOnly">ดูอย่างเดียว</option>
              <option value="Edit">แก้ไขได้</option>
            </select>
          </div>
        </div>
        <div class="flex gap-2 mt-5 justify-end">
          <button @click="showPermissions = false" class="btn-secondary">ยกเลิก</button>
          <button @click="savePermissions" class="btn-primary">บันทึก</button>
        </div>
      </div>
    </div>

    <!-- Department Form Modal -->
    <div v-if="showDeptForm" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div class="bg-white rounded-xl shadow-xl p-6 w-full max-w-sm">
        <h3 class="text-lg font-bold mb-4">{{ editingDept ? 'แก้ไขแผนก' : 'เพิ่มแผนก' }}</h3>
        <div class="space-y-3">
          <div>
            <label class="text-sm font-medium text-gray-700">ชื่อแผนก</label>
            <input v-model="deptForm.name" class="input-field mt-1" />
          </div>
          <div>
            <label class="text-sm font-medium text-gray-700">รหัสแผนก</label>
            <input v-model="deptForm.code" class="input-field mt-1" placeholder="เช่น HR, IT, FIN" />
          </div>
        </div>
        <div class="flex gap-2 mt-5 justify-end">
          <button @click="showDeptForm = false" class="btn-secondary">ยกเลิก</button>
          <button @click="saveDept" class="btn-primary">บันทึก</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { usersApi } from '@/api/users'
import type { UserDto, Department } from '@/types'

const activeTab = ref('users')
const tabs = [
  { id: 'users', label: 'ผู้ใช้งาน' },
  { id: 'departments', label: 'แผนก' }
]

const users = ref<UserDto[]>([])
const departments = ref<Department[]>([])

const showUserForm = ref(false)
const editingUser = ref<UserDto | null>(null)
const userForm = ref({ fullName: '', email: '', password: '', role: 'User', departmentId: null as number | null, isActive: true })

const showPermissions = ref(false)
const permUser = ref<UserDto | null>(null)
const permMap = ref<Map<number, string>>(new Map())

const showDeptForm = ref(false)
const editingDept = ref<Department | null>(null)
const deptForm = ref({ name: '', code: '' })

function openCreateUser() {
  editingUser.value = null
  userForm.value = { fullName: '', email: '', password: '', role: 'User', departmentId: null, isActive: true }
  showUserForm.value = true
}

function openEditUser(u: UserDto) {
  editingUser.value = u
  userForm.value = { fullName: u.fullName, email: u.email, password: '', role: u.role, departmentId: u.departmentId, isActive: u.isActive }
  showUserForm.value = true
}

async function saveUser() {
  if (editingUser.value) {
    await usersApi.update(editingUser.value.id, { fullName: userForm.value.fullName, role: userForm.value.role, departmentId: userForm.value.departmentId, isActive: userForm.value.isActive })
  } else {
    await usersApi.create(userForm.value)
  }
  showUserForm.value = false
  users.value = await usersApi.getAll().then(r => r.data)
}

async function openPermissions(u: UserDto) {
  permUser.value = u
  const perms = await usersApi.getPermissions(u.id).then(r => r.data)
  permMap.value = new Map(perms.map(p => [p.departmentId, p.permissionLevel]))
  showPermissions.value = true
}

function getPermLevel(deptId: number) { return permMap.value.get(deptId) ?? '' }
function setPermLevel(deptId: number, level: string) {
  if (!level) permMap.value.delete(deptId)
  else permMap.value.set(deptId, level)
}

async function savePermissions() {
  if (!permUser.value) return
  const perms = [...permMap.value.entries()].map(([deptId, permissionLevel]) => ({ departmentId: deptId, permissionLevel }))
  await usersApi.setPermissions(permUser.value.id, perms)
  showPermissions.value = false
}

function openCreateDept() {
  editingDept.value = null
  deptForm.value = { name: '', code: '' }
  showDeptForm.value = true
}

function openEditDept(d: Department) {
  editingDept.value = d
  deptForm.value = { name: d.name, code: d.code }
  showDeptForm.value = true
}

async function saveDept() {
  if (editingDept.value) await usersApi.updateDepartment(editingDept.value.id, deptForm.value)
  else await usersApi.createDepartment(deptForm.value)
  showDeptForm.value = false
  departments.value = await usersApi.getDepartments().then(r => r.data)
}

const roleLabel = (r: string) => ({ Admin: 'ผู้ดูแลระบบ', DeptManager: 'ผู้จัดการแผนก', User: 'ผู้ใช้' }[r] ?? r)
const roleClass = (r: string) => ({ Admin: 'bg-red-100 text-red-700', DeptManager: 'bg-purple-100 text-purple-700', User: 'bg-gray-100 text-gray-700' }[r] ?? '')

onMounted(async () => {
  [users.value, departments.value] = await Promise.all([
    usersApi.getAll().then(r => r.data),
    usersApi.getDepartments().then(r => r.data)
  ])
})
</script>
