# Flow Diagrams

## 1. Debt Inquiry → Payment Flow

```mermaid
sequenceDiagram
    participant User
    participant API
    participant SubscriptionService
    participant DebtInquiryService as Mock Debt Service
    participant PaymentService
    participant PaymentGateway as Mock Payment Gateway
    participant Database

    User->>API: GET /api/subscriptions/{id}/debt
    API->>SubscriptionService: GetByIdAsync(id)
    SubscriptionService->>Database: Find subscription
    Database-->>SubscriptionService: Subscription data
    API->>DebtInquiryService: InquireDebtAsync(subscriptionId)
    DebtInquiryService->>DebtInquiryService: Simulate delay (50-200ms)
    DebtInquiryService->>Database: Check existing payments
    DebtInquiryService-->>API: DebtInquiryResponse (amount, dueDate, period)
    API-->>User: 200 OK (debt info)

    Note over User: User decides to pay

    User->>API: POST /api/payments
    API->>PaymentService: ProcessPaymentAsync(dto)
    PaymentService->>Database: Check subscription exists & is active
    PaymentService->>Database: Check no existing payment for period
    PaymentService->>PaymentGateway: ProcessAsync(amount, subscriberNumber)
    PaymentGateway->>PaymentGateway: Simulate delay (100-500ms)
    PaymentGateway-->>PaymentService: Result (success/fail + txn ref)
    PaymentService->>Database: Save payment record
    PaymentService-->>API: PaymentResponse
    API-->>User: 201 Created (payment details)
```

## 2. Reminder Check Flow

```mermaid
sequenceDiagram
    participant User
    participant API
    participant ReminderService
    participant Database

    User->>API: GET /api/reminders?customerId=xxx&daysAhead=3
    API->>ReminderService: GetPendingRemindersAsync(customerId, daysAhead)
    ReminderService->>Database: Get all active subscriptions (with payments eager-loaded)
    
    loop For each active subscription
        loop For each billing period (month-3 to month+1)
            ReminderService->>ReminderService: Calculate days until payment date
            alt Subscription didn't exist yet
                ReminderService->>ReminderService: Skip period
            else Payment day too far in future (> daysAhead)
                ReminderService->>ReminderService: Skip period
            else Within reminder window
                ReminderService->>ReminderService: Check if paid for this period (in-memory)
                alt Not paid
                    ReminderService->>ReminderService: Generate reminder message (overdue/due/upcoming)
                else Already paid
                    ReminderService->>ReminderService: Skip (no reminder needed)
                end
            end
        end
    end
    
    ReminderService-->>API: List of reminders (sorted by period, then payment day)
    API-->>User: 200 OK (reminders with count)
```

## 3. Subscription Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Created: POST /api/subscriptions
    Created --> Active: Default status
    Active --> Active: Monthly payments
    Active --> Passive: PUT /api/subscriptions/{id} (status=Passive)
    Passive --> Active: PUT /api/subscriptions/{id} (status=Active)
    Active --> [*]: DELETE /api/subscriptions/{id}
    Passive --> [*]: DELETE /api/subscriptions/{id}
```

## 4. Payment Processing Decision Tree

```mermaid
flowchart TD
    A[User submits payment] --> B{Subscription exists?}
    B -->|No| C[404 Not Found]
    B -->|Yes| D{Subscription active?}
    D -->|No| E[400 Bad Request: Inactive subscription]
    D -->|Yes| F{Already paid this period?}
    F -->|Yes| G[400 Bad Request: Already paid]
    F -->|No| H[Call Payment Gateway]
    H --> I{Gateway response}
    I -->|Success| J[Save payment with 'Successful' status]
    I -->|Failure| K[Save payment with 'Failed' status]
    J --> L[201 Created]
    K --> L
```

## 5. Complete System Overview

```mermaid
flowchart LR
    subgraph Clients
        WEB[Web App<br/>React + Vite + Tailwind]
        MOB[Mobile App<br/>Kotlin + Compose]
    end
    
    subgraph API["ASP.NET Core API (.NET 10)"]
        CC[CustomersController]
        SC[SubscriptionsController]
        PC[PaymentsController]
        RC[RemindersController]
        TC[TimeController]
        SDC[SeedController]
    end
    
    subgraph Services
        CS[CustomerService]
        SS[SubscriptionService]
        PS[PaymentService]
        RS[ReminderService]
    end
    
    subgraph External["Mock External Services"]
        DIS[Debt Inquiry Service]
        PPS[Payment Processing Service]
        NS[Notification Service<br/>Email & SMS]
    end
    
    subgraph Data
        DB[(SQL Server 2022)]
    end
    
    WEB --> CC & SC & PC & RC & TC & SDC
    MOB --> CC & SC & PC & RC & TC & SDC
    CC --> CS
    SC --> SS & DIS
    PC --> PS
    RC --> RS & NS
    CS & SS & PS & RS --> DB
    PS --> PPS
```

## 6. Notification Send Flow

```mermaid
sequenceDiagram
    participant User
    participant API
    participant ReminderService
    participant NotificationService as Mock Notification Service
    participant Database

    User->>API: POST /api/reminders/notify?customerId=xxx
    API->>ReminderService: GetPendingRemindersAsync(customerId, daysAhead)
    ReminderService->>Database: Get active subscriptions with payments
    ReminderService-->>API: List of pending reminders

    loop For each reminder
        API->>NotificationService: SendEmailAsync(email, subject, body)
        NotificationService->>NotificationService: Simulate delay (100-300ms)
        NotificationService-->>API: EmailResult (success, channel, recipient)
        API->>NotificationService: SendSmsAsync(phone, message)
        NotificationService->>NotificationService: Simulate delay (50-150ms)
        NotificationService-->>API: SmsResult (success, channel, recipient)
    end

    API-->>User: 200 OK (notificationsSent, results)
```

## 7. Client User Flow (Web & Mobile)

> Both the React web app and Kotlin Android app follow the same user flow below. The web uses pages/modals, the mobile uses Compose screens/dialogs.

```mermaid
flowchart TD
    A[Home Page] --> B{Has customers?}
    B -->|No| C[Seed Test Data]
    B -->|No| D[Create Customer Manually]
    C --> E[Customer List]
    D --> E
    B -->|Yes| E

    E --> F[Select Customer → Login]
    E --> G[Delete Customer]

    F --> H[Dashboard]

    H --> I[🔔 Reminders Tab]
    H --> J[📋 Subscriptions Tab]
    H --> K[💰 Payment History Tab]

    I --> I1[View pending reminders]

    J --> J1[View Active/Passive subscriptions]
    J --> J2[➕ Add Subscription]
    J --> J3[✏️ Edit Subscription]
    J --> J4[🗑️ Delete Subscription]
    J --> J5[Check Debt - 3rd party]
    J5 --> J6{Has debt?}
    J6 -->|Yes| J7[Pay Now → Payment Modal]
    J6 -->|No| J8[No outstanding debt]
    J7 --> J9[Confirm & Process Payment]
    J9 --> J10{Payment result}
    J10 -->|Success| J11[✅ Payment recorded]
    J10 -->|Failed| J12[❌ Payment failed - retry]

    K --> K1[View all payment history]
```

