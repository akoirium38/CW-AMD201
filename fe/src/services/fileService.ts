import api from "@/lib/axios"
import type { FileRecord } from "@/types/file"

export const fileService = {
    uploadFile: async (
        file: File,
        options?: {
            expiryDate?: string;
            downloadLimit?: number;
            password?: string;
        },
        onProgress?: (progress: number) =>void
    ) => {
        const formData = new FormData();
        formData.append("file", file);

        if (options?.expiryDate) formData.append("expiryDate", options.expiryDate);
        if (options?.downloadLimit) formData.append("downloadLimit", options.downloadLimit.toString());
        if (options?.password) formData.append("password", options.password);

        const res = await api.post("/files/upload", formData, {
            headers:{
                "Content-Type":"multipart/form-data",
            },
            withCredentials: true,
            onUploadProgress: (progressEvent) =>{
                if (progressEvent.total && onProgress){
                    const percentCompleted = Math.round((progressEvent.loaded * 100)/ progressEvent.total);
                    onProgress(percentCompleted);
                }
            },

        });

        return res.data;
    },

    downloadFile: async (fileId: string, password?: string) =>{
        const query = password ? `?password=${encodeURIComponent(password)}` : "";
        const res = await api.get(`/files/${fileId}/download${query}`, {responseType:"blob",});
        return res.data;
    },

    verifyPassword: async (fileId: string, password: string) => {
        const res = await api.post(`/files/${fileId}/verify-password`, { password }, { withCredentials:true });
        return res.data;
    },

    deleteFile: async (fileId: string) => {
        const res = await api.delete(`/files/${fileId}`, {withCredentials:true});

        return res.data;

    },

    fetchMyFiles: async () => {
        const res = await api.get("/files/my-files",{withCredentials:true});

        return res.data as FileRecord[];
    },
    getFileDetails: async (fileId: string) => {
        const res = await api.get(`/files/${fileId}`, {withCredentials:true});
        return res.data as FileRecord;
    },

    getThumbnail: async (fileId: string) => {
        const res = await api.get(`/files/${fileId}/thumbnail`, {
            responseType: "blob",
            withCredentials: true,
        });
        return res.data as Blob;
    }
};