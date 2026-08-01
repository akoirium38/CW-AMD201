import logo from "../assets/logo.svg";
import { useLocation, useNavigate } from "react-router"; // 1. Import useNavigate
import { FolderHeart, UploadCloud } from "lucide-react";
import { useAuthStore } from "@/stores/useAuthStore";
import {
    NavigationMenu,
    NavigationMenuItem,
    NavigationMenuLink,
    NavigationMenuList,
    navigationMenuTriggerStyle,
    } from "@/components/ui/navigation-menu";

    export function Navbar() {
    const location = useLocation();
    const navigate = useNavigate(); 
    const { token } = useAuthStore();

    return (
        <header className="sticky top-0 z-50 w-full border-b border-slate-100/50 bg-white/60 backdrop-blur-xl">
        <div className="container mx-auto px-4 h-16 flex items-center justify-between">
            

            <div 
            onClick={() => navigate("/")} 
            className="flex items-center gap-2 hover:opacity-80 transition-opacity cursor-pointer"
            >
            <a href="/" className="flex items-center gap-2 !no-underline !border-none !shadow-none !outline-none hover:!no-underline">
                <img src={logo} alt="Logo" className="w-10 h-10" />
                <span className="text-xl font-semibold !no-underline">
                FileHub
                </span>
            </a>
            </div>


            <div className="flex items-center gap-6 !no-underline">
            {token ? (
                <>

                <NavigationMenu>
                    <NavigationMenuList className="gap-2">
                    
                    <NavigationMenuItem>
                        <NavigationMenuLink 
                        active={location.pathname === "/"}
                        onClick={() => navigate("/")} 
                        className={`${navigationMenuTriggerStyle()} bg-transparent rounded-2xl cursor-pointer data-[active]:bg-slate-100`}
                        >
                        <UploadCloud className="w-4 h-4 mr-2" />
                        Tải lên
                        </NavigationMenuLink>
                    </NavigationMenuItem>

                    {/* Menu 2: File của tôi */}
                    <NavigationMenuItem>
                        <NavigationMenuLink 
                        active={location.pathname === "/my-files"}
                        onClick={() => navigate("/my-files")} 
                        className={`${navigationMenuTriggerStyle()} bg-transparent rounded-2xl cursor-pointer data-[active]:bg-slate-100`}
                        >
                        <FolderHeart className="w-4 h-4 mr-2" />
                        File của tôi
                        </NavigationMenuLink>
                    </NavigationMenuItem>

                    <NavigationMenuItem>
                        <NavigationMenuLink
                        onClick={async () => {
                            await useAuthStore.getState().logOut();
                        }}
                        className={`${navigationMenuTriggerStyle()} bg-transparent rounded-2xl cursor-pointer data-[active]:bg-slate-100`}
                        >
                        Đăng xuất
                        </NavigationMenuLink>
                    </NavigationMenuItem>

                    </NavigationMenuList>
                </NavigationMenu>





                </>
            ) : (

                <button
                onClick={() => navigate("/login")} 
                className="px-5 py-2 text-sm font-medium text-white bg-slate-800 rounded-full hover:bg-slate-700 transition-colors shadow-sm cursor-pointer"
                >
                Đăng nhập
                </button>
            )}
            </div>
        </div>
        </header>
    );
}