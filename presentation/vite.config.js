import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
    base: "./",
    plugins: [react()],
    build: {
        license: { fileName: "dependency-licenses.md" },
        rolldownOptions: {
            output: { comments: { legal: true } },
        },
    },
});
