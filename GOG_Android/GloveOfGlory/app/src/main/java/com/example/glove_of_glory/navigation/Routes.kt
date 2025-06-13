package com.example.glove_of_glory.navigation

// En navigation/Routes.kt
sealed class Routes(val route: String) {
    object Splash : Routes("splash_screen")
    object Main : Routes("main_screen")
    object Login : Routes("login_screen")
    object Register : Routes("register_screen")
    object Profile : Routes("profile_screen") // <-- AÑADIDO
}