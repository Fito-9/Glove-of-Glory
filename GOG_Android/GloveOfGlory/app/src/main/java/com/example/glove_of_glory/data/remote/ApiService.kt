package com.example.glove_of_glory.data.remote

import com.example.glove_of_glory.data.remote.dto.LoginRequest
import com.example.glove_of_glory.data.remote.dto.LoginResponse
import com.example.glove_of_glory.data.remote.dto.RegisterRequestDto // <-- Lo importas desde su archivo
import com.example.glove_of_glory.data.remote.dto.RegisterResponse
import com.example.glove_of_glory.data.remote.dto.UserFullProfileDto
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST

interface ApiService {
    @POST("user/login")
    suspend fun login(@Body request: LoginRequest): Response<LoginResponse>

    @POST("user/register")
    suspend fun register(@Body request: RegisterRequestDto): Response<RegisterResponse>

    @GET("user/me")
    suspend fun getMyProfile(): Response<UserFullProfileDto>
}