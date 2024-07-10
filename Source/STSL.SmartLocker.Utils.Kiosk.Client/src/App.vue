<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router';
import { ref } from 'vue';
import InfoHelpDialog from './components/InfoHelpDialogComponent.vue';
import Status from './components/StatusComponent.vue';

const showHelpInfo = ref(false); // Initialize showModal
</script>

<template>
  <div class="w-full h-screen bg-[#0180cc] flex flex-col items-center justify-between">
    <header>
      <RouterLink to="/" aria-label="Homepage">
        <font-awesome-icon icon="house-chimney" color="#FAF9F6" />
      </RouterLink>
      <nav>
        <!-- Help Modal Button -->
        <button @click="showHelpInfo = true" class=" 2xl:mr-10">
          <font-awesome-icon icon="circle-info" color="#FAF9F6"/>
        </button>
      </nav>
    </header>
    
    <main>
      <div class="w-full flex flex-col">
        <div class="w-full h-full flex items-center justify-center px-5">
          <RouterView/>
        </div>
        <InfoHelpDialog :show-modal="showHelpInfo" @close="showHelpInfo = false" />
      </div>
    </main>
    <footer class="w-full text-white p-4">
      <!-- Status component which performs authentication every 29 mins to the VWS API via Service Bus -->
      <Status />
    </footer>
  </div>
</template>

<style scoped>
.link{
  @apply 
  text-white 
  hover:text-white 
  font-medium 
  hover:underline 
  transition-all 
  duration-150
}

header {
  @apply w-full flex items-center justify-between p-5 2xl:p-10 text-3xl 2xl:text-7xl;
}

main {
  @apply 
  flex 
  flex-col 
  items-center 
  text-center;
}

/* top navigation */
nav {
  margin-left: auto;
}

nav a:not(:first-child) {
  margin-left: 0.8rem;
}

nav a.router-link-exact-active {
  opacity: 0.6;
  pointer-events: none;
}

</style>
