package com.example.glove_of_glory.ui.screens

import android.os.Build
import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.size
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController
import coil.ImageLoader
import coil.compose.rememberAsyncImagePainter
import coil.decode.GifDecoder
import coil.decode.ImageDecoderDecoder
import com.example.glove_of_glory.R // Asegúrate de que este import está presente
import com.example.glove_of_glory.data.local.UserPreferencesRepository
import com.example.glove_of_glory.navigation.Routes
import kotlinx.coroutines.flow.first

@Composable
fun SplashScreen(navController: NavController) {
    val context = LocalContext.current

    LaunchedEffect(key1 = true) {
        val prefsRepository = UserPreferencesRepository(context)
        val authToken = prefsRepository.authToken.first()

        val destination = if (authToken.isNullOrEmpty()) {
            Routes.Login.route
        } else {
            Routes.Main.route
        }

        navController.navigate(destination) {
            popUpTo(Routes.Splash.route) { inclusive = true }
        }
    }

    val imageLoader = ImageLoader.Builder(context)
        .components {
            if (Build.VERSION.SDK_INT >= 28) {
                add(ImageDecoderDecoder.Factory())
            } else {
                add(GifDecoder.Factory())
            }
        }
        .build()

    Box(
        modifier = Modifier.fillMaxSize(),
        contentAlignment = Alignment.Center
    ) {
        Image(
            painter = rememberAsyncImagePainter(

                model = R.drawable.chomik,

                imageLoader = imageLoader
            ),
            contentDescription = "Animación de carga",
            modifier = Modifier.size(250.dp)
        )
    }
}