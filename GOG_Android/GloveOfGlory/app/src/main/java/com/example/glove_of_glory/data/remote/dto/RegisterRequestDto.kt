package com.example.glove_of_glory.data.remote.dto

data class RegisterRequestDto(
    val nombreUsuario: String,
    val email: String,
    val password: String,
    val avatarId: String?
)