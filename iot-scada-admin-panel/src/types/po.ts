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
  stats?: POStats;
}

export interface POStats {
  activeCodes: number;
  packedCodes: number;
  cartonCount: number;
  closedCartons: number;
  progressPercent: number;
}

export interface POListItem {
  orderNo: string;
  productName?: string;
  orderQty: number;
  productionDate?: string;
  gtin?: string;
  status?: string;
  createdTime?: string;
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
  cartonCapacity?: number;
}

export interface CreatePOResponse {
  success: boolean;
  message: string;
  orderNo?: string;
  loadedCodesCount?: number;
  createdCartonsCount?: number;
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
  timestamp?: string;
  appState?: string;
}

export interface POCode {
  id: number;
  code: string;
  status: number;
  cartonCode?: string;
  activateDate?: string;
  activateUser?: string;
  packingDate?: string;
}

export interface POCodeListResponse {
  success: boolean;
  count?: number;
  data?: POCode[];
  message?: string;
}

export interface POCarton {
  id: number;
  cartonCode?: string;
  startDatetime?: string;
  completedDatetime?: string;
  activateUser?: string;
  productionDate?: string;
  status?: string;
  codeCount?: number;
}

export interface POCartonListResponse {
  success: boolean;
  count?: number;
  data?: POCarton[];
  message?: string;
}

export interface ProductionStatusResponse {
  success: boolean;
  message?: string;
  state?: string;
  hasPO?: boolean;
  orderNo?: string;
  productName?: string;
  orderQty?: number;
  totalCodes?: number;
  activeCodes?: number;
  packedCodes?: number;
  cartonCount?: number;
  closedCartonCount?: number;
  progressPercent?: number;
}

export interface ApiResponse {
  success: boolean;
  message: string;
  count?: number;
}

export interface APIResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  count?: number;
}
