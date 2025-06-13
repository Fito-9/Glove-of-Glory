package com.example.glove_of_glory.ui.screens.main

import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack // <-- IMPORTAMOS ESTE
import androidx.compose.material.icons.filled.AccountCircle
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.rotate // <-- IMPORTAMOS ROTATE
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavController
import com.example.glove_of_glory.R
import com.example.glove_of_glory.data.local.UserPreferencesRepository
import com.example.glove_of_glory.data.remote.RetrofitClient
import com.example.glove_of_glory.data.repository.UserRepository
import com.example.glove_of_glory.navigation.Routes
import com.example.glove_of_glory.ui.models.AuthViewModel
import com.example.glove_of_glory.ui.models.AuthViewModelFactory
import com.example.glove_of_glory.ui.theme.SmashRed

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MainScreen(navController: NavController) {
    val context = LocalContext.current
    val authViewModel: AuthViewModel = viewModel(
        factory = AuthViewModelFactory(
            userRepository = UserRepository(RetrofitClient.getInstance(context)),
            prefsRepository = UserPreferencesRepository(context)
        )
    )

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Glove of Glory") },
                actions = {
                    // 1. Botón para ir al Perfil del Usuario
                    IconButton(onClick = {
                        navController.navigate(Routes.Profile.route)
                    }) {
                        Icon(
                            imageVector = Icons.Default.AccountCircle,
                            contentDescription = "Perfil de Usuario",
                            modifier = Modifier.size(32.dp)
                        )
                    }

                    // 2. Botón para Cerrar Sesión (Logout)
                    IconButton(onClick = {
                        authViewModel.logout()
                        navController.navigate(Routes.Login.route) {
                            popUpTo(navController.graph.startDestinationId) { inclusive = true }
                        }
                    }) {
                        Icon(
                            // --- CAMBIO AQUÍ ---
                            // Usamos el icono de "atrás" y lo rotamos 180 grados
                            imageVector = Icons.AutoMirrored.Filled.ArrowBack,
                            contentDescription = "Cerrar Sesión",
                            modifier = Modifier
                                .size(32.dp)
                                .rotate(180f) // <-- TRUCO PARA QUE APUNTE HACIA AFUERA
                        )
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = SmashRed,
                    titleContentColor = Color.White,
                    actionIconContentColor = Color.White
                )
            )
        }
    ) { paddingValues ->
        Box(modifier = Modifier.fillMaxSize().padding(paddingValues)) {
            Image(
                painter = painterResource(id = R.drawable.background_smash),
                contentDescription = "Fondo de pantalla de Smash",
                modifier = Modifier.fillMaxSize(),
                contentScale = ContentScale.Crop
            )
            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .padding(16.dp),
                verticalArrangement = Arrangement.Center,
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                Text(
                    text = "¡Prepárate para la Gloria!",
                    fontSize = 32.sp,
                    fontWeight = FontWeight.Bold,
                    color = Color.White,
                    style = MaterialTheme.typography.displaySmall
                )
            }
        }
    }
}