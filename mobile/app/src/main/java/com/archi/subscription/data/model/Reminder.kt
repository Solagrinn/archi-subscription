package com.archi.subscription.data.model

data class ReminderResponse(
    val count: Int,
    val checkedAt: String,
    val reminders: List<Reminder>
)

data class Reminder(
    val subscriptionId: String,
    val customerId: String,
    val customerName: String = "",
    val customerEmail: String = "",
    val customerPhone: String = "",
    val subscriptionType: String,
    val serviceProvider: String,
    val subscriberNumber: String,
    val paymentDayOfMonth: Int,
    val period: String,
    val message: String
)

