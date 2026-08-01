// src/components/files/edit-file.tsx
import { useEffect, useState } from "react";
import { useNavigate } from "react-router";
import { toast } from "sonner";
import { Clock, Download, Lock, Loader2, ArrowLeft } from "lucide-react";
import { fileService } from "@/services/fileService";
import { useFileStore } from "@/stores/useFileStore";
import type { FileRecord } from "@/types/file";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";

interface EditFileProps {
    fileId: string | undefined;
}

export function EditFile({ fileId }: EditFileProps) {
    const navigate = useNavigate();
    const updateFile = useFileStore((state) => state.updateFile);

    const [fileInfo, setFileInfo] = useState<FileRecord | null>(null);
    const [isLoadingInfo, setIsLoadingInfo] = useState(true);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const [expiry, setExpiry] = useState<string>("7");
    const [limit, setLimit] = useState<string>("0");
    const [changePassword, setChangePassword] = useState(false);
    const [password, setPassword] = useState("");

    const handleExpiryChange = (value: string | null) => {
        if (value !== null) {
            setExpiry(value);
        }
    };

    // const handleLimitChange = (value: string | null) => {
    //     if (value !== null) {
    //         setLimit(value);
    //     }
    // };

    useEffect(() => {
        const fetchInfo = async () => {
            if (!fileId) return;
            setIsLoadingInfo(true);
            try {
                const info = await fileService.getFileDetails(fileId);
                setFileInfo(info);
                const fileDetails = info as FileRecord & { downloadLimit?: number };
                if (fileDetails.downloadLimit) setLimit(fileDetails.downloadLimit.toString());
            } catch (error) {
                console.error("Lỗi khi tải thông tin file:", error);
                toast.error("Không thể tải thông tin file.");
            } finally {
                setIsLoadingInfo(false);
            }
        };

        fetchInfo();
    }, [fileId]);

    const handleSubmit = async () => {
        if (!fileId) return;

        if (changePassword && !password.trim()) {
            toast.error("Vui lòng nhập mật khẩu mới hoặc bỏ chọn đổi mật khẩu");
            return;
        }

        setIsSubmitting(true);

        const expiryDate = new Date(
            Date.now() + Number(expiry) * 24 * 60 * 60 * 1000
        ).toISOString();

        const success = await updateFile(fileId, {
            downloadLimit: Number(limit),
            expiryDate,
            password: changePassword ? password : undefined,
        });

        setIsSubmitting(false);

        if (success) {
            navigate("/my-files");
        }
    };

    if (isLoadingInfo) {
        return (
            <div className="flex flex-col items-center justify-center gap-3 py-24 text-slate-500">
                <Loader2 className="h-6 w-6 animate-spin" />
                <p className="text-sm">Đang tải thông tin file...</p>
            </div>
        );
    }

    if (!fileInfo) {
        return (
            <div className="flex flex-col items-center justify-center gap-3 py-24 text-center">
                <p className="text-red-500">Không tìm thấy file này.</p>
                <Button variant="ghost" onClick={() => navigate("/my-files")}>
                    <ArrowLeft className="h-4 w-4 mr-2" /> Quay lại
                </Button>
            </div>
        );
    }

    return (
        <Card className="w-full max-w-2xl mx-auto border-0 shadow-[0_8px_30px_rgb(0,0,0,0.04)] rounded-3xl bg-white/70 backdrop-blur-md">
            <CardContent className="p-6 md:p-8 space-y-6">

                <div className="flex items-center justify-between">
                    <div>
                        <p className="text-xs font-medium uppercase tracking-wider text-slate-400">
                            Sửa cài đặt chia sẻ
                        </p>
                        <h1 className="text-lg font-semibold text-slate-800 break-all">
                            {fileInfo.fileName}
                        </h1>
                    </div>
                </div>

                {/* Thông tin hiện tại */}
                <div className="grid grid-cols-3 gap-3 rounded-2xl border border-slate-200 bg-slate-50/50 p-4 text-center text-sm">
                    <div>
                        <p className="text-xs text-slate-400">Lượt tải hiện tại</p>
                        <p className="font-semibold text-slate-800">{fileInfo.downloadCount}</p>
                    </div>
                    <div>
                        <p className="text-xs text-slate-400">Hết hạn hiện tại</p>
                        <p className="font-semibold text-slate-800">
                            {fileInfo.expiryDate
                                ? new Date(fileInfo.expiryDate).toLocaleDateString("vi-VN")
                                : "Không giới hạn"}
                        </p>
                    </div>
                    <div>
                        <p className="text-xs text-slate-400">Mật khẩu</p>
                        <p className="font-semibold text-slate-800">
                            {fileInfo.hasPassword ? "Đang bật" : "Không có"}
                        </p>
                    </div>
                </div>

                {/* Form cập nhật */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-600 flex items-center gap-2">
                            <Clock className="w-4 h-4" /> Hết hạn sau (tính từ bây giờ)
                        </label>
                        <Select value={expiry} onValueChange={handleExpiryChange} disabled={isSubmitting}>
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

                    <div className="space-y-2">
                        <label className="text-sm font-medium text-slate-600 flex items-center gap-2">
                            <Download className="w-4 h-4" /> Giới hạn lượt tải
                        </label>
                        <Select value={limit} onValueChange={(value) => value !== null && setLimit(value)} disabled={isSubmitting}>
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

                <div className="space-y-3 rounded-2xl border border-slate-200 p-4">
                    <label className="flex items-center gap-2 text-sm font-medium text-slate-700">
                        <Checkbox
                            checked={changePassword}
                            onCheckedChange={(v) => setChangePassword(Boolean(v))}
                            disabled={isSubmitting}
                        />
                        <Lock className="w-4 h-4" />
                        Đổi mật khẩu chia sẻ
                    </label>

                    {changePassword && (
                        <Input
                            type="password"
                            placeholder="Nhập mật khẩu mới"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            disabled={isSubmitting}
                            className="rounded-xl h-11 bg-white/50 focus:bg-white transition-colors"
                            autoFocus
                        />
                    )}
                    {!changePassword && (
                        <p className="text-xs text-slate-400">
                            Bỏ chọn = giữ nguyên mật khẩu hiện tại (nếu có).
                        </p>
                    )}
                </div>

                <Button
                    onClick={handleSubmit}
                    disabled={isSubmitting}
                    className="w-full h-11 rounded-xl bg-black text-white hover:bg-slate-800"
                >
                    {isSubmitting ? (
                        <>
                            <Loader2 className="h-4 w-4 animate-spin mr-2" /> Đang cập nhật...
                        </>
                    ) : (
                        "Cập nhật cài đặt"
                    )}
                </Button>
            </CardContent>
        </Card>
    );
}

export default EditFile;