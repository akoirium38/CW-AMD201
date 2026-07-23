"use client"

import { cn } from "@/lib/utils"
import { Label } from "./ui/label"
import { Input } from "./ui/input"
import { Button } from "./ui/button"
import {z} from "zod"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"


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
      <form onSubmit={handleSubmit(onSubmit)}>
        <div className="flex flex-col gap-6">
          {/* Header and Logo */}
          <div className="flex flex-col items-center text-center gap-2">
            <a href="/" className="mx-auto block w-fit text-center">
              <img src="src/assets/logo.svg" alt="Logo" className="h-25  w-25" />
            </a>
            <h1 className="text-2xl font-bold">Verify Your Email</h1>
            <p className="text-muted-foreground text-balance">
              Enter the email to receive a verification code.
            </p>
          </div>
          {/* Email Input */}
          <div className="w-full max-w-md mx-auto">
            <div className="space-y-2">
              <Label htmlFor="email" className="w-24 text-right text-lg font-medium">
                Email
              </Label>
              <Input
                id="email"
                type="email"
                placeholder="your@example.com"
                {...register("email")}
              />
              {errors.email && (
                <p className="text-sm text-red-600">{errors.email.message}</p>
              )}

            </div>
          </div>
          {/* Submit Button */}
          <div className="w-full max-w-md mx-auto">
            <Button type="submit" className="w-full" disabled={isSubmitting}>
              Send Verification Code
            </Button>
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
