package com.example.glove_of_glory.ui.screens

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import com.example.glove_of_glory.data.remote.RetrofitClient
import com.example.glove_of_glory.data.remote.dto.UserFullProfileDto
import com.example.glove_of_glory.data.repository.UserRepository
import kotlinx.coroutines.launch

@Composable
fun ProfileScreen() {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()
    var userProfile by remember { mutableStateOf<UserFullProfileDto?>(null) }
    var isLoading by remember { mutableStateOf(true) }
    var errorMessage by remember { mutableStateOf<String?>(null) }

    LaunchedEffect(key1 = Unit) {
        scope.launch {
            try {
                val userRepository = UserRepository(RetrofitClient.getInstance(context))
                // Llamamos al nuevo endpoint que no necesita parámetros
                val response = userRepository.getMyProfile()
                if (response.isSuccessful) {
                    userProfile = response.body()
                } else {
                    errorMessage = "Error ${response.code()}: Sesión inválida o expirada. Por favor, inicia sesión de nuevo."
                }
            } catch (e: Exception) {
                errorMessage = "Error de red: ${e.message}"
            } finally {
                isLoading = false
            }
        }
    }

    Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
        when {
            isLoading -> CircularProgressIndicator()
            errorMessage != null -> Text(text = errorMessage!!, color = MaterialTheme.colorScheme.error)
            userProfile != null -> {
                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                    Text(text = userProfile!!.nickname, style = MaterialTheme.typography.headlineMedium)
                    Spacer(modifier = Modifier.height(8.dp))
                    Text(text = "Email: ${userProfile!!.email}")
                    Spacer(modifier = Modifier.height(4.dp))
                    Text(text = "ELO: ${userProfile!!.elo}")
                }
            }
        }
    }
}