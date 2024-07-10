<!-- Component Definition -->
<script setup lang="ts">
import { RouterLink } from 'vue-router'
</script>

<!-- Template -->
<template>
  <div class="wrapper">
    <h1 class="heading">UCLH Temporary ID Card Kiosk. This Kiosk is for Smart Locker Users Only.</h1>
    <RouterLink to="/main-menu" aria-label="Start Process" class="w-full">
      <button class="action-button">Start Process</button>
    </RouterLink>
  </div>
</template>

<!-- Styling -->
<style scoped>
.wrapper {
  @apply flex flex-col items-center gap-6 text-center mb-10 w-full max-w-xl 2xl:max-w-4xl;
}

.action-button {
  @apply w-full px-10 py-5 2xl:py-10 text-2xl 2xl:text-5xl text-dark font-bold border-4 border-dark rounded-md bg-white hover:bg-white/80 hover:text-black hover:border-black transition-all duration-150;
}

.heading {
  @apply text-5xl 
  text-white;
}
</style>
