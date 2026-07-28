import {create } from "zustand";
import {toast} from "sonner";
import { fileService } from "@/services/fileService";
import type { FileState } from "@/types/store";

export const useFileStore = create<FileState>((set, get) => ({
    files: [],
    loading: false,
    uploading: false,
    uploadProgress: 0,

    fetchMyFiles: async () => {
        try {
            set({ loading: true });
            const files = await fileService.fetchMyFiles();
            set({ files });
        } catch (error) {
            console.error("Failed to fetch files:", error);
            toast.error("Failed to load your files. Please try again.");
        } finally {
            set({ loading: false });
        }
    },

    uploadFile: async (file: File, options) => {
        try {
            set({ uploading: true, uploadProgress: 0 });
            
            await fileService.uploadFile(file, options, (progress) => {
                set({ uploadProgress: progress });
            });


            await get().fetchMyFiles();
            
            toast.success("File uploaded successfully! 🎉");
            return true;
        } catch (error) {
            console.error("Failed to upload file:", error);
            toast.error("Failed to upload file. Please try again.");
            return false;
        } finally {
            setTimeout(() => {
                set({ uploading: false, uploadProgress: 0 });
            }, 500);
        }
    },

    downloadFile: async (fileId: string, filename: string) => {
        try {
            toast.success("Preparing your download...");
            const blobData = await fileService.downloadFile(fileId);
            
            const url = window.URL.createObjectURL(new Blob([blobData]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', filename);
            document.body.appendChild(link);
            link.click();
            link.parentNode?.removeChild(link);
            window.URL.revokeObjectURL(url);

            return true;
        } catch (error) {
            console.error("Failed to download file:", error);
            toast.error("Failed to download file. It might be expired or protected.");
            return false;
        }
    },

    deleteFile: async (fileId: string) => {
        try {
            set({ loading: true });
            await fileService.deleteFile(fileId);
            
            // Cập nhật lại UI ngay lập tức bằng cách lọc bỏ file bị xóa
            set((state) => ({
                files: state.files.filter((f) => f.fileId !== fileId)
            }));

            toast.success("File deleted successfully!");
            return true;
        } catch (error) {
            console.error("Failed to delete file:", error);
            toast.error("Failed to delete file. Please try again.");
            return false;
        } finally {
            set({ loading: false });
        }
    }
}));