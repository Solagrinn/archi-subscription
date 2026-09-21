package com.archi.subscription.data.repository

import com.archi.subscription.data.model.*
import com.archi.subscription.data.remote.RetrofitClient

class ArchiRepository {
    private val api = RetrofitClient.api

    // ── Customers ──
    suspend fun getCustomers() = runCatching { api.getCustomers() }
    suspend fun createCustomer(req: CreateCustomerRequest) = runCatching { api.createCustomer(req) }
    suspend fun deleteCustomer(id: String) = runCatching { api.deleteCustomer(id) }

    // ── Seed ──
    suspend fun seed() = runCatching { api.seed() }
    suspend fun clearAll() = runCatching { api.clearAll() }

    // ── Subscriptions ──
    suspend fun getSubscriptionsByCustomer(customerId: String) = runCatching { api.getSubscriptionsByCustomer(customerId) }
    suspend fun createSubscription(req: CreateSubscriptionRequest) = runCatching { api.createSubscription(req) }
    suspend fun updateSubscription(id: String, req: UpdateSubscriptionRequest) = runCatching { api.updateSubscription(id, req) }
    suspend fun deleteSubscription(id: String) = runCatching { api.deleteSubscription(id) }
    suspend fun inquireDebt(id: String, period: String? = null) = runCatching { api.inquireDebt(id, period) }

    // ── Payments ──
    suspend fun getPaymentsByCustomer(customerId: String) = runCatching { api.getPaymentsByCustomer(customerId) }
    suspend fun getPaymentsBySubscription(subscriptionId: String) = runCatching { api.getPaymentsBySubscription(subscriptionId) }
    suspend fun createPayment(req: CreatePaymentRequest) = runCatching { api.createPayment(req) }

    // ── Reminders ──
    suspend fun getReminders(customerId: String?, daysAhead: Int = 7) = runCatching { api.getReminders(customerId, daysAhead) }
    suspend fun sendNotifications(customerId: String) = runCatching { api.sendNotifications(customerId) }

    // ── Time ──
    suspend fun getTime() = runCatching { api.getTime() }
    suspend fun timeForward() = runCatching { api.timeForward() }
    suspend fun timeBackward() = runCatching { api.timeBackward() }
    suspend fun timeReset() = runCatching { api.timeReset() }
}

