package com.archi.subscription.ui.theme

import androidx.compose.material3.*
import androidx.compose.runtime.Composable

private val LightColorScheme = lightColorScheme(
    primary = Indigo600,
    onPrimary = androidx.compose.ui.graphics.Color.White,
    primaryContainer = Indigo50,
    secondary = Emerald600,
    onSecondary = androidx.compose.ui.graphics.Color.White,
    secondaryContainer = Emerald50,
    tertiary = Amber600,
    tertiaryContainer = Amber50,
    error = Red600,
    errorContainer = Red50,
    surface = androidx.compose.ui.graphics.Color.White,
    background = Gray50,
    onBackground = Gray900,
    onSurface = Gray900,
    outline = Gray200,
    surfaceVariant = Gray100,
)

@Composable
fun ArchiTheme(content: @Composable () -> Unit) {
    MaterialTheme(
        colorScheme = LightColorScheme,
        content = content
    )
}

