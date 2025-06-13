package com.example.glove_of_glory.ui.models

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.example.glove_of_glory.data.local.UserPreferencesRepository
import com.example.glove_of_glory.data.repository.UserRepository

class AuthViewModelFactory(
    private val userRepository: UserRepository,
    private val prefsRepository: UserPreferencesRepository
) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        if (modelClass.isAssignableFrom(AuthViewModel::class.java)) {
            @Suppress("UNCHECKED_CAST")
            return AuthViewModel(userRepository, prefsRepository) as T
        }
        throw IllegalArgumentException("Unknown ViewModel class")
    }
}