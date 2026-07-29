export interface FileRecord {
    fileId: string;
    fileName: string;
    size: number;
    downloadCount: number;
    expiryDate: string | null;
    downloadUrl?: string;
    hasPassword?: boolean;
}

export interface UpdateFilePayload {
    fileName?: string;
    expiryDate?: string | null ;
    downloadLimit?: number | null;
    password?: string | null;
}