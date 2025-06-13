package com.example.glove_of_glory.ui.screens

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import com.example.glove_of_glory.data.remote.RetrofitClient
import com.example.glove_of_glory.data.repository.UserRepository
import kotlinx.coroutines.launch

@Composable
fun ProfileScreen() {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()
    var apiResponse by remember { mutableStateOf("Haciendo petición...") }

    // Usamos un botón para lanzar la petición manualmente. Esto nos da más control.
    Column(
        modifier = Modifier.fillMaxSize(),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        Button(onClick = {
            scope.launch {
                try {
                    // --- PRUEBA DIRECTA ---
                    // Vamos a llamar a la API con un ID que sabemos que existe.
                    // Reemplaza '8' con el ID de un usuario que exista en tu DB.
                    val userIdToTest = 8

                    val userRepository = UserRepository(RetrofitClient.getInstance(context))
                    val response = userRepository.getUserProfile(userIdToTest)

                    if (response.isSuccessful) {
                        apiResponse = "ÉXITO 200 OK!\n\nNickname: ${response.body()?.nickname}"
                    } else {
                        // Si falla, mostramos el código de error y el mensaje.
                        apiResponse = "ERROR ${response.code()}\n\n${response.message()}\n\n${response.errorBody()?.string()}"
                    }
                } catch (e: Exception) {
                    apiResponse = "EXCEPCIÓN: ${e.message}"
                }
            }
        }) {
            Text("Probar GET /api/user/8")
        }

        Spacer(modifier = Modifier.height(16.dp))

        // Mostramos la respuesta cruda de la API
        Text(text = apiResponse)
    }
}