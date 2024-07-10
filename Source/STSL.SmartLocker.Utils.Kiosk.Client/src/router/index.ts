import { createRouter, createWebHistory } from 'vue-router'
import CardRequest from '../views/CardRequestView.vue'
import DispenseCard from '@/views/DispenseCardView.vue'
import ReturningTemporary from '../views/ReturningTemporaryCardView.vue'
import ErrorView from '../views/ErrorView.vue'
import MainMenu from '@/views/MainMenuView.vue'
import HomeView from '../views/HomeView.vue'
import App from '../App.vue'
import { useCardRequestStore } from '@/stores/card-requests'
import { createPinia } from 'pinia'
import { createApp } from 'vue'

const app = createApp(App)
const pinia = createPinia();
app.use(pinia)

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView
    },
    {
      path: '/main-menu',
      name: 'mainMenu',
      component: MainMenu
    },
    {
      path: '/print-card',
      name: 'printCard',
      component: DispenseCard
    },
    {
      path: '/request-card',
      name: 'cardRequest',
      component: CardRequest
    },
    {
      path: '/return-card',
      name: 'returningTemporary',
      component: ReturningTemporary
    },
    {
      path: '/error-view',
      name: 'error',
      component: ErrorView
    },
  ]
})

/*
router.beforeEach((to, _from) => {
  const { isAuthenticated } = useAuthStore()
  if (!isAuthenticated && to.name?.toString() !== 'login') {
    return { name: 'login' }
  }
})*/


// Global navigation guard to reset values on route change
router.beforeEach((_to, _from, next) => {
  const cardRequestStore = useCardRequestStore(pinia);
  cardRequestStore.reset();
  next()
})

export default router