package com.example.gloveofglory.data.remote.dto

// Modelo genérico para errores del backend
data class ErrorResponse(
    val title: String?,
    val status: Int?,
    val detail: String?,
    val errors: Map<String, List<String>>?
)