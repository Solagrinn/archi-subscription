package com.archi.subscription.ui.home

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.grid.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.archi.subscription.data.model.Customer
import com.archi.subscription.ui.components.TimeWarpBar
import com.archi.subscription.ui.theme.*

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun HomeScreen(
    onCustomerSelected: (Customer) -> Unit,
    vm: HomeViewModel = viewModel()
) {
    var showCreateDialog by remember { mutableStateOf(false) }

    Scaffold(
        topBar = {
            Column {
                TimeWarpBar(vm.timeInfo, vm.timeLoading, vm::timeForward, vm::timeBackward, vm::timeReset)
                TopAppBar(
                    title = {
                        Row(verticalAlignment = Alignment.CenterVertically) {
                            Text("🏗️ ", fontSize = 22.sp)
                            Column {
                                Text("Archi Subscription", fontSize = 16.sp, fontWeight = FontWeight.Bold)
                                Text("Subscription & Payment Management", fontSize = 11.sp, color = Gray500)
                            }
                        }
                    }
                )
            }
        }
    ) { padding ->
        Column(
            modifier = Modifier
                .padding(padding)
                .padding(16.dp)
                .fillMaxSize()
        ) {
            // Hero card
            Card(
                modifier = Modifier.fillMaxWidth(),
                shape = RoundedCornerShape(16.dp)
            ) {
                Column(
                    modifier = Modifier.padding(24.dp),
                    horizontalAlignment = Alignment.CenterHorizontally
                ) {
                    Text(
                        if (vm.customers.isNotEmpty()) "Database Ready" else "Welcome",
                        style = MaterialTheme.typography.headlineSmall,
                        fontWeight = FontWeight.Bold
                    )
                    Spacer(Modifier.height(8.dp))
                    Text(
                        if (vm.customers.isNotEmpty()) "Select a customer to log in, or seed more test data."
                        else "Start by creating a customer or seeding the database.",
                        fontSize = 13.sp, color = Gray500
                    )
                    Spacer(Modifier.height(16.dp))
                    Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                        Button(
                            onClick = { showCreateDialog = true },
                            colors = ButtonDefaults.buttonColors(containerColor = Emerald600)
                        ) { Text("➕ New Customer") }
                        Button(onClick = { vm.seed() }, enabled = !vm.loading) {
                            Text("🌱 Seed Data")
                        }
                        if (vm.customers.isNotEmpty()) {
                            OutlinedButton(
                                onClick = { vm.clearAll() },
                                enabled = !vm.loading,
                                colors = ButtonDefaults.outlinedButtonColors(contentColor = Red600)
                            ) { Text("🗑️ Clear") }
                        }
                    }
                }
            }

            // Error
            vm.error?.let {
                Spacer(Modifier.height(8.dp))
                Card(colors = CardDefaults.cardColors(containerColor = Red50)) {
                    Text(it, modifier = Modifier.padding(12.dp), color = Red600, fontSize = 13.sp)
                }
            }

            // Loading
            if (vm.loading && vm.customers.isEmpty()) {
                Spacer(Modifier.height(32.dp))
                CircularProgressIndicator(Modifier.align(Alignment.CenterHorizontally))
            }

            // Customer list
            if (vm.customers.isNotEmpty()) {
                Spacer(Modifier.height(16.dp))
                Row(
                    Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Text("Customers (${vm.customers.size})", fontWeight = FontWeight.SemiBold, fontSize = 16.sp)
                    Text("Tap to login", fontSize = 11.sp, color = Gray400)
                }
                Spacer(Modifier.height(8.dp))
                LazyVerticalGrid(
                    columns = GridCells.Fixed(2),
                    horizontalArrangement = Arrangement.spacedBy(8.dp),
                    verticalArrangement = Arrangement.spacedBy(8.dp)
                ) {
                    items(vm.customers, key = { it.id }) { customer ->
                        CustomerCard(
                            customer = customer,
                            onLogin = { onCustomerSelected(customer) },
                            onDelete = { vm.deleteCustomer(customer.id) }
                        )
                    }
                }
            }
        }
    }

    if (showCreateDialog) {
        CreateCustomerDialog(
            onDismiss = { showCreateDialog = false },
            onCreate = { name, email, phone ->
                vm.createCustomer(name, email, phone)
                showCreateDialog = false
            }
        )
    }
}

