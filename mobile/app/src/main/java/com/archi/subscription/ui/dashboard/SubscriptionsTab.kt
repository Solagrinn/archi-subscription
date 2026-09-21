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
import com.archi.subscription.data.model.*
import com.archi.subscription.ui.components.subscriptionIcon
import com.archi.subscription.ui.theme.*
import kotlinx.coroutines.launch

@Composable
fun SubscriptionsTab(vm: DashboardViewModel) {
    val scope = rememberCoroutineScope()
    var showCreateDialog by remember { mutableStateOf(false) }
    var editTarget by remember { mutableStateOf<Subscription?>(null) }
    var paymentDebt by remember { mutableStateOf<DebtInfo?>(null) }
    var debtLoading by remember { mutableStateOf<String?>(null) }

    val active = vm.subscriptions.filter { it.status == "Active" }
    val passive = vm.subscriptions.filter { it.status != "Active" }

    Column(modifier = Modifier.padding(16.dp)) {
        Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween, verticalAlignment = Alignment.CenterVertically) {
            Text("Subscriptions", fontWeight = FontWeight.SemiBold, fontSize = 15.sp)
            Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                Button(onClick = { showCreateDialog = true }, contentPadding = PaddingValues(horizontal = 12.dp, vertical = 4.dp)) {
                    Text("➕ Add", fontSize = 11.sp)
                }
                TextButton(onClick = { vm.refreshAll() }) { Text("↻ Refresh", fontSize = 11.sp) }
            }
        }
        Spacer(Modifier.height(8.dp))

        LazyColumn(verticalArrangement = Arrangement.spacedBy(8.dp)) {
            if (active.isNotEmpty()) {
                item { Text("Active (${active.size})", fontSize = 11.sp, fontWeight = FontWeight.Medium, color = Gray400) }
                items(active, key = { it.id }) { sub ->
                    SubscriptionCard(
                        sub = sub,
                        periods = vm.periodStatuses[sub.id] ?: emptyList(),
                        debtLoading = debtLoading,
                        onPay = { period ->
                            val key = "${sub.id}|${period}"
                            scope.launch {
                                debtLoading = key
                                vm.inquireDebt(sub.id, period).onSuccess { if (it.hasDebt) paymentDebt = it }
                                debtLoading = null
                            }
                        },
                        onEdit = { editTarget = sub },
                        onDelete = { vm.deleteSubscription(sub.id) }
                    )
                }
            }
            if (passive.isNotEmpty()) {
                item { Spacer(Modifier.height(8.dp)); Text("Passive (${passive.size})", fontSize = 11.sp, fontWeight = FontWeight.Medium, color = Gray400) }
                items(passive, key = { it.id }) { sub ->
                    SubscriptionCard(sub = sub, periods = emptyList(), debtLoading = null, onPay = {}, onEdit = { editTarget = sub }, onDelete = { vm.deleteSubscription(sub.id) })
                }
            }
            if (vm.subscriptions.isEmpty()) {
                item {
                    Card(Modifier.fillMaxWidth(), shape = RoundedCornerShape(12.dp)) {
                        Text("No subscriptions yet. Add your first above.", modifier = Modifier.padding(32.dp), fontSize = 13.sp, color = Gray500)
                    }
                }
            }
        }
    }

    if (showCreateDialog) {
        CreateSubscriptionDialog(customerId = vm.customerId, onDismiss = { showCreateDialog = false }, onCreate = { vm.createSubscription(it); showCreateDialog = false })
    }
    editTarget?.let { sub ->
        EditSubscriptionDialog(sub = sub, onDismiss = { editTarget = null }, onUpdate = { vm.updateSubscription(sub.id, it); editTarget = null })
    }
    paymentDebt?.let { debt ->
        PaymentDialog(debt = debt, vm = vm, onDismiss = { paymentDebt = null })
    }
}

@Composable
fun SubscriptionCard(
    sub: Subscription,
    periods: List<PeriodStatus>,
    debtLoading: String?,
    onPay: (String) -> Unit,
    onEdit: () -> Unit,
    onDelete: () -> Unit
) {
    val isActive = sub.status == "Active"
    var showConfirm by remember { mutableStateOf(false) }

    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        colors = CardDefaults.cardColors(containerColor = if (isActive) MaterialTheme.colorScheme.surface else Gray100)
    ) {
        Column(Modifier.padding(12.dp)) {
            Row(verticalAlignment = Alignment.Top) {
                Text(subscriptionIcon(sub.type), fontSize = 22.sp)
                Spacer(Modifier.width(10.dp))
                Column(Modifier.weight(1f)) {
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        Text(sub.serviceProvider, fontWeight = FontWeight.SemiBold, fontSize = 13.sp)
                        Spacer(Modifier.width(6.dp))
                        Surface(color = if (isActive) Emerald50 else Gray200, shape = RoundedCornerShape(4.dp)) {
                            Text(sub.status, fontSize = 10.sp, color = if (isActive) Emerald600 else Gray500, modifier = Modifier.padding(horizontal = 6.dp, vertical = 2.dp))
                        }
                    }
                    Text("${sub.type} · #${sub.subscriberNumber}", fontSize = 11.sp, color = Gray500)
                    Text("Payment day: ${sub.paymentDayOfMonth} of each month", fontSize = 10.sp, color = Gray400)
                }
                IconButton(onClick = onEdit, modifier = Modifier.size(32.dp)) { Text("✏️", fontSize = 14.sp) }
                IconButton(onClick = { showConfirm = true }, modifier = Modifier.size(32.dp)) { Text("🗑️", fontSize = 14.sp) }
            }

            // Period statuses
            if (isActive && periods.isNotEmpty()) {
                Spacer(Modifier.height(8.dp))
                val allPaid = periods.all { it.paid }
                val unpaid = periods.filter { !it.paid }
                val paid = periods.filter { it.paid }

                if (allPaid) {
                    Surface(color = Emerald50, shape = RoundedCornerShape(8.dp), modifier = Modifier.fillMaxWidth()) {
                        Row(Modifier.padding(12.dp), horizontalArrangement = Arrangement.SpaceBetween, verticalAlignment = Alignment.CenterVertically) {
                            Column {
                                Text("✅ All payments up to date", fontSize = 12.sp, fontWeight = FontWeight.Medium, color = Emerald600)
                                Text(paid.joinToString(", ") { it.period }, fontSize = 10.sp, color = Emerald600)
                            }
                            Surface(color = Emerald50, shape = RoundedCornerShape(99.dp)) {
                                Text("Paid", fontSize = 10.sp, fontWeight = FontWeight.SemiBold, color = Emerald700, modifier = Modifier.padding(horizontal = 10.dp, vertical = 4.dp))
                            }
                        }
                    }
                }

                unpaid.forEach { p ->
                    val key = "${sub.id}|${p.period}"
                    val isLoading = debtLoading == key
                    Surface(color = Amber50, shape = RoundedCornerShape(8.dp), modifier = Modifier.fillMaxWidth().padding(top = 4.dp)) {
                        Row(Modifier.padding(12.dp), horizontalArrangement = Arrangement.SpaceBetween, verticalAlignment = Alignment.CenterVertically) {
                            Column {
                                Text("⚠️ Unpaid — ${p.period}", fontSize = 12.sp, fontWeight = FontWeight.Medium, color = Amber800)
                                Text("Due on the ${sub.paymentDayOfMonth}th", fontSize = 10.sp, color = Amber600)
                            }
                            Button(onClick = { onPay(p.period) }, enabled = !isLoading, contentPadding = PaddingValues(horizontal = 14.dp, vertical = 6.dp), shape = RoundedCornerShape(8.dp)) {
                                if (isLoading) CircularProgressIndicator(Modifier.size(12.dp), strokeWidth = 2.dp)
                                else Text("💳 Pay", fontSize = 11.sp)
                            }
                        }
                    }
                }

                if (!allPaid && paid.isNotEmpty()) {
                    Surface(color = Emerald50.copy(alpha = 0.6f), shape = RoundedCornerShape(8.dp), modifier = Modifier.fillMaxWidth().padding(top = 4.dp)) {
                        Row(Modifier.padding(10.dp), horizontalArrangement = Arrangement.spacedBy(6.dp), verticalAlignment = Alignment.CenterVertically) {
                            Text("✅ Paid:", fontSize = 10.sp, color = Emerald700)
                            paid.forEach { p ->
                                Text("${p.period} (₺${p.payment?.amount?.let { String.format("%.2f", it) } ?: ""})", fontSize = 10.sp, color = Emerald700)
                            }
                        }
                    }
                }
            }
        }
    }

    if (showConfirm) {
        AlertDialog(
            onDismissRequest = { showConfirm = false },
            title = { Text("Delete Subscription") },
            text = { Text("Delete ${sub.type} subscription for \"${sub.serviceProvider}\"?") },
            confirmButton = { TextButton(onClick = { onDelete(); showConfirm = false }, colors = ButtonDefaults.textButtonColors(contentColor = Red600)) { Text("Delete") } },
            dismissButton = { TextButton(onClick = { showConfirm = false }) { Text("Cancel") } }
        )
    }
}

