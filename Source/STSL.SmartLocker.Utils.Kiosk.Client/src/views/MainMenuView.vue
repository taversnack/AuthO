<!-- Component Definition -->
<script setup lang="ts">
import { useCardRequestStore } from '@/stores/card-requests'
import { onMounted } from 'vue'
import { RouterLink } from 'vue-router'

const { reset } = useCardRequestStore()

// Clear email / recovery code on mount
onMounted(() => {
  reset()
})
</script>

<!-- Template -->
<template>
  <div class="wrapper">
    <RouterLink to="/request-card" aria-label="Request temporary card" class="w-full">
      <button class="action-button">I've forgotten/lost my UCLH ID card</button>
    </RouterLink>
    <RouterLink to="/return-card" aria-label="Return a temporary card" class="w-full">
      <button class="action-button">Return UCLH Temporary ID card</button>
    </RouterLink>
  </div>
</template>

<!-- Styling -->
<style scoped>
.wrapper {
  @apply flex flex-col items-center gap-6 text-center mb-10 w-full max-w-xl 2xl:max-w-4xl;
}

.action-button {
  @apply w-full px-10 py-5 2xl:py-10 text-2xl 2xl:text-4xl text-dark font-bold border-4 border-dark rounded-md bg-white hover:bg-white/80 hover:text-black hover:border-black transition-all duration-150;
}
</style>
