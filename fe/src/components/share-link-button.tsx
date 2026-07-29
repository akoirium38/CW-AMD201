// src/components/ShareLinkButton.tsx
import { Share2 } from "lucide-react";
import { useFileStore } from "@/stores/useFileStore";

interface ShareLinkButtonProps {
    fileId: string;
    className?: string; // Cho phép ghi đè/thêm CSS từ bên ngoài
    text?: string;      // Tùy chọn thay đổi chữ (mặc định là "Share")
    }

    export function ShareLinkButton({ 
    fileId, 
    className = "", 
    text = "Share" 
    }: ShareLinkButtonProps) {
    
    return (
        <button
        onClick={() => useFileStore.getState().copyShareLink(fileId)}
        className={`inline-flex items-center gap-2 rounded-xl border border-blue-200 px-3 py-2 text-sm font-medium text-blue-600 transition-colors hover:bg-blue-50 ${className}`}
        title="Sao chép link chia sẻ"
        >
        <Share2 className="h-4 w-4" /> 
        {text}
        </button>
    );
}