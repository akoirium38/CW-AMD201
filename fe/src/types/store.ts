import type { FileRecord, UpdateFilePayload } from "./file";

export interface AuthState {
    token: string | null;
    user: any | null;
    email: string | null;
    loading:boolean;
    
    setAuth: (token: string, user:any) => Promise<void>

    logOut: () => Promise<void>

    authEmail:(email:string) => Promise<boolean> 

    authOtp:(email:string, code:string) => Promise<boolean>

    fetchMe: () => Promise<void>
}

export interface FileState {
    files: FileRecord[] | null;
    loading: boolean;
    uploading: boolean;
    uploadProgress: number;

    fetchMyFiles: () => Promise<void>,
    uploadFile: (file: File, options?: { expiryDate?:string; downloadLimit?: number; password?: string}) => Promise<boolean>
    downloadFile: (fileId:string, filename: string, password?: string) => Promise<boolean>
    deleteFile: (fileId: string) => Promise<boolean>
    updateFile: (fileId: string, payload: UpdateFilePayload) => Promise<boolean>

    copyShareLink: (fileId: string) => void
}