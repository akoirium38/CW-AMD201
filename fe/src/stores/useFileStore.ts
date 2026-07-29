import {create } from "zustand";
import {toast} from "sonner";
import { fileService } from "@/services/fileService";
import type { FileState } from "@/types/store";
import type { FileRecord } from "@/types/file";

export const useFileStore = create<FileState>((set, get) => ({
    files: [],
    loading: false,
    uploading: false,
    uploadProgress: 0,

    fetchMyFiles: async () => {
        try {
            set({ loading: true });
            const response = await fileService.fetchMyFiles();
            const files = Array.isArray(response) ? response : [];
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
            
            set((state) => ({
                files: Array.isArray(state.files) ? state.files.filter((f) => f.fileId !== fileId) : []
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
    },

    updateFile: async (fileId, payload) => {
        try {
            set({ loading: true });
            await (fileService as any).updateFile(fileId, payload);

            // Cập nhật ngay trong danh sách hiện có, khỏi phải gọi lại fetchMyFiles
            set((state) => ({
                files: Array.isArray(state.files)
                    ? state.files.map((f): FileRecord =>
                            f.fileId === fileId
                                ? ({
                                    ...f,
                                    downloadLimit: payload.downloadLimit,
                                    expiryDate: payload.expiryDate ?? null,
                                    hasPassword: payload.password ? true : f.hasPassword,
                                } as FileRecord)
                                : f
                        )
                    : []
            }));

            toast.success("Cập nhật cài đặt thành công!");
            return true;
        } catch (error) {
            console.error("Failed to update file:", error);
            toast.error("Cập nhật thất bại. Vui lòng thử lại.");
            return false;
        } finally {
            set({ loading: false });
        }
    },

    copyShareLink: (fileId: string) => {
        if (!fileId) return;
        
        const link = `${window.location.origin}/share/${fileId}`;
        
        navigator.clipboard.writeText(link)
            .then(() => {
                toast.success("Đã sao chép link chia sẻ! 🔗");
            })
            .catch((err) => {
                console.error("Failed to copy link:", err);
                toast.error("Không thể sao chép link, vui lòng thử lại.");
            });
    }
}));