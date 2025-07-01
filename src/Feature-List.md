# InvoiceGen – Feature List (Modular Monolith | ASP.NET Core + Razor Pages)

---

## 🔐 Authentication & Security

- Email & Password registration and login (custom logic)
- JWT-based access tokens for authentication
- Refresh token system (optional, recommended)
- Google and Facebook login (OAuth 2.0)
- Email confirmation with SendGrid
- Two-Factor Authentication (via email or authenticator app)
- Secure password hashing with BCrypt.Net
- Role-based authorization (Admin, User, etc.)
- Rate-limited login attempts (brute-force protection)

---

## 🧾 Invoice & Client Management

- Create, edit, and delete invoices
- Add clients and link invoices to clients
- Multi-currency support (USD, EUR, NGN, etc.)
- Invoice notes, terms, and custom footer
- Unique invoice number generation
- Filter invoices by client, date, or payment status

---

## 💸 Payments & Delivery

- Stripe integration for online invoice payments
- Public invoice view (via secure link)
- Download invoice as PDF (e.g., using QuestPDF)
- Share invoice via email
- Invoice status: draft, sent, paid, overdue

---

## 🧑‍💼 User & Team Features

- User profile with avatar, contact info, and settings
- Invite team members (optional roles: Admin, Editor, Viewer)
- Organization-wide branding (logo, theme color, address)
- Role management system via custom EF tables

---

## 📊 Analytics & Reporting

- Income vs expenses dashboard
- Paid vs unpaid invoice breakdown
- Monthly/Yearly revenue tracking
- Export reports to CSV or PDF
- Top clients and best-performing months

---

## 🎨 UI & UX (Razor Pages)

- Responsive UI with Bootstrap or TailwindCSS
- Razor Pages-based layout with clean, modular structure
- Animated transitions using @section Scripts + JS (or Alpine.js)
- Dark mode (optional with Tailwind + Razor toggle)
- Custom error pages (404, 401, etc.)

---

## 📂 Storage & File Management

- Upload business logo, company assets (stored locally or in blob storage)
- Razor Page to manage uploaded files
- File type/size validation

---

## 🌐 Global & Local Features

- Multi-currency formatting (user-selected locale)

- Localization-ready Razor pages

- Dynamic date/time formatting

---

## 🛠️ System Infrastructure

- ASP.NET Core middleware for auth, logging, CORS
- EF Core migrations and seeding (default admin user)
- Custom User and ExternalLogin tables
- Service layer (Application + Infrastructure separation)
- Dependency injection for clean architecture
