package com.example.glove_of_glory.ui.screens

import android.widget.Toast
import androidx.compose.foundation.Image
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.text.ClickableText
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.AnnotatedString
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.style.TextDecoration
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavController
import coil.compose.rememberAsyncImagePainter
import com.example.glove_of_glory.R
import com.example.glove_of_glory.data.local.UserPreferencesRepository
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
        factory = AuthViewModelFactory(
            userRepository = UserRepository(RetrofitClient.getInstance(context)),
            prefsRepository = UserPreferencesRepository(context)
        )
    )

    var username by remember { mutableStateOf("") }
    var email by remember { mutableStateOf("") }
    var password by remember { mutableStateOf("") }
    var confirmPassword by remember { mutableStateOf("") }
    var selectedAvatarId by remember { mutableStateOf<String?>(null) }
    val registerState by authViewModel.registerState.collectAsState()

    LaunchedEffect(registerState) {
        when (val state = registerState) {
            is Resource.Success -> {
                Toast.makeText(context, state.data?.message ?: context.getString(R.string.register_success_message), Toast.LENGTH_LONG).show()
                navController.navigate(Routes.Login.route) {
                    popUpTo(Routes.Login.route) { inclusive = true }
                    launchSingleTop = true
                }
                authViewModel.resetRegisterState()
            }
            is Resource.Error -> {
                Toast.makeText(context, state.message, Toast.LENGTH_LONG).show()
                authViewModel.resetRegisterState()
            }
            is Resource.Loading -> { }
            null -> { }
        }
    }

    val avatars = mapOf(
        "mario" to "https://www.smashbros.com/assets_v2/img/fighter/mario/main.png",
        "donkey_kong" to "https://www.smashbros.com/assets_v2/img/fighter/donkey_kong/main.png",
        "link" to "https://www.smashbros.com/assets_v2/img/fighter/link/main.png",
        "samus" to "https://www.smashbros.com/assets_v2/img/fighter/samus/main.png"
    )

    Box(
        modifier = Modifier.fillMaxSize().padding(16.dp),
        contentAlignment = Alignment.Center
    ) {
        Column(
            modifier = Modifier.fillMaxWidth().verticalScroll(rememberScrollState()),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center
        ) {
            Text(text = stringResource(id = R.string.register_title), style = MaterialTheme.typography.headlineLarge)
            Spacer(modifier = Modifier.height(24.dp))

            Text(
                text = stringResource(id = R.string.register_avatar_prompt),
                style = MaterialTheme.typography.titleMedium
            )
            Spacer(modifier = Modifier.height(8.dp))
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceEvenly
            ) {
                avatars.forEach { (id, url) ->
                    AvatarItem(
                        imageUrl = url,
                        isSelected = selectedAvatarId == id,
                        onClick = { selectedAvatarId = id }
                    )
                }
            }
            Spacer(modifier = Modifier.height(24.dp))

            OutlinedTextField(value = username, onValueChange = { username = it }, label = { Text(stringResource(id = R.string.username_label)) }, modifier = Modifier.fillMaxWidth(), singleLine = true, shape = MaterialTheme.shapes.medium)
            Spacer(modifier = Modifier.height(16.dp))
            OutlinedTextField(value = email, onValueChange = { email = it }, label = { Text(stringResource(id = R.string.email_label)) }, modifier = Modifier.fillMaxWidth(), keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Email), singleLine = true, shape = MaterialTheme.shapes.medium)
            Spacer(modifier = Modifier.height(16.dp))
            OutlinedTextField(value = password, onValueChange = { password = it }, label = { Text(stringResource(id = R.string.password_label)) }, modifier = Modifier.fillMaxWidth(), visualTransformation = PasswordVisualTransformation(), keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Password), singleLine = true, shape = MaterialTheme.shapes.medium)
            Spacer(modifier = Modifier.height(16.dp))
            OutlinedTextField(value = confirmPassword, onValueChange = { confirmPassword = it }, label = { Text(stringResource(id = R.string.confirm_password_label)) }, modifier = Modifier.fillMaxWidth(), visualTransformation = PasswordVisualTransformation(), keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Password), singleLine = true, shape = MaterialTheme.shapes.medium)
            Spacer(modifier = Modifier.height(32.dp))

            Button(
                onClick = {
                    if (username.isBlank() || email.isBlank() || password.isBlank()) {
                        Toast.makeText(context, context.getString(R.string.fill_all_fields_error), Toast.LENGTH_SHORT).show()
                        return@Button
                    }
                    if (password != confirmPassword) {
                        Toast.makeText(context, context.getString(R.string.passwords_do_not_match_error), Toast.LENGTH_SHORT).show()
                        return@Button
                    }
                    authViewModel.register(username, email, password, selectedAvatarId)
                },
                modifier = Modifier.fillMaxWidth().height(50.dp),
                colors = ButtonDefaults.buttonColors(containerColor = SmashRed),
                enabled = registerState !is Resource.Loading,
                shape = MaterialTheme.shapes.medium
            ) {
                if (registerState is Resource.Loading) {
                    CircularProgressIndicator(modifier = Modifier.size(24.dp), color = MaterialTheme.colorScheme.onPrimary)
                } else {
                    Text(stringResource(id = R.string.register_button), style = MaterialTheme.typography.labelLarge)
                }
            }
            Spacer(modifier = Modifier.height(24.dp))

            ClickableText(
                text = AnnotatedString(stringResource(id = R.string.already_have_account_link)),
                onClick = { navController.popBackStack() },
                style = TextStyle(
                    color = MaterialTheme.colorScheme.primary,
                    textDecoration = TextDecoration.Underline
                )
            )
        }
    }
}

@Composable
fun AvatarItem(
    imageUrl: String,
    isSelected: Boolean,
    onClick: () -> Unit
) {
    val borderColor = if (isSelected) MaterialTheme.colorScheme.primary else Color.Transparent
    Image(
        painter = rememberAsyncImagePainter(model = imageUrl),
        contentDescription = stringResource(id = R.string.avatar_content_description),
        modifier = Modifier
            .size(64.dp)
            .clip(CircleShape)
            .border(3.dp, borderColor, CircleShape)
            .clickable(onClick = onClick),
        contentScale = ContentScale.Crop
    )
}