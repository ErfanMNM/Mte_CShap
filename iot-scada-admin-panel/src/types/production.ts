export interface StartProductionRequest {
  orderNo: string;
  userName?: string;
}

export interface UpdateProductionDateRequest {
  productionDate: string;
  userName?: string;
}

export interface ProductionControlResponse {
  success: boolean;
  message: string;
}

export interface RetryRunResponse {
  success: boolean;
  message: string;
  availableCodes: number;
  orderQty: number;
  neededCodes: number;
}

export interface ProductionStatusResponse {
  success: boolean;
  message?: string;
  state: string;
  hasPO: boolean;
  orderNo: string | null;
  productName: string | null;
  productionDate: string | null;
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

export interface ActiveCounterSnapshot {
  PassCount: number;
  FailCount: number;
  DuplicateCount: number;
  CartonID: number;
  CartonCode: string;
}

export interface ProductionStateResponse {
  success: boolean;
  state: string;
  previousState: string;
  orderNo: string;
  productName: string;
  orderQty: number;
  activeCounter: ActiveCounterSnapshot;
  codesCount: number;
  cartonsCount: number;
  lastWarning: string;
  isAppReady: boolean;
  isDeviceReady: boolean;
}
