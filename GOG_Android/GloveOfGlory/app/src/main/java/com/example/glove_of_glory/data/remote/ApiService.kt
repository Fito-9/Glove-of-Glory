package com.example.glove_of_glory.data.remote

import com.example.glove_of_glory.data.remote.dto.LoginRequest
import com.example.glove_of_glory.data.remote.dto.LoginResponse
import com.example.glove_of_glory.data.remote.dto.RegisterResponse
import com.example.glove_of_glory.data.remote.dto.UserFullProfileDto
import okhttp3.RequestBody
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.Multipart
import retrofit2.http.POST
import retrofit2.http.Part

interface ApiService {
    @POST("user/login")
    suspend fun login(@Body request: LoginRequest): Response<LoginResponse>

    @Multipart
    @POST("user/register")
    suspend fun register(
        @Part("NombreUsuario") nombreUsuario: RequestBody,
        @Part("Email") email: RequestBody,
        @Part("Password") password: RequestBody
    ): Response<RegisterResponse>

    // Endpoint para obtener el perfil del usuario logueado usando el token
    @GET("user/me")
    suspend fun getMyProfile(): Response<UserFullProfileDto>
}