package com.ttmanager.pda

import android.util.Log
import com.android.decode.BarcodeManager
import com.android.decode.DecodeResult
import com.android.decode.ReadListener
import com.android.decode.StartListener
import com.android.decode.StopListener
import com.android.decode.TimeoutListener

class ScanManager {

    companion object {
        private const val TAG = "ScanManager"
    }

    private var barcodeManager: BarcodeManager? = null
    private var onScanRead: ((String, String) -> Unit)? = null

    private val readListener = object : ReadListener {
        override fun onRead(decodeResult: DecodeResult?) {
            decodeResult?.let {
                val text = it.text ?: ""
                val barcodeId = it.barcodeID?.toString() ?: "UNKNOWN"
                Log.d(TAG, "onRead: [$barcodeId] $text")
                onScanRead?.invoke(text, barcodeId)
            }
        }
    }

    private val startListener = object : StartListener {
        override fun onScanStarted() {
            Log.d(TAG, "onScanStarted")
        }
    }

    private val stopListener = object : StopListener {
        override fun onScanStopped() {
            Log.d(TAG, "onScanStopped")
        }
    }

    private val timeoutListener = object : TimeoutListener {
        override fun onScanTimeout() {
            Log.d(TAG, "onScanTimeout")
        }
    }

    fun setOnScanReadListener(callback: (code: String, barcodeType: String) -> Unit) {
        onScanRead = callback
    }

    fun initScan() {
        try {
            barcodeManager?.release()
            barcodeManager = BarcodeManager()
            val initialized = barcodeManager?.isInitialized ?: false
            Log.d(TAG, "BarcodeManager initialized: $initialized")

            barcodeManager?.addReadListener(readListener)
            barcodeManager?.addStartListener(startListener)
            barcodeManager?.addStopListener(stopListener)
            barcodeManager?.addTimeoutListener(timeoutListener)
        } catch (e: Exception) {
            Log.e(TAG, "Failed to init scan: ${e.message}")
        }
    }

    fun startScan(timeoutMs: Int = 30000) {
        try {
            barcodeManager?.startDecode(timeoutMs)
            Log.d(TAG, "startScan called, timeout=$timeoutMs ms")
        } catch (e: Exception) {
            Log.e(TAG, "startScan failed: ${e.message}")
        }
    }

    fun stopScan() {
        try {
            barcodeManager?.stopDecode()
            Log.d(TAG, "stopScan called")
        } catch (e: Exception) {
            Log.e(TAG, "stopScan failed: ${e.message}")
        }
    }

    fun release() {
        try {
            barcodeManager?.stopDecode()
            barcodeManager?.removeReadListener(readListener)
            barcodeManager?.removeStartListener(startListener)
            barcodeManager?.removeStopListener(stopListener)
            barcodeManager?.removeTimeoutListener(timeoutListener)
            barcodeManager?.release()
            barcodeManager = null
            Log.d(TAG, "ScanManager released")
        } catch (e: Exception) {
            Log.e(TAG, "release failed: ${e.message}")
        }
    }

    fun isInitialized(): Boolean = barcodeManager?.isInitialized ?: false
}
