package com.ttmanager.pda

import com.google.gson.annotations.SerializedName
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Call
import retrofit2.Response
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.Path
import java.util.concurrent.TimeUnit

data class ScanRequest(
    @SerializedName("code") val code: String,
    @SerializedName("pdaName") val pdaName: String
)

data class ScanResponse(
    @SerializedName("success") val success: Boolean,
    @SerializedName("message") val message: String
)

// Carton scan request/response
data class CartonScanRequest(
    @SerializedName("machineName") val machineName: String,
    @SerializedName("cartonCode") val cartonCode: String,
    @SerializedName("scannedAt") val scannedAt: String,
    @SerializedName("mode") val mode: String = "scan"
)

data class CartonScanResponse(
    @SerializedName("success") val success: Boolean,
    @SerializedName("message") val message: String,
    @SerializedName("status") val status: String,
    @SerializedName("cartonIndex") val cartonIndex: Int,
    @SerializedName("orderNo") val orderNo: String,
    @SerializedName("productCount") val productCount: Int = 0,
    @SerializedName("activateDate") val activateDate: String = ""
)

data class CartonInfoResponse(
    @SerializedName("success") val success: Boolean,
    @SerializedName("cartonCode") val cartonCode: String,
    @SerializedName("cartonIndex") val cartonIndex: Int,
    @SerializedName("activateDate") val activateDate: String,
    @SerializedName("activateUser") val activateUser: String,
    @SerializedName("productCount") val productCount: Int,
    @SerializedName("status") val status: String,
    @SerializedName("message") val message: String
)

data class CurrentPOResponse(
    @SerializedName("success") val success: Boolean,
    @SerializedName("orderNo") val orderNo: String,
    @SerializedName("productName") val productName: String,
    @SerializedName("orderQty") val orderQty: Int,
    @SerializedName("state") val state: String,
    @SerializedName("message") val message: String
)

data class HealthResponse(
    @SerializedName("status") val status: String,
    @SerializedName("pendingScans") val pendingScans: Int
)

interface PdaApiService {
    @POST("/api/scan")
    suspend fun postScan(@Body request: ScanRequest): Response<ScanResponse>

    @GET("/api/health")
    suspend fun getHealth(): Response<HealthResponse>

    @POST("/api/carton/scan")
    suspend fun postCartonScan(@Body request: CartonScanRequest): Response<CartonScanResponse>

    @GET("/api/carton/{cartonCode}/info")
    suspend fun getCartonInfo(@Path("cartonCode") code: String): Response<CartonInfoResponse>

    @GET("/api/carton/current-po")
    suspend fun getCurrentPO(): Response<CurrentPOResponse>
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

    suspend fun postCartonScan(machineName: String, cartonCode: String, scannedAt: String, mode: String = "scan"): Result<CartonScanResponse> {
        return try {
            val response = apiService?.postCartonScan(CartonScanRequest(machineName, cartonCode, scannedAt, mode))
            if (response != null && response.isSuccessful) {
                Result.success(response.body()!!)
            } else {
                Result.failure(Exception("HTTP ${response?.code()}"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun getCartonInfo(cartonCode: String): Result<CartonInfoResponse> {
        return try {
            val response = apiService?.getCartonInfo(cartonCode)
            if (response != null && response.isSuccessful) {
                Result.success(response.body()!!)
            } else {
                Result.failure(Exception("HTTP ${response?.code()}"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun getCurrentPO(): Result<CurrentPOResponse> {
        return try {
            val response = apiService?.getCurrentPO()
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
