package com.archi.subscription.ui.home

import androidx.compose.runtime.*
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.archi.subscription.data.model.*
import com.archi.subscription.data.repository.ArchiRepository
import kotlinx.coroutines.launch

class HomeViewModel : ViewModel() {
    private val repo = ArchiRepository()

    var customers by mutableStateOf<List<Customer>>(emptyList())
        private set
    var loading by mutableStateOf(false)
        private set
    var error by mutableStateOf<String?>(null)
        private set
    var timeInfo by mutableStateOf<TimeInfo?>(null)
        private set
    var timeLoading by mutableStateOf(false)
        private set

    init { loadAll() }

    fun loadAll() {
        viewModelScope.launch {
            loading = true; error = null
            repo.getCustomers()
                .onSuccess { customers = it }
                .onFailure { error = it.message }
            repo.getTime().onSuccess { timeInfo = it }
            loading = false
        }
    }

    fun seed() {
        viewModelScope.launch {
            loading = true; error = null
            repo.seed()
                .onFailure { error = it.message }
            repo.getCustomers().onSuccess { customers = it }
            loading = false
        }
    }

    fun clearAll() {
        viewModelScope.launch {
            loading = true; error = null
            repo.clearAll()
                .onSuccess { customers = emptyList() }
                .onFailure { error = it.message }
            loading = false
        }
    }

    fun createCustomer(name: String, email: String, phone: String) {
        viewModelScope.launch {
            loading = true; error = null
            repo.createCustomer(CreateCustomerRequest(name, email, phone))
                .onFailure { error = it.message }
            repo.getCustomers().onSuccess { customers = it }
            loading = false
        }
    }

    fun deleteCustomer(id: String) {
        viewModelScope.launch {
            error = null
            repo.deleteCustomer(id)
                .onSuccess { customers = customers.filter { it.id != id } }
                .onFailure { error = it.message }
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
        }
    }
}

