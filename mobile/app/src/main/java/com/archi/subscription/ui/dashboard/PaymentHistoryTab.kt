package com.archi.subscription.ui.dashboard

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.archi.subscription.data.model.Payment
import com.archi.subscription.ui.theme.*
import com.archi.subscription.util.PeriodUtils

@Composable
fun PaymentHistoryTab(payments: List<Payment>) {
    Column(modifier = Modifier.padding(16.dp)) {
        Text("Payment History", fontWeight = FontWeight.SemiBold, fontSize = 15.sp)
        Spacer(Modifier.height(8.dp))

        if (payments.isEmpty()) {
            Card(Modifier.fillMaxWidth(), shape = RoundedCornerShape(12.dp)) {
                Text("No payments yet.", modifier = Modifier.padding(32.dp), fontSize = 13.sp, color = Gray500)
            }
        } else {
            LazyColumn(verticalArrangement = Arrangement.spacedBy(6.dp)) {
                items(payments, key = { it.id }) { p ->
                    val isSuccess = p.status == "Successful"
                    Card(
                        modifier = Modifier.fillMaxWidth(),
                        shape = RoundedCornerShape(10.dp),
                        colors = CardDefaults.cardColors(containerColor = if (isSuccess) Emerald50.copy(0.5f) else Red50.copy(0.5f))
                    ) {
                        Row(Modifier.padding(12.dp), verticalAlignment = Alignment.CenterVertically) {
                            Text(if (isSuccess) "✅" else "❌", fontSize = 18.sp)
                            Spacer(Modifier.width(10.dp))
                            Column(Modifier.weight(1f)) {
                                Text(p.serviceProvider, fontWeight = FontWeight.SemiBold, fontSize = 12.sp)
                                Text("Period: ${p.period} · ${PeriodUtils.formatShort(p.paymentDate)}", fontSize = 10.sp, color = Gray500)
                                p.transactionReference?.let {
                                    Text("Ref: $it", fontSize = 9.sp, color = Gray400, fontFamily = FontFamily.Monospace)
                                }
                            }
                            Spacer(Modifier.width(8.dp))
                            Column(horizontalAlignment = Alignment.End) {
                                Text("₺${String.format("%.2f", p.amount)}", fontWeight = FontWeight.Bold, fontSize = 13.sp, color = if (isSuccess) Emerald700 else Red600)
                                Text(p.status, fontSize = 10.sp, color = if (isSuccess) Emerald600 else Red600)
                            }
                        }
                    }
                }
            }
        }
    }
}

