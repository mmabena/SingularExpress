import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// Vite config. In development, calls to /api are forwarded to the Server
// (dotnet run, https profile), so the Client never needs the API URL hard-coded.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/api": {
        target: "https://localhost:7147",
        changeOrigin: true,
        secure: false, // the local dev HTTPS certificate is self-signed
      },
    },
  },
  test: {
    environment: "jsdom",
    globals: true,
    setupFiles: "./src/setupTests.js",
    css: false,
  },
});
