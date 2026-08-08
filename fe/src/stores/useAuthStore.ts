import { create } from 'zustand';
import { toast } from 'sonner';
import { authService } from '@/services/authService';
import type { AuthState } from '@/types/store';
import { persist, createJSONStorage } from 'zustand/middleware';

export const useAuthStore = create<AuthState>()(
    persist(
        (set) => ({
            token: null,
            user: null,
            email: null,
            loading: false,

            setAuth: async (token, user, email) => {
                set({ token, user, email });
            },

            logOut: async () => {
                try {
                    set({ loading: true });
                    set({ token: null, email: null, user: null });

                    await authService.logOut();
                    toast.success('Logged out successfully!');
                } catch (error) {
                    console.error('Logout failed:', error);
                    toast.error('Logout failed. Please try again.');
                } finally {
                    set({ loading: false });
                }
            },

            login: async (email: string, password: string) => {
                try {
                    set({ loading: true });
                    const { token } = await authService.login(email, password);

                    set({ token, email });
                    toast.success('Welcome back!');
                    return true;
                } catch (error) {
                    console.error(error);
                    const message = error instanceof Error ? error.message : 'Login failed. Please try again.';
                    toast.error(message);
                    return false;
                } finally {
                    set({ loading: false });
                }
            },

            register: async (email: string, password: string) => {
                try {
                    set({ loading: true });
                    await authService.register(email, password);

                    toast.success('Account created successfully! Please sign in.');
                    return true;
                } catch (error) {
                    console.error(error);
                    const message = error instanceof Error ? error.message : 'Registration failed. Please try again.';
                    toast.error(message);
                    return false;
                } finally {
                    set({ loading: false });
                }
            },

            requestPasswordReset: async (email: string) => {
                try {
                    set({ loading: true });
                    await authService.requestPasswordReset(email);

                    toast.success('OTP sent successfully.');
                    return true;
                } catch (error) {
                    console.error(error);
                    const message = error instanceof Error ? error.message : 'Failed to send OTP. Please try again.';
                    toast.error(message);
                    return false;
                } finally {
                    set({ loading: false });
                }
            },

            resetPassword: async (email: string, otp: string, newPassword: string) => {
                try {
                    set({ loading: true });
                    await authService.resetPassword(email, otp, newPassword);

                    toast.success('Password updated successfully.');
                    return true;
                } catch (error) {
                    console.error(error);
                    const message = error instanceof Error ? error.message : 'Failed to reset password. Please try again.';
                    toast.error(message);
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
                    console.error('Failed to fetch user data. Please try again.', error);
                    set({ email: null, token: null, user: null });
                } finally {
                    set({ loading: false });
                }
            },
        }),
        {
            name: 'auth-storage',
            storage: createJSONStorage(() => localStorage),
        }
    )
);