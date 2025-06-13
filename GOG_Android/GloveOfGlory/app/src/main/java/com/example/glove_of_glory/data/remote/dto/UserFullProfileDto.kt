package com.example.glove_of_glory.data.remote.dto

// Este DTO representa la respuesta del backend para el perfil de un usuario.
// Asegúrate de que los nombres de las variables coincidan con el JSON que devuelve tu API.
data class UserFullProfileDto(
    val userId: Int,
    val nickname: String,
    val email: String,
    val elo: Int,
    val avatarUrl: String?
)