import { useState, useCallback } from "react";
import { useNavigate } from "react-router";
import { useDropzone, type FileRejection } from "react-dropzone";
import { UploadCloud, File as FileIcon, Lock, Clock, Download } from "lucide-react";
import { useFileStore } from "@/stores/useFileStore";
import { toast } from "sonner";
import { Progress } from "@/components/ui/progress";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
    } from "@/components/ui/select";

    const MAX_FILE_SIZE_BYTES = 10 * 1024 * 1024;
    export function FileUpload() {
    // Trạng thái từ file upload cũ
    const { uploadFile, uploading, uploadProgress } = useFileStore();

    // Trạng thái cục bộ cho file và options
    const [file, setFile] = useState<File | null>(null);
    const [password, setPassword] = useState<string|null>();
// Thêm rõ kiểu <string | null> cho useState
    const [expiry, setExpiry] = useState<string | null>("7");
    const [limit, setLimit] = useState<string | null>("0");

    // Chỉ lưu file vào state khi thả vào, không upload ngay
    const onDrop = useCallback((acceptedFiles: File[]) => {
        if (acceptedFiles[0]) {
        setFile(acceptedFiles[0]);
        }
    }, []);

    const onDropRejected = useCallback((fileRejections: FileRejection[]) => {
        const error = fileRejections[0]?.errors[0];

        if (error?.code === "file-too-large") {
        toast.error("File vượt quá 10MB");
        } else {
        toast.error("File không hợp lệ");
        }
    }, []);

    const { getRootProps, getInputProps, isDragActive } = useDropzone({
        onDrop,
        onDropRejected,
        maxFiles: 1,
        maxSize: MAX_FILE_SIZE_BYTES,
        disabled: uploading,
    });

    const navigate = useNavigate();

    // Hàm xử lý khi bấm nút Tải Lên
    const handleUpload = async () => {
        if (!file) return;

        if (file.size > MAX_FILE_SIZE_BYTES) {
        toast.error("File vượt quá 10MB");
        return;
        }
        
        // Convert expiry (days) to an ISO expiryDate string; 0 means no expiry
        let expiryDate: string | undefined = undefined;
        if (expiry && expiry !== "0") {
            expiryDate = new Date(Date.now() + Number(expiry) * 24 * 60 * 60 * 1000).toISOString();
        }

        const uploadOptions = {
        password: password || undefined,
        expiryDate: expiryDate,
        downloadLimit: Number(limit),
        };
        
        const success = await uploadFile(file, uploadOptions);
        if (success) {
    
        navigate("/my-files");
  
        setFile(null);
        setPassword("");
        }
    };

    return (
        <Card className="w-full max-w-3xl mx-auto border-0 shadow-[0_8px_30px_rgb(0,0,0,0.04)] rounded-3xl bg-white/70 backdrop-blur-md">
        <CardContent className="p-6 md:p-8">
            
            <div
            {...getRootProps()}
            className={`
                relative flex flex-col items-center justify-center w-full h-64 
                border-2 border-dashed rounded-2xl transition-all duration-300 ease-out 
                ${!file ? "cursor-pointer" : ""}
                ${
                isDragActive
                    ? "border-blue-400 bg-blue-50/50 scale-[1.02]"
                    : "border-slate-200 hover:border-slate-300 hover:bg-slate-50/50"
                }
                ${uploading ? "pointer-events-none opacity-80" : ""}
            `}
            >
            <input {...getInputProps()} />

            {uploading ? (
                <div className="flex flex-col items-center w-full max-w-xs space-y-4">
                <div className="p-3 bg-blue-100 text-blue-600 rounded-full animate-pulse">
                    <UploadCloud className="w-8 h-8" />
                </div>
                <div className="w-full space-y-1 text-center">
                    <div className="flex justify-between text-sm font-medium text-slate-600">
                    <span>Đang tải lên...</span>
                    <span>{uploadProgress}%</span>
                    </div>
                    <Progress value={uploadProgress} className="h-2 rounded-full" />
                </div>
                </div>
            ) : file ? (

                <div className="flex flex-col items-center space-y-4 text-center z-10">
                <div className="p-4 bg-black-50 text-blue-500 rounded-full shadow-sm">
                    <FileIcon className="w-8 h-8 " />
                </div>
                <div className="space-y-1">
                    <p className="text-base font-semibold text-slate-700 truncate max-w-[250px]">
                    {file.name}
                    </p>
                    <p className="text-sm text-slate-400">
                    {(file.size / 1024 / 1024).toFixed(2)} MB
                    </p>
                </div>
                <button
                    onClick={(e) => {
                    e.stopPropagation(); // Ngăn dropzone mở lại hộp thoại chọn file
                    setFile(null);
                    }}
                    className="px-4 py-1.5 text-sm font-medium text-red-500 bg-red-50 hover:bg-red-100 rounded-full transition-colors"
                >
                    Hủy chọn
                </button>
                </div>
            ) : (

                <div className="flex flex-col items-center space-y-4 text-center">
                <div className="p-4 bg-slate-100 text-slate-500 rounded-full shadow-sm">
                    <UploadCloud className="w-8 h-8" />
                </div>
                <div className="space-y-1">
                    <p className="text-base font-semibold text-slate-700">
                    Kéo thả file vào đây
                    </p>
                    <p className="text-sm text-slate-400">
                    hoặc bấm để chọn file từ máy tính
                    </p>
                </div>
                <div className="flex items-center gap-2 text-xs font-medium text-slate-400 bg-slate-100/50 px-3 py-1.5 rounded-full">
                    <FileIcon className="w-3.5 h-3.5" />
                    <span>Tối đa 10 MB</span>
                </div>
                </div>
            )}
            </div>

            {/* 2. KHU VỰC TÙY CHỌN NÂNG CAO */}
            <div className={`mt-8 transition-opacity duration-300 ${uploading ? 'opacity-50 pointer-events-none' : 'opacity-100'}`}>
            <h3 className="text-sm font-semibold text-slate-800 mb-4 uppercase tracking-wider">
                Tùy chọn chia sẻ
            </h3>
            
            <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
                {/* Input: Mật khẩu */}
                <div className="space-y-2">
                <label className="text-sm font-medium text-slate-600 flex items-center gap-2">
                    <Lock className="w-4 h-4" /> Mật khẩu
                </label>
                <Input 
                    type="password" 
                    placeholder="Bỏ trống nếu không cần..." 
                    className="rounded-xl h-8 bg-white/50 focus:bg-white transition-colors"
                    value={password??""}
                    onChange={(e) => setPassword(e.target.value)}
                    disabled={uploading}
                />
                </div>

                {/* Select: Thời gian hết hạn */}
                <div className="space-y-2">
                <label className="text-sm font-medium text-slate-600 flex items-center gap-2">
                    <Clock className="w-4 h-4" /> Hết hạn sau
                </label>
                <Select value={expiry} onValueChange={setExpiry} disabled={uploading}>
                    <SelectTrigger className="rounded-xl h-11 bg-white/50 focus:bg-white transition-colors">
                    <SelectValue placeholder="Chọn thời gian" />
                    </SelectTrigger>
                    <SelectContent className="rounded-xl">
                    <SelectItem value="1">1 ngày</SelectItem>
                    <SelectItem value="7">7 ngày</SelectItem>
                    <SelectItem value="30">30 ngày</SelectItem>
                    </SelectContent>
                </Select>
                </div>

                {/* Select: Giới hạn lượt tải */}
                <div className="space-y-2">
                <label className="text-sm font-medium text-slate-600 flex items-center gap-2">
                    <Download className="w-4 h-4" /> Lượt tải
                </label>
                <Select value={limit} onValueChange={setLimit} disabled={uploading}>
                    <SelectTrigger className="rounded-xl h-11 bg-white/50 focus:bg-white transition-colors">
                    <SelectValue placeholder="Giới hạn tải" />
                    </SelectTrigger>
                    <SelectContent className="rounded-xl">
                    <SelectItem value="0">Không giới hạn</SelectItem>
                    <SelectItem value="10">10 lượt</SelectItem>
                    <SelectItem value="50">50 lượt</SelectItem>
                    <SelectItem value="100">100 lượt</SelectItem>
                    </SelectContent>
                </Select>
                </div>
            </div>
            </div>

            <button 
            onClick={handleUpload}
            disabled={!file || uploading}
            className="w-full mt-8 h-11 rounded-xl bg-black text-white font-medium shadow-[0_8px_20px_rgba(0,0,0,0.16)] transition-all duration-200 hover:-translate-y-0.5 hover:shadow-[0_12px_24px_rgba(0,0,0,0.22)] disabled:bg-slate-300 disabled:cursor-not-allowed disabled:shadow-none disabled:translate-y-0 flex items-center justify-center gap-2"
            >
            {uploading ? (
                <>
                <UploadCloud className="w-5 h-5 animate-bounce" />
                Đang xử lý...
                </>
            ) : (
                "Tải lên ngay"
            )}
            </button>

        </CardContent>
        </Card>
    );
}