package com.example.glove_of_glory.data.repository

import android.content.Context
import com.example.glove_of_glory.data.model.Stage
import com.google.gson.Gson
import com.google.gson.reflect.TypeToken
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.io.IOException

class StageRepository(private val context: Context) {

    suspend fun getStages(): List<Stage> {
        return withContext(Dispatchers.IO) {
            try {
                val jsonString = context.assets.open("stages.json")
                    .bufferedReader()
                    .use { it.readText() }

                val listType = object : TypeToken<List<Stage>>() {}.type
                Gson().fromJson(jsonString, listType)
            } catch (e: IOException) {
                e.printStackTrace()
                emptyList()
            }
        }
    }
}