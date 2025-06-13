package com.example.glove_of_glory.data.remote.interceptor

import android.content.Context
import com.example.glove_of_glory.data.local.UserPreferencesRepository
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.runBlocking
import okhttp3.Interceptor
import okhttp3.Response

class AuthInterceptor(context: Context) : Interceptor {

    private val prefsRepository = UserPreferencesRepository(context)

    override fun intercept(chain: Interceptor.Chain): Response {
        // Obtenemos la petición original
        val originalRequest = chain.request()

        // Leemos el token de forma síncrona para el interceptor
        // runBlocking es aceptable aquí porque los interceptores de OkHttp no son corrutinas.
        val authToken = runBlocking {
            prefsRepository.authToken.first()
        }

        // Si no hay token, simplemente continuamos con la petición original (para login/register)
        if (authToken.isNullOrEmpty()) {
            return chain.proceed(originalRequest)
        }

        // Si hay token, creamos una nueva petición añadiendo la cabecera de autorización
        val newRequest = originalRequest.newBuilder()
            .header("Authorization", "Bearer $authToken")
            .build()

        // Procedemos con la nueva petición que ya incluye el token
        return chain.proceed(newRequest)
    }
}