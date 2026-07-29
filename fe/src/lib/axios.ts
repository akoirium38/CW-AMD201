import { useAuthStore } from "@/stores/useAuthStore";
import axios from "axios";

const api = axios.create({
    baseURL: import.meta.env.MODE === "development" ? "http://localhost:5186/api" :"/api",
    withCredentials:true,
})

api.interceptors.request.use((config) => {
    const token = useAuthStore.getState().token;
    if (token) {
        config.headers = config.headers ?? {};
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});
export default api;