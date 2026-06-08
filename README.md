# Budget Planner - ระบบวางแผนงบประมาณ

ระบบ Web Application สำหรับการวางแผนงบประมาณและ Action Plan แทนการใช้ Excel Form

## Tech Stack
- **Frontend**: Vue 3 + TypeScript + Vite + Pinia + AG Grid + Tailwind CSS
- **Backend**: .NET 8 Web API
- **Database**: SQL Server 2022
- **Infrastructure**: Docker Compose

## โครงสร้างโปรเจค

```
budget-planner/
├── docker-compose.yml
├── frontend/          # Vue 3 SPA
└── backend/           # .NET 8 API
    └── src/
        ├── BudgetPlanner.Api/           # Controllers, Middleware
        ├── BudgetPlanner.Application/   # Services, DTOs, Interfaces
        ├── BudgetPlanner.Domain/        # Entities, Enums
        └── BudgetPlanner.Infrastructure/ # EF Core, DbContext
```

## Quick Start (Docker)

```bash
# 1. Copy env file
cp .env.example .env

# 2. Start all services
docker-compose up -d

# 3. Access
# Frontend: http://localhost:3000
# API:      http://localhost:5000
# Swagger:  http://localhost:5000/swagger
```

## Default Credentials
- **Email**: `admin@company.com`
- **Password**: `Admin@1234`

## Features

### Budget Planning
- Spreadsheet-like grid interface (AG Grid)
- Monthly/Quarterly periods
- Department-level permissions (users edit only their dept)
- Auto-save with debouncing
- Lock template to prevent edits
- Export to Excel (.xlsx)
- Category hierarchy (parent/child rows, subtotals)

### Action Plan
- Inline editable table
- Status tracking (Not Started / In Progress / Completed / On Hold / Cancelled)
- Budget allocated vs spent tracking
- Timeline (start/end dates)
- Department-scoped editing

### Admin Panel
- User management (create, edit, deactivate)
- Role assignment (Admin / Dept Manager / User)
- Department permission matrix (None / Read-only / Edit per department)
- Department management

### Security
- JWT authentication (15-min access token)
- httpOnly cookie refresh token (7-day)
- Server-side permission checks on every write
- Template lock mechanism

## Development Setup

### Backend
```bash
cd backend
dotnet restore
dotnet run --project src/BudgetPlanner.Api
```

### Frontend
```bash
cd frontend
npm install
npm run dev
```

## Database Schema

### Key Tables
- `Departments` - แผนก
- `Users` - ผู้ใช้งาน (Role: Admin/DeptManager/User)
- `UserDepartmentPermissions` - สิทธิ์การเข้าถึงรายแผนก
- `BudgetTemplates` - Template งบประมาณรายปี
- `BudgetCategories` - หมวดหมู่งบประมาณ (รองรับ hierarchy)
- `BudgetCells` - ค่างบประมาณรายเซลล์ (sparse storage)
- `ActionPlans` - Action Plan
- `ActionPlanItems` - รายการโครงการ

## Seed Data

ระบบมีข้อมูลเริ่มต้น:
- 5 แผนก: บริหาร, การเงิน, HR, IT, การตลาด
- 1 Admin user: admin@company.com / Admin@1234

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/auth/login` | เข้าสู่ระบบ |
| POST | `/api/auth/refresh` | รีเฟรช Token |
| GET | `/api/budget/templates` | รายการ Template |
| GET | `/api/budget/templates/{id}/data` | ข้อมูล Grid |
| PUT | `/api/budget/templates/{id}/cells` | บันทึกเซลล์ |
| GET | `/api/action-plans` | รายการ Action Plan |
| GET | `/api/users` | รายการผู้ใช้ (Admin) |
| PUT | `/api/users/{id}/permissions` | ตั้งค่าสิทธิ์ |
