import tailwindcss from '@tailwindcss/vite';
import react from '@vitejs/plugin-react';
import path from 'path';
import {defineConfig} from 'vite';

export default defineConfig(() => {
  return {
    plugins: [react(), tailwindcss()],
    resolve: {
      alias: {
        '@': path.resolve(__dirname, '.'),
      },
      // Force a single React instance across CJS shims + ESM to prevent
      // "Invalid hook call" when useCameraSocket etc. import React from
      // a different vite-deps chunk than react-dom_client does.
      dedupe: ['react', 'react-dom'],
    },
    optimizeDeps: {
      // Pre-bundle these so Vite resolves them through the same optimized
      // chunk (and won't fall back to per-file CJS wrappers that load a
      // second copy of React).
      include: ['react', 'react-dom', 'react-dom/client'],
    },
    server: {
      // HMR is disabled in AI Studio via DISABLE_HMR env var.
      // Do not modifyâfile watching is disabled to prevent flickering during agent edits.
      hmr: process.env.DISABLE_HMR !== 'true',
      // Disable file watching when DISABLE_HMR is true to save CPU during agent edits.
      watch: process.env.DISABLE_HMR === 'true' ? null : {},
    },
  };
});
