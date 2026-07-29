import { useParams } from "react-router";
import { Navbar } from "@/components/nav-bar";
import { EditFile } from "@/components/files/edit-file";

export default function EditFilePage() {
    const { fileId } = useParams<{ fileId: string }>();

    return (
        <div className="min-h-screen bg-slate-50/50 flex flex-col">
            <Navbar />
            <main className="flex-1 w-full flex flex-col items-center pt-20 px-4 pb-10">
                <EditFile fileId={fileId} />
            </main>
        </div>
    );
}