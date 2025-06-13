package com.example.glove_of_glory.navigation

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.example.glove_of_glory.data.local.UserPreferencesRepository
import com.example.glove_of_glory.ui.screens.LoginScreen
import com.example.glove_of_glory.ui.screens.ProfileScreen
import com.example.glove_of_glory.ui.screens.RegisterScreen
import com.example.glove_of_glory.ui.screens.main.MainScreen
import kotlinx.coroutines.flow.first

@Composable
fun AppNavigation() {
    val context = LocalContext.current
    val prefsRepository = UserPreferencesRepository(context)

    // Usamos un estado para guardar la ruta de inicio. Inicialmente es nulo.
    var startDestination by remember { mutableStateOf<String?>(null) }

    // Este LaunchedEffect se ejecuta una vez para decidir la ruta de inicio.
    LaunchedEffect(key1 = true) {
        val authToken = prefsRepository.authToken.first()
        startDestination = if (authToken.isNullOrEmpty()) {
            Routes.Login.route
        } else {
            Routes.Main.route
        }
    }

    // Mientras startDestination sea nulo, mostramos una pantalla de carga.
    if (startDestination != null) {
        val navController = rememberNavController()
        NavHost(
            navController = navController,
            startDestination = startDestination!! // Usamos la ruta que hemos decidido
        ) {
            // Ya no necesitamos una ruta para "Splash", la lógica está fuera.
            composable(Routes.Main.route) {
                MainScreen(navController = navController)
            }
            composable(Routes.Login.route) {
                LoginScreen(navController = navController)
            }
            composable(Routes.Register.route) {
                RegisterScreen(navController = navController)
            }
            composable(Routes.Profile.route) {
                ProfileScreen()
            }
        }
    } else {
        // Pantalla de carga genérica mientras decidimos la ruta
        Box(
            modifier = Modifier.fillMaxSize(),
            contentAlignment = Alignment.Center
        ) {
            CircularProgressIndicator()
        }
    }
}