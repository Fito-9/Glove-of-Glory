package com.example.gloveofglory.data.remote.dto

import com.google.gson.annotations.SerializedName

data class LoginResponse(
    @SerializedName("accessToken")
    val accessToken: String,
    @SerializedName("usuarioId")
    val userId: Int,
    @SerializedName("avatar")
    val avatarUrl: String?
)