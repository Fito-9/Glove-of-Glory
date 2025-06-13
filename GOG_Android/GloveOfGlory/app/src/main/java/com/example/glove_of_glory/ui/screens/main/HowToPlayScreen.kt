package com.example.glove_of_glory.ui.screens.main

import android.content.Intent
import android.net.Uri
import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.PlayArrow
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import coil.compose.rememberAsyncImagePainter
import com.example.glove_of_glory.R

@Composable
fun HowToPlayScreen() {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .background(MaterialTheme.colorScheme.background)
            .verticalScroll(rememberScrollState())
            .padding(16.dp)
    ) {
        Text(
            text = stringResource(id = R.string.how_to_play_title),
            style = MaterialTheme.typography.headlineMedium,
            color = MaterialTheme.colorScheme.onBackground
        )
        Spacer(modifier = Modifier.height(16.dp))

        Text(
            text = stringResource(id = R.string.how_to_play_intro_text),
            style = MaterialTheme.typography.bodyLarge,
            color = MaterialTheme.colorScheme.onBackground
        )
        Spacer(modifier = Modifier.height(24.dp))

        Text(
            text = stringResource(id = R.string.how_to_play_video_title),
            style = MaterialTheme.typography.titleLarge,
            color = MaterialTheme.colorScheme.onBackground
        )
        Spacer(modifier = Modifier.height(8.dp))

        YoutubeThumbnailPlayer(
            videoId = "ncLSmmu7DCc",
            modifier = Modifier
                .fillMaxWidth()
                .clip(MaterialTheme.shapes.large)
        )
    }
}

@Composable
fun YoutubeThumbnailPlayer(
    videoId: String,
    modifier: Modifier = Modifier
) {
    val context = LocalContext.current
    val thumbnailUrl = "https://img.youtube.com/vi/$videoId/hqdefault.jpg"
    val videoUrl = "https://www.youtube.com/watch?v=$videoId"

    Box(
        modifier = modifier
            .height(200.dp)
            .clickable {
                val intent = Intent(Intent.ACTION_VIEW, Uri.parse(videoUrl))
                context.startActivity(intent)
            },
        contentAlignment = Alignment.Center
    ) {
        Image(
            painter = rememberAsyncImagePainter(model = thumbnailUrl),
            contentDescription = stringResource(id = R.string.video_content_description),
            modifier = Modifier.fillMaxSize(),
            contentScale = ContentScale.Crop
        )

        Box(
            modifier = Modifier
                .fillMaxSize()
                .background(Color.Black.copy(alpha = 0.3f))
        )

        Icon(
            imageVector = Icons.Filled.PlayArrow,
            contentDescription = stringResource(id = R.string.video_play_description),
            modifier = Modifier.size(64.dp),
            tint = Color.White.copy(alpha = 0.9f)
        )
    }
}