import client from './client'
import type { UserProfile } from '@/types'

export const authApi = {
  login: (email: string, password: string) =>
    client.post<{ accessToken: string; user: UserProfile }>('/auth/login', { email, password }),

  logout: () => client.post('/auth/logout'),

  me: () => client.get<UserProfile>('/auth/me')
}
