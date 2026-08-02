import { useAuthStore } from "@/stores/useAuthStore";
import axios from "axios";

const api = axios.create({
    baseURL: "/api",
    withCredentials: true,
});

api.interceptors.request.use((config) => {
    const token = useAuthStore.getState().token;
    if (token) {
        const headers = config.headers ?? {};

        if (typeof (headers as any).set === "function") {
            (headers as any).set("Authorization", `Bearer ${token}`);
        } else {
            (headers as Record<string, string>).Authorization = `Bearer ${token}`;
        }

        config.headers = headers;
    }

    return config;
});

export default api;