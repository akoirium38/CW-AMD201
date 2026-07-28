import axios from "axios";

const api = axios.create({
    baseURL: import.meta.env.MODE === "development" ? "https://localhost:7000/api" :"/api",
    withCredentials:true,
})
export default api;