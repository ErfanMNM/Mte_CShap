// TypeScript interfaces for Production Order (PO) Management

export interface POInfo {
  orderNo: string;
  site?: string;
  factory?: string;
  productionLine?: string;
  productionDate?: string;
  shift?: string;
  orderQty: number;
  lotNumber?: string;
  productCode?: string;
  productName?: string;
  gtin?: string;
  customerOrderNo?: string;
  uom?: string;
  createdTime?: string;
  modifiedTime?: string;
}

export interface POListItem {
  orderNo: string;
  productName?: string;
  orderQty: number;
  productionDate?: string;
  status?: string;
}

export interface CreatePORequest {
  orderNo: string;
  site?: string;
  factory?: string;
  productionLine?: string;
  productionDate?: string;
  shift?: string;
  orderQty: number;
  lotNumber?: string;
  productCode?: string;
  productName?: string;
  gtin?: string;
  customerOrderNo?: string;
  uom?: string;
  userName?: string;
  autoLoadCodes?: boolean;
}

export interface CreatePOResponse {
  success: boolean;
  message: string;
  orderNo?: string;
  loadedCodesCount?: number;
}

export interface PODetailResponse {
  success: boolean;
  data?: POInfo;
  message?: string;
}

export interface POListResponse {
  success: boolean;
  count?: number;
  data?: POListItem[];
  message?: string;
}

export interface HealthResponse {
  status: string;
  timestamp: string;
  appState: string;
}

export interface APIResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  count?: number;
}
