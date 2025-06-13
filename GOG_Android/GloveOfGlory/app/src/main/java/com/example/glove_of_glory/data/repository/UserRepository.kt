package com.example.glove_of_glory.data.repository

import com.example.glove_of_glory.data.remote.ApiService
import com.example.glove_of_glory.data.remote.dto.LoginRequest
import okhttp3.MediaType.Companion.toMediaTypeOrNull
import okhttp3.RequestBody.Companion.toRequestBody

class UserRepository(private val apiService: ApiService) {
    suspend fun loginUser(email: String, password: String) =
        apiService.login(LoginRequest(email = email, password = password))

    suspend fun registerUser(nombreUsuario: String, email: String, password: String) =
        apiService.register(
            nombreUsuario = nombreUsuario.toRequestBody("text/plain".toMediaTypeOrNull()),
            email = email.toRequestBody("text/plain".toMediaTypeOrNull()),
            password = password.toRequestBody("text/plain".toMediaTypeOrNull())
        )

    // --- AÑADIDO ---
    suspend fun getUserProfile(userId: Int) = apiService.getUserProfile(userId)
}