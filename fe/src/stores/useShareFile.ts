
// src/hooks/useSharedFile.ts
import { useEffect, useState } from "react";
import { toast } from "sonner";
import { fileService } from "@/services/fileService";
import { useFileStore } from "@/stores/useFileStore";
import type { FileRecord } from "@/types/file"; 

export const checkIsImage = (fileName: string) => {
    if (!fileName) return false;
    const ext = fileName.split('.').pop()?.toLowerCase();
    return ['jpg', 'jpeg', 'png', 'gif', 'webp'].includes(ext || '');
};

const formatSizeInMB = (sizeInBytes: number) => {
    if (!sizeInBytes || Number.isNaN(sizeInBytes)) return "0.00";
    return (sizeInBytes / (1024 * 1024)).toFixed(2);
};

export function useSharedFile(fileId: string | undefined) {
    const { downloadFile } = useFileStore(); 
    
    const [fileInfo, setFileInfo] = useState<FileRecord | null>(null);
    const [isLoadingInfo, setIsLoadingInfo] = useState(true);
    const [isDownloading, setIsDownloading] = useState(false);
    const [previewUrl, setPreviewUrl] = useState<string | null>(null);
    const [isLoadingPreview, setIsLoadingPreview] = useState(false);
    const [password, setPassword] = useState("");
    const [passwordError, setPasswordError] = useState("");
    const [isCheckingPassword, setIsCheckingPassword] = useState(false);
    const [isPasswordVerified, setIsPasswordVerified] = useState(false);

    useEffect(() => {
        let objectUrl: string | null = null;

        const fetchInfo = async () => {
            if (!fileId) return;
            setIsLoadingInfo(true);
            setPreviewUrl(null);
            setPassword("");
            setPasswordError("");
            setIsPasswordVerified(false);
            try {
                const info = await fileService.getFileDetails(fileId);
                setFileInfo(info);

                if (!info.hasPassword || isPasswordVerified) {
                    if (checkIsImage(info.fileName)) {
                        setIsLoadingPreview(true);
                        const blob = await fileService.downloadFile(fileId);
                        objectUrl = window.URL.createObjectURL(new Blob([blob]));
                        setPreviewUrl(objectUrl);
                    }
                }
            } catch (error) {
                console.error("Lỗi khi tải chi tiết file:", error);
                toast.error("Đường dẫn không tồn tại hoặc bạn không có quyền truy cập.");
            } finally {
                setIsLoadingPreview(false);
                setIsLoadingInfo(false);
            }
        };

        fetchInfo();

        return () => {
            if (objectUrl) window.URL.revokeObjectURL(objectUrl);
        };
    }, [fileId]);

    const handleDownloadClick = async () => {
        if (!fileId || !fileInfo) return;

        setPasswordError("");

        if (fileInfo.hasPassword) {
            if (!password.trim()) {
                setPasswordError("Vui lòng nhập mật khẩu.");
                return;
            }

            if (!isPasswordVerified) {
                setIsCheckingPassword(true);
                try {
                    const result = await fileService.verifyPassword(fileId, password);
                    if (!result?.isSuccess) {
                        setPasswordError(result?.message || "Mật khẩu không đúng.");
                        return;
                    }
                    setIsPasswordVerified(true);
                    toast.success("Mật khẩu đúng.");
                } catch (error) {
                    console.error("Lỗi khi xác minh mật khẩu:", error);
                    setPasswordError("Không thể kiểm tra mật khẩu. Vui lòng thử lại.");
                    return;
                } finally {
                    setIsCheckingPassword(false);
                }
            }
        }
        
        setIsDownloading(true);
        try {
            if (previewUrl) {
                const link = document.createElement("a");
                link.href = previewUrl;
                link.setAttribute("download", fileInfo.fileName); 
                document.body.appendChild(link);
                link.click();
                link.parentNode?.removeChild(link);
            } else {
                await downloadFile(fileId, fileInfo.fileName, fileInfo.hasPassword ? password : undefined); 
            }
        } catch (error) {
            console.error("Lỗi khi tải file:", error);
            toast.error("Tải xuống thất bại. Tệp có thể đã hết hạn hoặc bị chặn.");
        } finally {
            setIsDownloading(false);
        }
    };

    return {
        fileInfo,
        isLoadingInfo,
        isDownloading,
        previewUrl,
        isLoadingPreview,
        isImage: fileInfo ? checkIsImage(fileInfo.fileName) : false,
        fileSizeMB: fileInfo ? formatSizeInMB(fileInfo.size) : "0.00",
        password,
        setPassword,
        passwordError,
        setPasswordError,
        isCheckingPassword,
        isPasswordVerified,
        handleDownloadClick
    };
}