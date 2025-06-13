package com.example.glove_of_glory.data.remote.dto

data class ErrorResponse(
    val title: String?,
    val status: Int?,
    val detail: String?,
    val errors: Map<String, List<String>>?
)