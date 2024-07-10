export function validateEmail(email: string)  {
    const emailPattern = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/;
    return emailPattern.test(email);
}

export function validateRecoveryCode(code: string) {
    return /^[a-zA-Z0-9]+$/.test(code);
}