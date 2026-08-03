import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'
import tailwindcss from "@tailwindcss/vite"

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server:{
    port: 7070
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    proxy: {
      '/api/auth': {
        target: 'http://localhost:5186',
        changeOrigin: true,
      },
      '/api/files': {
        target: 'http://localhost:5186',
        changeOrigin: true,
      },
    },
  },
})
