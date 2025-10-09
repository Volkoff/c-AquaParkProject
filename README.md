# Aqua Park Manager

A comprehensive Windows desktop application for managing aqua park operations, built with WPF and Entity Framework Core.

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

## Database Schema

The application uses a comprehensive database schema that includes:

- Staff management with roles and certifications
- Pool and slide specifications
- Visitor and ticket management
- Booking and payment tracking
- Maintenance records
- Scheduling and shift management

## Getting Started

### Prerequisites

- .NET 8.0 or later
- SQL Server (LocalDB is used by default)
- Visual Studio 2022 or later (recommended)

### Installation

1. Clone the repository
2. Open the solution in Visual Studio
3. Build the solution (Ctrl+Shift+B)
4. Run the application (F5)

### Database Setup

The application will automatically:
- Create the database on first run
- Initialize with sample data including:
  - Sample staff members with roles
  - Pool and slide configurations
  - Ticket types and pricing
  - Sample visitors and bookings

### Configuration

The database connection string is configured in `AquaParkContext.cs`. By default, it uses LocalDB:

```csharp
optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=AquaParkDB;Trusted_Connection=true;MultipleActiveResultSets=true");
```

To use a different database, update this connection string.

## Usage

### Main Navigation

The main window provides a navigation menu with buttons for each management module. Click any button to open the corresponding management window.

### Managing Data

Each management window follows a consistent pattern:

1. **List View** - Shows all records in a data grid
2. **Search** - Filter records by typing in the search box
3. **Add** - Click the "Add" button to create new records
4. **Edit** - Select a record from the list to edit its details
5. **Save** - Click "Save" to persist changes
6. **Delete** - Select a record and click "Delete" to remove it
7. **Clear** - Clear the form to start fresh

### Key Features

- **Real-time Search** - Filter records as you type
- **Data Validation** - Input validation with helpful error messages
- **Status Updates** - Status bar shows operation results
- **Responsive UI** - Modern, clean interface with intuitive navigation

## Database Tables

The application manages the following main entities:

- `staff` - Staff members and their information
- `roles` - Staff roles and permissions
- `pools` - Swimming pool specifications
- `slides` - Water slide configurations
- `visitors` - Visitor profiles
- `tickets` - Ticket sales and types
- `bookings` - Reservations and bookings
- `attractions` - Park attractions
- `maintenance_records` - Maintenance and repair logs

## Technical Details

- **Framework**: .NET 8.0 WPF
- **Database**: SQL Server with Entity Framework Core
- **Architecture**: MVVM pattern with code-behind
- **UI**: Modern WPF with custom styling

## Sample Data

The application includes sample data to help you get started:

- 3 staff members with different roles
- 3 pools (main, kids, wave pool)
- 2 water slides with different types
- 2 ticket types (adult and child)
- Sample visitors and bookings

## Troubleshooting

### Database Connection Issues

If you encounter database connection issues:

1. Ensure SQL Server LocalDB is installed
2. Check the connection string in `AquaParkContext.cs`
3. Verify the database name doesn't conflict with existing databases

### Build Issues

If the project doesn't build:

1. Restore NuGet packages
2. Ensure .NET 8.0 SDK is installed
3. Check that all Entity Framework packages are properly referenced

## Contributing

This is a sample application demonstrating WPF and Entity Framework Core integration. Feel free to extend it with additional features or modify it for your specific needs.

## License

This project is provided as-is for educational and demonstration purposes.
