import { defineStore } from 'pinia'
import axios, { type AxiosRequestConfig, type AxiosError } from 'axios'
import type { EntityId } from '@/models/Common';
import { useToast } from 'vue-toastification';

export const useAxiosStore = defineStore('axios', () => {

  const apiURL = import.meta.env.VITE_API_URL

  const toast = useToast();

  /* Exported Functions */

  /* Get */
  async function get<T>(token: string, path: string, data?:any) {
    const config: AxiosRequestConfig = {
      headers: { Authorization: `Bearer ${token}` },
    }

    let url = `${apiURL}`
  
    if (path) url += `/${path}`;
    if (data?.code) url += `?code=${data.code}`;
  
    return axios.get<T>(url, config);
  }

  /* Post */
  async function post<T>(token: string, data: any, path?: string) {
    const config: AxiosRequestConfig = {
      headers: { Authorization: `Bearer ${token}`,"Content-Type": "application/json" }
    }

    let url =  `${apiURL}`
    if (path) url += `/${path}`;

    return axios.post<T>(url, data, config)
  }

  /* Non-Exported Functions */
  axios.interceptors.response.use(
    response => response, // forward successful responses
    (error: AxiosError) => {
      if(error.response?.status === 404)
      {
        reportApiError(error);
      }
      else if(error.response?.status === 409)
      {
        reportApiError(error);
      }
      else if(error.response?.status === 422)
      {
        reportApiError(error);
      }
      else
      {
        reportApiError(error, "Something went wrong, please try again")
      }
      console.error('API Error:', error.response?.status, error.response?.data);
      return Promise.reject(error); // Re-throw the error
    }
  )

  function reportApiError(error: AxiosError, context?: string) {
    let message: string;
    if(context)
    {
      message = `${error.response?.status}: ${context}`
    } else {
      message = `${error.response?.status}: ${error.response?.data}`
    }
    RejectCard();
    toast.error(message)
  }

  function RejectCard() {
    try{
      (window as any).chrome.webview.postMessage({
        type: 'request',
        handler: 'RejectCard'
      });
    } catch {
      console.error('Failed to reject card');
    }}

  return {
    post,
    get
  }
});