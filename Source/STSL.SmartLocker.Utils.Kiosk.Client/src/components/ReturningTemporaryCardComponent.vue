<template>
  <div class="form-wrapper gap-3">
    <h2 class="heading">Returning a UCLH Temporary ID card</h2>
    <div v-if="error && tryAgain" class="loadingText">
      <p>{{ error }}</p>
      <button @click="retryProcess" class="secondary-btn">Try Again</button>
    </div>
    <div class="loadingText" v-if="showThankYouMessage">
      <p>Thank you. Your UCLH Temporary ID card has successfully been returned.</p>
    </div>
    <div v-if="!(tryAgain) && !showThankYouMessage" class="loadingText">
      <p>We are unable to process your UCLH Temporary ID card, please contact security.</p>
    </div>
    <div class="loadingText" v-if="!showThankYouMessage && !error && tryAgain">
      <p>Please insert your UCLH Temporary ID card in the 'Card Returns' slot.</p>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import type { ICardRequest } from '../models/CardRequest'
import { useRouter } from 'vue-router';
import { useCardRequestStore } from '@/stores/card-requests';
import { useAuthStore } from '@/stores/auth'
import { ErrorType } from '@/models/EmailDetails'

export default defineComponent({
  name: 'ReturningTemporaryCardComponent',
  setup() {
    const cardRequest = ref<ICardRequest>({
      email: '',
      recoveryCode: '',
      showEmailInput: false,
      showCodeInput: false,
      showSuccessMessage: false,
      showErrorMessage: false,
      successMessage: '',
      errorMessage: '',
      loading: false,
      handleCodeInput: false,
      confirmationMessage: ''
    })
    const router = useRouter();
    const buttonClicked = ref(false)
    const returningCard = ref(false)
    const showThankYouMessage = ref(false)
    const error = ref('')
    const retryCount = ref(0);
    const tryAgain = ref(true);
    const debounceTime = 1000;
    let debounceTimer: any;
    const { getApiAccessToken } = useAuthStore()
    const { returnTemporaryCard, postReportProblem  } = useCardRequestStore();

    onMounted(() => {
      if ((window as any).chrome.webview) {
        (window as any).chrome.webview.addEventListener('message', listenToMessages);
      }
      ListenReturnTemporaryCard();
    })

    onBeforeUnmount(() => {
    if ((window as any).chrome.webview) {
        (window as any).chrome.webview.removeEventListener('message', listenToMessages);
      }
  })

      const listenToMessages = async (message:any) => {
          const data = message.data;
          if (data.success == false) {
            if(data.handler == 'PrinterError'){
              const errorType = ErrorType[data.message as keyof typeof ErrorType];
              await reportError(errorType);
              router.push('/error-view');
            }
            error.value = data.message
            retryCount.value++;
            returningCard.value = false
            return
          }
          if (data.handler == 'ReturnTemporaryCard'){
            try{
              const success = await returnTemporaryCard(data.data.hidNumber, data.data.serialNumber);
              if(success){
                tryAgain.value = false;
                ListenEjectOrMove('Move')
                showThankYouMessage.value = true;
                setTimeout(() => {
                  returnToInitialScreen();
                }, 8000);
              }
              else{
                ListenEjectOrMove('Eject')
                retryCount.value++;
                error.value = 'Failed to update our records. Please try again.';
              }
            } catch(e) {
              error.value = data.message || 'Error in returning card';
              const token = await getApiAccessToken();
              await postReportProblem(token, ErrorType.Other); 
            }
            returningCard.value = false;
          }
        };
      
    const returnToInitialScreen = () => {
      router.push('/');
    }

    const ListenReturnTemporaryCard = () => {
      returningCard.value=true;
        try {
          // Send the request message
          (window as any).chrome.webview.postMessage({
            type: 'request',
            handler: 'ReturnTemporaryCard',
          });
        } catch (e) {
          // Handle any exceptions
          cardRequest.value.errorMessage = 'An error occurred while processing the request.';
          cardRequest.value.showErrorMessage = true;
        }
    };

    const ListenEjectOrMove = (action: string) => {
        try {
          (window as any).chrome.webview.postMessage({
            type: 'request',
            handler: 'EjectOrMove',
            data: {
              action: action
            },
          });
        } catch (e) {
          console.error(e);
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

  watch(retryCount, async (newCount, oldCount) => {
    console.log(`retryCount changed from ${oldCount} to ${newCount}`);
      if (newCount < 3) {
        console.log('Retrying process = ' + newCount);
      } else {
        tryAgain.value = false;
        error.value = "We are unable to process your card, please contact security.";
        setTimeout(() => {
          returnToInitialScreen();
        }, 8000);
      }
    });

    const retryProcess = () => {
     if (retryCount.value < 3) {
        error.value = '';
        ListenReturnTemporaryCard();
      }
    };
  
    return {
      error,
      returningCard,
      buttonClicked,
      showThankYouMessage,
      retryCount,
      tryAgain,
      returnToInitialScreen,
      ListenReturnTemporaryCard,
      ListenEjectOrMove,
      retryProcess,
      listenToMessages
    }
  }
})
</script>
