
import { useParams } from "react-router";
import { SharedDownload } from "@/components/files/shared-download";
import {Navbar} from "@/components/nav-bar";


export default function SharedDownloadPage() {
    const { fileId } = useParams<{ fileId: string }>();
    
    return (
        <>
            <Navbar />
            <SharedDownload fileId={fileId} />
        </>
    );
}
