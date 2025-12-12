# Aqua Park Manager

Komplexní desktopová aplikace pro Windows pro správu provozu aquaparku, vytvořená pomocí WPF a Entity Framework Core. Aplikace se připojuje k databázi Oracle pomocí dialogového okna pro dynamickou konfiguraci připojení při spuštění.

## Funkce

Aplikace poskytuje rozhraní pro správu:

* **👥 Staff Management** - Správa zaměstnanců, rolí a certifikací
* **🏊 Pools Management** - Správa plaveckých bazénů a jejich specifikací
* **🎢 Slides Management** - Správa tobogánů a jejich konfigurací
* **👤 Visitors Management** - Správa informací o návštěvnících a profilech
* **🎫 Tickets Management** - Správa vstupenek a typů vstupenek
* **📅 Bookings Management** - Správa rezervací a bookingů
* **🎯 Attractions Management** - Správa atrakcí v parku a jejich stavu
* **🔧 Maintenance Records** - Sledování údržby a oprav
* **⏱️ Scheduling Management** - Správa směn zaměstnanců a rozvrhů
* **📦 Inventory Management** - Sledování skladových položek a stavu zásob
* **📋 Certifications Management** - Správa certifikací a školení zaměstnanců
* **💰 Concessions Management** - Správa prodeje jídla a nápojů
* **📊 Reports & Analytics** - Generování provozních reportů a analýz

## Začínáme

### Prerekvizity

* .NET 8.0 nebo novější
* Oracle Database 19c nebo novější
* Visual Studio 2022 nebo novější (doporučeno)
* Síťový přístup k serveru databáze Oracle

### Instalace

1.  Naklonujte repozitář
2.  Otevřete řešení (solution) ve Visual Studiu
3.  Sestavte řešení (Ctrl+Shift+B)
4.  Spusťte aplikaci (F5)

### Připojení k databázi

Při spuštění aplikace se před přihlášením zobrazí dialogové okno **Database Connection Configuration**:

1.  **Zadejte údaje o připojení**:
    * **Host**: Adresa serveru databáze Oracle (např. `fei-sql3.upceucebny.cz`)
    * **Port**: Port posluchače Oracle (výchozí: `1521`)
    * **SID**: Identifikátor systému Oracle (např. `BDAS`)
    * **User Id**: Uživatel databáze Oracle (např. `st72504`)
    * **Password**: Heslo k databázi Oracle
2.  **Test připojení**: Kliknutím na tlačítko "Test" ověřte své údaje.
    * Zpráva o úspěchu potvrdí, že připojení je platné.
    * Chybová zpráva indikuje problémy s připojením.
3.  **Připojit**: Kliknutím na "Connect" navažte připojení.
    * Připojovací řetězec je uložen v paměti pro relaci.
    * V případě úspěchu se zobrazí přihlašovací okno (Login window).

### Konfigurace databáze

Připojení k databázi je spravováno dynamicky:

* **Uložení Connection Stringu**: Uloženo ve statické vlastnosti `App.ConnectionString`
* **Umístění konfigurace**: Metoda `AquaParkContext.OnConfiguring()`
* **Fallback**: Pro vývoj je k dispozici zakomentovaný "hardcoded" připojovací řetězec

Aplikace používá Entity Framework Core 8 s Oracle.EntityFrameworkCore pro databázové operace.

```csharp
// Z AquaParkContext.cs
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

- **Location**: `Windows/ReportsWindow.xaml`

---

CZ / Čeština — Okna aplikace a moduly

Níže jsou uvedena stejná okna jako výše, stručně v češtině (podrobný popis je v angličtině nahoře):

1. **Dialog konfigurace připojení k databázi** (`Windows/ConnectionConfigWindow.xaml`)
   - Účel: Zadání a ověření parametrů připojení k Oracle DB (Host, Port, SID, User, Password).

2. **Přihlašovací okno** (`Windows/LoginWindow.xaml`)
   - Účel: Ověření uživatele proti tabulce `USERS`.

3. **Hlavní okno** (`MainWindow.xaml`)
   - Účel: Centrální rozhraní s navigací do všech modulů.

4. **Správa zaměstnanců** (`Windows/StaffManagementWindow.xaml`)
   - Účel: Přidávání, úpravy a mazání záznamů zaměstnanců.

5. **Správa bazénů** (`Windows/PoolsManagementWindow.xaml`)
   - Účel: Správa záznamů o bazénech (kapacita, hloubka, atd.).

6. **Správa tobogánů** (`Windows/SlidesManagementWindow.xaml`)
   - Účel: Správa konfigurací tobogánů.

7. **Správa návštěvníků** (`Windows/VisitorsManagementWindow.xaml`)
   - Účel: Správa profilů návštěvníků.

8. **Správa vstupenek** (`Windows/TicketsManagementWindow.xaml`)
   - Účel: Správa typů vstupenek a cen.

9. **Správa rezervací** (`Windows/BookingsManagementWindow.xaml`)
   - Účel: Správa rezervací a bookingů návštěvníků.

10. **Správa údržby** (`Windows/MaintenanceManagementWindow.xaml`)
    - Účel: Záznamy o údržbě a opravách zařízení.

11. **Správa certifikací** (`Windows/CertificationsManagementWindow.xaml`)
    - Účel: Sledování certifikací a školení zaměstnanců.

12. **Plánování směn** (`Windows/SchedulingManagementWindow.xaml`)
    - Účel: Správa směn a rozvrhů zaměstnanců.

13. **Správa zásob** (`Windows/InventoryManagementWindow.xaml`)
    - Účel: Sledování skladových položek a nízkého stavu zásob.

14. **Správa médií** (`Windows/MediaManagementWindow.xaml`)
    - Účel: Správa obrázků, videí a dalších mediálních souborů.

15. **Reporty a analytika** (`Windows/ReportsWindow.xaml`)
    - Účel: Generování provozních reportů (statistiky návštěvnosti, souhrny rezervací, historie údržby).

---

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

---

CZ / Čeština — Vzorce používání a funkce

### Běžné operace

Každé správcovské okno používá konzistentní sadu operací:

1. **Zobrazení záznamů** - Datová mřížka zobrazuje záznamy z databáze
2. **Vyhledávání / Filtry** - Filtrování záznamů při psaní
3. **Přidat záznam** - Tlačítko "Add" pro vytvoření nového záznamu
4. **Upravit záznam** - Dvojklik nebo výběr záznamu pro úpravy
5. **Uložit změny** - Tlačítko "Save" uloží změny do databáze
6. **Odstranit záznam** - Výběr a klik na "Delete" (s potvrzením)
7. **Vyčistit formulář** - Tlačítko "Clear" resetuje formulář
8. **Stavové zprávy** - Stavový řádek zobrazuje výsledky operací

### Funkce

- **Vyhledávání v reálném čase** - Filtrování záznamů během psaní
- **Validace dat** - Kontroly vstupních dat s chybovými hláškami
- **Zpětná vazba ve stavu** - Stavový řádek zobrazuje úspěch/chyby
- **Konzistentní UI** - Všechna okna sdílejí stylování a layout
- **Vzhled s vodní tematikou** - Pozadí s efektem vody a ikona kapky
- **Tmavý hlavičkový pruh** - Modrý pruh s bílým textem
- **Barevně kódovaná tlačítka**:
   - Zelená: Přidat / Vytvořit
   - Modrá: Hlavní akce
   - Červená: Odstranit / Nebezpečné akce
   - Šedá: Neutrální akce

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

- `OCCUPANCY_TRACKING` - Park occupancy monitoring

---

CZ / Čeština — Tabulky databáze

Aplikace pracuje s následujícími hlavními entitami (stejné názvy tabulek v DB):

- `USERS` - Přihlášení uživatelů a autentizace
- `STAFF` - Zaměstnanci a jejich informace
- `ROLES` - Role zaměstnanců a oprávnění
- `CERTIFICATIONS` - Záznamy o certifikacích zaměstnanců
- `POOLS` - Specifikace bazénů
- `SLIDES` - Konfigurace tobogánů
- `VISITORS` - Profily návštěvníků
- `TICKETS` - Typy a ceny vstupenek
- `BOOKINGS` - Rezervace návštěvníků
- `ATTRACTIONS` - Atrakce v parku
- `MAINTENANCE_RECORDS` - Záznamy o údržbě a opravách
- `SHIFTS` - Definice směn
- `STAFF_SHIFTS` - Přiřazení zaměstnanců ke směnám
- `INVENTORY` - Položky skladu a zásoby
- `MEDIA` - Mediální soubory
- `CONCESSIONS` - Prodej jídel a nápojů
- `MEMBERSHIP` - Informace o členství návštěvníků
- `LOYALTY_POINTS` - Program věrnostních bodů
- `TRAINING` - Záznamy o školeních zaměstnanců
- `OCCUPANCY_TRACKING` - Sledování obsazenosti parku

---

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

---

CZ / Čeština — Technická architektura a stylování

### Technologické zásobníky

- **UI framework**: Windows Presentation Foundation (WPF) - .NET 8.0
- **ORM databáze**: Entity Framework Core 8
- **Databáze**: Oracle Database (21c a novější)
- **Poskytovatel**: Oracle.EntityFrameworkCore
- **Architektonický vzor**: MVVM s částečným code-behind pro business logiku

### Hlavní komponenty

- **App.xaml.cs** - Spouštění aplikace, správa ukončení a uložení connection stringu
- **App.xaml** - Globální styly (barvy, styly tlačítek, styl oken, pozadí)
- **AquaParkContext.cs** - DbContext Entity Framework s více než 45 DbSety
- **Windows/** - Implementace jednotlivých správcovských oken

### Přístup k datům

- Používá Entity Framework Core pro mapování ORM
- Lazy loading a explicitní načítání příbuzných entit tam, kde je to potřeba
- Materializace LINQ dotazů (`.ToList()`) před výpočtem vypočtených vlastností kvůli omezením překladů do Oracle SQL
- Dynamická podpora connection stringu pro flexibilní konfiguraci databáze

### Stylování

- Globální WPF styly v `App.xaml` pro konzistenci
- `DrawingBrush` kombinující bílou základní vrstvu s PNG obrázkem vodní tříště
- Modrý hlavičkový pruh pro nadpisy (#FF2E86AB)
- Barevně kódovaná tlačítka podle významu akce
- Ikona aplikace (kapka vody) přiložena v `Resources/water_drop.ico`

---

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

---

CZ / Čeština — Řešení problémů

### Problémy s připojením k databázi

Pokud narazíte na potíže s připojením k databázi:

1. **Ověřte dostupnost Oracle serveru**
   - Ujistěte se, že Oracle server je online a přijímá připojení
   - Zkontrolujte síťovou konektivitu na daný host
   - Ověřte správnost hostu, portu a SID

2. **Tlačítko Test připojení**
   - Použijte tlačítko "Test" v okně konfigurace připojení pro ověření přihlašovacích údajů
   - Prostudujte chybové kódy (ORA-xxxxx) pro přesnou diagnostiku

3. **Problémy s autentizací**
   - Zkontrolujte správnost uživatelského jména a hesla
   - Ujistěte se, že uživatelský účet má potřebná oprávnění
   - Zkontrolujte, zda účet není zablokovaný nebo expirovaný

### Problémy s přihlášením

1. **Neplatné přihlašovací údaje**
   - Ověřte, že uživatel existuje v tabulce `USERS`
   - Zkontrolujte, že připojení k DB bylo úspěšné před přihlášením

2. **Aplikace se zavře po Connect**
   - Ověřte platnost connection stringu
   - Zkontrolujte existenci tabulky `USERS`

### Sestavení a závislosti

1. **Chyby NuGet balíčků**
   - Obnovte NuGet balíčky: `dotnet restore`
   - Vyčistěte NuGet cache a znovu sestavte projekt

2. **Chybějící .NET SDK**
   - Ujistěte se, že máte nainstalovaný .NET 8.0 SDK

3. **Problémy s Oracle.EntityFrameworkCore**
   - Zkontrolujte správné nastavení NuGet zdrojů
   - Některé systémy mohou vyžadovat nainstalované Oracle klientské knihovny

### Runtime chyby

1. **Žádné záznamy nebo prázdná mřížka**
   - Zkontrolujte připojení k databázi a existence tabulek
   - Ujistěte se, že přihlášený uživatel má oprávnění SELECT

2. **Ukládání změn selže**
   - Zkontrolujte INSERT/UPDATE/DELETE práva uživatele
   - Ověřte integritu primárních klíčů

3. **Výkonové problémy**
   - Zvažte filtrování a stránkování větších datasetů
   - Aplikace materializuje data do paměti pro některé operace
   - Archivace starých záznamů pomůže zlepšit výkon

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

---

CZ / Čeština — Struktura projektu

Struktura repozitáře (viz výše pro detailní soubory):

- `AquaParkManager/` – hlavní projekt WPF (složky `Windows/`, `Models/`, `Resources/`)
- `CREATEDB.sql`, `CREATETABLES.sql` – skripty pro vytvoření databáze/tabulek
- `convert_svg_to_ico.py` – skript pro generování ikony (volitelný)

---

## Development Notes

### Entity Framework Models

The application uses a code-first approach with Entity Framework Core. Models are defined in `AquaParkContext.cs` with DbSet properties for each entity.

CZ / Čeština — Poznámky k vývoji

### Modely Entity Framework

Aplikace používá přístup "code-first" s Entity Framework Core. Modely jsou definovány v `AquaParkContext.cs` pomocí DbSet vlastností pro každou entitu.

### Optimalizace LINQ dotazů

Due to Oracle's limitations with certain LINQ translations, the application:
- Materializes queries to memory before computing calculated properties
- Example: `.ToList().Select(x => new ViewModel { ... })`

CZ / Čeština — Optimalizace LINQ

Kvůli omezením překladů LINQ výrazů do Oracle SQL aplikace:

- materializuje dotazy do paměti před výpočtem vypočtených vlastností (`.ToList()`)
- Příklad: `.ToList().Select(x => new ViewModel { ... })`

### Connection Management / Správa připojení

- Connection string stored in `App.ConnectionString` static property
- Set by `ConnectionConfigWindow` after successful connection test
- Used by `AquaParkContext.OnConfiguring()` to initialize DbContext

CZ / Čeština — Správa připojení

- Připojovací řetězec je uložen ve statické vlastnosti `App.ConnectionString`
- Hodnota je nastavena oknem `ConnectionConfigWindow` po úspěšném testu
- `AquaParkContext.OnConfiguring()` používá tento řetězec pro inicializaci DbContextu

### Window Lifecycle / Životní cyklus oken

1. App startup → `ConnectionConfigWindow` (database config)
2. Successful connection → `LoginWindow` (user authentication)
3. Successful login → `MainWindow` (main application)
4. User can open management windows from main window
5. Application stays open until user closes main window

CZ / Čeština — Životní cyklus oken

1. Spuštění aplikace → `ConnectionConfigWindow` (konfigurace DB)
2. Po úspěšném připojení → `LoginWindow` (autentizace uživatele)
3. Po úspěšném přihlášení → `MainWindow` (hlavní rozhraní)
4. Uživatel může otevírat správcovská okna z hlavního okna
5. Aplikace zůstane spuštěná, dokud uživatel nezavře hlavní okno

## Contributing / Přispívání

This is a comprehensive aqua park management application. To contribute:

1. Maintain consistency with existing patterns and styling
2. Follow the window naming conventions
3. Add database migrations when modifying entity models
4. Test with actual Oracle database connection

CZ / Čeština — Přispívání

1. Dodržujte konzistenci s existujícími vzory a stylem
2. Dodržujte konvence pojmenování oken
3. Přidejte databázové migrace při změně modelů entit
4. Testujte s reálným připojením k Oracle DB

## License / Licence

This project is provided as-is for educational and demonstration purposes.

CZ / Čeština — Licence

Tento projekt je poskytován "tak jak je" pro vzdělávací a demonstrační účely.
