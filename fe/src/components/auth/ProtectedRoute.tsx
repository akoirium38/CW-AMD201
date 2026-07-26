import React from "react";
import { useAuthStore } from "@/stores/useAuthStore";
import { Navigate, Outlet } from "react-router";

const ProtectedRoute = () => {
    const {token, email, loading} = useAuthStore();

    if (!token) {
        return (
            <Navigate to="/auth" replace/>
        )
    }

    return (
        <Outlet></Outlet>
    )
}

export default ProtectedRoute;