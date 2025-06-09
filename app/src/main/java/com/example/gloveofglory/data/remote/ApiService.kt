package com.example.gloveofglory.data.remote

import com.example.gloveofglory.data.remote.dto.LoginRequest
import com.example.gloveofglory.data.remote.dto.LoginResponse
import com.example.gloveofglory.data.remote.dto.RegisterResponse
import okhttp3.MultipartBody
import okhttp3.RequestBody
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.Multipart
import retrofit2.http.POST
import retrofit2.http.Part

interface ApiService {

    @Multipart
    @POST("api/User/register")
    suspend fun register(
        @Part("NombreUsuario") username: RequestBody,
        @Part("Email") email: RequestBody,
        @Part("Password") password: RequestBody,
        @Part image: MultipartBody.Part?
    ): Response<RegisterResponse>

    @POST("api/User/login")
    suspend fun login(
        @Body loginRequest: LoginRequest
    ): Response<LoginResponse>
}