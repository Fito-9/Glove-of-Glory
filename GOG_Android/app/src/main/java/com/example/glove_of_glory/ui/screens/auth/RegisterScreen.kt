package com.example.glove_of_glory.ui.screens.auth

import android.widget.Toast
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.text.ClickableText
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
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
import com.example.glove_of_glory.ui.models.AuthViewModel
import com.example.glove_of_glory.ui.models.AuthViewModelFactory
import com.example.glove_of_glory.ui.theme.SmashRed
import com.example.glove_of_glory.util.Resource

@Composable
fun RegisterScreen(navController: NavController) {
    val context = LocalContext.current

    val authViewModel: AuthViewModel = viewModel(
        factory = AuthViewModelFactory(UserRepository(RetrofitClient.instance))
    )

    var username by remember { mutableStateOf("") }
    var email by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
    var confirmPassword by remember { mutableStateOf("") }

    val registerState by authViewModel.registerState.collectAsState()

    LaunchedEffect(registerState) {
        when (val state = registerState) {
            is Resource.Success -> {
                Toast.makeText(context, state.data?.message ?: "Registro exitoso. Ahora inicia sesión.", Toast.LENGTH_LONG).show()
                navController.navigate(Routes.Login.route) {
                    // Limpiamos la pila de navegación hasta la pantalla de Login para que el usuario no vuelva a Registro
                    popUpTo(Routes.Login.route) { inclusive = true }
                }
                authViewModel.resetRegisterState()
            }
            is Resource.Error -> {
                Toast.makeText(context, state.message, Toast.LENGTH_LONG).show()
                authViewModel.resetRegisterState()
            }
            is Resource.Loading -> { /* Indicador de carga en la UI */ }
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
            modifier = Modifier
                .fillMaxWidth()
                .verticalScroll(rememberScrollState()), // Para evitar que el teclado oculte los campos
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center
        ) {
            Text(
                text = "Crear Cuenta",
                style = MaterialTheme.typography.headlineLarge
            )
            Spacer(modifier = Modifier.height(32.dp))

            OutlinedTextField(
                value = username,
                onValueChange = { username = it },
                label = { Text("Nombre de Usuario") },
                modifier = Modifier.fillMaxWidth(),
                singleLine = true
            )
            Spacer(modifier = Modifier.height(16.dp))

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
            Spacer(modifier = Modifier.height(16.dp))

            OutlinedTextField(
                value = confirmPassword,
                onValueChange = { confirmPassword = it },
                label = { Text("Confirmar Contraseña") },
                modifier = Modifier.fillMaxWidth(),
                visualTransformation = PasswordVisualTransformation(),
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Password),
                singleLine = true
            )
            Spacer(modifier = Modifier.height(32.dp))

            Button(
                onClick = {
                    if (username.isBlank() || email.isBlank() || password.isBlank()) {
                        Toast.makeText(context, "Por favor, rellena todos los campos", Toast.LENGTH_SHORT).show()
                        return@Button
                    }
                    if (password != confirmPassword) {
                        Toast.makeText(context, "Las contraseñas no coinciden", Toast.LENGTH_SHORT).show()
                        return@Button
                    }
                    authViewModel.register(username, email, password)
                },
                modifier = Modifier
                    .fillMaxWidth()
                    .height(50.dp),
                colors = ButtonDefaults.buttonColors(containerColor = SmashRed),
                enabled = registerState !is Resource.Loading
            ) {
                if (registerState is Resource.Loading) {
                    CircularProgressIndicator(
                        modifier = Modifier.size(24.dp),
                        color = MaterialTheme.colorScheme.onPrimary
                    )
                } else {
                    Text("REGISTRARSE", style = MaterialTheme.typography.labelLarge)
                }
            }
            Spacer(modifier = Modifier.height(24.dp))

            ClickableText(
                text = AnnotatedString("¿Ya tienes cuenta? Inicia sesión"),
                onClick = { navController.popBackStack() },
                style = TextStyle(
                    color = MaterialTheme.colorScheme.primary,
                    textDecoration = TextDecoration.Underline
                )
            )
        }
    }
}