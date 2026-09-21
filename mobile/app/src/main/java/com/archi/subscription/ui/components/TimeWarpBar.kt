package com.archi.subscription.ui.components

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.archi.subscription.data.model.TimeInfo
import com.archi.subscription.ui.theme.*
import com.archi.subscription.util.PeriodUtils

@Composable
fun TimeWarpBar(
    timeInfo: TimeInfo?,
    loading: Boolean,
    onForward: () -> Unit,
    onBackward: () -> Unit,
    onReset: () -> Unit
) {
    Surface(
        color = Gray900,
        modifier = Modifier.fillMaxWidth()
    ) {
        Row(
            modifier = Modifier.padding(horizontal = 16.dp, vertical = 8.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.SpaceBetween
        ) {
            // Backend date
            Row(verticalAlignment = Alignment.CenterVertically, modifier = Modifier.weight(1f)) {
                Text("🕐 ", fontSize = 12.sp, color = Gray400)
                Text(
                    text = if (timeInfo != null) PeriodUtils.formatDate(timeInfo.currentDate) else "...",
                    fontSize = 12.sp,
                    fontWeight = FontWeight.SemiBold,
                    fontFamily = FontFamily.Monospace,
                    color = Color(0xFF34D399) // emerald-400
                )
                if (timeInfo != null && timeInfo.offsetDays != 0) {
                    Spacer(Modifier.width(6.dp))
                    Text(
                        text = "${if (timeInfo.offsetDays > 0) "+" else ""}${timeInfo.offsetDays}d",
                        fontSize = 10.sp,
                        fontWeight = FontWeight.Medium,
                        color = Color(0xFFFCD34D), // amber-300
                        modifier = Modifier
                            .background(Color(0x33F59E0B), RoundedCornerShape(4.dp))
                            .padding(horizontal = 6.dp, vertical = 2.dp)
                    )
                }
            }

            // Controls
            Row(verticalAlignment = Alignment.CenterVertically) {
                Text("Time Warp: ", fontSize = 10.sp, color = Gray500)
                TimeButton("◀ −1d", loading, onBackward)
                Spacer(Modifier.width(4.dp))
                TimeButton("+1d ▶", loading, onForward)
                if (timeInfo != null && timeInfo.offsetDays != 0) {
                    Spacer(Modifier.width(6.dp))
                    TextButton(
                        onClick = onReset,
                        enabled = !loading,
                        colors = ButtonDefaults.textButtonColors(contentColor = Color(0xFFEF4444)),
                        contentPadding = PaddingValues(horizontal = 8.dp, vertical = 2.dp),
                        modifier = Modifier.height(28.dp)
                    ) {
                        Text("↺ Reset", fontSize = 10.sp, fontWeight = FontWeight.Medium)
                    }
                }
            }
        }
    }
}

@Composable
private fun TimeButton(text: String, loading: Boolean, onClick: () -> Unit) {
    TextButton(
        onClick = onClick,
        enabled = !loading,
        colors = ButtonDefaults.textButtonColors(
            contentColor = Color.White,
            containerColor = Gray700
        ),
        contentPadding = PaddingValues(horizontal = 8.dp, vertical = 2.dp),
        shape = RoundedCornerShape(4.dp),
        modifier = Modifier.height(28.dp)
    ) {
        Text(text, fontSize = 10.sp, fontWeight = FontWeight.Medium)
    }
}

