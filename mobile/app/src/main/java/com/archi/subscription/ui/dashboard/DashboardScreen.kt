package com.archi.subscription.ui.dashboard

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.archi.subscription.ui.components.TimeWarpBar
import com.archi.subscription.ui.theme.*

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DashboardScreen(
    customerId: String,
    customerName: String,
    customerEmail: String,
    customerPhone: String,
    onBack: () -> Unit,
    vm: DashboardViewModel = viewModel()
) {
    LaunchedEffect(customerId) { vm.init(customerId, customerName, customerEmail, customerPhone) }

    var selectedTab by remember { mutableIntStateOf(0) }
    val tabs = listOf("🔔 Reminders", "📋 Subscriptions", "💰 Payments")

    val activeCount = vm.subscriptions.count { it.status == "Active" }

    Scaffold(
        topBar = {
            Column {
                TimeWarpBar(vm.timeInfo, vm.timeLoading, vm::timeForward, vm::timeBackward, vm::timeReset)
                TopAppBar(
                    title = {
                        Row(verticalAlignment = Alignment.CenterVertically) {
                            Text("🏗️ ", fontSize = 18.sp)
                            Column {
                                Text("Archi Subscription", fontSize = 14.sp, fontWeight = FontWeight.Bold)
                                Text("Dashboard", fontSize = 11.sp, color = Gray500)
                            }
                        }
                    },
                    actions = {
                        Column(horizontalAlignment = Alignment.End, modifier = Modifier.padding(end = 4.dp)) {
                            Text(vm.customerName, fontSize = 12.sp, fontWeight = FontWeight.Medium)
                            Text(vm.customerEmail, fontSize = 10.sp, color = Gray500)
                        }
                        TextButton(onClick = onBack) { Text("← Back", fontSize = 11.sp) }
                    }
                )
            }
        }
    ) { padding ->
        Column(modifier = Modifier.padding(padding).fillMaxSize()) {
            // Summary banner
            Card(
                modifier = Modifier.fillMaxWidth().padding(horizontal = 16.dp, vertical = 8.dp),
                shape = RoundedCornerShape(16.dp)
            ) {
                Row(
                    modifier = Modifier.padding(16.dp),
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Box(
                        modifier = Modifier.size(44.dp).background(Indigo100, CircleShape),
                        contentAlignment = Alignment.Center
                    ) { Text("👤", fontSize = 20.sp) }
                    Spacer(Modifier.width(12.dp))
                    Column(modifier = Modifier.weight(1f)) {
                        Text(vm.customerName, fontWeight = FontWeight.Bold, fontSize = 16.sp)
                        Text("📧 ${vm.customerEmail}  📞 ${vm.customerPhone}", fontSize = 10.sp, color = Gray500)
                    }
                }
                Row(
                    modifier = Modifier.fillMaxWidth().padding(start = 16.dp, end = 16.dp, bottom = 16.dp),
                    horizontalArrangement = Arrangement.spacedBy(12.dp)
                ) {
                    StatChip("Active", activeCount.toString(), Indigo50, Indigo600, Modifier.weight(1f))
                    StatChip("Unpaid", vm.reminders.size.toString(), Amber50, Amber800, Modifier.weight(1f))
                    StatChip("Payments", vm.payments.size.toString(), Emerald50, Emerald600, Modifier.weight(1f))
                }
            }

            // Error
            vm.error?.let {
                Card(colors = CardDefaults.cardColors(containerColor = Red50), modifier = Modifier.fillMaxWidth().padding(horizontal = 16.dp)) {
                    Text(it, modifier = Modifier.padding(12.dp), color = Red600, fontSize = 12.sp)
                }
            }

            // Tabs
            TabRow(selectedTabIndex = selectedTab, modifier = Modifier.padding(horizontal = 16.dp)) {
                tabs.forEachIndexed { idx, title ->
                    Tab(selected = selectedTab == idx, onClick = { selectedTab = idx }) {
                        Text(title, fontSize = 12.sp, modifier = Modifier.padding(vertical = 12.dp))
                    }
                }
            }

            // Tab content
            if (vm.loading) {
                Box(Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
                    CircularProgressIndicator()
                }
            } else {
                when (selectedTab) {
                    0 -> RemindersTab(vm)
                    1 -> SubscriptionsTab(vm)
                    2 -> PaymentHistoryTab(vm.payments)
                }
            }
        }
    }
}

@Composable
fun StatChip(label: String, value: String, bg: androidx.compose.ui.graphics.Color, fg: androidx.compose.ui.graphics.Color, modifier: Modifier = Modifier) {
    Surface(color = bg, shape = RoundedCornerShape(8.dp), modifier = modifier) {
        Column(horizontalAlignment = Alignment.CenterHorizontally, modifier = Modifier.padding(vertical = 8.dp, horizontal = 12.dp)) {
            Text(value, fontWeight = FontWeight.Bold, fontSize = 18.sp, color = fg)
            Text(label, fontSize = 10.sp, color = fg.copy(alpha = 0.7f))
        }
    }
}

