package com.archi.subscription.ui.dashboard

import androidx.compose.runtime.*
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.archi.subscription.data.model.*
import com.archi.subscription.data.repository.ArchiRepository
import com.archi.subscription.util.PeriodUtils
import kotlinx.coroutines.launch
import java.time.ZoneOffset
import java.time.temporal.ChronoUnit

data class PeriodStatus(val period: String, val paid: Boolean, val payment: Payment? = null)

class DashboardViewModel : ViewModel() {
    private val repo = ArchiRepository()

    var customerId by mutableStateOf("")
    var customerName by mutableStateOf("")
    var customerEmail by mutableStateOf("")
    var customerPhone by mutableStateOf("")

    var subscriptions by mutableStateOf<List<Subscription>>(emptyList()); private set
    var payments by mutableStateOf<List<Payment>>(emptyList()); private set
    var reminders by mutableStateOf<List<Reminder>>(emptyList()); private set
    var periodStatuses by mutableStateOf<Map<String, List<PeriodStatus>>>(emptyMap()); private set
    var timeInfo by mutableStateOf<TimeInfo?>(null); private set
    var loading by mutableStateOf(true); private set
    var error by mutableStateOf<String?>(null); private set
    var timeLoading by mutableStateOf(false); private set

    fun init(id: String, name: String, email: String, phone: String) {
        customerId = id; customerName = name; customerEmail = email; customerPhone = phone
        refreshAll()
    }

    fun refreshAll() {
        viewModelScope.launch {
            loading = true; error = null
            val subsResult = repo.getSubscriptionsByCustomer(customerId)
            val paysResult = repo.getPaymentsByCustomer(customerId)
            val remsResult = repo.getReminders(customerId)
            val timeResult = repo.getTime()

            subsResult.onSuccess { subscriptions = it }.onFailure { error = it.message }
            paysResult.onSuccess { payments = it }.onFailure { error = it.message }
            remsResult.onSuccess { reminders = it.reminders }.onFailure { error = it.message }
            timeResult.onSuccess { timeInfo = it }

            // Build period statuses
            val pays = paysResult.getOrDefault(emptyList())
            val subs = subsResult.getOrDefault(emptyList())
            val ti = timeResult.getOrNull()
            if (ti != null) {
                periodStatuses = buildPeriodStatuses(subs, pays, ti)
            }
            loading = false
        }
    }

    private fun buildPeriodStatuses(subs: List<Subscription>, pays: List<Payment>, ti: TimeInfo): Map<String, List<PeriodStatus>> {
        val paidLookup = mutableMapOf<String, Payment>()
        for (p in pays) {
            if (p.status == "Successful") paidLookup["${p.subscriptionId}|${p.period}"] = p
        }

        val backendDate = PeriodUtils.parseDate(ti.currentDate).withZoneSameInstant(ZoneOffset.UTC)
        val baseYear = backendDate.year
        val baseMonth = backendDate.monthValue
        val todayDate = backendDate.toLocalDate()

        val map = mutableMapOf<String, List<PeriodStatus>>()
        for (sub in subs) {
            if (sub.status != "Active") continue
            val periods = mutableListOf<PeriodStatus>()
            for (offset in -3..1) {
                val refDate = java.time.LocalDate.of(baseYear, 1, 1).withMonth(1).plusMonths((baseMonth - 1 + offset).toLong())
                val prd = "${refDate.year}-${refDate.monthValue.toString().padStart(2, '0')}"

                // Skip periods before subscription was created
                try {
                    val subCreated = PeriodUtils.parseDate(sub.createdAt).withZoneSameInstant(ZoneOffset.UTC).toLocalDate()
                    val subCreatedMonth = subCreated.withDayOfMonth(1)
                    if (refDate.isBefore(subCreatedMonth)) continue
                } catch (_: Exception) {}

                // For +1 month, only include if payment day within 7 days
                if (offset == 1) {
                    val daysInMonth = refDate.lengthOfMonth()
                    val payDay = minOf(sub.paymentDayOfMonth, daysInMonth)
                    val nextDue = refDate.withDayOfMonth(payDay)
                    val daysUntil = ChronoUnit.DAYS.between(todayDate, nextDue)
                    if (daysUntil > 7) continue
                }

                val key = "${sub.id}|${prd}"
                val paidPayment = paidLookup[key]
                periods.add(PeriodStatus(prd, paidPayment != null, paidPayment))
            }
            map[sub.id] = periods
        }
        return map
    }

    // ── Actions ──
    fun sendNotifications(onResult: (String) -> Unit) {
        viewModelScope.launch {
            repo.sendNotifications(customerId)
                .onSuccess { onResult("Notifications sent!") }
                .onFailure { onResult("Failed: ${it.message}") }
        }
    }

    suspend fun inquireDebt(subscriptionId: String, period: String?): Result<DebtInfo> =
        repo.inquireDebt(subscriptionId, period)

    suspend fun makePayment(subscriptionId: String, amount: Double, period: String): Result<Payment> =
        repo.createPayment(CreatePaymentRequest(subscriptionId, amount, period))

    fun deleteSubscription(id: String) {
        viewModelScope.launch {
            repo.deleteSubscription(id).onFailure { error = it.message }
            refreshAll()
        }
    }

    fun createSubscription(req: CreateSubscriptionRequest) {
        viewModelScope.launch {
            repo.createSubscription(req).onFailure { error = it.message }
            refreshAll()
        }
    }

    fun updateSubscription(id: String, req: UpdateSubscriptionRequest) {
        viewModelScope.launch {
            repo.updateSubscription(id, req).onFailure { error = it.message }
            refreshAll()
        }
    }

    fun timeForward() { timeAction { repo.timeForward() } }
    fun timeBackward() { timeAction { repo.timeBackward() } }
    fun timeReset() { timeAction { repo.timeReset() } }

    private fun timeAction(action: suspend () -> Result<TimeInfo>) {
        viewModelScope.launch {
            timeLoading = true
            action().onSuccess { timeInfo = it }
            timeLoading = false
            refreshAll()
        }
    }
}

