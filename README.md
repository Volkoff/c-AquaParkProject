# Aqua Park Manager

A comprehensive Windows desktop application for managing aqua park operations, built with WPF and Entity Framework Core. The application connects to an Oracle database with a dynamic connection configuration dialog at startup.

## Features

The application provides management interfaces for:

- **👥 Staff Management** - Manage staff members, roles, and certifications
- **🏊 Pools Management** - Manage swimming pools and their specifications
- **🎢 Slides Management** - Manage water slides and their configurations
- **👤 Visitors Management** - Manage visitor information and profiles
- **🎫 Tickets Management** - Manage tickets and ticket types
- **📅 Bookings Management** - Manage reservations and bookings
- **🎯 Attractions Management** - Manage park attractions and their status
- **🔧 Maintenance Records** - Track maintenance and repairs
- **⏱️ Scheduling Management** - Manage staff shifts and schedules
- **📦 Inventory Management** - Track inventory items and stock levels
- **📋 Certifications Management** - Manage staff certifications and training
- **💰 Concessions Management** - Manage food and beverage sales
- **📊 Reports & Analytics** - Generate operational reports and insights

## Getting Started

### Prerequisites

- .NET 8.0 or later
- Oracle Database 19c or later
- Visual Studio 2022 or later (recommended)
- Network access to Oracle database server

### Installation

1. Clone the repository
2. Open the solution in Visual Studio
3. Build the solution (Ctrl+Shift+B)
4. Run the application (F5)

### Database Connection

When you launch the application, a **Database Connection Configuration** dialog appears before login:

1. **Enter Connection Details**:
   - **Host**: Oracle database server address (e.g., `fei-sql3.upceucebny.cz`)
   - **Port**: Oracle listener port (default: `1521`)
   - **SID**: Oracle System Identifier (e.g., `BDAS`)
   - **User Id**: Oracle database user (e.g., `st72504`)
   - **Password**: Oracle database password

2. **Test Connection**: Click the "Test" button to verify your credentials
   - A success message confirms the connection is valid
   - An error message indicates connection issues

3. **Connect**: Click "Connect" to establish the connection
   - The connection string is stored in memory for the session
   - If successful, the Login window appears

### Database Configuration

The database connection is managed dynamically:

- **Connection String Storage**: Stored in `App.ConnectionString` static property
- **Configuration Location**: `AquaParkContext.OnConfiguring()` method
- **Fallback**: A commented-out hardcoded connection string is available for development

The application uses Entity Framework Core 8 with Oracle.EntityFrameworkCore for database operations.

```csharp
// From AquaParkContext.cs
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    if (!string.IsNullOrWhiteSpace(App.ConnectionString))
    {
        optionsBuilder.UseOracle(App.ConnectionString);
    }
}

## Application Windows & Management Modules

### Startup Windows

#### 1. Database Connection Configuration Window
- **Purpose**: Capture and validate Oracle database connection parameters
- **Fields**:
  - Host: Database server address
  - Port: Oracle listener port
  - SID: Oracle System Identifier
  - User Id: Database username
  - Password: Database password
- **Buttons**:
  - **Test**: Validates the connection without saving
  - **Connect**: Establishes connection and proceeds to login
- **Location**: `Windows/ConnectionConfigWindow.xaml`

#### 2. Login Window
- **Purpose**: User authentication against the USERS table in the database
- **Fields**:
  - Username: User login identifier
  - Password: User password
- **Buttons**:
  - **Login**: Authenticates user and opens main application
  - **Exit**: Closes the application
- **Location**: `Windows/LoginWindow.xaml`

### Main Application Window

#### 3. Main Window
- **Purpose**: Central hub with navigation to all management modules
- **Layout**: Menu buttons for quick access to each module
- **Features**:
  - Modern WPF interface with water-themed styling
  - Water drop icon in title bar
  - Status bar showing user information and operation results
  - Background image with water splash theme
- **Location**: `MainWindow.xaml`

### Management Windows

#### 4. Staff Management Window
- **Purpose**: Manage staff members and their information
- **Operations**: Add, Edit, Delete, Search staff records
- **Fields**: Name, Position, Department, Hire Date, Status
- **Database Table**: `STAFF`
- **Location**: `Windows/StaffManagementWindow.xaml`

#### 5. Pools Management Window
- **Purpose**: Manage swimming pool records and specifications
- **Operations**: Add, Edit, Delete, Search pool records
- **Fields**: Pool Name, Capacity, Depth, Water Type, Temperature Setting
- **Database Table**: `POOLS`
- **Location**: `Windows/PoolsManagementWindow.xaml`

#### 6. Slides Management Window
- **Purpose**: Manage water slides and their configurations
- **Operations**: Add, Edit, Delete, Search slide records
- **Fields**: Slide Name, Type, Height, Speed, Safety Rating
- **Database Table**: `SLIDES`
- **Location**: `Windows/SlidesManagementWindow.xaml`

#### 7. Visitors Management Window
- **Purpose**: Manage visitor profiles and information
- **Operations**: Add, Edit, Delete, Search visitor records
- **Fields**: First Name, Last Name, Email, Phone, Membership Status
- **Database Table**: `VISITORS`
- **Location**: `Windows/VisitorsManagementWindow.xaml`

#### 8. Tickets Management Window
- **Purpose**: Manage ticket types and pricing
- **Operations**: Add, Edit, Delete, Search ticket records
- **Fields**: Ticket Type, Price, Description, Valid From/Until
- **Database Table**: `TICKETS`
- **Location**: `Windows/TicketsManagementWindow.xaml`

#### 9. Bookings Management Window
- **Purpose**: Manage visitor reservations and bookings
- **Operations**: Add, Edit, Delete, Search booking records
- **Fields**: Visitor ID, Booking Date, Check-in Time, Duration, Number of Guests
- **Database Table**: `BOOKINGS`
- **Location**: `Windows/BookingsManagementWindow.xaml`

#### 10. Maintenance Management Window
- **Purpose**: Track maintenance records and equipment status
- **Operations**: Add, Edit, Delete, Search maintenance records
- **Fields**: Equipment, Issue Description, Date, Technician, Status
- **Database Table**: `MAINTENANCE_RECORDS`
- **Location**: `Windows/MaintenanceManagementWindow.xaml`

#### 11. Certifications Management Window
- **Purpose**: Manage staff certifications and training records
- **Operations**: Add, Edit, Delete, Search certification records
- **Fields**: Staff Member, Certification Type, Issue Date, Expiry Date, Status
- **Database Tables**: `CERTIFICATIONS`, `STAFF`
- **Location**: `Windows/CertificationsManagementWindow.xaml`

#### 12. Scheduling Management Window
- **Purpose**: Manage staff shifts and work schedules
- **Operations**: Add, Edit, Delete, Search schedule records
- **Fields**: Staff Member, Date, Start Time, End Time, Location
- **Database Tables**: `SHIFTS`, `STAFF_SHIFTS`
- **Location**: `Windows/SchedulingManagementWindow.xaml`

#### 13. Inventory Management Window
- **Purpose**: Track inventory items and stock levels
- **Operations**: Add, Edit, Delete, Search inventory records
- **Fields**: Item Name, Category, Quantity, Min Stock Level, Supplier
- **Features**: Highlights low-stock items
- **Database Table**: `INVENTORY`
- **Location**: `Windows/InventoryManagementWindow.xaml`

#### 14. Media Management Window
- **Purpose**: Manage media files and resources (images, videos, documents)
- **Operations**: Add, Edit, Delete, Search media records
- **Fields**: Media Name, Type, File Path, Description, Upload Date
- **Database Table**: `MEDIA`
- **Location**: `Windows/MediaManagementWindow.xaml`

#### 15. Reports & Analytics Window
- **Purpose**: Generate and view operational reports and analytics
- **Reports Available**:
  - Visitor statistics and trends
  - Booking summaries
  - Maintenance history reports
  - Staff performance metrics
- **Location**: `Windows/ReportsWindow.xaml`

## Usage Patterns

### Common Operations

Each management window follows a consistent pattern:

1. **View Records** - Data grid displays all records from the database
2. **Search/Filter** - Type in search box to filter records in real-time
3. **Add Record** - Click "Add" button to create new record
4. **Edit Record** - Double-click or select record and edit fields
5. **Save Changes** - Click "Save" to persist changes to database
6. **Delete Record** - Select record and click "Delete" (with confirmation)
7. **Clear Form** - Click "Clear" to reset form for new entry
8. **Status Updates** - Status bar shows operation results

### Features

- **Real-time Search** - Filter records as you type
- **Data Validation** - Input validation with helpful error messages
- **Status Feedback** - Status bar displays success/error messages
- **Consistent UI** - All windows use standard styling and layout
- **Water-themed Design** - Background image and water drop icon throughout
- **Dark Header Bar** - Blue header bar with white text
- **Color-coded Buttons**:
  - Green: Add/Create actions
  - Blue: Primary actions
  - Red: Delete/Danger actions
  - Gray: Neutral actions

## Database Tables

The application manages the following main entities:

- `USERS` - User login and authentication
- `STAFF` - Staff members and their information
- `ROLES` - Staff roles and permissions
- `CERTIFICATIONS` - Staff certification records
- `POOLS` - Swimming pool specifications
- `SLIDES` - Water slide configurations
- `VISITORS` - Visitor profiles
- `TICKETS` - Ticket types and pricing
- `BOOKINGS` - Visitor reservations
- `ATTRACTIONS` - Park attractions
- `MAINTENANCE_RECORDS` - Maintenance and repair logs
- `SHIFTS` - Work shift definitions
- `STAFF_SHIFTS` - Staff-to-shift assignments
- `INVENTORY` - Inventory items and stock
- `MEDIA` - Media files and resources
- `CONCESSIONS` - Food and beverage sales
- `MEMBERSHIP` - Visitor membership information
- `LOYALTY_POINTS` - Loyalty program tracking
- `TRAINING` - Staff training records
- `OCCUPANCY_TRACKING` - Park occupancy monitoring

## Technical Architecture

### Technology Stack

- **UI Framework**: Windows Presentation Foundation (WPF) - .NET 8.0
- **Database ORM**: Entity Framework Core 8
- **Database**: Oracle Database 21c
- **Database Provider**: Oracle.EntityFrameworkCore
- **Design Pattern**: MVVM with code-behind for business logic

### Key Components

- **App.xaml.cs** - Application startup, shutdown management, connection string storage
- **App.xaml** - Global styling (brushes, button styles, window style, background brush)
- **AquaParkContext.cs** - Entity Framework DbContext with 45+ DbSets
- **Windows/** - Individual management window implementations

### Data Access

- Uses Entity Framework Core for ORM mapping
- Lazy loading and explicit loading patterns for related entities
- Materialization of LINQ queries (.ToList()) before computing calculated properties to avoid Oracle translation issues
- Dynamic connection string support for flexible database configuration

### Styling

- Global WPF styles in App.xaml for consistency
- DrawingBrush background combining white base layer with water-splash PNG
- Blue header color (#FF2E86AB) for management window headers
- Color-coded button styles for user guidance
- Water drop icon (.ico) applied globally to all windows

## Troubleshooting

### Database Connection Issues

If you encounter database connection issues:

1. **Verify Oracle Server Accessibility**
   - Ensure the Oracle server is online and accepting connections
   - Check network connectivity to the host
   - Verify the host address, port, and SID are correct

2. **Test Connection Button**
   - Use the "Test" button in the Database Connection Configuration window to validate credentials
   - Check error messages for specific issues (ORA-xxxxx codes)

3. **Authentication Failures**
   - Verify username and password are correct
   - Ensure the user has appropriate database privileges
   - Check if the user account is locked or expired

4. **Connection String Format**
   - The application constructs Oracle connection strings dynamically
   - Required format: Host, Port, SID, User Id, Password

### Login Issues

1. **Invalid Credentials**
   - Verify login username and password match a USERS table record
   - Check the connection was successful before attempting login

2. **Application Closes After Connect**
   - Ensure the connection string is valid and the database is accessible
   - Check that a USERS table exists in the database

### Build Issues

1. **NuGet Package Errors**
   - Restore NuGet packages: `dotnet restore`
   - Clear NuGet cache and rebuild

2. **.NET 8.0 SDK Not Found**
   - Ensure .NET 8.0 SDK is installed
   - Verify Visual Studio has necessary workloads installed

3. **Oracle.EntityFrameworkCore Package Issues**
   - Ensure you have a valid NuGet source configured
   - The package requires Oracle Client libraries on some systems

### Runtime Errors

1. **"No records found" or Empty Grid**
   - Verify the database connection is valid
   - Check that tables exist in the database
   - Ensure the logged-in user has SELECT permissions on tables

2. **Save Changes Not Working**
   - Check database user has INSERT/UPDATE/DELETE permissions
   - Verify no primary key conflicts exist
   - Review error messages in status bar

3. **Performance Issues**
   - Large datasets may load slowly; consider adding filters/search
   - The application materializes large result sets to memory
   - Consider archiving old records to improve performance

## Project Structure

```
c-AquaParkProject/
├── AquaParkManager/
│   ├── App.xaml                          # Global styles and brushes
│   ├── App.xaml.cs                       # Application startup logic
│   ├── MainWindow.xaml                   # Main application window
│   ├── MainWindow.xaml.cs                # Main window logic
│   ├── Models/
│   │   └── AquaParkContext.cs            # Entity Framework DbContext
│   ├── Windows/                          # Management window implementations
│   │   ├── ConnectionConfigWindow.xaml   # Database connection config
│   │   ├── LoginWindow.xaml              # User login window
│   │   ├── StaffManagementWindow.xaml    # Staff management
│   │   ├── PoolsManagementWindow.xaml    # Pools management
│   │   ├── SlidesManagementWindow.xaml   # Slides management
│   │   ├── VisitorsManagementWindow.xaml # Visitors management
│   │   ├── TicketsManagementWindow.xaml  # Tickets management
│   │   ├── BookingsManagementWindow.xaml # Bookings management
│   │   ├── MaintenanceManagementWindow.xaml # Maintenance records
│   │   ├── CertificationsManagementWindow.xaml # Certifications
│   │   ├── SchedulingManagementWindow.xaml # Shift scheduling
│   │   ├── InventoryManagementWindow.xaml # Inventory management
│   │   └── MediaManagementWindow.xaml    # Media management
│   ├── Resources/
│   │   ├── water_drop.ico                # Application icon
│   │   ├── water_drop.svg                # Icon source (SVG)
│   │   └── pngimg.com - water_splash_PNG72.png # Background image
│   └── AquaParkManager.csproj            # Project configuration
├── CREATEDB.sql                          # Database creation script
├── CREATETABLES.sql                      # Database table creation script
├── AquaParkManager.sln                   # Visual Studio solution
├── convert_svg_to_ico.py                 # Icon generation script
└── README.md                             # This file
```

## Development Notes

### Entity Framework Models

The application uses a code-first approach with Entity Framework Core. Models are defined in `AquaParkContext.cs` with DbSet properties for each entity.

### LINQ Query Optimization

Due to Oracle's limitations with certain LINQ translations, the application:
- Materializes queries to memory before computing calculated properties
- Example: `.ToList().Select(x => new ViewModel { ... })`

### Connection Management

- Connection string stored in `App.ConnectionString` static property
- Set by `ConnectionConfigWindow` after successful connection test
- Used by `AquaParkContext.OnConfiguring()` to initialize DbContext

### Window Lifecycle

1. App startup → `ConnectionConfigWindow` (database config)
2. Successful connection → `LoginWindow` (user authentication)
3. Successful login → `MainWindow` (main application)
4. User can open management windows from main window
5. Application stays open until user closes main window

## Contributing

This is a comprehensive aqua park management application. To contribute:

1. Maintain consistency with existing patterns and styling
2. Follow the window naming conventions
3. Add database migrations when modifying entity models
4. Test with actual Oracle database connection

## License

This project is provided as-is for educational and demonstration purposes.
