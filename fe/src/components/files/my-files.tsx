import { useEffect } from "react";
import { useNavigate } from "react-router";
import { Clock3, Download, File as FileIcon, Pencil, Trash2 } from "lucide-react";
import { useFileStore } from "@/stores/useFileStore";
import { ShareLinkButton } from "@/components/share-link-button"; 
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";

export function MyFiles() {
    const navigate = useNavigate();
    const { files, loading, fetchMyFiles, deleteFile } = useFileStore();

    useEffect(() => {
        fetchMyFiles();
    }, [fetchMyFiles]);

    const handleDelete = async (fileId: string) => {
        await deleteFile(fileId);
    };

    return (
        <div className="w-full rounded-2xl border border-slate-200 bg-white shadow-sm">
            <div className="border-b border-slate-200 px-6 py-4">
                <h2 className="text-lg font-semibold text-slate-800">Danh sách file</h2>
                <p className="text-sm text-slate-500">Quản lý các file bạn đã upload.</p>
            </div>

            {loading ? (
                <div className="p-8 text-center text-slate-500">Đang tải danh sách file...</div>
            ) : !Array.isArray(files) || files.length === 0 ? (
                <div className="p-8 text-center text-slate-500">Chưa có file nào.</div>
            ) : (
                <Table>
                    <TableHeader >
                        <TableRow className="border-none">
                            <TableHead className="w-[260px] border-none">Tên file</TableHead>
                            <TableHead className="border-none">Kích thước</TableHead>
                            <TableHead className="border-none">Lượt tải</TableHead>
                            <TableHead className="border-none">Hết hạn</TableHead>
                            <TableHead className="border-none text-right">Thao tác</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody className="border-none">
                        {files?.map((file) => (
                            <TableRow
                                key={file.fileId}
                                className="border-none hover:bg-slate-50"
                            >
                                <TableCell>
                                    <div className="flex items-center gap-3">
                                        <div className="rounded-xl bg-slate-100 p-2 text-slate-800 border border-slate-200">
                                            <FileIcon className="h-4 w-4" />
                                        </div>
                                        <span className="font-medium text-slate-800">{file.fileName}</span>
                                    </div>
                                </TableCell>
                                <TableCell>{file.size.toFixed(2)} MB</TableCell>
                                <TableCell>
                                    <div className="flex items-center gap-1 text-slate-800">
                                        <Download className="h-4 w-4" />
                                        {file.downloadCount}
                                        {(file as { downloadLimit?: number }).downloadLimit ? ` / ${(file as { downloadLimit?: number }).downloadLimit}` : ""}
                                    </div>
                                </TableCell>
                                <TableCell>
                                    <div className="flex items-center gap-1 text-slate-800">
                                        <Clock3 className="h-4 w-4" />
                                        {file.expiryDate ? new Date(file.expiryDate).toLocaleDateString("vi-VN") : "Không giới hạn"}
                                    </div>
                                </TableCell>
                                <TableCell className="text-right">
                                    <div className="flex justify-end gap-2">
                                        {/* Nút Copy Link hiện ra ở đây */}
                                        <ShareLinkButton fileId={file.fileId} />

                                        <button
                                            onClick={() => navigate(`/my-files/${file.fileId}/edit`)}
                                            className="inline-flex items-center gap-2 rounded-xl border border-slate-200 px-3 py-2 text-sm font-medium text-slate-600 hover:bg-slate-50"
                                        >
                                            <Pencil className="h-4 w-4" /> Sửa
                                        </button>

                                        <button
                                            onClick={() => handleDelete(file.fileId)}
                                            className="inline-flex items-center gap-2 rounded-xl border border-red-200 px-3 py-2 text-sm font-medium text-red-600 hover:bg-red-50"
                                        >
                                            <Trash2 className="h-4 w-4" /> Xóa
                                        </button>
                                    </div>
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            )}
        </div>
    );
}

export default MyFiles;