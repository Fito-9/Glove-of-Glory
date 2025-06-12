package com.example.glove_of_glory.navigation

import androidx.compose.runtime.Composable
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.example.glove_of_glory.ui.screens.auth.LoginScreen
import com.example.glove_of_glory.ui.screens.MainScreen
import com.example.glove_of_glory.ui.screens.auth.RegisterScreen



@Composable
fun AppNavigation() {
    val navController = rememberNavController()
    NavHost(
        navController = navController,
        startDestination = Routes.Login.route
    ) {
        composable(Routes.Login.route) {
            LoginScreen(navController = navController)
        }
        composable(Routes.SignUp.route) {
            RegisterScreen(navController = navController)
        }
        composable(Routes.Main.route) {
            MainScreen(navController = navController)
        }
    }
}