import {create } from "zustand";
import {toast} from "sonner";
import { authService } from "@/services/authService";
import type { AuthState } from "@/types/store";
import { persist, createJSONStorage } from 'zustand/middleware';

export const useAuthStore = create<AuthState>()(
    persist(
        (set, get) => ({
            token: null,
            user: null, 
            email: null,
            loading: false,


            setAuth: async (token, user) => {
                set({ token, user });
            },


            logout: async () => {
                set({ token: null, email: null, user: null });
            },

            authEmail: async (email: string) => {
                try {
                    set({ loading: true });
                    
                    await authService.authEmail(email);

                    toast.success("OTP sent to your email!");
                    return true;

                } catch (error) {
                    console.error(error);
                    toast.error("Failed to send OTP. Please check your email and try again.");
                    return false;
                } finally {
                    set({ loading: false });
                }
            },

            authOtp: async (email: string, code: string) => {
                try {
                    set({ loading: true });
                    const { token } = await authService.authOtp(email, code);
                    
                    set({ token });

                    await get().fetchMe();

                    toast.success("Welcome to FileHub🎉");
                    return true;
                } catch (error) {
                    console.error(error);
                    toast.error("Failed to verify OTP. Please try again.");
                    return false;
                } finally {
                    set({ loading: false });
                }
            },

            fetchMe: async () => {
                try {
                    set({ loading: true }); 
                    const email = await authService.fetchMe();
                    
                    set({ email }); 
                    
                } catch (error) {
                    console.error("Failed to fetch user data. Please try again.", error);

                    set({ email: null, token: null, user: null }); 
                    toast.error("Failed to fetch user data. Please try again.");

                } finally {
                    set({ loading: false });
                }
            }
        }),
        {
            name: 'auth-storage', 
            storage: createJSONStorage(() => localStorage), 
        }
    )
);