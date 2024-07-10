import './assets/main.css'
import 'vue-toastification/dist/index.css'
import 'vue-loading-overlay/dist/css/index.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import Toast, { type PluginOptions, POSITION } from 'vue-toastification'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { library } from '@fortawesome/fontawesome-svg-core'
import { faHouseChimney, faPrint, faIdCard, faGear, faCircleInfo, fa1, fa2, fa3, fa4, fa5, fa6 } from '@fortawesome/free-solid-svg-icons'

library.add(faHouseChimney, faPrint, faIdCard, faGear, faCircleInfo, fa1, fa2, fa3, fa4, fa5, fa6);
  
const toastOptions: PluginOptions = {
  timeout: 3000,
  position: POSITION.TOP_RIGHT
}

const app = createApp(App)

app.component('font-awesome-icon', FontAwesomeIcon)

app.use(createPinia())
app.use(router)
app.use(Toast, toastOptions)

app.mount('#app')
