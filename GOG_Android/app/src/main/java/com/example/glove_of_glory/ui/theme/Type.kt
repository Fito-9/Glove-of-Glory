package com.example.glove_of_glory.ui.theme

import androidx.compose.material3.Typography
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.sp

// No necesitamos la familia de fuentes personalizada "Teko"
// val tekoFontFamily = FontFamily(...)

// Set of Material typography styles to start with
val Typography = Typography(
    // Usaremos la fuente por defecto para los títulos, pero con los pesos y tamaños que definimos.
    displayLarge = TextStyle(
        fontFamily = FontFamily.Default, // <-- CAMBIADO
        fontWeight = FontWeight.Bold,
        fontSize = 57.sp,
        lineHeight = 64.sp,
        letterSpacing = (-0.25).sp
    ),
    displayMedium = TextStyle(
        fontFamily = FontFamily.Default, // <-- CAMBIADO
        fontWeight = FontWeight.Bold,
        fontSize = 45.sp,
        lineHeight = 52.sp
    ),
    headlineLarge = TextStyle(
        fontFamily = FontFamily.Default, // <-- CAMBIADO
        fontWeight = FontWeight.SemiBold,
        fontSize = 32.sp,
        lineHeight = 40.sp
    ),
    titleLarge = TextStyle(
        fontFamily = FontFamily.Default, // <-- CAMBIADO
        fontWeight = FontWeight.SemiBold,
        fontSize = 22.sp,
        lineHeight = 28.sp
    ),
    // El cuerpo del texto ya usaba la fuente por defecto, así que no cambia.
    bodyLarge = TextStyle(
        fontFamily = FontFamily.Default,
        fontWeight = FontWeight.Normal,
        fontSize = 16.sp,
        lineHeight = 24.sp,
        letterSpacing = 0.5.sp
    ),
    // Las etiquetas de los botones también usarán la fuente por defecto.
    labelLarge = TextStyle(
        fontFamily = FontFamily.Default, // <-- CAMBIADO
        fontWeight = FontWeight.Bold,
        fontSize = 18.sp,
        lineHeight = 20.sp,
        letterSpacing = 0.1.sp
    )
)