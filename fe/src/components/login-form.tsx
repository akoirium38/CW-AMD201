"use client"

import logo from "../assets/logo.svg";
import { useState } from "react";
import { cn } from "@/lib/utils";
import { Label } from "./ui/label";
import { Input } from "./ui/input";
import { Button } from "./ui/button";
import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useAuthStore } from "@/stores/useAuthStore";
import { useNavigate } from "react-router";

const loginSchema = z.object({
  email: z.string().email({ message: "Invalid email address" }),
  password: z.string().min(8, { message: "Password must be at least 8 characters" }),
});

const registerSchema = z.object({
  email: z.string().email({ message: "Invalid email address" }),
  password: z.string().min(8, { message: "Password must be at least 8 characters" }),
});

const resetRequestSchema = z.object({
  email: z.string().email({ message: "Invalid email address" }),
});

const resetVerifySchema = z.object({
  email: z.string().email({ message: "Invalid email address" }),
  otp: z.string().length(6, { message: "OTP must be 6 digits" }),
  newPassword: z.string().min(8, { message: "Password must be at least 8 characters" }),
});

type LoginFormValues = z.infer<typeof loginSchema>;
type RegisterFormValues = z.infer<typeof registerSchema>;
type ResetRequestFormValues = z.infer<typeof resetRequestSchema>;
type ResetVerifyFormValues = z.infer<typeof resetVerifySchema>;

export function LoginForm({
  className,
  ...props
}: React.ComponentProps<"div">) {
  const login = useAuthStore((state) => state.login);
  const register = useAuthStore((state) => state.register);
  const requestPasswordReset = useAuthStore((state) => state.requestPasswordReset);
  const resetPassword = useAuthStore((state) => state.resetPassword);
  const loading = useAuthStore((state) => state.loading);
  const navigate = useNavigate();

  const [view, setView] = useState<"login" | "register" | "reset">("login");
  const [resetStep, setResetStep] = useState<"request" | "verify">("request");

  const loginForm = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: "", password: "" },
  });

  const registerForm = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: { email: "", password: "" },
  });

  const resetRequestForm = useForm<ResetRequestFormValues>({
    resolver: zodResolver(resetRequestSchema),
    defaultValues: { email: "" },
  });

  const resetVerifyForm = useForm<ResetVerifyFormValues>({
    resolver: zodResolver(resetVerifySchema),
    defaultValues: { email: "", otp: "", newPassword: "" },
  });

  const handleLogin = async (data: LoginFormValues) => {
    const success = await login(data.email, data.password);
    if (success) {
      navigate("/");
    } else {
      loginForm.setError("password", {
        type: "manual",
        message: "Invalid login credentials.",
      });
    }
  };

  const handleRegister = async (data: RegisterFormValues) => {
    const success = await register(data.email, data.password);
    if (success) {
      setView("login");
      registerForm.reset();
    }
  };

  const handleResetRequest = async (data: ResetRequestFormValues) => {
    const success = await requestPasswordReset(data.email);
    if (success) {
      setResetStep("verify");
      resetVerifyForm.setValue("email", data.email);
    } else {
      resetRequestForm.setError("email", {
        type: "manual",
        message: "Failed to send OTP. Please try again.",
      });
    }
  };

  const handleResetVerify = async (data: ResetVerifyFormValues) => {
    const success = await resetPassword(data.email, data.otp, data.newPassword);
    if (success) {
      setView("login");
      setResetStep("request");
      resetVerifyForm.reset();
      resetRequestForm.reset();
    } else {
      resetVerifyForm.setError("otp", {
        type: "manual",
        message: "Invalid OTP or password reset failed.",
      });
    }
  };

  return (
    <div className={cn("flex flex-col gap-6", className)} {...props}>
      <div className="rounded-3xl border border-slate-200 bg-white/90 p-8 shadow-lg shadow-slate-200/40 backdrop-blur-xl">
        <div className="flex flex-col items-center text-center gap-2 mb-8">
          <img src={logo} alt="Logo" className="h-16 w-16" />
          <h1 className="text-3xl font-bold">
            {view === "login"
              ? "Welcome back"
              : view === "register"
              ? "Create an account"
              : "Reset your password"}
          </h1>
          <p className="text-sm text-muted-foreground max-w-md">
            {view === "login"
              ? "Sign in with your Gmail and password."
              : view === "register"
              ? "Create a new FileHub account using your Gmail."
              : resetStep === "request"
              ? "Enter your email to receive a password reset OTP."
              : "Enter the OTP and your new password to reset your account."}
          </p>
        </div>

        <div className="flex flex-wrap justify-center gap-2 mb-6">
          <Button
            type="button"
            variant={view === "login" ? "secondary" : "ghost"}
            onClick={() => {
              setView("login");
              setResetStep("request");
            }}
          >
            Login
          </Button>
          <Button
            type="button"
            variant={view === "register" ? "secondary" : "ghost"}
            onClick={() => {
              setView("register");
              setResetStep("request");
            }}
          >
            Register
          </Button>
          <Button
            type="button"
            variant={view === "reset" ? "secondary" : "ghost"}
            onClick={() => setView("reset")}
          >
            Reset Password
          </Button>
        </div>

        {view === "login" && (
          <form onSubmit={loginForm.handleSubmit(handleLogin)} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="login-email">Email</Label>
              <Input
                id="login-email"
                type="email"
                placeholder="user@gmail.com"
                {...loginForm.register("email")}
              />
              {loginForm.formState.errors.email && (
                <p className="text-sm text-red-600">{loginForm.formState.errors.email.message}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="login-password">Password</Label>
              <Input
                id="login-password"
                type="password"
                placeholder="Your password"
                {...loginForm.register("password")}
              />
              {loginForm.formState.errors.password && (
                <p className="text-sm text-red-600">{loginForm.formState.errors.password.message}</p>
              )}
            </div>

            <Button type="submit" className="w-full" disabled={loading || loginForm.formState.isSubmitting}>
              Sign in
            </Button>
          </form>
        )}

        {view === "register" && (
          <form onSubmit={registerForm.handleSubmit(handleRegister)} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="register-email">Email</Label>
              <Input
                id="register-email"
                type="email"
                placeholder="user@gmail.com"
                {...registerForm.register("email")}
              />
              {registerForm.formState.errors.email && (
                <p className="text-sm text-red-600">{registerForm.formState.errors.email.message}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="register-password">Password</Label>
              <Input
                id="register-password"
                type="password"
                placeholder="Your password"
                {...registerForm.register("password")}
              />
              {registerForm.formState.errors.password && (
                <p className="text-sm text-red-600">{registerForm.formState.errors.password.message}</p>
              )}
            </div>

            <Button type="submit" className="w-full" disabled={loading || registerForm.formState.isSubmitting}>
              Create account
            </Button>
          </form>
        )}

        {view === "reset" && (
          <div className="space-y-4">
            {resetStep === "request" ? (
              <form onSubmit={resetRequestForm.handleSubmit(handleResetRequest)} className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="reset-email">Email</Label>
                  <Input
                    id="reset-email"
                    type="email"
                    placeholder="user@gmail.com"
                    {...resetRequestForm.register("email")}
                  />
                  {resetRequestForm.formState.errors.email && (
                    <p className="text-sm text-red-600">{resetRequestForm.formState.errors.email.message}</p>
                  )}
                </div>
                <Button type="submit" className="w-full" disabled={loading || resetRequestForm.formState.isSubmitting}>
                  Send OTP
                </Button>
              </form>
            ) : (
              <form onSubmit={resetVerifyForm.handleSubmit(handleResetVerify)} className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="verify-email">Email</Label>
                  <Input
                    id="verify-email"
                    type="email"
                    disabled
                    {...resetVerifyForm.register("email")}
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="reset-otp">OTP</Label>
                  <Input
                    id="reset-otp"
                    type="text"
                    placeholder="123456"
                    {...resetVerifyForm.register("otp")}
                  />
                  {resetVerifyForm.formState.errors.otp && (
                    <p className="text-sm text-red-600">{resetVerifyForm.formState.errors.otp.message}</p>
                  )}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="reset-password">New password</Label>
                  <Input
                    id="reset-password"
                    type="password"
                    placeholder="New password"
                    {...resetVerifyForm.register("newPassword")}
                  />
                  {resetVerifyForm.formState.errors.newPassword && (
                    <p className="text-sm text-red-600">{resetVerifyForm.formState.errors.newPassword.message}</p>
                  )}
                </div>

                <div className="flex gap-2 col-span-2">
                  <Button type="submit" className="w-full" disabled={loading || resetVerifyForm.formState.isSubmitting}>
                    Reset password
                  </Button>
                  <Button
                    type="button"
                    variant="ghost"
                    className="w-full"
                    onClick={() => {
                      setResetStep("request");
                      resetVerifyForm.reset();
                    }}
                  >
                    Start again
                  </Button>
                </div>
              </form>
            )}
          </div>
        )}
      </div>

      <div className="px-6 text-center text-sm text-muted-foreground">
        By continuing, you agree to our <a className="text-primary underline" href="#">Terms of Service</a> and <a className="text-primary underline" href="#">Privacy Policy</a>.
      </div>
    </div>
  );
}
