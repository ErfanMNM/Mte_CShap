package com.ttmanager.pda

import com.google.gson.annotations.SerializedName
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Call
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import java.util.concurrent.TimeUnit

data class ScanRequest(
    @SerializedName("code") val code: String,
    @SerializedName("pdaName") val pdaName: String
)

data class ScanResponse(
    @SerializedName("success") val success: Boolean,
    @SerializedName("message") val message: String
)

data class HealthResponse(
    @SerializedName("status") val status: String,
    @SerializedName("pendingScans") val pendingScans: Int
)

interface PdaApiService {
    @POST("/api/scan")
    suspend fun postScan(@Body request: ScanRequest): ScanResponse

    @GET("/api/health")
    suspend fun getHealth(): HealthResponse
}

class ApiClient {

    private var baseUrl: String = "http://127.0.0.1:6969/"
    private var apiService: PdaApiService? = null

    fun configure(ip: String, port: Int = 6969) {
        baseUrl = "http://${ip.trim()}:${port}/"
        buildService()
    }

    private fun buildService() {
        val logging = HttpLoggingInterceptor().apply {
            level = HttpLoggingInterceptor.Level.BODY
        }

        val okHttp = OkHttpClient.Builder()
            .connectTimeout(10, TimeUnit.SECONDS)
            .readTimeout(10, TimeUnit.SECONDS)
            .writeTimeout(10, TimeUnit.SECONDS)
            .addInterceptor(logging)
            .retryOnConnectionFailure(true)
            .build()

        val retrofit = Retrofit.Builder()
            .baseUrl(baseUrl)
            .client(okHttp)
            .addConverterFactory(GsonConverterFactory.create())
            .build()

        apiService = retrofit.create(PdaApiService::class.java)
    }

    suspend fun postScan(code: String, pdaName: String): Result<ScanResponse> {
        return try {
            val response = apiService?.postScan(ScanRequest(code, pdaName))
            if (response != null && response.isSuccessful) {
                Result.success(response.body()!!)
            } else {
                Result.failure(Exception("HTTP ${response?.code()}"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun checkHealth(): Result<HealthResponse> {
        return try {
            val response = apiService?.getHealth()
            if (response != null && response.isSuccessful) {
                Result.success(response.body()!!)
            } else {
                Result.failure(Exception("HTTP ${response?.code()}"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    fun getBaseUrl(): String = baseUrl
}
