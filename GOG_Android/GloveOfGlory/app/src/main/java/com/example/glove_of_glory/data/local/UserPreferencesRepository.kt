package com.example.glove_of_glory.data.local

import android.content.Context
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.intPreferencesKey
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map

private val Context.dataStore: DataStore<Preferences> by preferencesDataStore(name = "user_preferences")

class UserPreferencesRepository(context: Context) {

    private val appContext = context.applicationContext

    private object PreferencesKeys {
        val AUTH_TOKEN = stringPreferencesKey("auth_token")
        val USER_ID = intPreferencesKey("user_id") // <-- AÑADIDA CLAVE PARA EL ID
    }

    // Flow para observar el token
    val authToken: Flow<String?> = appContext.dataStore.data.map { preferences ->
        preferences[PreferencesKeys.AUTH_TOKEN]
    }

    // Flow para observar el ID del usuario
    val userId: Flow<Int?> = appContext.dataStore.data.map { preferences ->
        preferences[PreferencesKeys.USER_ID]
    }

    // --- MÉTODO MODIFICADO ---
    // Ahora guarda tanto el token como el ID del usuario
    suspend fun saveAuthTokenAndId(token: String, userId: Int) {
        appContext.dataStore.edit { preferences ->
            preferences[PreferencesKeys.AUTH_TOKEN] = token
            preferences[PreferencesKeys.USER_ID] = userId
        }
    }

    // Borra todas las preferencias al hacer logout
    suspend fun clear() {
        appContext.dataStore.edit { preferences ->
            preferences.clear()
        }
    }
}