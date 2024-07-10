<!-- Component Definition -->
<script setup lang="ts">
import { ref, toRefs } from 'vue';
import { useRouter } from 'vue-router';
import { useCardRequestStore } from '@/stores/card-requests';
import EmailInput from '@/components/EmailInputComponent.vue';
import CodeInput from '@/components/CodeInputComponent.vue';
import Loading from 'vue-loading-overlay';

const router = useRouter();
const { sendRecoveryCodeRequest, submitAccessCode, createTemporaryCard } = useCardRequestStore();

const { currentStep, isLoading } = toRefs({
  currentStep: ref('EmailStep'),
  isLoading: ref(false),
});

const submitEmail = async () => {
  try {
    isLoading.value = true;
    await sendRecoveryCodeRequest();
    isLoading.value = false;
  
    currentStep.value = 'CodeStep';
  } catch (error) {
    console.error(error)
    isLoading.value = false;
  }
};

const submitRecoveryCode = async () => {
  try {
    isLoading.value = true;
    await submitAccessCode();
    isLoading.value = false

    //TODO: Move this function call to be a part of the print card process 
    //await createTemporaryCard();
    
    router.push({name: 'printCard'})
  } catch (error) {
    console.error("error in component:", error)
    isLoading.value = false;
  }
}
</script>

<!-- Template -->
<template>
<div>
  <div class="wrapper">
    <EmailInput 
      v-if="currentStep === 'EmailStep'"
      @send-recovery-code="() => submitEmail()"
      @received-recovery-code="() => currentStep = 'CodeStep'"
    />

    <CodeInput
      v-if="currentStep === 'CodeStep'"
      @submit-recovery-code="() => submitRecoveryCode()"
      @does-not-have-recovery-code="() => currentStep = 'EmailStep'"
    />
    
  </div>
  <!-- Loading Component -->
  <Loading v-model:active="isLoading"
    :loader='"dots"'
    :color="'#0180cc'"
    :background-color="'black'"
    :is-full-page="false"/>
</div>
</template>

<!-- Styling -->
<style scoped>
.wrapper {
  @apply flex 
  flex-col 
  items-center 
  text-center w-full max-w-2xl 2xl:max-w-5xl;
}

.forgot-button {
  @apply 
  bg-offwhite 
  px-5 py-3 
  rounded-3xl 
  border-dark 
  text-gray
  hover:bg-white 
  font-semibold;

  font-size: 3rem;
  border-width: 0.2em;
  height: 4em;
}

.input-container {
  @apply 
  flex 
  flex-col 
  items-center 
  text-center 
  mb-4;

  width: 40em;
}

.input {
  @apply 
  flex 
  flex-col 
  items-center 
  text-gray 
  bg-offwhite 
  w-full 
  rounded-3xl 
  focus:outline-none
  text-4xl
  h-20
  text-center;
}

.action-button {
  @apply 
  bg-offwhite
  px-5 py-5 
  rounded-3xl 
  border-4 
  border-dark 
  text-gray 
  hover:bg-white 
  text-2xl 
  font-semibold 
  w-full;
}
</style>
