// src/components/files/shared-download.tsx
import { ArrowDownToLine, File as FileIcon, ImageOff, Loader2 } from "lucide-react";
import { useSharedFile } from "@/stores/useShareFile";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";

interface SharedDownloadProps {
    fileId: string | undefined;
}

/** Khung 4 góc kiểu "scan target" — dùng làm khung cho icon file, lặp lại như một chi tiết nhận diện */
function CornerFrame({
    children,
    className = "",
}: {
    children: React.ReactNode;
    className?: string;
}) {
    return (
        <div className={`relative flex h-24 w-24 items-center justify-center ${className}`}>
            <span className="absolute left-0 top-0 h-4 w-4 border-l-2 border-t-2 border-black" />
            <span className="absolute right-0 top-0 h-4 w-4 border-r-2 border-t-2 border-black" />
            <span className="absolute bottom-0 left-0 h-4 w-4 border-b-2 border-l-2 border-black" />
            <span className="absolute bottom-0 right-0 h-4 w-4 border-b-2 border-r-2 border-black" />
            {children}
        </div>
    );
}

/** Đường viền đứt kiểu vé xé, có hai vòng khuyết ở hai đầu */
function TicketDivider() {
    return (
        <div className="relative flex items-center">
            <span className="absolute -left-3 h-6 w-6 rounded-full bg-neutral-50" />
            <div className="w-full border-t-2 border-dashed border-black/15" />
            <span className="absolute -right-3 h-6 w-6 rounded-full bg-neutral-50" />
        </div>
    );
}

export function SharedDownload({ fileId }: SharedDownloadProps) {
    const {
        fileInfo,
        isLoadingInfo,
        isDownloading,
        previewUrl,
        isLoadingPreview,
        isImage,
        fileSizeMB,
        password,
        setPassword,
        passwordError,
        setPasswordError,
        isCheckingPassword,
        isPasswordVerified,
        handleDownloadClick
    } = useSharedFile(fileId);

    if (isLoadingInfo) {
        return (
            <div className="flex min-h-screen flex-col items-center justify-center gap-4 bg-neutral-50">
                <CornerFrame>
                    <Loader2 className="h-6 w-6 animate-spin text-black" />
                </CornerFrame>
                <p className="text-sm uppercase tracking-widest text-neutral-500">
                    Đang kiểm tra tệp
                </p>
            </div>
        );
    }

    if (!fileInfo) {
        return (
            <div className="flex min-h-screen flex-col items-center justify-center gap-4 bg-neutral-50 px-4 text-center">
                <CornerFrame>
                    <ImageOff className="h-7 w-7 text-black" />
                </CornerFrame>
                <div className="space-y-1">
                    <p className="text-sm uppercase tracking-widest text-neutral-500">
                        Không tìm thấy
                    </p>
                    <h1 className="text-lg font-semibold text-black">
                        Đường dẫn không tồn tại hoặc đã hết hạn
                    </h1>
                </div>
            </div>
        );
    }

    return (
        <div className="flex min-h-screen flex-col items-center justify-center bg-neutral-50 p-4">
            <Card className="w-full max-w-md overflow-hidden rounded-3xl border-black/10 py-0 shadow-none">
                <CardContent className="p-0">

                    {/* Eyebrow */}
                    <div className="flex items-center justify-between px-6 pt-6 text-xs uppercase tracking-widest text-neutral-500">
                        <span>Tệp được chia sẻ</span>
                        <span>{fileInfo.hasPassword ? "Có mật khẩu" : "Công khai"}</span>
                    </div>

                    {/* Khu vực Preview */}
                    <div className="flex min-h-[220px] items-center justify-center px-6 py-8">
                        {isImage ? (
                            isLoadingPreview ? (
                                <CornerFrame>
                                    <Loader2 className="h-6 w-6 animate-spin text-black" />
                                </CornerFrame>
                            ) : previewUrl ? (
                                <img
                                    src={previewUrl}
                                    alt={fileInfo.fileName}
                                    className="max-h-[280px] max-w-full rounded-xl object-contain"
                                />
                            ) : null
                        ) : (
                            <CornerFrame>
                                <FileIcon className="h-8 w-8 text-black" strokeWidth={1.5} />
                            </CornerFrame>
                        )}
                    </div>

                    <div className="px-6">
                        <TicketDivider />
                    </div>

                    {/* Thông tin file — dạng biên nhận */}
                    <div className="space-y-6 px-6 pb-6 pt-6">
                        <h1 className="break-all text-center text-lg font-semibold leading-snug text-black">
                            {fileInfo.fileName}
                        </h1>

                        <dl className="grid grid-cols-2 divide-x divide-black/10 rounded-xl border border-black/10 font-mono text-sm">
                            <div className="flex flex-col items-center gap-0.5 py-3">
                                <dt className="text-[10px] uppercase tracking-widest text-neutral-400">
                                    Dung lượng
                                </dt>
                                <dd className="font-semibold text-black">{fileSizeMB} MB</dd>
                            </div>
                            <div className="flex flex-col items-center gap-0.5 py-3">
                                <dt className="text-[10px] uppercase tracking-widest text-neutral-400">
                                    Lượt tải
                                </dt>
                                <dd className="font-semibold text-black">{fileInfo.downloadCount}</dd>
                            </div>
                        </dl>

                        {fileInfo.hasPassword ? (
                            <div className="space-y-2 rounded-xl border border-black/10 bg-neutral-100/70 p-3">
                                <label className="text-[10px] uppercase tracking-widest text-neutral-500">
                                    Mật khẩu
                                </label>
                                <input
                                    type="password"
                                    value={password}
                                    onChange={(e) => {
                                        setPassword(e.target.value);
                                        if (passwordError) {
                                            setPasswordError("");
                                        }
                                    }}
                                    placeholder="Nhập mật khẩu"
                                    className="w-full rounded-lg border border-black/10 bg-white px-3 py-2 text-sm text-black outline-none"
                                />
                                {passwordError ? (
                                    <p className="text-sm text-red-600">{passwordError}</p>
                                ) : null}
                                {isPasswordVerified ? (
                                    <p className="text-sm text-green-600">Mật khẩu đúng, bạn có thể tải xuống.</p>
                                ) : null}
                            </div>
                        ) : null}

                        <Button
                            onClick={handleDownloadClick}
                            disabled={isDownloading || isCheckingPassword}
                            className="h-12 w-full rounded-xl bg-black text-white hover:bg-neutral-800 disabled:bg-neutral-300"
                        >
                            {isCheckingPassword ? (
                                <>
                                    <Loader2 className="h-4 w-4 animate-spin" />
                                    Đang kiểm tra...
                                </>
                            ) : isDownloading ? (
                                <>
                                    <Loader2 className="h-4 w-4 animate-spin" />
                                    Đang xử lý...
                                </>
                            ) : (
                                <>
                                    <ArrowDownToLine className="h-4 w-4" />
                                    {fileInfo.hasPassword && !isPasswordVerified ? "Xác minh mật khẩu" : "Tải xuống máy"}
                                </>
                            )}
                        </Button>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}

export default SharedDownload;