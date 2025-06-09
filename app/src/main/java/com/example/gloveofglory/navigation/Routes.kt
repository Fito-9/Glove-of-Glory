package com.example.gloveofglory.navigation

sealed class Routes(val route: String) {
    object Splash : Routes("splash_screen")
    object Login : Routes("login_screen")
    object SignUp : Routes("signup_screen")
    object Main : Routes("main_screen")
}