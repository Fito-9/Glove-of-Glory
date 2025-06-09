// app/src/main/java/com/example/gloveofglory/ui/screens/splash/SplashViewModel.kt
package com.example.gloveofglory.ui.screens.splash

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.gloveofglory.data.local.UserPreferencesRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class SplashViewModel @Inject constructor(
    private val userPreferencesRepository: UserPreferencesRepository
) : ViewModel() {

    fun checkUserSession(onResult: (isLoggedIn: Boolean) -> Unit) {
        viewModelScope.launch {
            val token = userPreferencesRepository.authToken.first()
            onResult(!token.isNullOrBlank())
        }
    }
}

