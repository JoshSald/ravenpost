# RavenPost API
A Game-of-Thrones themed Raven Postal Service built with ASP.NET Core Minimal APIs and EF Core.

Ravens carry messages. The crown tracks the coin.

This project demonstrates:
- Minimal APIs
- Entity Framework Core (SQLite)
- Migrations & Seeding
- DTOs & Validation
- Reporting & Aggregations
- OpenAPI + Scalar UI

It started as a restaurant ordering system and evolved into a medieval postal logistics service.

---

## Features

### Supplies (Inventory)
- CRUD endpoints
- Search & filtering
- Categories & pricing

### Dispatches (Orders)
- Create dispatch with multiple supplies
- Server-side total calculation
- Validation (no empty or invalid quantities)
- Summary + detailed views

### Reports
- Daily summary
- Total revenue
- Top 3 most used supplies
- Supports `?date=YYYY-MM-DD` or `?date=today`

### Infrastructure
- SQLite database
- EF Core migrations
- Seed data on startup
- Scalar API UI
- Conventional commits
- Clean `.gitignore`

---

## Tech Stack

- .NET 10 Minimal APIs
- Entity Framework Core
- SQLite
- Scalar (OpenAPI UI)

---

## 🛠 Setup

### 1. Clone
```bash
git clone https://github.com/JoshSald/ravenpost.git
cd ravenpost/RavenPost.Api