package com.example.glove_of_glory.data.remote.dto


data class UserFullProfileDto(
    val userId: Int,
    val nickname: String,
    val email: String,
    val elo: Int,
    val avatarUrl: String?
)