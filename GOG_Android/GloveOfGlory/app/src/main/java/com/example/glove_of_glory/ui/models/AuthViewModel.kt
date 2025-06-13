package com.example.glove_of_glory.ui.models

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.glove_of_glory.data.local.UserPreferencesRepository
import com.example.glove_of_glory.data.remote.dto.ErrorResponse
import com.example.glove_of_glory.data.remote.dto.LoginResponse
import com.example.glove_of_glory.data.remote.dto.RegisterResponse
import com.example.glove_of_glory.data.repository.UserRepository
import com.example.glove_of_glory.util.Resource
import com.google.gson.Gson
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import java.io.IOException

class AuthViewModel(
    private val userRepository: UserRepository,
    private val prefsRepository: UserPreferencesRepository
) : ViewModel() {

    private val _loginState = MutableStateFlow<Resource<LoginResponse>?>(null)
    val loginState: StateFlow<Resource<LoginResponse>?> = _loginState.asStateFlow()

    private val _registerState = MutableStateFlow<Resource<RegisterResponse>?>(null)
    val registerState: StateFlow<Resource<RegisterResponse>?> = _registerState.asStateFlow()

    private val gson = Gson()

    fun login(email: String, password: String) {
        viewModelScope.launch {
            _loginState.value = Resource.Loading()
            try {
                val response = userRepository.loginUser(email, password)
                if (response.isSuccessful && response.body() != null) {
                    val loginData = response.body()!!
                    prefsRepository.saveAuthTokenAndId(loginData.accessToken, loginData.usuarioId)
                    _loginState.value = Resource.Success(loginData)
                } else {
                    val errorBody = response.errorBody()?.string()
                    val errorMessage = if (!errorBody.isNullOrEmpty()) {
                        try {
                            if (errorBody.trim().startsWith("{")) {
                                val errorResponse = gson.fromJson(errorBody, ErrorResponse::class.java)
                                errorResponse.detail ?: errorResponse.title ?: "Error desconocido"
                            } else {
                                errorBody
                            }
                        } catch (e: Exception) {
                            response.message() ?: "Error al parsear la respuesta"
                        }
                    } else {
                        response.message() ?: "Error desconocido"
                    }
                    _loginState.value = Resource.Error(errorMessage)
                }
            } catch (e: IOException) {
                _loginState.value = Resource.Error("Error de red. Por favor, comprueba tu conexión.")
            } catch (e: Exception) {
                _loginState.value = Resource.Error(e.message ?: "Ocurrió un error inesperado.")
            }
        }
    }

    // --- CAMBIO: Añadimos avatarId a la firma ---
    fun register(nombreUsuario: String, email: String, password: String, avatarId: String?) {
        viewModelScope.launch {
            _registerState.value = Resource.Loading()
            try {
                // --- CAMBIO: Pasamos el avatarId al repositorio ---
                val response = userRepository.registerUser(nombreUsuario, email, password, avatarId)
                if (response.isSuccessful && response.body() != null) {
                    _registerState.value = Resource.Success(response.body()!!)
                } else {
                    val errorMessage = response.errorBody()?.string() ?: response.message() ?: "Error desconocido"
                    _registerState.value = Resource.Error(errorMessage)
                }
            } catch (e: IOException) {
                _registerState.value = Resource.Error("Error de red. Por favor, comprueba tu conexión.")
            } catch (e: Exception) {
                _registerState.value = Resource.Error(e.message ?: "Ocurrió un error inesperado.")
            }
        }
    }

    fun logout() {
        viewModelScope.launch {
            prefsRepository.clear()
            _loginState.value = null
            _registerState.value = null
        }
    }

    fun resetLoginState() {
        _loginState.value = null
    }

    fun resetRegisterState() {
        _registerState.value = null
    }
}