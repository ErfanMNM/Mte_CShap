// TypeScript interfaces for DataPool API

export interface DataPoolInfo {
  name: string;
  fileName: string;
  size: number;
}

export interface DataPoolCode {
  code: string;
  status: number;
  batchID?: string;
  note?: string;
  importedTime?: string;
  usedTime?: string;
  usedBy?: string;
}

export interface DataPoolListResponse {
  success: boolean;
  count?: number;
  data?: DataPoolInfo[];
  message?: string;
}

export interface DataPoolCodesResponse {
  success: boolean;
  count?: number;
  data?: DataPoolCode[];
  message?: string;
}

export interface AddCodeRequest {
  poolName: string;
  code: string;
  status?: number;
  batchID?: string;
  note?: string;
  userName?: string;
}

export interface APIResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  count?: number;
}
