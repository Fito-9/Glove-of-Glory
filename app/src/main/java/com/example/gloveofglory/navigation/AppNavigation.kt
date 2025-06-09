// app/src/main/java/com/example/gloveofglory/navigation/AppNavigation.kt
package com.example.gloveofglory.navigation

import androidx.compose.runtime.Composable
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.example.gloveofglory.ui.screens.login.LoginScreen
import com.example.gloveofglory.ui.screens.main.MainScreen
import com.example.gloveofglory.ui.screens.signup.SignUpScreen
import com.example.gloveofglory.ui.screens.splash.SplashScreen

@Composable
fun AppNavigation() {
    val navController = rememberNavController()
    NavHost(
        navController = navController,
        startDestination = Routes.Splash.route
    ) {
        composable(Routes.Splash.route) {
            SplashScreen(navController = navController)
        }
        composable(Routes.Login.route) {
            LoginScreen(navController = navController)
        }
        composable(Routes.SignUp.route) {
            SignUpScreen(navController = navController)
        }
        composable(Routes.Main.route) {
            MainScreen(navController = navController)
        }
    }
}