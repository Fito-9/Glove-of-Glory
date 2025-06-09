package com.example.gloveofglory.data.remote.dto

import com.google.gson.annotations.SerializedName

data class RegisterResponse(
    @SerializedName("message")
    val message: String,
    @SerializedName("avatar")
    val avatar: String?
)
