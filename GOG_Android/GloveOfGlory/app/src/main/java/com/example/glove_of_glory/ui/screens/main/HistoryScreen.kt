

package com.example.glove_of_glory.ui.screens.main

import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.example.glove_of_glory.R

@Composable
fun HistoryScreen() {
    Box(modifier = Modifier.fillMaxSize()) {
        // Imagen de fondo
        Image(
            painter = painterResource(id = R.drawable.background_history),
            contentDescription = stringResource(id = R.string.history_background_description),
            modifier = Modifier.fillMaxSize(),
            contentScale = ContentScale.Crop
        )

        // Capa de gradiente oscuro para legibilidad
        Box(
            modifier = Modifier
                .fillMaxSize()
                .background(
                    Brush.verticalGradient(
                        colors = listOf(
                            Color.Black.copy(alpha = 0.8f),
                            Color.Black.copy(alpha = 0.6f),
                            Color.Black.copy(alpha = 0.8f)
                        )
                    )
                )
        )

        // Contenido de texto con scroll
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(horizontal = 24.dp, vertical = 16.dp)
                .verticalScroll(rememberScrollState())
        ) {
            // Título principal
            Text(
                text = stringResource(id = R.string.history_main_title),
                style = MaterialTheme.typography.displaySmall,
                fontWeight = FontWeight.Bold,
                color = Color.White,
                modifier = Modifier.padding(bottom = 24.dp)
            )

            // Secciones de la historia
            HistorySection(
                title = stringResource(id = R.string.history_n64_title),
                content = stringResource(id = R.string.history_n64_content)
            )
            HistorySection(
                title = stringResource(id = R.string.history_melee_title),
                content = stringResource(id = R.string.history_melee_content)
            )
            HistorySection(
                title = stringResource(id = R.string.history_brawl_title),
                content = stringResource(id = R.string.history_brawl_content)
            )
            HistorySection(
                title = stringResource(id = R.string.history_wiiu_3ds_title),
                content = stringResource(id = R.string.history_wiiu_3ds_content)
            )
            HistorySection(
                title = stringResource(id = R.string.history_ultimate_title),
                content = stringResource(id = R.string.history_ultimate_content)
            )
        }
    }
}

@Composable
fun HistorySection(title: String, content: String) {
    Column(modifier = Modifier.padding(bottom = 24.dp)) {
        Text(
            text = title,
            style = MaterialTheme.typography.headlineMedium,
            fontWeight = FontWeight.SemiBold,
            color = MaterialTheme.colorScheme.tertiary, // Usamos el color dorado para los títulos
            modifier = Modifier.padding(bottom = 8.dp)
        )
        Text(
            text = content,
            style = MaterialTheme.typography.bodyLarge,
            color = Color.White.copy(alpha = 0.9f),
            lineHeight = 24.sp
        )
    }
}