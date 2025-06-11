package com.example.glove_of_glory.ui.screens.auth

import android.widget.Toast
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.text.ClickableText
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.AnnotatedString
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.style.TextDecoration
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavController
import com.example.glove_of_glory.data.remote.RetrofitClient
import com.example.glove_of_glory.data.repository.UserRepository
import com.example.glove_of_glory.navigation.Routes
import com.example.glove_of_glory.ui.theme.SmashRed
import com.example.glove_of_glory.ui.models.AuthViewModel
import com.example.glove_of_glory.ui.models.AuthViewModelFactory
import com.example.glove_of_glory.util.Resource
import com.example.gloveofglory.navigation.Routes

@Composable
fun LoginScreen(navController: NavController) {
    val context = LocalContext.current

    // Inyección manual de dependencias para el ViewModel
    val authViewModel: AuthViewModel = viewModel(
        factory = AuthViewModelFactory(UserRepository(RetrofitClient.instance))
    )

    var email by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }

    val loginState by authViewModel.loginState.collectAsState()

    LaunchedEffect(loginState) {
        when (val state = loginState) {
            is Resource.Success -> {
                Toast.makeText(context, "Login exitoso!", Toast.LENGTH_SHORT).show()
                navController.navigate(Routes.Main.route) {
                    popUpTo(Routes.Login.route) {
                        inclusive = true
                    }
                }
                authViewModel.resetLoginState()
            }
            is Resource.Error -> {
                Toast.makeText(context, state.message, Toast.LENGTH_LONG).show()
                authViewModel.resetLoginState()
            }
            is Resource.Loading -> { /* El indicador de carga se muestra en la UI */ }
            null -> { /* Estado inicial */ }
        }
    }

    Box(
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp),
        contentAlignment = Alignment.Center
    ) {
        Column(
            modifier = Modifier.fillMaxWidth(),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center
        ) {
            Text(
                text = "Glove of Glory",
                style = MaterialTheme.typography.displayMedium
            )
            Spacer(modifier = Modifier.height(8.dp))
            Text(
                text = "Iniciar Sesión",
                style = MaterialTheme.typography.headlineLarge
            )
            Spacer(modifier = Modifier.height(32.dp))

            OutlinedTextField(
                value = email,
                onValueChange = { email = it },
                label = { Text("Email") },
                modifier = Modifier.fillMaxWidth(),
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Email),
                singleLine = true
            )
            Spacer(modifier = Modifier.height(16.dp))

            OutlinedTextField(
                value = password,
                onValueChange = { password = it },
                label = { Text("Contraseña") },
                modifier = Modifier.fillMaxWidth(),
                visualTransformation = PasswordVisualTransformation(),
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Password),
                singleLine = true
            )
            Spacer(modifier = Modifier.height(32.dp))

            Button(
                onClick = {
                    if (email.isNotBlank() && password.isNotBlank()) {
                        authViewModel.login(email, password)
                    } else {
                        Toast.makeText(context, "Por favor, rellena todos los campos", Toast.LENGTH_SHORT).show()
                    }
                },
                modifier = Modifier
                    .fillMaxWidth()
                    .height(50.dp),
                colors = ButtonDefaults.buttonColors(containerColor = SmashRed),
                enabled = loginState !is Resource.Loading
            ) {
                if (loginState is Resource.Loading) {
                    CircularProgressIndicator(
                        modifier = Modifier.size(24.dp),
                        color = MaterialTheme.colorScheme.onPrimary
                    )
                } else {
                    Text("INICIAR SESIÓN", style = MaterialTheme.typography.labelLarge)
                }
            }
            Spacer(modifier = Modifier.height(24.dp))

            ClickableText(
                text = AnnotatedString("¿No tienes cuenta? Regístrate"),
                onClick = { navController.navigate(Routes.SignUp.route) },
                style = TextStyle(
                    color = MaterialTheme.colorScheme.primary,
                    textDecoration = TextDecoration.Underline
                )
            )
        }
    }
}