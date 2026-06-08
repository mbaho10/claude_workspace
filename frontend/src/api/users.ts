import client from './client'
import type { UserDto, UserPermission, Department } from '@/types'

export const usersApi = {
  getAll: () => client.get<UserDto[]>('/users'),

  create: (data: { email: string; fullName: string; password: string; role: string; departmentId?: number | null }) =>
    client.post<UserDto>('/users', data),

  update: (id: number, data: { fullName: string; role: string; departmentId?: number | null; isActive: boolean }) =>
    client.put<UserDto>(`/users/${id}`, data),

  getPermissions: (id: number) =>
    client.get<UserPermission[]>(`/users/${id}/permissions`),

  setPermissions: (id: number, permissions: Array<{ departmentId: number; permissionLevel: string }>) =>
    client.put(`/users/${id}/permissions`, { permissions }),

  getDepartments: () => client.get<Department[]>('/departments'),

  createDepartment: (data: { name: string; code: string }) =>
    client.post<Department>('/departments', data),

  updateDepartment: (id: number, data: { name: string; code: string }) =>
    client.put<Department>(`/departments/${id}`, data)
}
