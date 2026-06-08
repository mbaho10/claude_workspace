import axios from 'axios'
import { useAuthStore } from '@/stores/auth'

const client = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  withCredentials: true,
  headers: { 'Content-Type': 'application/json' }
})

client.interceptors.request.use(config => {
  const auth = useAuthStore()
  if (auth.accessToken) {
    config.headers.Authorization = `Bearer ${auth.accessToken}`
  }
  return config
})

let refreshing = false
let refreshQueue: Array<(token: string) => void> = []

client.interceptors.response.use(
  res => res,
  async error => {
    const original = error.config
    if (error.response?.status === 401 && !original._retry) {
      original._retry = true

      if (refreshing) {
        return new Promise(resolve => {
          refreshQueue.push(token => {
            original.headers.Authorization = `Bearer ${token}`
            resolve(client(original))
          })
        })
      }

      refreshing = true
      try {
        const { data } = await axios.post('/api/auth/refresh', {}, { withCredentials: true })
        const auth = useAuthStore()
        auth.setAccessToken(data.accessToken)
        refreshQueue.forEach(cb => cb(data.accessToken))
        refreshQueue = []
        original.headers.Authorization = `Bearer ${data.accessToken}`
        return client(original)
      } catch {
        const auth = useAuthStore()
        auth.logout()
        window.location.href = '/login'
        return Promise.reject(error)
      } finally {
        refreshing = false
      }
    }
    return Promise.reject(error)
  }
)

export default client
