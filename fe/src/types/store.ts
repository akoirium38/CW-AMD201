import type { User } from "./user";

export interface AuthState {
    token: string | null;
    user: User | null;
    loading:boolean;

    authEmail:(email:string) => Promise<void> 

    authOtp:(email:string, code:string) => Promise<void>
}