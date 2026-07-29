import api from '@/lib/axios'


export const authService = {
    authEmail: async (email:string) =>{
        const res = await api.post("/auth/request-otp", {email}, {withCredentials:true});
        if (!res.data.success) {
            throw new Error(res.data.message || "Failed to send OTP");
        }
        return res.data
    },
    authOtp: async (email:string,code:string) =>{
        const res = await api.post("/auth/verify-otp", {email,code}, {withCredentials:true});
        return res.data;
    },

    fetchMe: async () => {
        const res = await api.get("/auth/me", {withCredentials:true});
        return res.data.email;
    },
    logOut: async () => {
        const res = await api.post("/auth/logout", {}, {withCredentials:true});
        return res.data;
    }
};