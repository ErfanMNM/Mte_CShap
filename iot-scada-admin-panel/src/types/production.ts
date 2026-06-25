export interface StartProductionRequest {
  orderNo: string;
  userName?: string;
}

export interface ProductionControlResponse {
  success: boolean;
  message: string;
}

export interface ProductionStatusResponse {
  success: boolean;
  message?: string;
  state: string;
  hasPO: boolean;
  orderNo: string | null;
  productName: string | null;
  orderQty: number;
  totalCount: number;
  passCount: number;
  failCount: number;
  duplicateCount: number;
  cartonCount: number;
  cartonClosedCount: number;
  currentCartonId: number;
  currentCartonCode: string;
  itemsInCarton: number;
  cartonCapacity: number;
  progressPercent: number;
}
