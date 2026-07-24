"use client"

import { useState } from "react";
import { cn } from "@/lib/utils";
import { Label } from "./ui/label";
import { Input } from "./ui/input";
import { Button } from "./ui/button";
import { InputOTP, InputOTPGroup, InputOTPSlot } from "./ui/input-otp";
import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useAuthStore } from "@/stores/useAuthStore";
import { useNavigate } from "react-router";


const emailSchema = z.object({
  email: z.string().email({ message: "Invalid email address" }),
})

const otpSchema = z.object({
  otp: z.string().length(6, { message: "OTP must be 6 digits" }),
})

type EmailFormValues = z.infer<typeof emailSchema>
type OtpFormValues = z.infer<typeof otpSchema>

export function LoginForm({
  className,
  ...props
}: React.ComponentProps<"div">) {
  const authEmail = useAuthStore((state) => state.authEmail)
  const authOtp = useAuthStore((state) => state.authOtp)
  const [step, setStep] = useState<"email" | "otp">("email")
  const [submittedEmail, setSubmittedEmail] = useState("")

  const navigate = useNavigate();

  const emailForm = useForm<EmailFormValues>({
    resolver: zodResolver(emailSchema),
    defaultValues: {
      email: "",
    },
  })

  const otpForm = useForm<OtpFormValues>({
    resolver: zodResolver(otpSchema),
    defaultValues: {
      otp: "",
    },
  })

  const onEmailSubmit = async (data: EmailFormValues) => {
    const { email } = data;
    await authEmail(email);
    setStep("otp");
    setSubmittedEmail(email);
    console.log("Email submitted:", email);

  }

  const onOtpSubmit = async (data: OtpFormValues) => {
    await authOtp(submittedEmail, data.otp);
    console.log("Verify code", data.otp, "for", submittedEmail)
    navigate("/");
  }

  return (
    <div className={cn("flex flex-col gap-6", className)} {...props}>
      <form
        onSubmit={
          step === "email"
            ? emailForm.handleSubmit(onEmailSubmit)
            : otpForm.handleSubmit(onOtpSubmit)
        }
      >
        <div className="flex flex-col gap-6">
          <div className="flex flex-col items-center text-center gap-2">
            <a href="/" className="mx-auto block w-fit text-center">
              <img src="src/assets/logo.svg" alt="Logo" className="h-25  w-25" />
            </a>
            <h1 className="text-2xl font-bold">Verify Your Email</h1>
            <p className="text-muted-foreground text-balance">
              {step === "email"
                ? "Enter the email to receive a verification code."
                : `We sent a 6-digit code to ${submittedEmail}.`}
            </p>
          </div>

          {step === "email" ? (
            <div className="w-full max-w-md mx-auto">
              <div className="space-y-2">
                <Label htmlFor="email" className="w-24 text-right text-lg font-medium">
                  Email
                </Label>
                <Input
                  id="email"
                  type="email"
                  placeholder="your@example.com"
                  {...emailForm.register("email")}
                />
                {emailForm.formState.errors.email && (
                  <p className="text-sm text-red-600">
                    {emailForm.formState.errors.email.message}
                  </p>
                )}
              </div>
            </div>
          ) : (
            <div className="w-full max-w-md mx-auto space-y-4">
              <div className="space-y-2">
                <Label htmlFor="otp" className="text-lg font-medium">
                  Verification code
                </Label>
                <InputOTP
                  id="otp"
                  maxLength={6}
                  value={otpForm.watch("otp") || ""}
                  onChange={(value) => otpForm.setValue("otp", value, { shouldValidate: true })}
                  containerClassName="justify-center gap-2"
                >
                  <InputOTPGroup>
                    {Array.from({ length: 6 }).map((_, index) => (
                      <InputOTPSlot key={index} index={index} />
                    ))}
                  </InputOTPGroup>
                </InputOTP>
                {otpForm.formState.errors.otp && (
                  <p className="text-sm text-red-600">
                    {otpForm.formState.errors.otp.message}
                  </p>
                )}
              </div>
            </div>
          )}

          <div className="w-full max-w-md mx-auto flex flex-col gap-3">
            <Button
              type="submit"
              className="w-full"
              disabled={step === "email" ? emailForm.formState.isSubmitting : otpForm.formState.isSubmitting}
            >
              {step === "email" ? "Send Verification Code" : "Verify Code"}
            </Button>

            {step === "otp" && (
              <Button
                type="button"
                variant="ghost"
                className="w-full"
                onClick={() => setStep("email")}
              >
                Change email
              </Button>
            )}
          </div>
        </div>
      </form>

      <div className="px-6 text-center">
        By clicking continue, you agree to our <a href="#">Terms of Service</a>{" "}
        and <a href="#">Privacy Policy</a>.
      </div>
    </div>
  )
}
