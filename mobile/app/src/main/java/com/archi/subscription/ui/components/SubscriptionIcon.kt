package com.archi.subscription.ui.components

import com.archi.subscription.data.model.SubscriptionType

fun subscriptionIcon(type: String): String =
    SubscriptionType.fromName(type).icon

