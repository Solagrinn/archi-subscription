package com.archi.subscription.data.model

data class Customer(
    val id: String,
    val fullName: String,
    val email: String,
    val phoneNumber: String,
    val createdAt: String,
    val activeSubscriptionCount: Int = 0
)

data class CreateCustomerRequest(
    val fullName: String,
    val email: String,
    val phoneNumber: String
)

