package com.archi.subscription.data.remote

import com.archi.subscription.data.model.*
import retrofit2.http.*

interface ApiService {

    // ── Customers ──
    @GET("customers")
    suspend fun getCustomers(): List<Customer>

    @GET("customers/{id}")
    suspend fun getCustomer(@Path("id") id: String): Customer

    @POST("customers")
    suspend fun createCustomer(@Body request: CreateCustomerRequest): Customer

    @DELETE("customers/{id}")
    suspend fun deleteCustomer(@Path("id") id: String)

    // ── Seed ──
    @POST("seed")
    suspend fun seed(): Map<String, Any>

    @DELETE("seed")
    suspend fun clearAll()

    // ── Subscriptions ──
    @GET("subscriptions/customer/{customerId}")
    suspend fun getSubscriptionsByCustomer(@Path("customerId") customerId: String): List<Subscription>

    @POST("subscriptions")
    suspend fun createSubscription(@Body request: CreateSubscriptionRequest): Subscription

    @PUT("subscriptions/{id}")
    suspend fun updateSubscription(@Path("id") id: String, @Body request: UpdateSubscriptionRequest): Subscription

    @DELETE("subscriptions/{id}")
    suspend fun deleteSubscription(@Path("id") id: String)

    @GET("subscriptions/{id}/debt")
    suspend fun inquireDebt(@Path("id") id: String, @Query("period") period: String? = null): DebtInfo

    // ── Payments ──
    @GET("payments/customer/{customerId}")
    suspend fun getPaymentsByCustomer(@Path("customerId") customerId: String): List<Payment>

    @GET("payments/subscription/{subscriptionId}")
    suspend fun getPaymentsBySubscription(@Path("subscriptionId") subscriptionId: String): List<Payment>

    @POST("payments")
    suspend fun createPayment(@Body request: CreatePaymentRequest): Payment

    // ── Reminders ──
    @GET("reminders")
    suspend fun getReminders(
        @Query("customerId") customerId: String? = null,
        @Query("daysAhead") daysAhead: Int = 7
    ): ReminderResponse

    @POST("reminders/notify")
    suspend fun sendNotifications(
        @Query("customerId") customerId: String,
        @Query("daysAhead") daysAhead: Int = 7
    ): Map<String, Any>

    // ── Time Warp ──
    @GET("time")
    suspend fun getTime(): TimeInfo

    @POST("time/forward")
    suspend fun timeForward(): TimeInfo

    @POST("time/backward")
    suspend fun timeBackward(): TimeInfo

    @POST("time/reset")
    suspend fun timeReset(): TimeInfo
}

