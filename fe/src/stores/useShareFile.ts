
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
    const [previewIsThumbnail, setPreviewIsThumbnail] = useState(false);
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
            setPreviewIsThumbnail(false);
            setPassword("");
            setPasswordError("");
            setIsPasswordVerified(false);
            try {
                    const info = await fileService.getFileDetails(fileId);
                    console.debug("useSharedFile: file details:", info);
                    setFileInfo(info);

                if (checkIsImage(info.fileName)) {
                    if (info.thumbnailUrl) {
                        // Try to fetch thumbnail as blob and create object URL so it reliably displays
                        try {
                            setIsLoadingPreview(true);
                            const thumbBlob = await fileService.getThumbnail(fileId);
                            objectUrl = window.URL.createObjectURL(thumbBlob);
                            setPreviewUrl(objectUrl);
                            setPreviewIsThumbnail(true);
                        } catch (err) {
                            console.warn("Failed to fetch thumbnail blob, falling back to thumbnailUrl", err);
                            setPreviewUrl(info.thumbnailUrl);
                            setPreviewIsThumbnail(true);
                        } finally {
                            setIsLoadingPreview(false);
                        }
                    } else if (!info.hasPassword || isPasswordVerified) {
                        setIsLoadingPreview(true);
                        try {
                            const blob = await fileService.getThumbnail(fileId);
                            objectUrl = window.URL.createObjectURL(blob);
                            setPreviewUrl(objectUrl);
                            setPreviewIsThumbnail(true);
                        } catch (thumbnailError) {
                            console.warn("Thumbnail not available, falling back to full image preview", thumbnailError);
                            const blob = await fileService.downloadFile(fileId, info.hasPassword ? password : undefined);
                            objectUrl = window.URL.createObjectURL(new Blob([blob]));
                            setPreviewUrl(objectUrl);
                            setPreviewIsThumbnail(false);
                        }
                    }
                }
            } catch (error) {
                console.error("Error fetching file info:", error);
                toast.error("This file may have expired or been blocked.");
            } finally {
                setIsLoadingPreview(false);
                setIsLoadingInfo(false);
            }
        };

        fetchInfo();

        return () => {
            if (objectUrl) window.URL.revokeObjectURL(objectUrl);
        };
    }, [fileId, isPasswordVerified]);

    const handleDownloadClick = async () => {
        if (!fileId || !fileInfo) return;

        setPasswordError("");

        if (fileInfo.hasPassword) {
            if (!password.trim()) {
                setPasswordError("Please enter the password.");
                return;
            }

            if (!isPasswordVerified) {
                setIsCheckingPassword(true);
                try {
                    const result = await fileService.verifyPassword(fileId, password);
                    if (!result?.isSuccess) {
                        setPasswordError(result?.message || "Incorrect password.");
                        return;
                    }
                    setIsPasswordVerified(true);
                    toast.success("Password is correct.");
                } catch (error) {
                    console.error("Error verifying password:", error);
                    setPasswordError("Unable to verify password. Please try again.");
                    return;
                } finally {
                    setIsCheckingPassword(false);
                }
            }
        }
        
        setIsDownloading(true);
        try {
            if (previewUrl && !previewIsThumbnail) {
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
            console.error("Error downloading file:", error);
            toast.error("Download failed. The file may have expired or been blocked.");
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
        previewIsThumbnail,
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