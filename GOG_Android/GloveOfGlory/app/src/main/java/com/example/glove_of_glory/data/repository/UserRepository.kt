package com.example.glove_of_glory.data.repository

import com.example.glove_of_glory.data.remote.ApiService

import com.example.glove_of_glory.data.remote.dto.LoginRequest
import com.example.glove_of_glory.data.remote.dto.RegisterRequestDto

class UserRepository(private val apiService: ApiService) {
    suspend fun loginUser(email: String, password: String) =
        apiService.login(LoginRequest(email = email, password = password))

    // --- CAMBIO: Actualizamos la firma y la implementación ---
    suspend fun registerUser(nombreUsuario: String, email: String, password: String, avatarId: String?) =
        apiService.register(
            RegisterRequestDto(
                nombreUsuario = nombreUsuario,
                email = email,
                password = password,
                avatarId = avatarId
            )
        )

    suspend fun getMyProfile() = apiService.getMyProfile()
}