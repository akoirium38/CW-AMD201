import api from '@/lib/axios'

export const authService = {
    register: async (gmail: string, password: string) => {
        const res = await api.post('/auth/register', { gmail, password });
        if (!res.data.success) {
            throw new Error(res.data.message || 'Registration failed');
        }
        return res.data;
    },

    login: async (gmail: string, password: string) => {
        const res = await api.post('/auth/login', { gmail, password });
        if (!res.data.token) {
            throw new Error(res.data.message || 'Login failed');
        }
        return res.data;
    },

    requestPasswordReset: async (email: string) => {
        const res = await api.post('/auth/request-password-reset', { email });
        if (!res.data.success) {
            throw new Error(res.data.message || 'Failed to request password reset');
        }
        return res.data;
    },

    resetPassword: async (gmail: string, otp: string, newPassword: string) => {
        const res = await api.post('/auth/reset-password', {
            gmail,
            otp,
            newPassword,
        });
        if (!res.data.success) {
            throw new Error(res.data.message || 'Failed to reset password');
        }
        return res.data;
    },

    logOut: async () => {
        try {
            const res = await api.post('/auth/logout', {}, { withCredentials: true });
            return res.data;
        } catch (error) {
            return { success: false };
        }
    },
};