# Archi Subscription & Automatic Payment Reminder Application

A full-stack subscription management and automatic payment reminder banking application with three client platforms. Built with **.NET 10** (Clean Architecture) on the backend, **React 19** (Vite + Tailwind CSS) on the web frontend, and **Kotlin** (Jetpack Compose + Material 3) on Android mobile.

## 🏗️ Architecture

```
┌─────────────────────────────┐   ┌─────────────────────────────┐
│   Web (React 19 + Vite)     │   │  Mobile (Kotlin + Compose)  │
│   Tailwind CSS, Redux       │   │  Retrofit, MVVM, Material 3 │
└──────────────┬──────────────┘   └──────────────┬──────────────┘
               │          REST API (JSON)         │
               └──────────────┬───────────────────┘
                              │
               ┌──────────────▼──────────────────┐
               │      API Layer (.NET 10)         │
               │  Controllers, Middleware, DI     │
               ├─────────────────────────────────┤
               │    Infrastructure Layer          │
               │  EF Core, Repositories,          │
               │  Services, Mock External APIs    │
               ├─────────────────────────────────┤
               │       Core Layer                 │
               │  Entities, DTOs, Interfaces,     │
               │  Validators, Enums               │
               └──────────────┬──────────────────┘
                              │
               ┌──────────────▼──────────────────┐
               │    Microsoft SQL Server 2022     │
               └─────────────────────────────────┘
```

## 🚀 Quick Start

### Prerequisites

- .NET 10 SDK
- Node.js 18+ & npm
- Microsoft SQL Server (2019+ recommended)
- Android Studio (for mobile app — Arctic Fox+ with Kotlin 2.0)

### Run with Docker (Recommended)

```bash
docker-compose up --build
```

This starts **SQL Server**, the **API**, and the **Web Frontend (nginx)**:
- Web Frontend: `http://localhost:3000`
- API: `http://localhost:8080`
- Swagger UI: `http://localhost:8080` (root URL)

> **Note:** The Android mobile app is not containerized — it runs on a physical device or emulator and connects to the Docker API.

### Run Backend Locally

1. Ensure SQL Server is running and update the connection string in `backend/src/ArchiSubscription.API/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost,1433;Database=ArchiSubscriptionDb;User Id=sa;Password=YourStrong!Password123;TrustServerCertificate=True;MultipleActiveResultSets=true"
   }
   ```
2. Run the API:
   ```bash
   cd backend/src/ArchiSubscription.API
   dotnet run
   ```

The API will start on `http://localhost:5000` (check console output).  
Swagger UI is available at the root URL: `http://localhost:5000/`

### Run Frontend (Web)

```bash
cd frontend
npm install
npm run dev
```

Frontend runs on `http://localhost:3000` and proxies API calls to the backend.

### Run Mobile (Android)

1. Open the `mobile/` directory in **Android Studio**
2. Ensure the backend is running (Docker or local — the emulator uses `10.0.2.2:8080`)
3. Build & run on an Android emulator (API 26+) or physical device
4. For a physical device on the same network, update the base URL in `mobile/app/build.gradle.kts`:
   ```kotlin
   buildConfigField("String", "API_BASE_URL", "\"http://YOUR_PC_IP:8080/api\"")
   ```

> The mobile app uses `http://10.0.2.2:8080/api` by default, which maps to the host machine's `localhost` inside the Android emulator.

## 📊 Data Model

### Entities

| Entity | Description |
|--------|-------------|
| **Customer** | Bank customer with name, email, phone |
| **Subscription** | Recurring payment definition (electricity, water, internet, etc.) |
| **Payment** | Individual payment record for a subscription in a specific billing period |

### Relationships

- One Customer → Many Subscriptions
- One Subscription → Many Payments

See [docs/er-diagram.md](docs/er-diagram.md) for the full ER diagram.

## 🔌 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/customers` | List all customers |
| `GET` | `/api/customers/{id}` | Get customer by ID |
| `POST` | `/api/customers` | Create a new customer |
| `DELETE` | `/api/customers/{id}` | Delete a customer |
| `GET` | `/api/subscriptions` | List all active subscriptions |
| `GET` | `/api/subscriptions/{id}` | Get subscription by ID |
| `GET` | `/api/subscriptions/customer/{customerId}` | Get subscriptions by customer |
| `GET` | `/api/subscriptions/customer/{customerId}/unpaid` | Get unpaid subscriptions this month |
| `POST` | `/api/subscriptions` | Create a new subscription |
| `PUT` | `/api/subscriptions/{id}` | Update a subscription |
| `DELETE` | `/api/subscriptions/{id}` | Delete a subscription |
| `GET` | `/api/subscriptions/{id}/debt` | Query debt from third-party service |
| `POST` | `/api/payments` | Process a payment |
| `GET` | `/api/payments/{id}` | Get payment by ID |
| `GET` | `/api/payments/subscription/{subscriptionId}` | Payment history by subscription |
| `GET` | `/api/payments/customer/{customerId}` | Payment history by customer |
| `GET` | `/api/reminders` | Get pending payment reminders |
| `POST` | `/api/reminders/notify` | Send mock Email & SMS notifications |
| `GET` | `/api/time` | Get current virtual time |
| `POST` | `/api/time/forward` | Advance virtual time +1 day |
| `POST` | `/api/time/backward` | Rewind virtual time -1 day |
| `POST` | `/api/time/reset` | Reset to real time |
| `POST` | `/api/seed` | Seed demo data (new customer + subscriptions + payments) |
| `DELETE` | `/api/seed` | Clear all data |

See [docs/api-endpoints.md](docs/api-endpoints.md) for detailed request/response examples.

## 🖥️ Web Frontend Features

The React frontend provides a complete UI for all application features:

### Home Page (Customer Management)
- **Create Customer** — Form with full name, email, phone number validation
- **Seed Test Data** — One-click seeding of realistic demo data (customers, subscriptions, payments)
- **Delete Customer** — Delete with confirmation dialog (cascades to subscriptions & payments)
- **Login as Customer** — Select any customer to view their dashboard

### Dashboard (Per-Customer View)
- **📊 Summary Banner** — Active subscription count, pending reminders, total payments at a glance
- **🔔 Reminders Tab** — Shows upcoming payment reminders with due dates and overdue alerts
- **📋 Subscriptions Tab**
  - View all subscriptions (grouped by Active/Passive)
  - **Add Subscription** — Create new subscription with type, provider, subscriber number, payment day
  - **Edit Subscription** — Update any field including status (Active/Passive)
  - **Delete Subscription** — Remove with confirmation dialog
  - **Check Debt** — Query mock third-party debt inquiry service
  - **Pay Now** — Opens payment modal with debt amount pre-filled
- **💰 Payment History Tab** — Full payment record table with status, reference numbers, amounts

### Payment Flow (Exactly as specified)
```
Select Subscription → Check Debt (3rd party) → View Amount → Pay Now → Confirm → Result
```

## 📱 Mobile App Features (Android / Kotlin)

The Kotlin Android app mirrors the web frontend with a native experience:

- **Home Screen** — Customer list, seed test data, create customer dialog
- **Dashboard Screen** — 3 tabs (Reminders, Subscriptions, Payment History)
- **Payment Flow** — Same debt inquiry → confirm → process flow as web
- **Subscription CRUD** — Create, edit, delete subscriptions via dialogs
- **Time Warp Bar** — Persistent on all screens, same forward/backward/reset controls
- **MVVM Architecture** — ViewModels with Kotlin coroutines, Retrofit for networking
- **Material 3 Design** — Color theme matching the web frontend

## 🔧 Technology Stack

| Component | Technology |
|-----------|-----------|
| Backend | C# / .NET 10 |
| Frontend (Web) | React 19 + Vite 6 + Tailwind CSS 4 |
| Frontend (Mobile) | Kotlin + Jetpack Compose + Material 3 |
| Web State Management | Redux Toolkit |
| Web Routing | React Router v7 |
| Mobile Navigation | Navigation Compose |
| Mobile Networking | Retrofit 2 + OkHttp |
| Database | Microsoft SQL Server 2022 |
| ORM | Entity Framework Core 10 |
| Validation | FluentValidation |
| API Documentation | Swagger / OpenAPI (Swashbuckle) |
| Architecture | Clean Architecture (3-layer backend), MVVM (mobile) |
| Containerization | Docker & Docker Compose (backend + web) |

## 📁 Project Structure

```
├── docker-compose.yml                       # Multi-service orchestration
├── backend/                                 # Backend (.NET)
│   ├── Dockerfile                           # API container build
│   └── src/
│       ├── ArchiSubscription.API/           # Web API layer
│       │   ├── Controllers/                 # API endpoints
│       │   │   ├── CustomersController.cs   # Customer CRUD (Create/Read/Delete)
│       │   │   ├── SubscriptionsController.cs # Subscription CRUD + Debt Inquiry
│       │   │   ├── PaymentsController.cs    # Payment processing + history
│       │   │   ├── RemindersController.cs   # Payment reminder checks + notifications
│       │   │   ├── SeedController.cs        # Demo data seeding
│       │   │   └── TimeController.cs        # Time warp for testing
│       │   ├── Middleware/                  # Global exception handling
│       │   └── Program.cs                  # DI & app configuration
│       ├── ArchiSubscription.Core/         # Domain layer (no dependencies)
│       │   ├── Entities/                   # Customer, Subscription, Payment
│       │   ├── Enums/                      # SubscriptionType, Status, PaymentStatus
│       │   ├── DTOs/                       # Request/Response data transfer objects
│       │   ├── Interfaces/                 # Repository & service contracts
│       │   ├── Validators/                 # FluentValidation rules
│       │   └── Exceptions/                 # NotFoundException, BadRequestException
│       └── ArchiSubscription.Infrastructure/ # Implementation layer
│           ├── Data/                       # EF Core DbContext & configurations
│           ├── Repositories/               # Data access (Generic + specialized)
│           ├── Services/                   # Business logic implementations
│           └── ExternalServices/           # Mock 3rd-party integrations
│
├── frontend/                               # Frontend (React)
│   ├── Dockerfile                          # Multi-stage build (Node → nginx)
│   ├── nginx.conf                          # SPA routing + API reverse proxy
│   └── src/
│       ├── api/client.js                   # API client (fetch wrapper)
│       ├── store/                          # Redux Toolkit store & slices
│       ├── pages/                          # SeedPage (home), DashboardPage
│       └── components/                     # Reusable UI components
│           ├── CustomerCard.jsx            # Customer card with login/delete
│           ├── CreateCustomerModal.jsx     # Customer creation form
│           ├── SubscriptionList.jsx        # Subscription list with debt/pay/edit/delete
│           ├── CreateSubscriptionModal.jsx # Subscription creation form
│           ├── EditSubscriptionModal.jsx   # Subscription edit form
│           ├── PaymentModal.jsx            # Payment confirmation dialog
│           ├── PaymentHistoryTable.jsx     # Payment history table
│           ├── ReminderList.jsx            # Reminder cards with pay buttons
│           ├── TopBar.jsx                  # Time warp controls
│           └── ConfirmDialog.jsx           # Generic confirmation dialog
│
├── mobile/                                 # Mobile App (Kotlin / Android)
│   ├── app/
│   │   └── src/main/java/com/archi/subscription/
│   │       ├── MainActivity.kt             # Single-activity Compose host
│   │       ├── data/model/                 # API data classes
│   │       ├── data/remote/                # Retrofit API service
│   │       ├── data/repository/            # Repository wrapping API calls
│   │       ├── ui/home/                    # Home screen (customer list + seed)
│   │       ├── ui/dashboard/               # Dashboard (3 tabs + payment flow)
│   │       ├── ui/components/              # Shared composables (TimeWarpBar)
│   │       ├── ui/navigation/              # NavGraph (Home → Dashboard)
│   │       └── ui/theme/                   # Material 3 theme colors
│   └── build.gradle.kts
│
└── docs/                                   # Documentation
    ├── er-diagram.md                       # Entity-Relationship diagram
    ├── api-endpoints.md                    # Detailed API documentation
    └── flow-diagrams.md                    # Sequence & flow diagrams
```

## 🧪 Third-Party Service Integrations (Mock)

### 1. Debt Inquiry Service (Borç Sorgulama)
- Simulates querying utility companies for outstanding debt
- Returns realistic amounts based on subscription type (Electricity: 80-350₺, Water: 30-150₺, etc.)
- Checks if already paid for current period — returns 0 debt if paid
- Includes network delay simulation (50-200ms)
- Returns: debt amount, due date, billing period, hasDebt flag

### 2. Payment Processing Service (Ödeme İşleme)
- Simulates a payment gateway
- **90% success rate** (realistic failure scenarios)
- Returns transaction references (e.g., `TXN-20260511161209-43487`)
- Includes network delay simulation (100-500ms)
- Failed payments are recorded with "Failed" status for audit trail

### 3. Notification Service (Bildirim — Email & SMS)
- Simulates sending Email and SMS notifications for pending payment reminders
- Triggered via `POST /api/reminders/notify?customerId=xxx`
- Sends both Email and SMS per reminder (dual-channel)
- Includes network delay simulation (50-300ms)
- Returns per-reminder success/failure results with channel and recipient details

## 💡 Key Design Decisions

1. **Microsoft SQL Server** — Production-grade RDBMS with filtered unique indexes and retry-on-failure for resilience.
2. **Endpoint-based Reminders** — No background job complexity; query reminders on demand via `GET /api/reminders`. Reminders are generated only when payment is due within the configured window AND no successful payment exists for the current period.
3. **Period-based Payment Tracking** — Format `YYYY-MM` ensures one successful payment per subscription per month. Duplicate payment prevention is enforced at the service layer.
4. **Generic Repository Pattern** — Reduces boilerplate while allowing specialized queries per entity.
5. **Global Exception Middleware** — Maps `NotFoundException` → 404, `BadRequestException` → 400 for consistent error responses.
6. **Redux Toolkit State Management** — Centralized state with async thunks for all API operations, enabling optimistic UI and consistent loading/error states.

---

## 🤖 AI Usage Documentation - Notlarım

> **Projede:** 
> - Bilindik mimarilerin taslağını oluşturmak için.
> - Çizmiş olduğumuz flow'un uç noktalarını geliştirmek için.
> - Planlama konusunda eleştiri almak için.
> - Konteynerizasyon yapmak için (Docker).
> - Dokümantasyon oluşturmak için.
> - Frontend'de basit ancak zaman alan kısımları yazmak için (Basic Layout - CSS)
> - Mobilde çok tecrübem olmadığı için Frontenddeki tasarımı yansıtmak için
>
> *Yapay Zeka Kullandım.*
> 
> - Flowun dışına çıkılmaması için yaptığım müdahalelerde.
> - Giderek karmaşıklaşan kodu dizginlemekte.
> - Sürekli hata veren ve AI'ı zorlayan kısımları geliştirmekte (Time warping)
> - Genel programlama hatalarını tespit etmekte (4 adet N+1 problemi, birkaç mükerrer DB Hit)
> - Frontend'de Flow'u, Komponent ağacının temizliği yakalamakta
> - Kodu okunaklı kılmak için yaptığım müdahalelerde
> - Kullanılacak teknolojileri belirlemekte.
> - Ve Dokümantasyonların hatasız yazıldığından emin olmakta
> 
> *Yapay Zeka Kullanmadım*
>

> **Aşağıda AI'ın tüm Github Copilot sohbetimize dayanarak nasıl birlikte çalıştığımızı dürüstçe anlatmasını istedim.** 

> **Transparency Note:** This project was developed with significant AI assistance via **GitHub Copilot**. Below is a detailed, honest account of how AI was used, what was accepted, what was modified, and what was rejected.

### Philosophy: AI as a Pair Programmer, Not a Replacement

My approach to AI throughout this project was to treat it as an accelerator for well-understood patterns rather than a decision-maker for architecture. Every suggestion was reviewed through the lens of: *"Does this match the requirements? Is this what I would write?"*

### Detailed AI Usage Breakdown

#### 1. 🏛️ Architecture & Project Scaffolding
**What AI helped with:**
- Suggested the 3-layer Clean Architecture structure (API → Infrastructure → Core)
- Generated the initial `DependencyInjection.cs` service registration pattern
- Proposed the generic repository pattern with specialized repository interfaces

**What I modified:**
- Adjusted the dependency flow to ensure Core has zero external dependencies
- Renamed interfaces and reorganized DTOs into domain-specific subfolders
- Added cancellation token support throughout the entire stack (AI initially omitted this)

#### 2. 📝 Entity & DTO Design
**What AI helped with:**
- Generated initial entity classes (Customer, Subscription, Payment) based on requirements
- Suggested record types for DTOs (immutable by design)
- Created enum definitions for SubscriptionType and PaymentStatus

**What I reviewed and adjusted:**
- Ensured `PaymentDayOfMonth` was constrained to 1–28 (not 1–31) to avoid month-boundary issues
- Added `BaseEntity` with common fields (Id, CreatedAt, UpdatedAt) — AI initially duplicated these
- Verified navigation property configurations match the required cascade delete behavior

#### 3. ✅ Validation Rules (FluentValidation)
**What AI generated:**
- Validation rules for CreateCustomerDto, CreateSubscriptionDto, CreatePaymentDto
- Regex patterns for phone number and email validation
- Period format validation (`YYYY-MM` pattern)

**What I verified:**
- Tested edge cases: empty strings, boundary values (paymentDayOfMonth = 0, 29)
- Ensured error messages are user-friendly and descriptive
- Confirmed the period regex correctly rejects malformed inputs like "2026-13" or "26-05"

#### 4. 🔌 Mock External Services
**What AI proposed:**
- The debt inquiry service structure with random amount generation per subscription type
- Payment processing service with 90% success rate simulation
- Network delay simulation using `Task.Delay`

**What I designed and tuned:**
- The debt amount ranges to be realistic for Turkish utility bills
- The logic for checking existing payments before returning debt (critical business rule)
- Transaction reference format and logging patterns for debugging

#### 5. 🔔 Reminder Logic
**What I architected:**
- The reminder algorithm: check each active subscription → calculate days until payment → verify no existing payment for current period → generate contextual message
- Overdue detection logic (negative days until payment)
- The decision to use endpoint-based reminders instead of background jobs (simpler, sufficient for requirements)

**What AI helped with:**
- Generating the `GetPaymentDateForMonth` helper method
- Structuring the response DTO with all required fields

#### 6. ⚛️ Frontend (React)
**What AI accelerated:**
- Tailwind CSS class compositions for UI components
- Redux Toolkit slice boilerplate (createAsyncThunk patterns)
- Vite configuration with API proxy setup

**What I designed and directed:**
- The complete user flow: Home → Select Customer → Dashboard → Tabs (Reminders/Subscriptions/Payments)
- Component hierarchy and prop passing strategy
- State management architecture (separate slices for auth, customers, subscriptions, payments, reminders)
- The debt inquiry → payment modal flow (check debt → pre-fill amount → confirm payment)
- All CRUD modals (create customer, create/edit/delete subscription)
- Fixed a UTC timezone bug where `toISOString()` shifted billing periods for UTC+ timezones

#### 7. 📱 Mobile App (Kotlin / Android)
**What AI generated:**
- The full Kotlin codebase was largely AI-generated since I have limited Kotlin/Compose experience
- Retrofit API interface, data model classes, repository pattern
- Compose UI screens, dialogs, and navigation graph
- Material 3 theming to match the web color scheme

**What I verified and fixed:**
- Reviewed the generated code structure (MVVM, data/ui separation) to ensure it follows Android conventions
- Tested the full flow on Android emulator: seed → view reminders → check debt → pay → verify payment history
- Confirmed API connectivity via `10.0.2.2` emulator-to-host mapping

> **Honesty note:** The mobile app had the highest AI contribution ratio compared to backend and frontend. I directed the requirements and screen flow, but the Kotlin/Compose implementation was primarily AI-generated with manual testing and build fixes on my end.

#### 8. 📄 Documentation
**What AI helped draft:**
- Mermaid diagrams for ER diagram and flow diagrams
- API endpoint documentation tables
- README structure and formatting

**What I authored:**
- All architectural decisions and their justifications
- Verification that all diagrams match the actual implementation

### AI Usage Summary Table

| Area | AI Contribution | Human Verification |
|------|----------------|-------------------|
| Architecture | Suggested patterns | Validated dependency flow, adjusted structure |
| Entity Design | Initial generation | Added constraints, fixed navigation properties |
| Validation | Rule generation | Tested edge cases, verified regex patterns |
| Mock Services | Boilerplate | Tuned business logic, amounts, success rates |
| Reminder Logic | Helper methods | Designed algorithm, overdue detection |
| Frontend UI | CSS classes, boilerplate | Designed UX flow, component hierarchy, CRUD |
| Mobile App | Full Kotlin/Compose generation | Directed requirements, fixed build errors, tested flow |
| Documentation | Formatting, diagrams | Verified accuracy, wrote design decisions |

### Key Takeaway

AI was most valuable for **eliminating boilerplate** and **accelerating pattern implementation** where the patterns were already well-understood. The critical business logic — period-based payment tracking, reminder window calculation, duplicate payment prevention, debt inquiry integration — was human-directed with AI assisting in syntax and structure. The mobile app was an exception where AI handled most of the Kotlin/Compose implementation, with human oversight on build issues and functional testing.

Every AI-generated output was reviewed line-by-line before integration. Several suggestions were rejected outright:
- ❌ AI suggested using `DateTime.Now` instead of `DateTime.UtcNow` (timezone issues)
- ❌ AI proposed a `BackgroundService` for reminders (over-engineering for the scope)
- ❌ AI generated authentication/JWT boilerplate (not in requirements)
- ❌ AI suggested `int` primary keys (GUIDs are better for distributed systems)

---

## 📑 System Design Documents

- **[ER Diagram](docs/er-diagram.md)** — Customer – Subscription – Payment relationships with full column details
- **[API Endpoints](docs/api-endpoints.md)** — Detailed request/response examples for every endpoint
- **[Flow Diagrams](docs/flow-diagrams.md)** — Debt inquiry → payment flow, reminder check flow, subscription lifecycle, payment decision tree, system overview, notification flow, client user flow

## 📄 License

This project was created as a case study demonstration.

