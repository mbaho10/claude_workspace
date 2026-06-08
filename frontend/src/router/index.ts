import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
      meta: { public: true }
    },
    {
      path: '/',
      component: () => import('@/components/layout/AppShell.vue'),
      children: [
        {
          path: '',
          redirect: '/dashboard'
        },
        {
          path: 'dashboard',
          name: 'dashboard',
          component: () => import('@/views/DashboardView.vue')
        },
        {
          path: 'budget',
          name: 'budget',
          component: () => import('@/views/BudgetPlanningView.vue')
        },
        {
          path: 'action-plan',
          name: 'action-plan',
          component: () => import('@/views/ActionPlanView.vue')
        },
        {
          path: 'admin',
          name: 'admin',
          component: () => import('@/views/AdminView.vue'),
          meta: { requiresAdmin: true }
        }
      ]
    }
  ]
})

router.beforeEach(async (to, _from, next) => {
  const auth = useAuthStore()

  if (to.meta.public) {
    if (auth.isAuthenticated) return next('/dashboard')
    return next()
  }

  if (!auth.isAuthenticated) {
    // Try refresh via cookie
    try {
      const { default: axios } = await import('axios')
      const { data } = await axios.post('/api/auth/refresh', {}, { withCredentials: true })
      auth.setAccessToken(data.accessToken)
      auth.user = data.user
    } catch {
      return next('/login')
    }
  }

  if (to.meta.requiresAdmin && !auth.isAdmin) {
    return next('/dashboard')
  }

  next()
})

export default router
