package com.example.glove_of_glory.data.remote

import com.example.glove_of_glory.data.remote.dto.LoginRequest
import com.example.glove_of_glory.data.remote.dto.LoginResponse
import com.example.glove_of_glory.data.remote.dto.RegisterResponse
import com.example.glove_of_glory.data.remote.dto.UserFullProfileDto // <-- IMPORTADO
import okhttp3.RequestBody
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.GET // <-- IMPORTADO
import retrofit2.http.Multipart
import retrofit2.http.POST
import retrofit2.http.Part
import retrofit2.http.Path // <-- IMPORTADO

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

    // --- AÑADIDO ---
    // Endpoint para obtener el perfil de un usuario.
    // Esta ruta estará protegida por el token que añade el AuthInterceptor.
    @GET("user/{id}")
    suspend fun getUserProfile(@Path("id") userId: Int): Response<UserFullProfileDto>
}