import { ref } from 'vue'
import { defineStore } from 'pinia'
import { jwtDecode } from 'jwt-decode'
import type { EntityId } from '@/models/Common'
import { useAxiosStore } from './axios'

const claimsTenant = "https://goto-secure.stsl.co.uk/tenant"
export const useAuthStore = defineStore('auth', () => {

  const { get } = useAxiosStore();

  // Single use variables (reset per use)
  const blankAccessToken: string = ''
  const currentAccessToken = ref(blankAccessToken)
  
  const isAuthenticatedDefault: boolean = false;
  const isAuthenticated = ref(isAuthenticatedDefault);

  /* Exported Functions */

  // Get API access token
  async function getApiAccessToken(): Promise<string> {

    if (!currentAccessToken.value) {
      await getNewAccessToken();
    } else {
      const decoded = await getDecodedToken();
      const timeRemaining = decoded.exp - Math.floor(Date.now() / 1000);

      if (timeRemaining <= 300) {
        console.log("Token is about to expire. Requesting a new token.");
        await getNewAccessToken();
      }
    }

    return currentAccessToken.value;
  }

  // Get TenantId from decoded token
  /*async function getTenant(): Promise<EntityId> {
    const token = await getDecodedToken()
    const tenant = token[claimsTenant] as EntityId
    return tenant;
  }*/

  // Send authentication request API / Service Bus / VWS API
  async function authenticateKiosk() {
    try {
      const response = await getAuthenticationResponse(await getApiAccessToken())
      isAuthenticated.value = response;
    } catch(error) {
      isAuthenticated.value = false;
    }
  }
  
  /* Private Functions */

  // Get Token from wrapper
  async function getNewAccessToken(): Promise<void> {
    return new Promise((resolve, reject) => {

      // Handle response
      const messageHandler = (message: any) => {
        try {
          currentAccessToken.value = message.data.data;
          resolve();
        } catch (error) {
          console.error("Error recieving access token", error);
          reject(error);
        } finally {
          (window as any).chrome.webview.removeEventListener('message', messageHandler);
        }
      };
  
      // Add event listener
      (window as any).chrome.webview.addEventListener('message', messageHandler);
  
      // Post message
      try {
        (window as any).chrome.webview.postMessage({
          type: 'request',
          handler: 'ApiAccessToken',
          async: true,
        });
      } catch (error) {
        console.log("Error posting message", error);
        reject(error);
      }
    });
  }

  // Decode access token
  async function getDecodedToken(): Promise<any> {
    return jwtDecode(currentAccessToken.value);
  }

  async function getAuthenticationResponse(
    token: string): Promise<boolean> {
      const response = await get<any>(token, 'kiosks/authenticate');
      return response.status === 200 ? true : false;
  }

  return {
    getApiAccessToken,
    //getTenant,
    authenticateKiosk,
    isAuthenticated
  }
})