package com.example.glove_of_glory.navigation

import androidx.compose.runtime.Composable
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.example.glove_of_glory.ui.screens.LoginScreen
import com.example.glove_of_glory.ui.screens.RegisterScreen
import com.example.glove_of_glory.ui.screens.main.MainScreen

@Composable
fun AppNavigation() {
    val navController = rememberNavController()
    NavHost(
        navController = navController,
        startDestination = Routes.Main.route // <-- Empezamos en MainScreen
    ) {
        composable(Routes.Main.route) {
            MainScreen(navController = navController)
        }
        composable(Routes.Login.route) {
            LoginScreen(navController = navController)
        }
        composable(Routes.Register.route) {
            RegisterScreen(navController = navController)
        }
    }
}