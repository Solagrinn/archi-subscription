package com.archi.subscription.ui.dashboard

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.archi.subscription.data.model.CreateSubscriptionRequest
import com.archi.subscription.data.model.SubscriptionType

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun CreateSubscriptionDialog(customerId: String, onDismiss: () -> Unit, onCreate: (CreateSubscriptionRequest) -> Unit) {
    var selectedType by remember { mutableStateOf(SubscriptionType.Electricity) }
    var provider by remember { mutableStateOf("") }
    var subscriberNumber by remember { mutableStateOf("") }
    var payDay by remember { mutableStateOf("15") }
    var typeExpanded by remember { mutableStateOf(false) }

    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text("Add Subscription") },
        text = {
            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                // Type dropdown
                ExposedDropdownMenuBox(expanded = typeExpanded, onExpandedChange = { typeExpanded = it }) {
                    OutlinedTextField(
                        value = "${selectedType.icon} ${selectedType.label}",
                        onValueChange = {},
                        readOnly = true,
                        label = { Text("Type") },
                        modifier = Modifier.menuAnchor().fillMaxWidth(),
                        trailingIcon = { ExposedDropdownMenuDefaults.TrailingIcon(typeExpanded) }
                    )
                    ExposedDropdownMenu(expanded = typeExpanded, onDismissRequest = { typeExpanded = false }) {
                        SubscriptionType.entries.forEach { type ->
                            DropdownMenuItem(
                                text = { Text("${type.icon} ${type.label}") },
                                onClick = { selectedType = type; typeExpanded = false }
                            )
                        }
                    }
                }
                OutlinedTextField(value = provider, onValueChange = { provider = it }, label = { Text("Service Provider") }, singleLine = true, modifier = Modifier.fillMaxWidth())
                OutlinedTextField(value = subscriberNumber, onValueChange = { subscriberNumber = it }, label = { Text("Subscriber Number") }, singleLine = true, modifier = Modifier.fillMaxWidth())
                OutlinedTextField(value = payDay, onValueChange = { payDay = it.filter { c -> c.isDigit() }.take(2) }, label = { Text("Payment Day (1-28)") }, singleLine = true, modifier = Modifier.fillMaxWidth())
            }
        },
        confirmButton = {
            Button(
                onClick = {
                    val day = payDay.toIntOrNull() ?: 15
                    onCreate(CreateSubscriptionRequest(customerId, selectedType.value, provider.trim(), subscriberNumber.trim(), day.coerceIn(1, 28)))
                },
                enabled = provider.isNotBlank() && subscriberNumber.isNotBlank()
            ) { Text("Create") }
        },
        dismissButton = { TextButton(onClick = onDismiss) { Text("Cancel") } }
    )
}

