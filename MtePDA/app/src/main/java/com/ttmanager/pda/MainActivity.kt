package com.ttmanager.pda

import android.content.Context
import android.content.Intent
import android.content.SharedPreferences
import android.os.Bundle
import android.view.inputmethod.EditorInfo
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.isVisible
import androidx.lifecycle.Lifecycle
import androidx.lifecycle.lifecycleScope
import androidx.lifecycle.repeatOnLifecycle
import androidx.recyclerview.widget.LinearLayoutManager
import com.google.android.material.snackbar.Snackbar
import com.ttmanager.pda.databinding.ActivityMainBinding
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.isActive
import kotlinx.coroutines.launch
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

enum class ScanMode { QUET_THUNG, KIEM_TRA }

class MainActivity : AppCompatActivity() {

    private lateinit var binding: ActivityMainBinding
    private lateinit var scanAdapter: ScanAdapter
    private lateinit var scanManager: ScanManager
    private lateinit var apiClient: ApiClient
    private lateinit var prefs: SharedPreferences

    private val history = mutableListOf<ScanHistoryItem>()
    private var totalScans = 0
    private var todayScans = 0
    private var last24hScans = 0
    private var healthCheckJob: Job? = null
    private var isServerOnline = false
    private var currentMode = ScanMode.QUET_THUNG

    private val dateFormatter = SimpleDateFormat("HH:mm:ss", Locale.getDefault())
    private val fullDateFormatter = SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.getDefault())
    private val todayDateFormatter = SimpleDateFormat("yyyyMMdd", Locale.getDefault())

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityMainBinding.inflate(layoutInflater)
        setContentView(binding.root)

        prefs = getSharedPreferences("mtepda_prefs", Context.MODE_PRIVATE)
        scanManager = ScanManager()
        apiClient = ApiClient()

        loadSettings()
        setupRecyclerView()
        setupListeners()
        startHealthCheck()
    }

    override fun onResume() {
        super.onResume()
        // Reload settings in case they changed in SettingsActivity
        loadSettings()
        scanManager.initScan()
        scanManager.setOnScanReadListener { code, type ->
            runOnUiThread {
                handleScan(code, type)
            }
        }
    }

    override fun onPause() {
        super.onPause()
        scanManager.release()
    }

    private fun loadSettings() {
        val savedIp = prefs.getString("server_ip", "") ?: ""
        val savedPort = prefs.getString("server_port", "9999")?.toIntOrNull() ?: 9999
        val savedMode = prefs.getString("default_mode", "QUET_THUNG") ?: "QUET_THUNG"

        currentMode = if (savedMode == "QUET_THUNG") ScanMode.QUET_THUNG else ScanMode.KIEM_TRA

        if (currentMode == ScanMode.QUET_THUNG) {
            binding.tvScanModeLabel.text = getString(R.string.mode_quet_thung)
            binding.tvScanModeLabel.setBackgroundResource(R.drawable.badge_mode_quet)
        } else {
            binding.tvScanModeLabel.text = getString(R.string.mode_kiem_tra)
            binding.tvScanModeLabel.setBackgroundResource(R.drawable.badge_mode_kiem_tra)
        }

        if (savedIp.isNotEmpty()) {
            apiClient.configure(savedIp, savedPort)
        }
    }

    private fun configureApi() {
        val ip = prefs.getString("server_ip", "") ?: ""
        val port = prefs.getString("server_port", "9999")?.toIntOrNull() ?: 9999
        if (ip.isNotEmpty()) {
            apiClient.configure(ip, port)
        }
    }

    private fun setupRecyclerView() {
        scanAdapter = ScanAdapter()
        binding.rvHistory.apply {
            layoutManager = LinearLayoutManager(this@MainActivity)
            adapter = scanAdapter
        }
    }

    private fun setupListeners() {
        // Nut Cai Dat
        binding.btnSettings.setOnClickListener {
            startActivity(Intent(this, SettingsActivity::class.java))
        }

        // Nut Gui
        binding.btnSend.setOnClickListener { sendManualCode() }

        // Phim tren o ma code
        binding.etCode.setOnEditorActionListener { _, actionId, _ ->
            if (actionId == EditorInfo.IME_ACTION_SEND || actionId == EditorInfo.IME_ACTION_GO) {
                sendManualCode()
                true
            } else false
        }

        // Mode: QUET THUNG
        binding.btnScan.setOnClickListener {
            currentMode = ScanMode.QUET_THUNG
            binding.tvScanModeLabel.text = getString(R.string.mode_quet_thung)
            binding.tvScanModeLabel.setBackgroundResource(R.drawable.badge_mode_quet)
            prefs.edit().putString("default_mode", "QUET_THUNG").apply()
            configureApi()
            scanManager.startScan()
        }

        // Mode: KIEM TRA THUNG
        binding.btnKiemTra.setOnClickListener {
            currentMode = ScanMode.KIEM_TRA
            binding.tvScanModeLabel.text = getString(R.string.mode_kiem_tra)
            binding.tvScanModeLabel.setBackgroundResource(R.drawable.badge_mode_kiem_tra)
            prefs.edit().putString("default_mode", "KIEM_TRA").apply()
            configureApi()
            scanManager.startScan()
        }

        // Nut Xoa lich su
        binding.btnClearHistory.setOnClickListener {
            history.clear()
            scanAdapter.submitList(history.toList())
            binding.emptyState.isVisible = history.isEmpty()
            binding.rvHistory.isVisible = history.isNotEmpty()
            totalScans = 0
            todayScans = 0
            last24hScans = 0
            updateStats()
        }
    }

    private fun handleScan(code: String, barcodeType: String) {
        if (code.isBlank()) return
        binding.etCode.setText(code)

        val ip = prefs.getString("server_ip", "") ?: ""
        if (ip.isBlank()) {
            showSnack(getString(R.string.msg_no_ip))
            return
        }

        when (currentMode) {
            ScanMode.QUET_THUNG -> sendCartonScan(code, barcodeType)
            ScanMode.KIEM_TRA -> sendCartonInfo(code, barcodeType)
        }
    }

    private fun sendCartonScan(code: String, barcodeType: String) {
        val pdaName = prefs.getString("pda_name", getString(R.string.default_pda_name))
            ?: getString(R.string.default_pda_name)
        val timeStr = dateFormatter.format(Date())

        totalScans++
        todayScans++
        last24hScans++
        updateStats()

        val historyItem = ScanHistoryItem(
            code = code,
            time = timeStr,
            pdaName = pdaName,
            sendStatus = SendStatus.PENDING,
            barcodeType = barcodeType
        )
        addToHistory(historyItem)

        lifecycleScope.launch {
            val scannedAt = fullDateFormatter.format(Date())
            val result = apiClient.postCartonScan(pdaName, code, scannedAt, "scan")
            result.fold(
                onSuccess = { response ->
                    updateHistoryStatus(code, timeStr, if (response.success) SendStatus.SUCCESS else SendStatus.ERROR)
                    val msg = if (response.success) {
                        getString(
                            R.string.carton_scan_ok,
                            response.status,
                            response.cartonIndex,
                            response.message
                        )
                    } else {
                        response.message
                    }
                    showSnack(msg, isError = !response.success)
                },
                onFailure = { error ->
                    updateHistoryStatus(code, timeStr, SendStatus.ERROR)
                    showSnack(getString(R.string.msg_error, error.message ?: "Lỗi không xác định"), isError = true)
                }
            )
        }
    }

    private fun sendCartonInfo(code: String, barcodeType: String) {
        val pdaName = prefs.getString("pda_name", getString(R.string.default_pda_name))
            ?: getString(R.string.default_pda_name)
        val timeStr = dateFormatter.format(Date())

        totalScans++
        todayScans++
        last24hScans++
        updateStats()

        val historyItem = ScanHistoryItem(
            code = code,
            time = timeStr,
            pdaName = pdaName,
            sendStatus = SendStatus.PENDING,
            barcodeType = barcodeType
        )
        addToHistory(historyItem)

        lifecycleScope.launch {
            val result = apiClient.getCartonInfo(code)
            result.fold(
                onSuccess = { response ->
                    updateHistoryStatus(code, timeStr, if (response.success) SendStatus.SUCCESS else SendStatus.ERROR)
                    val msg = if (response.success) {
                        getString(
                            R.string.carton_info_ok,
                            response.status,
                            response.cartonIndex,
                            response.productCount,
                            response.activateUser,
                            response.activateDate
                        )
                    } else {
                        response.message
                    }
                    showSnack(msg, isError = !response.success)
                },
                onFailure = { error ->
                    updateHistoryStatus(code, timeStr, SendStatus.ERROR)
                    showSnack(getString(R.string.msg_error, error.message ?: "Lỗi không xác định"), isError = true)
                }
            )
        }
    }

    private fun sendManualCode() {
        val code = binding.etCode.text?.toString()?.trim()
        if (code.isNullOrBlank()) {
            showSnack(getString(R.string.msg_no_code))
            return
        }
        configureApi()
        handleScan(code, "MANUAL")
    }

    private fun addToHistory(item: ScanHistoryItem) {
        history.add(0, item)
        if (history.size > 50) history.removeAt(history.size - 1)
        scanAdapter.submitList(history.toList())
        binding.emptyState.isVisible = history.isEmpty()
        binding.rvHistory.isVisible = history.isNotEmpty()
        binding.tvScanCount.text = getString(R.string.scan_count_format, history.size)
    }

    private fun updateHistoryStatus(code: String, time: String, status: SendStatus) {
        val idx = history.indexOfFirst { it.code == code && it.time == time }
        if (idx >= 0) {
            history[idx] = history[idx].copy(sendStatus = status)
            scanAdapter.submitList(history.toList())
        }
    }

    private fun updateStats() {
        binding.tvTotalScans.text = totalScans.toString()
        binding.tvTodayScans.text = todayScans.toString()
        binding.tvLast24hScans.text = last24hScans.toString()
    }

    private fun startHealthCheck() {
        healthCheckJob?.cancel()
        healthCheckJob = lifecycleScope.launch {
            repeatOnLifecycle(Lifecycle.State.STARTED) {
                while (isActive) {
                    checkServerHealth()
                    delay(10000)
                }
            }
        }
    }

    private suspend fun checkServerHealth() {
        configureApi()
        val result = apiClient.checkHealth()
        isServerOnline = result.isSuccess

        runOnUiThread {
            if (isServerOnline) {
                binding.statusDot.setBackgroundResource(R.drawable.circle_online)
                binding.tvServerStatus.text = getString(R.string.status_online)
            } else {
                binding.statusDot.setBackgroundResource(R.drawable.circle_offline)
                binding.tvServerStatus.text = getString(R.string.status_offline)
            }
        }
    }

    private fun showSnack(message: String, isError: Boolean = false) {
        val view = binding.root
        val bgColor = if (isError) getColor(R.color.error) else getColor(R.color.success)
        Snackbar.make(view, message, Snackbar.LENGTH_SHORT)
            .setBackgroundTint(bgColor)
            .setTextColor(getColor(R.color.white))
            .show()
    }
}
