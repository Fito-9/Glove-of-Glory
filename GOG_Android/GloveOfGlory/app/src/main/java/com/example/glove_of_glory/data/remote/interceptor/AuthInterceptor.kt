package com.example.glove_of_glory.data.remote.interceptor

import android.content.Context
import com.example.glove_of_glory.data.local.UserPreferencesRepository
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.runBlocking
import okhttp3.Interceptor
import okhttp3.Response

class AuthInterceptor(context: Context) : Interceptor {

    private val prefsRepository = UserPreferencesRepository(context.applicationContext)

    override fun intercept(chain: Interceptor.Chain): Response {
        val originalRequest = chain.request()

        val authToken = runBlocking {
            prefsRepository.authToken.first()
        }

        if (authToken.isNullOrEmpty()) {
            return chain.proceed(originalRequest)
        }

        val newRequest = originalRequest.newBuilder()
            .header("X-Session-Token", authToken)
            .build()

        return chain.proceed(newRequest)
    }
}