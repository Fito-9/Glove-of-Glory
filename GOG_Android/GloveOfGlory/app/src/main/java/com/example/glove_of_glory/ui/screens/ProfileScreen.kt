package com.example.glove_of_glory.ui.screens

import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.Email
import androidx.compose.material.icons.filled.Star
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.navigation.NavController
import coil.compose.rememberAsyncImagePainter
import com.example.glove_of_glory.R
import com.example.glove_of_glory.data.remote.RetrofitClient
import com.example.glove_of_glory.data.remote.dto.UserFullProfileDto
import com.example.glove_of_glory.data.repository.UserRepository
import com.example.glove_of_glory.ui.theme.SmashGold
import com.example.glove_of_glory.ui.theme.SmashRed
import kotlinx.coroutines.launch

// --- CAMBIO: La firma ahora acepta un NavController ---
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ProfileScreen(navController: NavController) {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()
    var userProfile by remember { mutableStateOf<UserFullProfileDto?>(null) }
    var isLoading by remember { mutableStateOf(true) }
    var errorMessage by remember { mutableStateOf<String?>(null) }

    LaunchedEffect(key1 = Unit) {
        scope.launch {
            isLoading = true
            try {
                val userRepository = UserRepository(RetrofitClient.getInstance(context))
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

    // --- CAMBIO: Envolvemos todo en un Scaffold ---
    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text(stringResource(id = R.string.profile_title)) },
                navigationIcon = {
                    // --- El botón de volver ---
                    IconButton(onClick = { navController.popBackStack() }) {
                        Icon(
                            imageVector = Icons.AutoMirrored.Filled.ArrowBack,
                            contentDescription = stringResource(id = R.string.profile_back_button_description)
                        )
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = SmashRed,
                    titleContentColor = Color.White,
                    navigationIconContentColor = Color.White
                )
            )
        }
    ) { paddingValues ->
        // El Surface ahora usa el padding del Scaffold para no quedar debajo de la TopAppBar
        Surface(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues),
            color = MaterialTheme.colorScheme.background
        ) {
            when {
                isLoading -> {
                    Box(contentAlignment = Alignment.Center, modifier = Modifier.fillMaxSize()) {
                        CircularProgressIndicator()
                    }
                }
                errorMessage != null -> {
                    Box(contentAlignment = Alignment.Center, modifier = Modifier.fillMaxSize()) {
                        Text(
                            text = errorMessage!!,
                            color = MaterialTheme.colorScheme.error,
                            modifier = Modifier.padding(16.dp)
                        )
                    }
                }
                userProfile != null -> {
                    ProfileContent(user = userProfile!!)
                }
            }
        }
    }
}

@Composable
fun ProfileContent(user: UserFullProfileDto) {
    Column(
        modifier = Modifier
            .fillMaxSize()
            .verticalScroll(rememberScrollState())
    ) {
        Box(
            modifier = Modifier
                .fillMaxWidth()
                .height(200.dp),
            contentAlignment = Alignment.Center
        ) {
            Box(
                modifier = Modifier
                    .fillMaxWidth()
                    .fillMaxHeight(0.6f)
                    .align(Alignment.TopCenter)
                    .background(SmashRed)
            )
            Image(
                painter = rememberAsyncImagePainter(
                    model = user.avatarUrl,
                    fallback = painterResource(id = R.drawable.ic_launcher_foreground)
                ),
                contentDescription = stringResource(id = R.string.profile_avatar_description),
                contentScale = ContentScale.Crop,
                modifier = Modifier
                    .size(128.dp)
                    .clip(CircleShape)
                    .background(MaterialTheme.colorScheme.surface)
                    .padding(4.dp)
                    .clip(CircleShape)
                    .align(Alignment.BottomCenter)
            )
        }

        Spacer(modifier = Modifier.height(16.dp))

        Text(
            text = user.nickname,
            style = MaterialTheme.typography.headlineLarge,
            fontWeight = FontWeight.Bold,
            modifier = Modifier.align(Alignment.CenterHorizontally)
        )

        Spacer(modifier = Modifier.height(24.dp))

        InfoCard(
            icon = Icons.Default.Star,
            iconColor = SmashGold,
            title = stringResource(id = R.string.profile_elo_label),
            value = user.elo.toString(),
            iconDescription = stringResource(id = R.string.profile_elo_icon_description)
        )

        InfoCard(
            icon = Icons.Default.Email,
            iconColor = MaterialTheme.colorScheme.secondary,
            title = stringResource(id = R.string.profile_email_label),
            value = user.email,
            iconDescription = stringResource(id = R.string.profile_email_icon_description)
        )
    }
}

@Composable
fun InfoCard(
    icon: ImageVector,
    iconColor: Color,
    title: String,
    value: String,
    iconDescription: String
) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .padding(horizontal = 16.dp, vertical = 8.dp),
        shape = MaterialTheme.shapes.large,
        elevation = CardDefaults.cardElevation(defaultElevation = 4.dp),
        colors = CardDefaults.cardColors(containerColor = MaterialTheme.colorScheme.surfaceVariant)
    ) {
        Row(
            modifier = Modifier.padding(16.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Icon(
                imageVector = icon,
                contentDescription = iconDescription,
                modifier = Modifier.size(40.dp),
                tint = iconColor
            )
            Spacer(modifier = Modifier.width(16.dp))
            Column {
                Text(
                    text = title,
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
                Text(
                    text = value,
                    style = MaterialTheme.typography.bodyLarge,
                    fontWeight = FontWeight.Bold,
                    fontSize = 18.sp
                )
            }
        }
    }
}