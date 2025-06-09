package com.example.gloveofglory.data.repository

import com.example.gloveofglory.data.remote.ApiService
import com.example.gloveofglory.data.remote.dto.LoginRequest
import com.example.gloveofglory.data.remote.dto.LoginResponse
import com.example.gloveofglory.data.remote.dto.RegisterResponse
import com.example.gloveofglory.util.Resource
import okhttp3.MultipartBody
import okhttp3.RequestBody
import retrofit2.HttpException
import java.io.IOException
import javax.inject.Inject

class AuthRepositoryImpl @Inject constructor(
    private val apiService: ApiService
) : AuthRepository {

    override suspend fun login(loginRequest: LoginRequest): Resource<LoginResponse> {
        return try {
            val response = apiService.login(loginRequest)
            if (response.isSuccessful && response.body() != null) {
                Resource.Success(response.body()!!)
            } else {
                val errorMsg = response.errorBody()?.string() ?: "Error desconocido"
                Resource.Error(errorMsg)
            }
        } catch (e: HttpException) {
            Resource.Error("Error de red: ${e.message()}")
        } catch (e: IOException) {
            Resource.Error("Error de conexión. Revisa tu conexión a internet.")
        }
    }

    override suspend fun register(
        username: RequestBody,
        email: RequestBody,
        password: RequestBody,
        image: MultipartBody.Part?
    ): Resource<RegisterResponse> {
        return try {
            val response = apiService.register(username, email, password, image)
            if (response.isSuccessful && response.body() != null) {
                Resource.Success(response.body()!!)
            } else {
                val errorMsg = response.errorBody()?.string() ?: "Error desconocido"
                Resource.Error(errorMsg)
            }
        } catch (e: HttpException) {
            Resource.Error("Error de red: ${e.message()}")
        } catch (e: IOException) {
            Resource.Error("Error de conexión. Revisa tu conexión a internet.")
        }
    }
}