import React from "react";
import { LoginForm } from "@/components/login-form";

const AuthPage = () => {
    return (
    <div className="min-h-screen flex items-center justify-center bg-background px-4 py-12">
        <div className="w-full max-w-lg ">
        <LoginForm />
        </div>
    </div>
    );
};

export default AuthPage;