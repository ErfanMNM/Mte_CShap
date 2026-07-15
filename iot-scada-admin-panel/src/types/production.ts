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
  passCount: number; // alias for passTotal
  passTotal: number;
  failCount: number; // alias for failTotal
  failTotal: number;
  duplicateCount: number;
  notFoundCount: number;
  readFailCount: number;
  errorCount: number;
  timeoutCount: number;
  cartonCount: number;
  cartonClosedCount: number;
  currentCartonId: number;
  currentCartonCode: string;
  itemsInCarton: number;
  cartonCapacity: number;
  progressPercent: number;
}

export interface ActiveCounterSnapshot {
  PassTotal: number;
  FailTotal: number;
  DuplicateCount: number;
  NotFoundCount: number;
  ReadFailCount: number;
  ErrorCount: number;
  TimeoutCount: number;
  TotalCount: number;
  CartonID: number;
  CartonCode: string;
}

export interface ProductionStateResponse {
  success: boolean;
  state: string;
  previousState: string;
  orderNo: string;
  productName: string;
  productionDate: string;
  orderQty: number;
  activeCounter: ActiveCounterSnapshot;
  cartonCount: number;
  cartonClosedCount: number;
  itemsInCarton: number;
  cartonCapacity: number;
  progressPercent: number;
  hasPO: boolean;
  codesCount: number;
  cartonsCount: number;
  lastWarning: string;
  isAppReady: boolean;
  isDeviceReady: boolean;
}
