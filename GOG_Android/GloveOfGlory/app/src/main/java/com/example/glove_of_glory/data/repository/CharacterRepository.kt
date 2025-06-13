package com.example.glove_of_glory.data.repository

import android.content.Context
import com.example.glove_of_glory.data.model.Character
import com.google.gson.Gson
import com.google.gson.reflect.TypeToken
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.io.IOException

class CharacterRepository(private val context: Context) {

    suspend fun getCharacters(): List<Character> {
        return withContext(Dispatchers.IO) {
            try {
                val jsonString = context.assets.open("characters.json")
                    .bufferedReader()
                    .use { it.readText() }

                val listType = object : TypeToken<List<Character>>() {}.type
                Gson().fromJson(jsonString, listType)
            } catch (e: IOException) {
                e.printStackTrace()
                emptyList()
            }
        }
    }
}