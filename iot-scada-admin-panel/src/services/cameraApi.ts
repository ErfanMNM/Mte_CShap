import datapoolApi from "./datapoolApi";
import { productionApi } from "./productionApi";

/**
 * Khi nhận event Received từ camera active, đẩy code vào datapool
 * (đánh dấu Used) rồi kéo lại status production.
 *
 * @param code      Mã code được camera quét
 * @param poolName  Tên pool đích (mặc định "active")
 * @param batchID   Batch ID - mặc định dùng orderNo PO hiện tại
 */
export async function handleActiveCodeScanned(
  code: string,
  poolName = "active",
  batchID?: string
): Promise<void> {
  if (!code) return;

  const effectiveBatch = batchID || "default";
  await datapoolApi.addFromReader({
    poolName,
    code,
    batchID: effectiveBatch,
    note: "ws-camera",
  });

  // Refresh production state - backend state machine có thể đã tự advance
  try {
    await productionApi.getStatus();
  } catch {
    // ignore - polling ở view sẽ tự cập nhật
  }
}
