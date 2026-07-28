import { useAuthStore } from "@/stores/useAuthStore";
import { Navigate, Outlet } from "react-router";
import { toast } from "sonner";

const ProtectedRoute = () => {
    const { token } = useAuthStore();

    if (!token) {
        toast.warning("Please log in first before continuing.");
        return <Navigate to="/auth" replace />;
    }

    return <Outlet />;
};

export default ProtectedRoute;