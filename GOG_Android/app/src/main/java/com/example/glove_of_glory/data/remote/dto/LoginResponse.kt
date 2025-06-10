package com.example.glove_of_glory.data.remote.dto
data class LoginResponse(
    val accessToken: String,
    val usuarioId: Int,
    val avatar: String?
)