import { ref, computed } from 'vue'
import { defineStore, storeToRefs } from 'pinia'
import { useToast } from 'vue-toastification'
import { useAuthStore } from './auth'
import { CardType, type EntityId } from '@/models/Common';
import type { IAccessCode, IKioskAccessCode } from '@/models/KioskAccessCode';
import  { validateEmail, validateRecoveryCode} from '@/services/utils';
import type { ICreateAccessCodeRequestDTO } from '@/models/AccessCodeRequest'
import type { ICreateTemporaryCardCredential, IReturnTemporaryCardCredential } from '@/models/TemporaryCardCredential'
import { useAxiosStore } from './axios'
import { useRouter } from 'vue-router';
import { ErrorType } from '@/models/EmailDetails';

export const useCardRequestStore = defineStore('card-requests', () => {
    const { getApiAccessToken } = useAuthStore()
    const { isAuthenticated } = storeToRefs(useAuthStore())
    const { post } = useAxiosStore()

    const router = useRouter();
    const toast = useToast()

    // Single use variables (reset per use)
    const blankEmail: string = ''
    const currentEmail = ref(blankEmail)

    const blankRecoveryCode: IAccessCode = { code: '' }
    const currentRecoveryCode = ref(structuredClone(blankRecoveryCode))

    const blankCardHolderId: EntityId = ''
    const currentCardHolderId = ref(blankCardHolderId)

    /* Exported functions */

    async function sendRecoveryCodeRequest()
    {
      if (!isAuthenticated.value)
      {
        handleUnauthenticated();
        return;
      }

      const recoveryCodeRequest: ICreateAccessCodeRequestDTO = {
        email: currentEmail.value,
        requestDateTime: new Date()
      }

      await postRecoveryCodeRequest(await getApiAccessToken(), recoveryCodeRequest)
      displaySuccess('A code has been sent to your email, it may take a few moments to appear')
    }
    
    async function submitAccessCode()
    {
      if (!isAuthenticated.value)
      {
        handleUnauthenticated();
        return;
      }

      const accessCode = await postAccessCode(await getApiAccessToken(), currentRecoveryCode.value)
      // Set CardHolderId which we need to send a Temporary Card request
      currentCardHolderId.value = accessCode.cardHolderId;
    }

    async function createTemporaryCard(hidNumber: string, serialNumber: string)
    {
      if (!isAuthenticated.value)
      {
        handleUnauthenticated();
        return false;
      }

      const temporaryCard: ICreateTemporaryCardCredential = {
        serialNumber: serialNumber,
        hidNumber: hidNumber,
        cardType: CardType.Temporary,
        cardHolderId: currentCardHolderId.value,
        cardLabel: "Temporary " + hidNumber
      }
    try {
      console.log(serialNumber, hidNumber, temporaryCard);
      await postTemporaryCard(await getApiAccessToken(), temporaryCard)
      displaySuccess('Access Control system successully updated') // remove this
      return true;
    } catch (error) {
      displayError('Failed to prepare the card.');
      console.error(error);
      return false;
    }
    }

    async function returnTemporaryCard(hidNumber: string, serialNumber: string)
    {
      if (!isAuthenticated.value)
      {
        handleUnauthenticated();
        return;
      }

      const temporaryCard: IReturnTemporaryCardCredential = {
        serialNumber: serialNumber, 
        hidNumber: hidNumber,
        cardType: CardType.Temporary,
        cardHolderId: undefined,
        cardLabel: "Temporary " + hidNumber
      }

      try {
        await postReturnTemporaryCard(await getApiAccessToken(), temporaryCard)
        displaySuccess('Temporary card returned successfully') // remove this
        return true;
      } catch (error) {
        displayError('Failed to return the card. Please try again.');
        console.error(error);
        return false;
      }
    }

    // Clear reference variables
    function reset() {
      currentEmail.value = blankEmail;
      currentRecoveryCode.value = { ...blankRecoveryCode };
      currentCardHolderId.value = blankCardHolderId;
    }

    /* Validators */
    const isValidEmail = computed(() => {
      return (currentEmail.value.length > 1 && validateEmail(currentEmail.value));
    })

    const isValidRecoveryCode = computed(() =>  {
      return (currentRecoveryCode.value.code.length > 1 && validateRecoveryCode(currentRecoveryCode.value.code));
    })

    /* Non-exported Functions */

    async function postRecoveryCodeRequest(
      token: string,
      codeRequest: ICreateAccessCodeRequestDTO): Promise<any> {
        const response = await post<any>(token, codeRequest, 'kiosks/access-code-request')
        return response.data;
    }

    async function postReportProblem(token: string, errorType: ErrorType): Promise<any> {
      try{
        const response = await post<any>(token, errorType, 'kiosks/report-problem');
        return response.data;
      } catch (error) {
      console.error('Error reporting problem:', error);
      throw error;
      }
    }

    async function postTemporaryCard(
      token: string,
      temporaryCard: ICreateTemporaryCardCredential): Promise<any> {
        const response = await post<any>(token, temporaryCard, 'kiosks/card-request')
        return response.data;
    }

    async function postReturnTemporaryCard(
      token: string,
      temporaryCard: IReturnTemporaryCardCredential): Promise<any> {
        const response = await post<any>(token, temporaryCard, 'kiosks/return-card')
        return response.data;
    }

    async function postAccessCode(
      token: string,
      code: IAccessCode): Promise<IKioskAccessCode> {
        const response = await post<IKioskAccessCode>(token, code, 'kiosks/post-access-code')
        return response.data;
    }

    function handleUnauthenticated()
    {
      displayUnauthenticatedError();
      router.push({name: 'home'});
    }

    function displaySuccess(context: string) {
      toast.success(context);
    }

    function displayError(context: string) {
      toast.error(context);
    }
    
    function displayUnauthenticatedError() {
      toast.error("Kiosk is not connected to required services");
    }

    return {
      isValidEmail,
      isValidRecoveryCode,
      sendRecoveryCodeRequest,
      submitAccessCode,
      createTemporaryCard,
      returnTemporaryCard,
      postReportProblem,
      currentEmail,
      currentRecoveryCode,
      reset
    }
});