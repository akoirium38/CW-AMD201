import type { User } from "./user";

export interface AuthState {
    token: string | null;
    email: string | null;
    loading:boolean;

    authEmail:(email:string) => Promise<boolean> 

    authOtp:(email:string, code:string) => Promise<boolean>

    fetchMe: () => Promise<void>
}