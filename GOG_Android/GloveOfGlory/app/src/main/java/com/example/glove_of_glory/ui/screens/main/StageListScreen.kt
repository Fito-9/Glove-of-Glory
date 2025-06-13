package com.example.glove_of_glory.ui.screens.main

import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import coil.compose.rememberAsyncImagePainter
import com.example.glove_of_glory.R
import com.example.glove_of_glory.data.model.Stage
import com.example.glove_of_glory.data.repository.StageRepository

@Composable
fun StageListScreen() {
    val context = LocalContext.current
    var stages by remember { mutableStateOf<List<Stage>>(emptyList()) }
    var isLoading by remember { mutableStateOf(true) }
    var selectedStage by remember { mutableStateOf<Stage?>(null) }

    LaunchedEffect(Unit) {
        isLoading = true
        val repository = StageRepository(context)
        stages = repository.getStages()
        isLoading = false
    }

    if (isLoading) {
        Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
            CircularProgressIndicator()
        }
    } else {
        LazyColumn(
            modifier = Modifier.fillMaxSize(),
            contentPadding = PaddingValues(16.dp),
            verticalArrangement = Arrangement.spacedBy(20.dp)
        ) {
            items(stages) { stage ->
                StageCard(stage = stage, onClick = { selectedStage = stage })
            }
        }
    }

    if (selectedStage != null) {
        RecommendedCharactersDialog(
            stage = selectedStage!!,
            onDismiss = { selectedStage = null }
        )
    }
}

@Composable
fun StageCard(stage: Stage, onClick: () -> Unit) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .height(200.dp)
            .clickable(onClick = onClick),
        // --- CAMBIO: Usamos la forma del tema ---
        shape = MaterialTheme.shapes.large,
        elevation = CardDefaults.cardElevation(defaultElevation = 8.dp)
    ) {
        Box(modifier = Modifier.fillMaxSize()) {
            Image(
                painter = rememberAsyncImagePainter(model = stage.imageUrl),
                contentDescription = stage.name,
                modifier = Modifier.fillMaxSize(),
                contentScale = ContentScale.Crop
            )
            Box(
                modifier = Modifier
                    .fillMaxSize()
                    .background(
                        Brush.verticalGradient(
                            colors = listOf(Color.Transparent, Color.Black.copy(alpha = 0.8f)),
                            startY = 300f
                        )
                    )
            )
            Text(
                text = stage.name,
                style = MaterialTheme.typography.headlineMedium,
                fontWeight = FontWeight.Bold,
                color = Color.White,
                modifier = Modifier
                    .align(Alignment.BottomStart)
                    .padding(16.dp)
            )
        }
    }
}

@Composable
fun RecommendedCharactersDialog(stage: Stage, onDismiss: () -> Unit) {
    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text(text = stringResource(id = R.string.stage_dialog_title, stage.name)) },
        text = {
            Column {
                stage.recommendedCharacters.forEach { characterName ->
                    Text(text = "• $characterName", style = MaterialTheme.typography.bodyLarge)
                }
            }
        },
        confirmButton = {
            TextButton(onClick = onDismiss) {
                Text(stringResource(id = R.string.dialog_close_button))
            }
        },
        // --- CAMBIO: Aplicamos la forma del tema ---
        shape = MaterialTheme.shapes.large
    )
}