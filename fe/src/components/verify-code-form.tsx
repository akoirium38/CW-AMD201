"use client"

import { cn } from "@/lib/utils"
import { Label } from "./ui/label"
import { Input } from "./ui/input"
import { Button } from "./ui/button"
import {z} from "zod"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { InputOTP } from "./ui/input-otp"
import { REGEXP_ONLY_DIGITS_AND_CHARS } from "input-otp"


const emailSchema = z.object({
  email: z.string().email({ message: "Invalid email address" }),
});



type EmailFormValues = z.infer<typeof emailSchema>;

export function LoginForm({
  className,
  ...props
}: React.ComponentProps<"div">) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<EmailFormValues>({
    resolver: zodResolver(emailSchema),
  });

  const onSubmit = async (data: EmailFormValues) => {
    // Handle form submission logic here
    console.log(data);
  }

  return (
    <div className={cn("flex flex-col gap-6", className)} {...props}>
      <InputOTP maxLength={6} pattern={REGEXP_ONLY_DIGITS_AND_CHARS}>
      </InputOTP>
      <div className="px-6 text-center">
        By clicking continue, you agree to our <a href="#">Terms of Service</a>{" "}
        and <a href="#">Privacy Policy</a>.
      </div>
    </div>
  )
}
