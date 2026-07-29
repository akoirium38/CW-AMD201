

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

    export function useSharedFile(fileId: string | undefined) {
    const { downloadFile } = useFileStore(); 
    
    // Dùng FileRecord thay vì tự định nghĩa FileDetails
    const [fileInfo, setFileInfo] = useState<FileRecord | null>(null);
    const [isLoadingInfo, setIsLoadingInfo] = useState(true);
    const [isDownloading, setIsDownloading] = useState(false);
    const [previewUrl, setPreviewUrl] = useState<string | null>(null);
    const [isLoadingPreview, setIsLoadingPreview] = useState(false);

    useEffect(() => {
        let objectUrl: string | null = null;

        const fetchInfo = async () => {
        if (!fileId) return;
        setIsLoadingInfo(true);
        try {
            const info = await fileService.getFileDetails(fileId);
            setFileInfo(info);


            if (checkIsImage(info.fileName)) {
            setIsLoadingPreview(true);
            const blob = await fileService.downloadFile(fileId);
            objectUrl = window.URL.createObjectURL(new Blob([blob]));
            setPreviewUrl(objectUrl);
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
        
        setIsDownloading(true);
        
        if (previewUrl) {
            const link = document.createElement("a");
            link.href = previewUrl;
            link.setAttribute("download", fileInfo.fileName); 
            document.body.appendChild(link);
            link.click();
            link.parentNode?.removeChild(link);
        } else {
            await downloadFile(fileId, fileInfo.fileName); 
        }
        
        setIsDownloading(false);
    };

    return {
        fileInfo,
        isLoadingInfo,
        isDownloading,
        previewUrl,
        isLoadingPreview,
        isImage: fileInfo ? checkIsImage(fileInfo.fileName) : false,
        handleDownloadClick
    };
}