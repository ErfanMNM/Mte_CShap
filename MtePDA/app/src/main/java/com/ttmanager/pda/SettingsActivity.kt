package com.ttmanager.pda

import android.content.Context
import android.content.Intent
import android.os.Bundle
import android.view.inputmethod.EditorInfo
import androidx.appcompat.app.AppCompatActivity
import com.google.android.material.snackbar.Snackbar
import com.ttmanager.pda.databinding.ActivitySettingsBinding

class SettingsActivity : AppCompatActivity() {

    private lateinit var binding: ActivitySettingsBinding
    private lateinit var prefs: android.content.SharedPreferences

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivitySettingsBinding.inflate(layoutInflater)
        setContentView(binding.root)

        prefs = getSharedPreferences("mtepda_prefs", Context.MODE_PRIVATE)

        loadSettings()
        setupListeners()
    }

    private fun loadSettings() {
        binding.etServerIp.text?.clear()
        binding.etServerIp.text?.append(prefs.getString("server_ip", "") ?: "")

        binding.etServerPort.text?.clear()
        binding.etServerPort.text?.append(prefs.getString("server_port", "9999") ?: "9999")

        binding.etPdaName.text?.clear()
        binding.etPdaName.text?.append(
            prefs.getString("pda_name", getString(R.string.default_pda_name))
                ?: getString(R.string.default_pda_name)
        )

        val mode = prefs.getString("default_mode", "QUET_THUNG") ?: "QUET_THUNG"
        binding.rgDefaultMode.check(
            if (mode == "QUET_THUNG") R.id.rbModeQuetThung else R.id.rbModeKiemTra
        )
    }

    private fun setupListeners() {
        // Quay ve man hinh chinh
        binding.btnBack.setOnClickListener {
            finish()
        }

        // An nut Luu
        binding.btnSave.setOnClickListener {
            saveSettings()
        }

        // An nut Mac dinh
        binding.btnDefault.setOnClickListener {
            binding.etServerIp.text?.clear()
            binding.etServerIp.text?.append("")
            binding.etServerPort.text?.clear()
            binding.etServerPort.text?.append("9999")
            binding.etPdaName.text?.clear()
            binding.etPdaName.text?.append(getString(R.string.default_pda_name))
            binding.rgDefaultMode.check(R.id.rbModeQuetThung)
            showSnack(getString(R.string.settings_reset_done))
        }

        // Enter tren o IP -> nhay sang Port
        binding.etServerIp.setOnEditorActionListener { _, actionId, _ ->
            if (actionId == EditorInfo.IME_ACTION_NEXT) {
                binding.etServerPort.requestFocus()
                true
            } else false
        }

        // Enter tren o Port -> nhay sang PDA Name
        binding.etServerPort.setOnEditorActionListener { _, actionId, _ ->
            if (actionId == EditorInfo.IME_ACTION_NEXT) {
                binding.etPdaName.requestFocus()
                true
            } else false
        }
    }

    private fun saveSettings() {
        val ip = binding.etServerIp.text?.toString()?.trim() ?: ""
        val portStr = binding.etServerPort.text?.toString()?.trim() ?: "9999"
        val pdaName = binding.etPdaName.text?.toString()?.trim()
            ?.ifBlank { getString(R.string.default_pda_name) }
            ?: getString(R.string.default_pda_name)
        val mode = if (binding.rgDefaultMode.checkedRadioButtonId == R.id.rbModeQuetThung) {
            "QUET_THUNG"
        } else {
            "KIEM_TRA"
        }

        // Validate
        if (ip.isEmpty()) {
            binding.tilServerIp.error = getString(R.string.settings_ip_required)
            binding.etServerIp.requestFocus()
            return
        }
        binding.tilServerIp.error = null

        val port = portStr.toIntOrNull()
        if (port == null || port <= 0 || port > 65535) {
            binding.tilServerPort.error = getString(R.string.settings_port_invalid)
            binding.etServerPort.requestFocus()
            return
        }
        binding.tilServerPort.error = null

        // Luu vao SharedPreferences
        prefs.edit().apply {
            putString("server_ip", ip)
            putString("server_port", portStr)
            putString("pda_name", pdaName)
            putString("default_mode", mode)
            apply()
        }

        showSnack(getString(R.string.settings_saved))
        finish()
    }

    private fun showSnack(message: String) {
        Snackbar.make(binding.root, message, Snackbar.LENGTH_SHORT).show()
    }
}
