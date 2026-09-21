package com.archi.subscription.ui.dashboard

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.archi.subscription.data.model.DebtInfo
import com.archi.subscription.data.model.Payment
import com.archi.subscription.ui.theme.*
import com.archi.subscription.util.PeriodUtils
import kotlinx.coroutines.launch

@Composable
fun PaymentDialog(debt: DebtInfo, vm: DashboardViewModel, onDismiss: () -> Unit) {
    var amount by remember { mutableStateOf(debt.debtAmount.toString()) }
    var loading by remember { mutableStateOf(false) }
    var result by remember { mutableStateOf<Payment?>(null) }
    var error by remember { mutableStateOf<String?>(null) }
    val scope = rememberCoroutineScope()

    if (result != null) {
        val isSuccess = result!!.status == "Successful"
        AlertDialog(
            onDismissRequest = { onDismiss(); vm.refreshAll() },
            title = { Text(if (isSuccess) "✅ Payment Successful" else "❌ Payment Failed", fontWeight = FontWeight.Bold) },
            text = {
                Column {
                    if (isSuccess) {
                        Text("₺${String.format("%.2f", result!!.amount)} paid to ${result!!.serviceProvider} for period ${result!!.period}.", fontSize = 13.sp, color = Gray700)
                    } else {
                        Text("The payment was declined. Please try again.", fontSize = 13.sp, color = Red600)
                    }
                    result!!.transactionReference?.let { Text("Ref: $it", fontSize = 10.sp, color = Gray400, fontFamily = FontFamily.Monospace) }
                }
            },
            confirmButton = {
                Button(
                    onClick = { onDismiss(); vm.refreshAll() },
                    colors = ButtonDefaults.buttonColors(containerColor = if (isSuccess) Emerald600 else Red600)
                ) { Text(if (isSuccess) "Done" else "Close") }
            }
        )
        return
    }

    AlertDialog(
        onDismissRequest = { if (!loading) onDismiss() },
        title = { Text("Make Payment", fontWeight = FontWeight.Bold) },
        text = {
            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                Card(colors = CardDefaults.cardColors(containerColor = Gray50), shape = RoundedCornerShape(8.dp)) {
                    Column(Modifier.padding(12.dp).fillMaxWidth(), verticalArrangement = Arrangement.spacedBy(4.dp)) {
                        InfoRow("Provider", debt.serviceProvider)
                        InfoRow("Subscriber #", debt.subscriberNumber)
                        InfoRow("Period", debt.period)
                        InfoRow("Due Date", PeriodUtils.formatShort(debt.dueDate))
                    }
                }
                OutlinedTextField(
                    value = amount,
                    onValueChange = { amount = it },
                    label = { Text("Amount (₺)") },
                    singleLine = true,
                    modifier = Modifier.fillMaxWidth()
                )
                Text("Suggested: ₺${String.format("%.2f", debt.debtAmount)}", fontSize = 11.sp, color = Gray400)
                error?.let {
                    Card(colors = CardDefaults.cardColors(containerColor = Red50)) {
                        Text(it, modifier = Modifier.padding(8.dp), color = Red600, fontSize = 12.sp)
                    }
                }
            }
        },
        confirmButton = {
            Button(
                onClick = {
                    scope.launch {
                        loading = true; error = null
                        val amt = amount.toDoubleOrNull()
                        if (amt == null || amt <= 0) { error = "Invalid amount"; loading = false; return@launch }
                        vm.makePayment(debt.subscriptionId, amt, debt.period)
                            .onSuccess { result = it }
                            .onFailure { error = it.message }
                        loading = false
                    }
                },
                enabled = !loading && amount.toDoubleOrNull() != null && (amount.toDoubleOrNull() ?: 0.0) > 0,
                colors = ButtonDefaults.buttonColors(containerColor = Emerald600)
            ) {
                if (loading) CircularProgressIndicator(Modifier.size(16.dp), strokeWidth = 2.dp, color = MaterialTheme.colorScheme.onPrimary)
                else Text("Confirm Payment")
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss, enabled = !loading) { Text("Cancel") }
        }
    )
}

@Composable
private fun InfoRow(label: String, value: String) {
    Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
        Text(label, fontSize = 12.sp, color = Gray500)
        Text(value, fontSize = 12.sp, fontWeight = FontWeight.Medium)
    }
}

