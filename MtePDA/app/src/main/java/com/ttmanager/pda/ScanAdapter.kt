package com.ttmanager.pda

import android.view.LayoutInflater
import android.view.ViewGroup
import androidx.recyclerview.widget.DiffUtil
import androidx.recyclerview.widget.ListAdapter
import androidx.recyclerview.widget.RecyclerView
import com.ttmanager.pda.databinding.ItemScanHistoryBinding

data class ScanHistoryItem(
    val code: String,
    val time: String,
    val pdaName: String,
    val sendStatus: SendStatus,
    val barcodeType: String = ""
)

enum class SendStatus {
    PENDING, SUCCESS, ERROR
}

class ScanAdapter : ListAdapter<ScanHistoryItem, ScanAdapter.ViewHolder>(DiffCallback()) {

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        val binding = ItemScanHistoryBinding.inflate(
            LayoutInflater.from(parent.context), parent, false
        )
        return ViewHolder(binding)
    }

    override fun onBindViewHolder(holder: ViewHolder, position: Int) {
        holder.bind(getItem(position))
    }

    class ViewHolder(private val binding: ItemScanHistoryBinding) : RecyclerView.ViewHolder(binding.root) {
        fun bind(item: ScanHistoryItem) {
            binding.tvScanCode.text = item.code
            binding.tvScanTime.text = item.time
            binding.tvScanPda.text = item.pdaName

            when (item.sendStatus) {
                SendStatus.SUCCESS -> {
                    binding.tvSendStatus.text = "Gui OK"
                    binding.tvSendStatus.setBackgroundResource(R.drawable.badge_success)
                    binding.tvSendStatus.setTextColor(binding.root.context.getColor(R.color.success))
                }
                SendStatus.ERROR -> {
                    binding.tvSendStatus.text = "Loi"
                    binding.tvSendStatus.setBackgroundResource(R.drawable.badge_error)
                    binding.tvSendStatus.setTextColor(binding.root.context.getColor(R.color.error))
                }
                SendStatus.PENDING -> {
                    binding.tvSendStatus.text = "Dang gui..."
                    binding.tvSendStatus.setBackgroundResource(R.drawable.badge_pending)
                    binding.tvSendStatus.setTextColor(binding.root.context.getColor(R.color.warning))
                }
            }
        }
    }

    private class DiffCallback : DiffUtil.ItemCallback<ScanHistoryItem>() {
        override fun areItemsTheSame(oldItem: ScanHistoryItem, newItem: ScanHistoryItem): Boolean {
            return oldItem.code == newItem.code && oldItem.time == newItem.time
        }
        override fun areContentsTheSame(oldItem: ScanHistoryItem, newItem: ScanHistoryItem): Boolean {
            return oldItem == newItem
        }
    }
}
