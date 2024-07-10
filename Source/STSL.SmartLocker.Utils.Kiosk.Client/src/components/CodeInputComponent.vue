<!-- Component Definition -->
<script setup lang="ts">
import { useCardRequestStore } from '@/stores/card-requests'
import { storeToRefs } from 'pinia'

const emit = defineEmits<{
  submitRecoveryCode: any,
  doesNotHaveRecoveryCode: any
}>();

const { currentRecoveryCode, isValidRecoveryCode } = storeToRefs(useCardRequestStore())

</script>

<!-- Template -->
<template>
<div class="input-container">
  <input 
    v-model="currentRecoveryCode.code" 
    type="text" 
    placeholder="Enter your code here" 
    class="input"
    autocomplete="autocomplete_off_1234567899"
    required />
</div>
<div class="w-full grid gap-5 grid-cols-2">
    <button 
      @click="emit('submitRecoveryCode')" 
      :disabled="!isValidRecoveryCode"
      class="action-button disabled:opacity-30 disabled:pointer-events-none" >
      <span>Dispense UCLH temporary ID card</span>
    </button>
    <button 
      @click="emit('doesNotHaveRecoveryCode')" 
      class="action-button">
      <span>I don't have a code</span>
    </button>
</div>
</template>

<!-- Styling -->
<style scoped>

.input-container {
  @apply flex flex-col items-center gap-6 text-center mb-10 w-full max-w-2xl md:max-w-3xl 2xl:max-w-6xl;
}
 
.input {
  @apply w-full rounded-md px-10 py-8 2xl:py-14 text-2xl 2xl:text-4xl;
}
 
.action-button {
  @apply w-full px-8 py-5 text-2xl 2xl:text-4xl text-dark font-bold border-4 border-dark rounded-md bg-white hover:bg-white/80 hover:text-black hover:border-black transition-all duration-150;
}
</style>