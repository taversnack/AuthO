<script setup lang="ts">
import { onMounted } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { storeToRefs } from 'pinia';

const { authenticateKiosk } = useAuthStore()
const { isAuthenticated } = storeToRefs(useAuthStore())

const authenticate = async () => {
 // try {
    await authenticateKiosk();
  /*} catch (error) {
    console.error(error);
  }*/
};

onMounted(() => { 
  authenticate();
  setInterval(authenticate, 300000); // Ping every 5 minutes
});
</script>

<template>
  <footer class="status-footer">
    <div :class="['status-indicator', isAuthenticated ? 'connected' : 'disconnected']"></div>
    {{ isAuthenticated ? 'Connected' : 'Disconnected' }}
  </footer>
</template>

<style scoped>
.status-footer {
@apply p-4 bg-[#0180cc] text-white flex justify-center items-center text-xl 2xl:text-4xl;
}

.status-indicator {
@apply rounded-full w-3 h-3 2xl:w-10 2xl:h-10 mx-2 2xl:mr-5;
}

@keyframes pulse-green {
  0%, 100% {
    box-shadow: 0 0 8px rgba(5, 150, 105, 0.7), 0 0 20px rgba(5, 150, 105, 0.5);
  }
  50% {
    box-shadow: 0 0 20px rgba(5, 150, 105, 1), 0 0 40px rgba(5, 150, 105, 0.7);
  }
}

@keyframes pulse-red {
  0%, 100% {
    box-shadow: 0 0 8px rgba(220, 38, 38, 0.7), 0 0 20px rgba(220, 38, 38, 0.5);
  }
  50% {
    box-shadow: 0 0 20px rgba(220, 38, 38, 1), 0 0 40px rgba(220, 38, 38, 0.7);
  }
}

.connected {
  @apply bg-green-400 shadow-lg;
  animation: pulse-green 2s infinite;
}

.disconnected {
  @apply bg-red-400 shadow-lg;
  animation: pulse-red 2s infinite;
}
</style>