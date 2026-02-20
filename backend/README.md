# BidSnap API - OLX-Style Auction Marketplace Backend

A complete ASP.NET Core 8 Web API backend for an OLX-style auction marketplace with membership-based restrictions, JWT authentication, and advanced bidding features.

## ?? Features

### Core Functionality
- ? **User Authentication** - JWT-based login/registration with role-based authorization
- ? **Product Auctions** - Create, update, delete, and manage product listings
- ? **Bidding System** - Place bids, withdraw bids (max 3 before suspension), bid history
- ? **Membership Plans** - Free, Silver, Gold, Platinum with restrictions on ads, bids, and features
- ? **Automatic Auction Closing** - Background service runs every minute to close expired auctions
- ? **Email Notifications** - Outbid alerts, auction won, suspension notices, membership activated
- ? **User Suspension** - Auto-suspend after 3+ bid withdrawals; manual admin suspension with penalty system
- ? **Admin Dashboard** - Statistics, user management, transaction tracking, force close auctions

### Advanced Features
- ?? **Product Filtering & Sorting** - By category, price range, featured status, ending soon
- ?? **Pagination** - Efficient product listing with configurable page size
- ??? **Global Exception Handling** - Centralized error responses
- ?? **CORS Enabled** - Pre-configured for React/Vue/Angular frontends
- ?? **Swagger/OpenAPI** - Interactive API documentation with JWT support
- ??? **SQL Server Integration** - Production-ready database with EF Core migrations
- ?? **Clean Architecture** - Repositories, Services, DTOs, Validators

## ??? Tech Stack

- **Framework:** ASP.NET Core 8
- **Database:** SQL Server with Entity Framework Core 8
- **Authentication:** JWT Bearer Tokens
- **Validation:** FluentValidation
- **Mapping:** AutoMapper
- **Email:** MailKit (SMTP)
- **Hashing:** BCrypt.Net-Next
- **Background Jobs:** HostedService
- **API Documentation:** Swagger/OpenAPI

## ?? Prerequisites

- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- **SQL Server** (LocalDB, Express, or Full)
- **Git** - [Download](https://git-scm.com/)
- **Visual Studio / VS Code** (Optional)

## ?? Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/pspk2004/bnds-backend.git
cd bnds-backend/backend
```

### 2. Configure appsettings.json
Update `appsettings.json` with your configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BidSnapDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "Your-Very-Long-Secret-Key-At-Least-32-Chars",
    "Issuer": "BidSnapAPI",
    "Audience": "BidSnapClient",
    "ExpiryMinutes": "1440"
  },
  "SmtpSettings": {
    "Server": "smtp.gmail.com",
    "Port": "587",
    "SenderName": "BidSnap",
    "SenderEmail": "your-email@gmail.com",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  },
  "CorsSettings": {
    "AllowedOrigins": [ "http://localhost:3000", "http://localhost:5173" ]
  }
}
```

### 3. Create & Migrate Database
```bash
dotnet ef database update
```
This will create the database and seed sample data.

### 4. Run the Application
```bash
dotnet run
```
API will be available at: `https://localhost:5001` (or configured port)
Swagger UI: `https://localhost:5001/swagger`

## ?? API Endpoints

### Authentication
```
POST   /api/auth/register       - Register new user
POST   /api/auth/login          - Login user
```

### Products
```
GET    /api/products            - Get all products (with filters)
GET    /api/products/{id}       - Get product details
POST   /api/products            - Create product [Authenticated]
PUT    /api/products/{id}       - Update product [Authenticated]
DELETE /api/products/{id}       - Delete product [Authenticated]
GET    /api/products/my         - Get user's products [Authenticated]
PUT    /api/products/{id}/feature - Toggle featured [Authenticated]
PUT    /api/products/{id}/force-close - Force close [Admin]
```

### Bids
```
GET    /api/bids/product/{id}   - Get bids for product
GET    /api/bids/my             - Get user's bids [Authenticated]
POST   /api/bids                - Place bid [Authenticated]
PUT    /api/bids/{id}/withdraw  - Withdraw bid [Authenticated]
```

### Memberships
```
GET    /api/memberships         - Get all membership plans
GET    /api/memberships/{id}    - Get membership details
POST   /api/memberships/purchase - Purchase membership [Authenticated]
```

### Users
```
GET    /api/users/profile       - Get user profile [Authenticated]
GET    /api/users/{id}          - Get user details [Admin]
GET    /api/users              - Get all users [Admin]
POST   /api/users/suspend       - Suspend user [Admin]
PUT    /api/users/{id}/unsuspend - Unsuspend user [Admin]
POST   /api/users/pay-penalty   - Pay suspension penalty [Authenticated]
GET    /api/users/notifications - Get notifications [Authenticated]
PUT    /api/users/notifications/{id}/read - Mark read [Authenticated]
PUT    /api/users/notifications/read-all  - Mark all read [Authenticated]
```

### Categories
```
GET    /api/categories          - Get all categories
GET    /api/categories/{id}     - Get category details
POST   /api/categories          - Create category [Admin]
PUT    /api/categories/{id}     - Update category [Admin]
DELETE /api/categories/{id}     - Delete category [Admin]
```

### Admin
```
GET    /api/admin/dashboard     - Get dashboard stats [Admin]
GET    /api/admin/transactions  - Get all transactions [Admin]
```

## ?? Test Credentials (Seed Data)

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@bidsnap.com` | `Admin@123` |
| User | `alice@example.com` | `User@123` |
| User | `bob@example.com` | `User@123` |
| User | `charlie@example.com` | `User@123` |
| User | `diana@example.com` | `User@123` |
| User | `eve@example.com` | `User@123` |

## ?? Database Models

### Core Entities
- **User** - Account with membership, suspension status, bid withdrawal count
- **Product** - Auction listing with seller, category, bidding timeline
- **Bid** - Bid record with amount, timestamp, withdrawal status
- **Membership** - Plan (Free/Silver/Gold/Platinum) with feature limits
- **Transaction** - Payment records for membership/penalty
- **Category** - Product category
- **Notification** - User notifications

## ?? Membership Restrictions

| Feature | Free | Silver | Gold | Platinum |
|---------|------|--------|------|----------|
| Max Ads | 1 | 5 | 20 | Unlimited |
| Max Bids | 5 | 30 | 100 | Unlimited |
| Featured Ads | 0 | 1 | 5 | Unlimited |
| Duration | 6 mo | 6 mo | 6 mo | 12 mo |
| Price | Free | ?499 | ?999 | ?1999 |

## ?? Product Filtering Example

```
GET /api/products?category=1&minPrice=5000&maxPrice=50000&isFeatured=true&sortBy=priceAsc&page=1&pageSize=12
```

**Query Parameters:**
- `category` - Category ID filter
- `minPrice` - Minimum price filter
- `maxPrice` - Maximum price filter
- `isFeatured` - Featured products only
- `endingSoon` - Ending within 24 hours
- `search` - Search in title/description
- `sortBy` - `priceAsc`, `priceDesc`, `newest`, `endingSoon`
- `page` - Page number (default: 1)
- `pageSize` - Items per page (default: 12)

## ?? Business Logic

### Bidding Flow
1. User places bid (amount > current price)
2. System checks: membership bid limit, suspension status, product validity
3. Previous highest bidder notified via email
4. Bid history tracked; withdrawn bids marked as inactive
5. Current price updated

### Bid Withdrawal Penalties
- Each withdrawal increases user's `BidWithdrawCount`
- **> 3 withdrawals** ? Auto-suspension for 30 days
- User can pay ?500 penalty to lift suspension immediately

### Auction Auto-Close
- Background service runs **every 60 seconds**
- Closes expired auctions (`BidEndTime` <= now)
- Sets winner to highest bidder
- Sends email notification to winner
- Creates notification records

## ?? Email Configuration (Gmail)

1. Enable 2-Factor Authentication in Gmail
2. Generate "App Password" from Gmail Security settings
3. Use app password in `SmtpSettings:Password`

Example:
```json
"SmtpSettings": {
  "Server": "smtp.gmail.com",
  "Port": "587",
  "SenderEmail": "your-email@gmail.com",
  "Username": "your-email@gmail.com",
  "Password": "xxxx xxxx xxxx xxxx"
}
```

## ?? Project Structure

```
backend/
??? Controllers/           # API endpoints (7 controllers)
??? Services/             # Business logic (7 service pairs)
??? Repositories/         # Data access layer (generic repository)
??? Models/              # EF Core entities (7 models)
??? DTOs/                # Data transfer objects (8 DTO files)
??? Data/                # DbContext & seed data
??? Mappings/            # AutoMapper profiles
??? Validators/          # FluentValidation rules
??? Middleware/          # Exception handling
??? BackgroundServices/  # Auction closing service
??? appsettings.json     # Configuration
??? Program.cs           # Startup & DI configuration
??? backend.csproj       # Project file
```

## ?? Error Handling

All errors are caught by `ExceptionHandlingMiddleware` and return standardized JSON response:

```json
{
  "success": false,
  "message": "Error description",
  "statusCode": 400
}
```

## ?? Frontend Integration

### CORS Configuration
Already configured for:
- `http://localhost:3000` (React)
- `http://localhost:5173` (Vite)
- `http://localhost:4200` (Angular)

Add more origins in `appsettings.json`:
```json
"CorsSettings": {
  "AllowedOrigins": [ "http://localhost:3000", "https://yourdomain.com" ]
}
```

### JWT Token Usage
Include in request header:
```
Authorization: Bearer <your_jwt_token>
```

## ?? Troubleshooting

### Database Connection Failed
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure database name is unique

### Migration Issues
```bash
dotnet ef database drop --force
dotnet ef database update
```

### Swagger Not Loading
- Ensure `app.UseSwagger()` is called in `Program.cs`
- Check if running in development environment
- Clear browser cache

## ?? API Response Format

### Success Response
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { ... }
}
```

### Error Response
```json
{
  "success": false,
  "message": "Detailed error message",
  "statusCode": 400
}
```

## ?? Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open Pull Request

## ?? License

This project is licensed under the MIT License - see the LICENSE file for details.

## ?? Support

For issues and questions, please open an issue on GitHub or contact the development team.

---

**Built with ?? by BidSnap Team**
