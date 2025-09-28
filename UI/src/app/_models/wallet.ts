export interface WalletBalanceDto { amount: number; }
export interface WalletMethodDto { code: string; name: string; type: number; }
export interface WalletTransactionRequest { code: string; amount: number; paymentProperty: string; }
export interface WalletTransactionResponse { paymentStatus: number; }

// 0 = Pending, 1 = Failed, 2 = Success (based on current backend response examples)
export enum PaymentStatus { Pending = 0, Failed = 1, Success = 2 }
