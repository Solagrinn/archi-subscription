package com.archi.subscription.data.model

data class Subscription(
    val id: String,
    val customerId: String,
    val customerName: String = "",
    val type: String,
    val serviceProvider: String,
    val subscriberNumber: String,
    val status: String,
    val paymentDayOfMonth: Int,
    val createdAt: String
)

data class CreateSubscriptionRequest(
    val customerId: String,
    val type: Int,
    val serviceProvider: String,
    val subscriberNumber: String,
    val paymentDayOfMonth: Int
)

data class UpdateSubscriptionRequest(
    val type: Int? = null,
    val serviceProvider: String? = null,
    val subscriberNumber: String? = null,
    val status: Int? = null,
    val paymentDayOfMonth: Int? = null
)

enum class SubscriptionType(val value: Int, val label: String, val icon: String) {
    Electricity(1, "Electricity", "⚡"),
    Water(2, "Water", "💧"),
    Internet(3, "Internet", "🌐"),
    GSM(4, "GSM", "📱"),
    NaturalGas(5, "Natural Gas", "🔥"),
    Insurance(6, "Insurance", "🛡️"),
    Other(99, "Other", "📄");

    companion object {
        fun fromValue(value: Int) = entries.find { it.value == value } ?: Other
        fun fromName(name: String) = entries.find { it.name == name || it.label == name } ?: Other
    }
}

