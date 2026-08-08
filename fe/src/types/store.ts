import type { FileRecord, UpdateFilePayload } from './file';

export interface AuthState {
    token: string | null;
    user: any | null;
    email: string | null;
    loading: boolean;

    setAuth: (token: string, user: any, email: string | null) => Promise<void>;
    logOut: () => Promise<void>;
    login: (email: string, password: string) => Promise<boolean>;
    register: (email: string, password: string) => Promise<boolean>;
    requestPasswordReset: (email: string) => Promise<boolean>;
    resetPassword: (email: string, otp: string, newPassword: string) => Promise<boolean>;
    fetchMe: () => Promise<void>;
}

export interface FileState {
    files: FileRecord[] | null;
    loading: boolean;
    uploading: boolean;
    uploadProgress: number;

    fetchMyFiles: () => Promise<void>;
    uploadFile: (file: File, options?: { expiryDate?: string; downloadLimit?: number; password?: string }) => Promise<boolean>;
    downloadFile: (fileId: string, filename: string, password?: string) => Promise<boolean>;
    deleteFile: (fileId: string) => Promise<boolean>;
    updateFile: (fileId: string, payload: UpdateFilePayload) => Promise<boolean>;

    copyShareLink: (fileId: string) => void;
}