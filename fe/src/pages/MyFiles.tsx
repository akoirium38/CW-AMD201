import { Navbar } from '@/components/nav-bar';
import { MyFiles } from '@/components/files/my-files';

export default function MyFilesPage() {
    return (
        <div className="min-h-screen flex flex-col">
            <Navbar />

            <main className="flex-1 w-full flex flex-col items-center pt-20 px-4 pb-10">
                <div className="w-full max-w-5xl">
                    <div className="flex items-center justify-between mb-6">
                        <div>
                            <h1 className="text-2xl font-semibold text-slate-800">My Files</h1>
                            <p className="text-sm text-slate-500">List of files you have uploaded and their current download limits.</p>
                        </div>
                    </div>

                    <MyFiles />
                </div>
            </main>
        </div>
    );
}
