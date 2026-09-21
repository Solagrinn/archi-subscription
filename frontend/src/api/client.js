const BASE_URL = '/api';

async function request(url, options = {}) {
  const config = {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  };

  const response = await fetch(`${BASE_URL}${url}`, config);

  if (!response.ok) {
    const body = await response.json().catch(() => null);
    const message = body?.message || body?.title || `Request failed with status ${response.status}`;
    throw new Error(message);
  }

  if (response.status === 204) return null;
  return response.json();
}

// ── Seed ─────────────────────────────────────────────────────────────────────
export const seedDatabase = () => request('/seed', { method: 'POST' });
export const clearDatabase = () => request('/seed', { method: 'DELETE' });

// ── Customers ────────────────────────────────────────────────────────────────
export const fetchCustomers = () => request('/customers');
export const fetchCustomerById = (id) => request(`/customers/${id}`);
export const createCustomer = (dto) =>
  request('/customers', { method: 'POST', body: JSON.stringify(dto) });
export const deleteCustomer = (id) =>
  request(`/customers/${id}`, { method: 'DELETE' });

// ── Subscriptions ────────────────────────────────────────────────────────────
export const fetchSubscriptionsByCustomer = (customerId) =>
  request(`/subscriptions/customer/${customerId}`);
export const fetchUnpaidSubscriptions = (customerId) =>
  request(`/subscriptions/customer/${customerId}/unpaid`);
export const fetchDebt = (subscriptionId, period) => {
  const params = period ? `?period=${encodeURIComponent(period)}` : '';
  return request(`/subscriptions/${subscriptionId}/debt${params}`);
};
export const createSubscription = (dto) =>
  request('/subscriptions', { method: 'POST', body: JSON.stringify(dto) });
export const updateSubscription = (id, dto) =>
  request(`/subscriptions/${id}`, { method: 'PUT', body: JSON.stringify(dto) });
export const deleteSubscription = (id) =>
  request(`/subscriptions/${id}`, { method: 'DELETE' });

// ── Payments ─────────────────────────────────────────────────────────────────
export const fetchPaymentsByCustomer = (customerId) =>
  request(`/payments/customer/${customerId}`);
export const fetchPaymentsBySubscription = (subscriptionId) =>
  request(`/payments/subscription/${subscriptionId}`);
export const createPayment = (dto) =>
  request('/payments', { method: 'POST', body: JSON.stringify(dto) });

// ── Reminders ────────────────────────────────────────────────────────────
export const fetchReminders = (customerId, daysAhead = 7) => {
  const params = new URLSearchParams({ daysAhead: String(daysAhead) });
  if (customerId) params.set('customerId', customerId);
  return request(`/reminders?${params}`);
};

export const sendNotifications = (customerId, daysAhead = 7) => {
  const params = new URLSearchParams({ customerId, daysAhead: String(daysAhead) });
  return request(`/reminders/notify?${params}`, { method: 'POST' });
};

// ── Time Warp ────────────────────────────────────────────────────────────
export const fetchCurrentTime = () => request('/time');
export const timeForward = () => request('/time/forward', { method: 'POST' });
export const timeBackward = () => request('/time/backward', { method: 'POST' });
export const timeReset = () => request('/time/reset', { method: 'POST' });

