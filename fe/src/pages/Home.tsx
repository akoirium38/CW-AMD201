import { FileUpload } from "@/components/files/file-upload";
import { Navbar } from "@/components/nav-bar"; // 1. Import Navbar vào đây

export default function Home() {
    return (
        // 2. Bỏ 'items-center' và 'pt-20' ở thẻ div cha cùng để Navbar có thể trải dài toàn màn hình và dính lên trên cùng
        <div className="min-h-screen bg-slate-50/50 flex flex-col">
            
            {/* 3. Đặt Navbar ở ngay dưới div cha */}
            <Navbar />
            
            {/* 4. Đưa các thuộc tính căn giữa (items-center) và đệm trên (pt-20) xuống một thẻ main bọc lấy nội dung cũ */}
            <main className="flex-1 w-full flex flex-col items-center pt-20 px-4">
                <div className="text-center mb-10 space-y-3">
                    <h1 className="text-4xl font-bold text-slate-800 tracking-tight">
                    Chia sẻ tệp tin dễ dàng
                    </h1>
                    <p className="text-slate-500 text-lg">
                    Tải lên an toàn, chia sẻ nhanh chóng với bất kỳ ai.
                    </p>
                </div>
                
                <FileUpload />
            </main>
        </div>
    );
}