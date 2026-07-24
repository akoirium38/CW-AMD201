import api from '@/lib/axios'


export const authService = {
    authEmail: async (email:string) =>{
        const res = await api.post("/auth/request-otp", {email}, {withCredentials:true});
        return res.data
    },
    authOtp: async (email:string,code:string) =>{
        const res = await api.post("/auth/verify-otp", {email,code},{withCredentials:true});
        return res.data
    }
};