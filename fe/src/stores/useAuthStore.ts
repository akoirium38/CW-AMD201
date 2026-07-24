import {create } from "zustand"
import {toast} from "sonner"
import { authService } from "@/services/authService"
import type { AuthState } from "@/types/store"

export const useAuthStore = create<AuthState>((set,get)=>({
    token:null,
    user:null,
    loading:false,

    authEmail:async (email:string)=>{
        try{
            set({loading:true})
            
            await authService.authEmail(email);

            toast.success("OTP sent to your email!")
            return true;

        } catch (error){
            console.error(error);
            toast.error("Failed to send OTP. Please check your email and try again.")
            return false;
        } finally {
            set({loading:false})
        }
    },

    authOtp:async (email:string,code:string)=>{
        try{
            set({loading:true})
            const {token} = await authService.authOtp(email,code);
            set({token});

            toast.success("Welcome to FileHub🎉")

            await authService.authOtp(email,code);
        } catch(error){
            set({loading:false})
            console.error(error);
    
            toast.error("Failed to verify OTP. Please try again.")
        }
    }
}))