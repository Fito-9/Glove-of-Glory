package com.example.glove_of_glory.ui.screens

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import com.example.glove_of_glory.R
import com.example.glove_of_glory.data.local.UserPreferencesRepository
import com.example.glove_of_glory.data.remote.RetrofitClient
import com.example.glove_of_glory.data.remote.dto.UserFullProfileDto
import com.example.glove_of_glory.data.repository.UserRepository
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.launch

@Composable
fun ProfileScreen() {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()
    var userProfile by remember { mutableStateOf<UserFullProfileDto?>(null) }
    var isLoading by remember { mutableStateOf(true) }
    var errorMessage by remember { mutableStateOf<String?>(null) }

    // LaunchedEffect se ejecuta una vez para cargar los datos del perfil.
    LaunchedEffect(key1 = Unit) {
        scope.launch {
            // --- LÓGICA CORREGIDA ---
            // 1. Creamos una instancia del repositorio de preferencias.
            val prefsRepository = UserPreferencesRepository(context)

            // 2. Leemos el ID del usuario que hemos guardado en DataStore al hacer login.
            val userId = prefsRepository.userId.first()

            // 3. Si por alguna razón no encontramos un ID, mostramos un error.
            if (userId == null) {
                errorMessage = context.getString(R.string.session_invalid_error)
                isLoading = false
                return@launch
            }
            // --- FIN DE LA LÓGICA CORREGIDA ---

            // Si tenemos un ID, procedemos a llamar a la API.
            try {
                val userRepository = UserRepository(RetrofitClient.getInstance(context))
                // Ahora la llamada a getMyProfile usará el token que el interceptor añade
                val response = userRepository.getMyProfile()
                if (response.isSuccessful) {
                    userProfile = response.body()
                } else {
                    errorMessage = context.getString(R.string.profile_load_error, response.message())
                }
            } catch (e: Exception) {
                errorMessage = context.getString(R.string.network_error, e.message ?: "Unknown error")
            } finally {
                isLoading = false
            }
        }
    }

    // La UI para mostrar los diferentes estados: carga, error o el perfil.
    Box(
        modifier = Modifier.fillMaxSize(),
        contentAlignment = Alignment.Center
    ) {
        when {
            isLoading -> {
                Column(horizontalAlignment = Alignment.CenterHorizontally) {
                    CircularProgressIndicator()
                    Spacer(modifier = Modifier.height(8.dp))
                    Text(text = stringResource(id = R.string.loading_text))
                }
            }
            errorMessage != null -> {
                Text(text = errorMessage!!, color = MaterialTheme.colorScheme.error)
            }
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