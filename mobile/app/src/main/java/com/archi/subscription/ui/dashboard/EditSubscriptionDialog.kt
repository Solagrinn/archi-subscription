package com.archi.subscription.ui.dashboard

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.archi.subscription.data.model.Subscription
import com.archi.subscription.data.model.SubscriptionType
import com.archi.subscription.data.model.UpdateSubscriptionRequest

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun EditSubscriptionDialog(sub: Subscription, onDismiss: () -> Unit, onUpdate: (UpdateSubscriptionRequest) -> Unit) {
    var provider by remember { mutableStateOf(sub.serviceProvider) }
    var subscriberNumber by remember { mutableStateOf(sub.subscriberNumber) }
    var payDay by remember { mutableStateOf(sub.paymentDayOfMonth.toString()) }
    var isActive by remember { mutableStateOf(sub.status == "Active") }

    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text("Edit Subscription") },
        text = {
            Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                OutlinedTextField(value = provider, onValueChange = { provider = it }, label = { Text("Service Provider") }, singleLine = true, modifier = Modifier.fillMaxWidth())
                OutlinedTextField(value = subscriberNumber, onValueChange = { subscriberNumber = it }, label = { Text("Subscriber Number") }, singleLine = true, modifier = Modifier.fillMaxWidth())
                OutlinedTextField(value = payDay, onValueChange = { payDay = it.filter { c -> c.isDigit() }.take(2) }, label = { Text("Payment Day (1-28)") }, singleLine = true, modifier = Modifier.fillMaxWidth())
                Row(verticalAlignment = androidx.compose.ui.Alignment.CenterVertically) {
                    Text("Status: ")
                    Switch(checked = isActive, onCheckedChange = { isActive = it })
                    Text(if (isActive) "Active" else "Passive")
                }
            }
        },
        confirmButton = {
            Button(onClick = {
                val day = payDay.toIntOrNull()?.coerceIn(1, 28)
                onUpdate(UpdateSubscriptionRequest(
                    serviceProvider = provider.trim().ifBlank { null },
                    subscriberNumber = subscriberNumber.trim().ifBlank { null },
                    paymentDayOfMonth = day,
                    status = if (isActive) 1 else 2
                ))
            }) { Text("Save") }
        },
        dismissButton = { TextButton(onClick = onDismiss) { Text("Cancel") } }
    )
}

