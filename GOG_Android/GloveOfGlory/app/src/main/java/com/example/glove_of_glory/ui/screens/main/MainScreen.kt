package com.example.glove_of_glory.ui.screens.main

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.AccountCircle
import androidx.compose.material.icons.filled.Menu
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.rotate
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import com.example.glove_of_glory.R
import com.example.glove_of_glory.data.local.UserPreferencesRepository
import com.example.glove_of_glory.data.remote.RetrofitClient
import com.example.glove_of_glory.data.repository.UserRepository
import com.example.glove_of_glory.navigation.Routes
import com.example.glove_of_glory.ui.models.AuthViewModel
import com.example.glove_of_glory.ui.models.AuthViewModelFactory
import com.example.glove_of_glory.ui.theme.SmashRed
import kotlinx.coroutines.launch

// Pantalla de marcador de posición para la última que nos queda.
@Composable fun HistoryScreen() { Text("Aquí irá la historia de Smash Ultimate.", color = MaterialTheme.colorScheme.onBackground) }

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

    val mainContentNavController = rememberNavController()
    val drawerState = rememberDrawerState(initialValue = DrawerValue.Closed)
    val scope = rememberCoroutineScope()

    ModalNavigationDrawer(
        drawerState = drawerState,
        drawerContent = {
            ModalDrawerSheet {
                Spacer(Modifier.height(12.dp))
                NavigationDrawerItem(
                    label = { Text(stringResource(id = R.string.menu_home)) },
                    selected = false,
                    onClick = {
                        mainContentNavController.navigate("home") {
                            popUpTo(mainContentNavController.graph.startDestinationId) { inclusive = true }
                        }
                        scope.launch { drawerState.close() }
                    }
                )
                Divider()
                NavigationDrawerItem(
                    label = { Text(stringResource(id = R.string.menu_how_to_play)) },
                    selected = false,
                    onClick = {
                        mainContentNavController.navigate("how_to_play")
                        scope.launch { drawerState.close() }
                    }
                )
                NavigationDrawerItem(
                    label = { Text(stringResource(id = R.string.menu_characters)) },
                    selected = false,
                    onClick = {
                        mainContentNavController.navigate("character_list")
                        scope.launch { drawerState.close() }
                    }
                )
                NavigationDrawerItem(
                    label = { Text(stringResource(id = R.string.menu_stages)) },
                    selected = false,
                    onClick = {
                        mainContentNavController.navigate("stage_list")
                        scope.launch { drawerState.close() }
                    }
                )
                NavigationDrawerItem(
                    label = { Text(stringResource(id = R.string.menu_history)) },
                    selected = false,
                    onClick = {
                        mainContentNavController.navigate("history")
                        scope.launch { drawerState.close() }
                    }
                )
            }
        }
    ) {
        Scaffold(
            topBar = {
                TopAppBar(
                    title = { Text(stringResource(id = R.string.main_title)) },
                    navigationIcon = {
                        IconButton(onClick = { scope.launch { drawerState.open() } }) {
                            Icon(
                                imageVector = Icons.Default.Menu,
                                contentDescription = stringResource(id = R.string.menu_description)
                            )
                        }
                    },
                    actions = {
                        IconButton(onClick = { navController.navigate(Routes.Profile.route) }) {
                            Icon(
                                imageVector = Icons.Default.AccountCircle,
                                contentDescription = stringResource(id = R.string.profile_button),
                                modifier = Modifier.size(32.dp)
                            )
                        }
                        IconButton(onClick = {
                            authViewModel.logout()
                            navController.navigate(Routes.Login.route) {
                                popUpTo(navController.graph.startDestinationId) { inclusive = true }
                            }
                        }) {
                            Icon(
                                imageVector = Icons.AutoMirrored.Filled.ArrowBack,
                                contentDescription = stringResource(id = R.string.logout_button),
                                modifier = Modifier.size(32.dp).rotate(180f)
                            )
                        }
                    },
                    colors = TopAppBarDefaults.topAppBarColors(
                        containerColor = SmashRed,
                        titleContentColor = Color.White,
                        actionIconContentColor = Color.White,
                        navigationIconContentColor = Color.White
                    )
                )
            }
        ) { paddingValues ->
            NavHost(
                navController = mainContentNavController,
                startDestination = "home",
                modifier = Modifier.padding(paddingValues)
            ) {
                composable("home") { HomeScreen() }
                composable("how_to_play") { HowToPlayScreen() }
                composable("character_list") { CharacterListScreen() }
                composable("stage_list") { StageListScreen() } // Ahora llama a la pantalla real
                composable("history") { Box(modifier=Modifier.fillMaxSize(), contentAlignment = Alignment.Center) { HistoryScreen() } }
            }
        }
    }
}