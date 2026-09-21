package com.archi.subscription.data.model

data class DebtInfo(
    val subscriptionId: String,
    val serviceProvider: String,
    val subscriberNumber: String,
    val debtAmount: Double,
    val dueDate: String,
    val period: String,
    val hasDebt: Boolean
)

