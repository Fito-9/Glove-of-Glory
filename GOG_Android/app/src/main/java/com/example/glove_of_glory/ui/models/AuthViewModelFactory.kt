package com.example.glove_of_glory.ui.models

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.example.glove_of_glory.data.repository.UserRepository

class AuthViewModelFactory(private val userRepository: UserRepository) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        // Comprueba si la clase que se pide es AuthViewModel
        if (modelClass.isAssignableFrom(AuthViewModel::class.java)) {
            // Si lo es, crea una instancia y la devuelve
            @Suppress("UNCHECKED_CAST")
            return AuthViewModel(userRepository) as T
        }
        // Si se pide crear otro tipo de ViewModel que no conocemos, lanza un error.
        throw IllegalArgumentException("Unknown ViewModel class")
    }
}