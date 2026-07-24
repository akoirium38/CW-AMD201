import api from '@/lib/axios'


export const authService = {
    authEmail: async (email:string) =>{
        const res = await api.post("/auth/request-otp", {email});
        if (!res.data.success) {
            throw new Error(res.data.message || "Failed to send OTP");
        }
        return res.data
    },
    authOtp: async (email:string,code:string) =>{
        const res = await api.post("/auth/verify-otp", {email,code});
        return res.data;
    }
};