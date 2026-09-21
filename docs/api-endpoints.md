# API Endpoints

## Base URL
- Local: `http://localhost:5000`
- Docker: `http://localhost:8080`
- Mobile (Android Emulator): `http://10.0.2.2:8080`

---

## Customers

### GET /api/customers
List all customers.

**Response:** `200 OK`
```json
[
  {
    "id": "7aa2bf38-8d7d-49f3-ba88-3c61da077041",
    "fullName": "Ahmet Yılmaz",
    "email": "ahmet@example.com",
    "phoneNumber": "+905551234567",
    "createdAt": "2026-05-11T16:11:51.028Z",
    "activeSubscriptionCount": 2
  }
]
```

### GET /api/customers/{id}
Get a single customer by ID.

**Response:** `200 OK` | `404 Not Found`

### POST /api/customers
Create a new customer.

**Request Body:**
```json
{
  "fullName": "Ahmet Yılmaz",
  "email": "ahmet@example.com",
  "phoneNumber": "+905551234567"
}
```

**Validation Rules:**
- `fullName`: Required, max 150 characters
- `email`: Required, valid email format
- `phoneNumber`: Required, 7-15 digits with optional +, spaces, dashes

**Response:** `201 Created` | `400 Bad Request`

### DELETE /api/customers/{id}
Delete a customer and all associated subscriptions/payments.

**Response:** `204 No Content` | `404 Not Found`

---

## Subscriptions

### GET /api/subscriptions
List all active subscriptions.

**Response:** `200 OK`
```json
[
  {
    "id": "a9f36fa2-7aa7-4ba1-9431-82b1adba7768",
    "customerId": "7aa2bf38-8d7d-49f3-ba88-3c61da077041",
    "customerName": "Ahmet Yılmaz",
    "type": "Internet",
    "serviceProvider": "Türk Telekom",
    "subscriberNumber": "1234567890",
    "status": "Active",
    "paymentDayOfMonth": 15,
    "createdAt": "2026-05-11T16:12:00.275Z"
  }
]
```

### GET /api/subscriptions/{id}
Get a subscription by ID.

### GET /api/subscriptions/customer/{customerId}
Get all subscriptions for a customer.

### GET /api/subscriptions/customer/{customerId}/unpaid
Get subscriptions that haven't been paid this month.

### POST /api/subscriptions
Create a new subscription.

**Request Body:**
```json
{
  "customerId": "7aa2bf38-8d7d-49f3-ba88-3c61da077041",
  "type": 3,
  "serviceProvider": "Türk Telekom",
  "subscriberNumber": "1234567890",
  "paymentDayOfMonth": 15
}
```

**Subscription Types (enum values):**
| Value | Name |
|-------|------|
| 1 | Electricity |
| 2 | Water |
| 3 | Internet |
| 4 | GSM |
| 5 | NaturalGas |
| 6 | Insurance |
| 99 | Other |

**Validation Rules:**
- `customerId`: Required, must exist
- `type`: Required, valid enum value
- `serviceProvider`: Required, max 200 characters
- `subscriberNumber`: Required, max 50 characters
- `paymentDayOfMonth`: Required, between 1 and 28

**Response:** `201 Created` | `400 Bad Request` | `404 Not Found`

### PUT /api/subscriptions/{id}
Update a subscription (partial update supported).

**Request Body:**
```json
{
  "serviceProvider": "Vodafone",
  "status": 2
}
```

**Response:** `200 OK` | `400 Bad Request` | `404 Not Found`

### DELETE /api/subscriptions/{id}
Delete a subscription and all its payments.

**Response:** `204 No Content` | `404 Not Found`

### GET /api/subscriptions/{id}/debt
Query debt information from the third-party mock service.

**Response:** `200 OK`
```json
{
  "subscriptionId": "a9f36fa2-7aa7-4ba1-9431-82b1adba7768",
  "serviceProvider": "Türk Telekom",
  "subscriberNumber": "1234567890",
  "debtAmount": 213.49,
  "dueDate": "2026-05-15T00:00:00Z",
  "period": "2026-05",
  "hasDebt": true
}
```

---

## Payments

### POST /api/payments
Process a payment for a subscription.

**Request Body:**
```json
{
  "subscriptionId": "a9f36fa2-7aa7-4ba1-9431-82b1adba7768",
  "amount": 189.50,
  "period": "2026-05"
}
```

**Validation Rules:**
- `subscriptionId`: Required, must exist and be active
- `amount`: Required, must be > 0
- `period`: Required, format "YYYY-MM"

**Business Rules:**
- Cannot pay for an inactive subscription
- Cannot pay twice for the same period (if previous payment was successful)
- Payment is processed via mock payment gateway (90% success rate)

**Response:** `201 Created` | `400 Bad Request` | `404 Not Found`
```json
{
  "id": "eb8ed755-62c7-40fc-abaf-02fd3aa39eb8",
  "subscriptionId": "a9f36fa2-7aa7-4ba1-9431-82b1adba7768",
  "serviceProvider": "Türk Telekom",
  "subscriberNumber": "1234567890",
  "amount": 189.50,
  "paymentDate": "2026-05-11T16:12:09.866Z",
  "period": "2026-05",
  "status": "Successful",
  "transactionReference": "TXN-20260511161209-43487"
}
```

### GET /api/payments/{id}
Get a payment by ID.

### GET /api/payments/subscription/{subscriptionId}
Get payment history for a subscription.

### GET /api/payments/customer/{customerId}
Get all payments for a customer.

---

## Reminders

### GET /api/reminders
Get pending payment reminders for subscriptions where payment is due soon.

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| customerId | GUID | null | Filter by customer (optional) |
| daysAhead | int | 3 | Days ahead to check for upcoming payments |

**Response:** `200 OK`
```json
{
  "count": 1,
  "checkedAt": "2026-05-11T16:12:09.913Z",
  "reminders": [
    {
      "subscriptionId": "a9f36fa2-7aa7-4ba1-9431-82b1adba7768",
      "customerId": "7aa2bf38-8d7d-49f3-ba88-3c61da077041",
      "customerName": "Ahmet Yılmaz",
      "customerEmail": "ahmet@example.com",
      "customerPhone": "+905551234567",
      "subscriptionType": "Internet",
      "serviceProvider": "Türk Telekom",
      "subscriberNumber": "1234567890",
      "paymentDayOfMonth": 15,
      "period": "2026-05",
      "message": "📅 Payment due in 4 day(s) for Türk Telekom."
    }
  ]
}
```

**Reminder Logic:**
- Only checks **active** subscriptions
- Only generates reminders if payment day is within `daysAhead` window
- **Does NOT** generate reminder if a successful payment exists for the current period
- Includes overdue payment alerts

### POST /api/reminders/notify
Send mock Email & SMS notifications for all pending reminders of a customer.

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| customerId | GUID | required | Customer to send notifications for |
| daysAhead | int | 7 | Days ahead to check for upcoming payments |

**Response:** `200 OK`
```json
{
  "notificationsSent": 2,
  "results": [
    {
      "subscriptionId": "a9f36fa2-7aa7-4ba1-9431-82b1adba7768",
      "serviceProvider": "Türk Telekom",
      "email": {
        "success": true,
        "channel": "Email",
        "recipient": "ahmet@example.com"
      },
      "sms": {
        "success": true,
        "channel": "SMS",
        "recipient": "+905551234567"
      }
    }
  ]
}
```

---

## Utility Endpoints (Testing/Demo)

### GET /api/time
Get the current virtual backend date/time (for time warp testing).

### POST /api/time/forward
Advance virtual time by 1 day.

### POST /api/time/backward
Rewind virtual time by 1 day.

### POST /api/time/reset
Reset to real time.

### POST /api/seed
Seed a new customer with randomized subscriptions and payment history.

### DELETE /api/seed
Clear all data from the database.

