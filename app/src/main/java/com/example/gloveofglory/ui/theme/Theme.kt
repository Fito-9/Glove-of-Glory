// app/src/main/java/com/example/gloveofglory/ui/theme/Theme.kt
package com.example.gloveofglory.ui.theme

import android.app.Activity
import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.runtime.SideEffect
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.toArgb
import androidx.compose.ui.platform.LocalView
import androidx.core.view.WindowCompat

private val DarkColorScheme = darkColorScheme(
    primary = SmashRed,
    secondary = SmashBlue,
    tertiary = SmashGold,
    background = DarkBackground,
    surface = DarkSurface,
    onPrimary = SmashWhite,
    onSecondary = SmashWhite,
    onTertiary = SmashBlack,
    onBackground = SmashWhite,
    onSurface = SmashWhite,
    error = SmashRed
)

private val LightColorScheme = lightColorScheme(
    primary = SmashRed,
    secondary = SmashBlue,
    tertiary = SmashGold,
    background = SmashWhite,
    surface = Color(0xFFEFEFEF),
    onPrimary = SmashWhite,
    onSecondary = SmashWhite,
    onTertiary = SmashBlack,
    onBackground = SmashBlack,
    onSurface = SmashBlack,
    error = SmashRed
)

@Composable
fun GloveOfGloryTheme(
    darkTheme: Boolean = isSystemInDarkTheme(),
    content: @Composable () -> Unit
) {
    val colorScheme = if (darkTheme) DarkColorScheme else LightColorScheme
    val view = LocalView.current
    if (!view.isInEditMode) {
        SideEffect {
            val window = (view.context as Activity).window
            window.statusBarColor = colorScheme.primary.toArgb()
            WindowCompat.getInsetsController(window, view).isAppearanceLightStatusBars = darkTheme
        }
    }

    MaterialTheme(
        colorScheme = colorScheme,
        typography = Typography,
        shapes = Shapes,
        content = content
    )
}