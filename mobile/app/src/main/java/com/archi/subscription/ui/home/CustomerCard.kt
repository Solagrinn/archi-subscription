package com.archi.subscription.ui.home

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.archi.subscription.data.model.Customer
import com.archi.subscription.ui.theme.*

@Composable
fun CustomerCard(customer: Customer, onLogin: () -> Unit, onDelete: () -> Unit) {
    var showConfirm by remember { mutableStateOf(false) }

    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp)
    ) {
        Column(modifier = Modifier.padding(12.dp)) {
            Text("👤", fontSize = 20.sp)
            Spacer(Modifier.height(4.dp))
            Text(customer.fullName, fontWeight = FontWeight.SemiBold, fontSize = 14.sp)
            Text(customer.email, fontSize = 11.sp, color = Gray500)
            Text(customer.phoneNumber, fontSize = 11.sp, color = Gray400)
            Spacer(Modifier.height(8.dp))
            Row(horizontalArrangement = Arrangement.spacedBy(6.dp)) {
                Button(
                    onClick = onLogin,
                    modifier = Modifier.weight(1f),
                    contentPadding = PaddingValues(horizontal = 8.dp, vertical = 4.dp),
                    shape = RoundedCornerShape(8.dp)
                ) {
                    Text("Login →", fontSize = 11.sp)
                }
                OutlinedButton(
                    onClick = { showConfirm = true },
                    contentPadding = PaddingValues(horizontal = 8.dp, vertical = 4.dp),
                    shape = RoundedCornerShape(8.dp),
                    colors = ButtonDefaults.outlinedButtonColors(contentColor = Red600)
                ) {
                    Text("🗑️", fontSize = 11.sp)
                }
            }
        }
    }

    if (showConfirm) {
        AlertDialog(
            onDismissRequest = { showConfirm = false },
            title = { Text("Delete Customer") },
            text = { Text("Delete ${customer.fullName} and all their subscriptions?") },
            confirmButton = {
                TextButton(onClick = { onDelete(); showConfirm = false }, colors = ButtonDefaults.textButtonColors(contentColor = Red600)) {
                    Text("Delete")
                }
            },
            dismissButton = { TextButton(onClick = { showConfirm = false }) { Text("Cancel") } }
        )
    }
}

