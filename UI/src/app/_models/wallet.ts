export interface WalletBalanceDto { amount: number; }
export interface WalletMethodDto { code: string; name: string; type: number; }
export interface WalletTransactionRequest { code: string; amount: number; paymentProperty: string; }
export interface WalletTransactionResponse { paymentStatus: number; }

// Must mirror backend (WasteFree.Domain.Enums.PaymentStatus)
// 0 = Invalid, 1 = Pending, 2 = Completed
export enum PaymentStatus { Invalid = 0, Pending = 1, Completed = 2 }
