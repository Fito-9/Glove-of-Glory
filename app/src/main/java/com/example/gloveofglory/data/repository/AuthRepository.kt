// app/src/main/java/com/example/gloveofglory/data/repository/AuthRepository.kt
package com.example.gloveofglory.data.repository

import com.example.gloveofglory.data.remote.dto.LoginRequest
import com.example.gloveofglory.data.remote.dto.LoginResponse
import com.example.gloveofglory.data.remote.dto.RegisterResponse
import com.example.gloveofglory.util.Resource
import okhttp3.MultipartBody
import okhttp3.RequestBody

interface AuthRepository {
    suspend fun login(loginRequest: LoginRequest): Resource<LoginResponse>
    suspend fun register(
        username: RequestBody,
        email: RequestBody,
        password: RequestBody,
        image: MultipartBody.Part?
    ): Resource<RegisterResponse>
}
