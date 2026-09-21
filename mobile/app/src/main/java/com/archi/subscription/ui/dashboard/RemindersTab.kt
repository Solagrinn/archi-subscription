package com.archi.subscription.ui.dashboard

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.archi.subscription.ui.components.subscriptionIcon
import com.archi.subscription.ui.theme.*
import kotlinx.coroutines.launch

@Composable
fun RemindersTab(vm: DashboardViewModel) {
    val scope = rememberCoroutineScope()
    var paymentDebt by remember { mutableStateOf<com.archi.subscription.data.model.DebtInfo?>(null) }
    var debtLoading by remember { mutableStateOf<String?>(null) }

    Column(modifier = Modifier.padding(16.dp)) {
        Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween, verticalAlignment = Alignment.CenterVertically) {
            Text("Payment Reminders", fontWeight = FontWeight.SemiBold, fontSize = 15.sp)
            Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                if (vm.reminders.isNotEmpty()) {
                    OutlinedButton(onClick = { vm.sendNotifications {} }, contentPadding = PaddingValues(horizontal = 12.dp, vertical = 4.dp)) {
                        Text("📧 Send Notifications", fontSize = 11.sp)
                    }
                }
                TextButton(onClick = { vm.refreshAll() }) { Text("↻ Refresh", fontSize = 11.sp) }
            }
        }
        Spacer(Modifier.height(8.dp))

        if (vm.reminders.isEmpty()) {
            Card(modifier = Modifier.fillMaxWidth(), shape = RoundedCornerShape(12.dp)) {
                Column(Modifier.padding(32.dp).fillMaxWidth(), horizontalAlignment = Alignment.CenterHorizontally) {
                    Text("🎉", fontSize = 24.sp)
                    Spacer(Modifier.height(8.dp))
                    Text("All caught up!", fontWeight = FontWeight.Medium, fontSize = 14.sp)
                    Text("No pending payment reminders.", fontSize = 12.sp, color = Gray400)
                }
            }
        } else {
            LazyColumn(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                items(vm.reminders, key = { "${it.subscriptionId}-${it.period}" }) { r ->
                    val loadingKey = "${r.subscriptionId}|${r.period}"
                    Card(
                        colors = CardDefaults.cardColors(containerColor = Amber50),
                        shape = RoundedCornerShape(12.dp),
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        Row(Modifier.padding(12.dp), verticalAlignment = Alignment.Top) {
                            Text(subscriptionIcon(r.subscriptionType), fontSize = 24.sp)
                            Spacer(Modifier.width(12.dp))
                            Column(modifier = Modifier.weight(1f)) {
                                Row(verticalAlignment = Alignment.CenterVertically) {
                                    Text(r.serviceProvider, fontWeight = FontWeight.SemiBold, fontSize = 13.sp)
                                    Spacer(Modifier.width(6.dp))
                                    Text("#${r.subscriberNumber}", fontSize = 11.sp, color = Gray400)
                                }
                                Spacer(Modifier.height(4.dp))
                                Text(r.message, fontSize = 12.sp, color = Amber800)
                                Spacer(Modifier.height(4.dp))
                                Text("📅 Due day: ${r.paymentDayOfMonth}  📆 Period: ${r.period}", fontSize = 10.sp, color = Gray500)
                            }
                            Spacer(Modifier.width(8.dp))
                            Button(
                                onClick = {
                                    scope.launch {
                                        debtLoading = loadingKey
                                        vm.inquireDebt(r.subscriptionId, r.period)
                                            .onSuccess { if (it.hasDebt) paymentDebt = it }
                                        debtLoading = null
                                    }
                                },
                                enabled = debtLoading != loadingKey,
                                contentPadding = PaddingValues(horizontal = 16.dp, vertical = 8.dp),
                                shape = RoundedCornerShape(8.dp)
                            ) {
                                if (debtLoading == loadingKey) CircularProgressIndicator(Modifier.size(14.dp), strokeWidth = 2.dp)
                                else Text("💳 Pay", fontSize = 11.sp)
                            }
                        }
                    }
                }
            }
        }
    }

    paymentDebt?.let { debt ->
        PaymentDialog(debt = debt, vm = vm, onDismiss = { paymentDebt = null })
    }
}

