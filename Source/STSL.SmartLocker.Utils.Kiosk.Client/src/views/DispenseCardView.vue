<template>
  <div class="wrapper">
    <h1 class="heading" v-if="isLoading">Dispensing UCLH Temporary ID card...</h1>
    <div class="loading-container" v-if="isLoading">
      <div class="loading-spinner"></div>
    </div>
        <!-- Error Handling -->
    <div v-if="error">
      <h2 class="sub-heading">We are unable to dispense your UCLH Temporary ID card</h2>
      <div class="content">
        <span> We are unable to process your UCLH Temporary ID card, please contact security.</span><br>
        <span> We are returning you to the home screen</span><br>
      </div>
    </div>
      <template v-else>
        <h2 class="sub-heading" v-if="!isLoading">Please take your UCLH Temporary ID card below</h2>
        <div class="content">
            <span> Please wait while we dispense your temporary card.</span><br>
            <span> Your temporary card will grant access for 10 days before being disabled.</span><br>
            <span> Make sure to find or replace your orignal UCLH card and return this UCLH Temporary ID card within 
                <bold>10 Days</bold>. 
            </span>
        </div>
    </template>
  </div>
</template>

<script lang="ts">
import { defineComponent, onBeforeUnmount, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useCardRequestStore } from '@/stores/card-requests'
import { ErrorType } from '@/models/EmailDetails'
import { useAuthStore } from '@/stores/auth'

export default defineComponent({
  name: 'DispenseCardView',
  setup() {


  const router = useRouter()
  const { createTemporaryCard, postReportProblem } = useCardRequestStore()
  const { getApiAccessToken } = useAuthStore()
  const isLoading = ref(true)
  const error = ref('')
  const debounceTime = 1000;
  let debounceTimer: any;

  onMounted(() => {
      isLoading.value = true
      if ((window as any).chrome.webview) {
        (window as any).chrome.webview.addEventListener('message', listenToMessages);
      }
      ReadyCardWithDetails();

  })

  onBeforeUnmount(() => {
    if ((window as any).chrome.webview) {
        (window as any).chrome.webview.removeEventListener('message', listenToMessages);
      }
  })

  const listenToMessages = async (message:any) => {
      const data = message.data;
      if (data.success === false) {
        if(data.handler === 'PrinterError'){
          const errorType = ErrorType[data.message as keyof typeof ErrorType];
          await reportError(errorType);
          router.push('/error-view');
            }
        error.value = data.message || 'Error in dispensing card.';
        isLoading.value = false;
        setTimeout(() => {
            returnToInitialScreen();
        }, 5000); // 5 seconds before redirect
        return;
      }
      isLoading.value = false;
      if (data.handler === 'ReadyCard') {
        try {
          const success = await createTemporaryCard(data.data.hidNumber, data.data.serialNumber);
          if (success) {
            dispenseCard();
            setTimeout(() => {
                returnToInitialScreen();
            }, 7000);
          } else {
            error.value = 'Error creating temporary card';
            setTimeout(() => {
            returnToInitialScreen();
        }, 8000);
          }
        } catch (e) {
          console.error(e);
          error.value = data.message || 'Error in dispensing card.';
          setTimeout(() => {
            returnToInitialScreen();
        }, 8000);
        }
      }
    };

const reportError = async (errorType: ErrorType) => {
      clearTimeout(debounceTimer);
      debounceTimer = setTimeout(async () => {
      try {
        console.log(`Reporting error of type: ${errorType}`);
        await postReportProblem(await getApiAccessToken(), errorType);
      } catch (e) {
        console.error('Failed to report error to the backend', e);
      }
    }, debounceTime);
  };

  
function dispenseCard(){
  try{
    (window as any).chrome.webview.postMessage({
          type: 'request',
          handler: 'DispenseCard',
        })
  }
  catch(e){
    console.error(e)
  }
}

function ReadyCardWithDetails() {
    try{
      (window as any).chrome.webview.postMessage({
          type: 'request',
          handler: 'ReadyCard',
        })
    }
    catch(e){
      console.error(e);
    }
}

const returnToInitialScreen = () => {
      router.push('/');
    }

return {
    isLoading,
    error,
    returnToInitialScreen,

  };
  },
});

</script>


<!-- Styling -->
<style scoped>
.wrapper {
  @apply flex 
  flex-col 
  items-center 
  gap-10
  text-center;
}

.heading {
  @apply text-4xl 
  text-white;
}
.sub-heading {
  @apply text-3xl 
  text-white ;
}

.content {
  @apply text-center 
  text-2xl 
  text-white;
}

.loading-container {
  text-align: center;
}

.loading-spinner {
  border: 8px solid #f3f3f3;
  border-top: 8px solid #3498db;
  border-radius: 50%;
  width: 50px;
  height: 50px;
  animation: spin 1s linear infinite;
  margin-bottom: 10px;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}
</style>