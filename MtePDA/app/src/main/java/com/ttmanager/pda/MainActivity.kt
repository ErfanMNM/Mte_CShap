package com.ttmanager.pda

import android.content.Context
import android.content.SharedPreferences
import android.os.Bundle
import android.view.inputmethod.EditorInfo
import android.view.inputmethod.InputMethodManager
import androidx.activity.viewModels
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

    private val dateFormatter = SimpleDateFormat("HH:mm:ss", Locale.getDefault())
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
        val savedPdaName = prefs.getString("pda_name", getString(R.string.default_pda_name)) ?: getString(R.string.default_pda_name)
        binding.etServerIp.setText(savedIp)
        binding.etPdaName.setText(savedPdaName)
        configureApi()
    }

    private fun configureApi() {
        val ip = binding.etServerIp.text.toString().trim()
        if (ip.isNotEmpty()) {
            apiClient.configure(ip)
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
        binding.etServerIp.setOnFocusChangeListener { _, hasFocus ->
            if (!hasFocus) {
                prefs.edit().putString("server_ip", binding.etServerIp.text.toString()).apply()
                configureApi()
            }
        }

        binding.etPdaName.setOnFocusChangeListener { _, hasFocus ->
            if (!hasFocus) {
                prefs.edit().putString("pda_name", binding.etPdaName.text.toString()).apply()
            }
        }

        binding.btnSend.setOnClickListener { sendManualCode() }

        binding.etCode.setOnEditorActionListener { _, actionId, _ ->
            if (actionId == EditorInfo.IME_ACTION_SEND || actionId == EditorInfo.IME_NULL) {
                sendManualCode()
                true
            } else false
        }

        binding.btnScan.setOnClickListener {
            if (binding.etServerIp.text.isNullOrBlank()) {
                showSnack(getString(R.string.msg_no_ip))
                binding.etServerIp.requestFocus()
                return@setOnClickListener
            }
            prefs.edit()
                .putString("server_ip", binding.etServerIp.text.toString())
                .putString("pda_name", binding.etPdaName.text.toString())
                .apply()
            configureApi()
            scanManager.startScan()
        }

        binding.btnQuickQcOk.setOnClickListener { quickSend("QC-OK") }
        binding.btnQuickQcNg.setOnClickListener { quickSend("QC-NG") }
        binding.btnQuickStart.setOnClickListener { quickSend("START") }
        binding.btnQuickStop.setOnClickListener { quickSend("STOP") }

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
        sendToServer(code, barcodeType)
    }

    private fun sendManualCode() {
        val code = binding.etCode.text?.toString()?.trim()
        if (code.isNullOrBlank()) {
            showSnack(getString(R.string.msg_no_code))
            return
        }
        if (binding.etServerIp.text.isNullOrBlank()) {
            showSnack(getString(R.string.msg_no_ip))
            binding.etServerIp.requestFocus()
            return
        }
        prefs.edit()
            .putString("server_ip", binding.etServerIp.text.toString())
            .putString("pda_name", binding.etPdaName.text.toString())
            .apply()
        configureApi()
        sendToServer(code, "MANUAL")
    }

    private fun quickSend(code: String) {
        if (binding.etServerIp.text.isNullOrBlank()) {
            showSnack(getString(R.string.msg_no_ip))
            binding.etServerIp.requestFocus()
            return
        }
        prefs.edit()
            .putString("server_ip", binding.etServerIp.text.toString())
            .putString("pda_name", binding.etPdaName.text.toString())
            .apply()
        configureApi()
        binding.etCode.setText(code)
        sendToServer(code, "QUICK")
    }

    private fun sendToServer(code: String, barcodeType: String) {
        val pdaName = binding.etPdaName.text?.toString()?.trim()
            ?.ifBlank { getString(R.string.default_pda_name) }
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
            val result = apiClient.postScan(code, pdaName)
            result.fold(
                onSuccess = { response ->
                    updateHistoryStatus(code, timeStr, if (response.success) SendStatus.SUCCESS else SendStatus.ERROR)
                    if (response.success) {
                        showSnack(getString(R.string.msg_sent, code))
                    } else {
                        showSnack(response.message, isError = true)
                    }
                },
                onFailure = { error ->
                    updateHistoryStatus(code, timeStr, SendStatus.ERROR)
                    showSnack(getString(R.string.msg_error, error.message ?: "Unknown"), isError = true)
                }
            )
        }
    }

    private fun addToHistory(item: ScanHistoryItem) {
        history.add(0, item)
        if (history.size > 50) history.removeAt(history.size - 1)
        scanAdapter.submitList(history.toList())
        binding.emptyState.isVisible = false
        binding.rvHistory.isVisible = true
        binding.tvScanCount.text = "${history.size} lan quet"
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
        val wasOnline = isServerOnline
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
