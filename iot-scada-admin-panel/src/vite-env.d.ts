/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_URL?: string;
  readonly VITE_WS_URL?: string;
  readonly VITE_VNQR_WS_URL?: string;
  readonly VITE_PO_API_URL?: string;
  readonly VITE_CAMERA_WS_URL?: string;
  readonly VITE_PLC_WS_URL?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
