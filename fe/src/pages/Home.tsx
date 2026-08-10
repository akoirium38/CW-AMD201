import { FileUpload } from "@/components/files/file-upload";
import { Navbar } from "@/components/nav-bar";

export default function Home() {
    return (

        <div className="min-h-screen flex flex-col">
            
            <Navbar />
            

            <main className="flex-1 w-full flex flex-col items-center pt-20 px-4">
                <div className="text-center mb-10 space-y-3">
                    <h1 className="text-4xl font-bold text-slate-800 tracking-tight">
                    Share and manage your files with ease
                    </h1>
                    <p className="text-slate-500 text-lg">
                    Upload safely and share quickly with anyone.
                    </p>
                </div>
                
                <FileUpload />
            </main>
        </div>
    );
}