package com.example.glove_of_glory.navigation

sealed class Routes(val route: String) {
    object Main : Routes("main_screen")
    object Login : Routes("login_screen")
    object Register : Routes("register_screen")
}