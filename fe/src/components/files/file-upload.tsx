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
    const { uploadFile, uploading, uploadProgress } = useFileStore();


    const [files, setFiles] = useState<File[]>([]);
    const [password, setPassword] = useState<string|null>();
    const [expiry, setExpiry] = useState<string | null>("7");
    const [limit, setLimit] = useState<string | null>("0");


    const onDrop = useCallback((acceptedFiles: File[]) => {
        setFiles((currentFiles) => [...currentFiles, ...acceptedFiles].slice(0, 10));
    }, []);

    const onDropRejected = useCallback((fileRejections: FileRejection[]) => {
        const error = fileRejections[0]?.errors[0];

        if (error?.code === "file-too-large") {
        toast.error("File over 10MB is not allowed");
        } else {
        toast.error("File is not valid");
        }
    }, []);

    const { getRootProps, getInputProps, isDragActive } = useDropzone({
        onDrop,
        onDropRejected,
        maxFiles: 10,
        maxSize: MAX_FILE_SIZE_BYTES,
        disabled: uploading,
    });

    const navigate = useNavigate();

    const handleUpload = async () => {
        if (files.length === 0) return;

        if (files.some((selectedFile) => selectedFile.size > MAX_FILE_SIZE_BYTES)) {
        toast.error("One or more files exceed the 10MB limit");
        return;
        }
        

        let expiryDate: string | undefined = undefined;
        if (expiry && expiry !== "0") {
            expiryDate = new Date(Date.now() + Number(expiry) * 24 * 60 * 60 * 1000).toISOString();
        }

        const uploadOptions = {
        password: password || undefined,
        expiryDate: expiryDate,
        downloadLimit: Number(limit),
        };
        
        let uploadedCount = 0;
        for (const selectedFile of files) {
            if (await uploadFile(selectedFile, uploadOptions)) {
                uploadedCount += 1;
            }
        }

        if (uploadedCount === files.length) {
        navigate("/my-files");
        setFiles([]);
        setPassword("");
        } else if (uploadedCount > 0) {
            toast.warning(`${uploadedCount}/${files.length} file has been uploaded`);
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
                ${files.length === 0 ? "cursor-pointer" : ""}
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
                    <span>Uploading...</span>
                    <span>{uploadProgress}%</span>
                    </div>
                    <Progress value={uploadProgress} className="h-2 rounded-full" />
                </div>
                </div>
            ) : files.length > 0 ? (

                <div className="flex flex-col items-center space-y-3 text-center z-10 w-full px-6">
                <div className="p-4 bg-black-50 text-blue-500 rounded-full shadow-sm">
                    <FileIcon className="w-8 h-8 " />
                </div>
                <div className="w-full max-w-md max-h-28 overflow-y-auto space-y-1">
                    {files.map((selectedFile, index) => (
                    <div key={`${selectedFile.name}-${selectedFile.lastModified}-${index}`} className="flex items-center justify-between gap-3 rounded-lg bg-slate-50 px-3 py-2 text-left">
                        <div className="min-w-0">
                        <p className="text-sm font-semibold text-slate-700 truncate">
                            {selectedFile.name}
                        </p>
                        <p className="text-xs text-slate-400">
                            {(selectedFile.size / 1024 / 1024).toFixed(2)} MB
                        </p>
                        </div>
                        <button
                        type="button"
                        onClick={(e) => {
                            e.stopPropagation();
                            setFiles((currentFiles) => currentFiles.filter((_, fileIndex) => fileIndex !== index));
                        }}
                        className="shrink-0 px-2 py-1 text-xs font-medium text-red-500 bg-red-50 hover:bg-red-100 rounded-full transition-colors"
                        >
                        Remove
                        </button>
                    </div>
                    ))}
                </div>
                <button
                    type="button"
                    onClick={(e) => {
                    e.stopPropagation(); 
                    setFiles([]);
                    }}
                    className="px-4 py-1.5 text-sm font-medium text-red-500 bg-red-50 hover:bg-red-100 rounded-full transition-colors"
                >
                    Clear Selection
                </button>
                </div>
            ) : (

                <div className="flex flex-col items-center space-y-4 text-center">
                <div className="p-4 bg-slate-100 text-slate-500 rounded-full shadow-sm">
                    <UploadCloud className="w-8 h-8" />
                </div>
                <div className="space-y-1">
                    <p className="text-base font-semibold text-slate-700">
                    Drag and drop files here
                    </p>
                    <p className="text-sm text-slate-400">
                    or click to select files from your computer
                    </p>
                </div>
                <div className="flex items-center gap-2 text-xs font-medium text-slate-400 bg-slate-100/50 px-3 py-1.5 rounded-full">
                    <FileIcon className="w-3.5 h-3.5" />
                    <span>Maximum 10 MB</span>
                </div>
                </div>
            )}
            </div>


            <div className={`mt-8 transition-opacity duration-300 ${uploading ? 'opacity-50 pointer-events-none' : 'opacity-100'}`}>
            <h3 className="text-sm font-semibold text-slate-800 mb-4 uppercase tracking-wider">
                Optional Sharing Settings
            </h3>
            
            <div className="grid grid-cols-1 md:grid-cols-3 gap-5">

                <div className="space-y-2">
                <label className="text-sm font-medium text-slate-600 flex items-center gap-2">
                    <Lock className="w-4 h-4" /> Password
                </label>
                <Input 
                    type="password" 
                    placeholder="Leave blank if not needed..." 
                    className="rounded-xl h-8 bg-white/50 focus:bg-white transition-colors"
                    value={password??""}
                    onChange={(e) => setPassword(e.target.value)}
                    disabled={uploading}
                />
                </div>


                <div className="space-y-2">
                <label className="text-sm font-medium text-slate-600 flex items-center gap-2">
                    <Clock className="w-4 h-4" /> Expiry after
                </label>
                <Select value={expiry} onValueChange={setExpiry} disabled={uploading}>
                    <SelectTrigger className="rounded-xl h-11 bg-white/50 focus:bg-white transition-colors">
                    <SelectValue placeholder="Select duration" />
                    </SelectTrigger>
                    <SelectContent className="rounded-xl">
                    <SelectItem value="1">1 day</SelectItem>
                    <SelectItem value="7">7 days</SelectItem>
                    <SelectItem value="30">30 days</SelectItem>
                    </SelectContent>
                </Select>
                </div>


                <div className="space-y-2">
                <label className="text-sm font-medium text-slate-600 flex items-center gap-2">
                    <Download className="w-4 h-4" /> Download limit
                </label>
                <Select value={limit} onValueChange={setLimit} disabled={uploading}>
                    <SelectTrigger className="rounded-xl h-11 bg-white/50 focus:bg-white transition-colors">
                    <SelectValue placeholder="Download limit" />
                    </SelectTrigger>
                    <SelectContent className="rounded-xl">
                    <SelectItem value="0">No limit</SelectItem>
                    <SelectItem value="10">10 downloads</SelectItem>
                    <SelectItem value="50">50 downloads</SelectItem>
                    <SelectItem value="100">100 downloads</SelectItem>
                    </SelectContent>
                </Select>
                </div>
            </div>
            </div>

            <button 
            onClick={handleUpload}
            disabled={files.length === 0 || uploading}
            className="w-full mt-8 h-11 rounded-xl bg-black text-white font-medium shadow-[0_8px_20px_rgba(0,0,0,0.16)] transition-all duration-200 hover:-translate-y-0.5 hover:shadow-[0_12px_24px_rgba(0,0,0,0.22)] disabled:bg-slate-300 disabled:cursor-not-allowed disabled:shadow-none disabled:translate-y-0 flex items-center justify-center gap-2"
            >
            {uploading ? (
                <>
                <UploadCloud className="w-5 h-5 animate-bounce" />
                Loading...
                </>
            ) : (
                "Upload"
            )}
            </button>

        </CardContent>
        </Card>
    );
}