//TODO: Remove this
export interface ICardRequest {
  email: string
  recoveryCode?: string
  showEmailInput?: boolean
  showCodeInput?: boolean
  showSuccessMessage?: boolean
  showErrorMessage?: boolean
  successMessage?: string
  errorMessage?: string
  loading?: boolean
  handleCodeInput?: boolean
  confirmationMessage?: string
  message?: string
}