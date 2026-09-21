package com.archi.subscription.data.model

data class Payment(
    val id: String,
    val subscriptionId: String,
    val serviceProvider: String = "",
    val subscriberNumber: String = "",
    val amount: Double,
    val paymentDate: String,
    val period: String,
    val status: String,
    val transactionReference: String? = null
)

data class CreatePaymentRequest(
    val subscriptionId: String,
    val amount: Double,
    val period: String
)

