using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaParkManager.Models
{
    public class AquaParkContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<StaffRole> StaffRoles { get; set; }
        public DbSet<Visitor> Visitors { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingItem> BookingItems { get; set; }
        public DbSet<Attraction> Attractions { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<Address> Address { get; set; }
        public DbSet<PostalCode> PostalCodes { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
        public DbSet<PriceList> PriceLists { get; set; }
        public DbSet<SlideType> SlideTypes { get; set; }

        public DbSet<Pool> Pools { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            optionsBuilder.UseOracle("User Id=st72504;Password=Sejmutvojihoe106;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=fei-sql3.upceucebny.cz)(PORT=1521))(CONNECT_DATA=(SID=BDAS)))");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("USERS");
            modelBuilder.Entity<Staff>().ToTable("STAFF");
            modelBuilder.Entity<Role>().ToTable("ROLES");
            modelBuilder.Entity<StaffRole>().ToTable("STAFF_ROLES");
            modelBuilder.Entity<Visitor>().ToTable("VISITORS");
            modelBuilder.Entity<Booking>().ToTable("BOOKINGS");
            modelBuilder.Entity<BookingItem>().ToTable("BOOKING_ITEMS");
            modelBuilder.Entity<Attraction>().ToTable("ATTRACTIONS");
            modelBuilder.Entity<Media>().ToTable("MEDIA");
            modelBuilder.Entity<Address>().ToTable("ADDRESS");
            modelBuilder.Entity<PostalCode>().ToTable("POSTAL_CODES");
            modelBuilder.Entity<TicketType>().ToTable("TICKET_TYPES");
            modelBuilder.Entity<PriceList>().ToTable("PRICE_LIST");
            modelBuilder.Entity<SlideType>().ToTable("SLIDE_TYPES");

            modelBuilder.Entity<Pool>().ToTable("PARK_AREAS");
            modelBuilder.Entity<MaintenanceRecord>().ToTable("OPERATIONAL_LOGS");
            modelBuilder.Entity<Payment>().ToTable("PAYMENTS");
        }
    }

    // --- ENTITY ---

    [Table("USERS")]
    public class User
    {
        [Key][Column("USER_ID")] public int UserId { get; set; }
        [Column("USERNAME")] public string Username { get; set; } = string.Empty;
        [Column("PASSWORD_HASH")] public string PasswordHash { get; set; } = string.Empty;
        [Column("EMAIL")] public string Email { get; set; } = string.Empty;
        [Column("IS_ACTIVE")] public string IsActive { get; set; } = "Y";

        [Column("STAFF_STAFF_ID")] public int? StaffId { get; set; }
        [Column("VISITORS_VISITOR_ID")] public int? VisitorId { get; set; }
    }

    [Table("STAFF")]
    public class Staff
    {
        [Key][Column("STAFF_ID")] public int StaffId { get; set; }
        [Column("FIRST_NAME")] public string FirstName { get; set; } = string.Empty;
        [Column("LAST_NAME")] public string LastName { get; set; } = string.Empty;
        [Column("EMAIL")] public string? Email { get; set; }
        [Column("PHONE")] public string? Phone { get; set; }
        [Column("JOB_TITLE")] public string? JobTitle { get; set; }

        // OPRAVA: Vráceny chybìjící sloupce HIRE_DATE a NOTES
        [Column("HIRE_DATE")] public DateTime HireDate { get; set; } = DateTime.Now;
        [Column("NOTES")] public string? Notes { get; set; }

        [Column("ACTIVE")] public string Active { get; set; } = "Y";
        [Column("ADDRESS_ADDRESS_ID")] public int AddressId { get; set; }

        [NotMapped] public string FullName => $"{FirstName} {LastName}";
    }

    [Table("ROLES")]
    public class Role
    {
        [Key][Column("ROLE_ID")] public int RoleId { get; set; }
        [Column("ROLE_NAME")] public string RoleName { get; set; } = string.Empty;
    }

    [Table("STAFF_ROLES")]
    public class StaffRole
    {
        [Key][Column("STAFF_ROLE_ID")] public int StaffRoleId { get; set; }
        [Column("STAFF_STAFF_ID")] public int StaffId { get; set; }
        [Column("ROLES_ROLE_ID")] public int RoleId { get; set; }
    }

    [Table("VISITORS")]
    public class Visitor
    {
        [Key][Column("VISITOR_ID")] public int VisitorId { get; set; }
        [Column("FIRST_NAME")] public string? FirstName { get; set; }
        [Column("LAST_NAME")] public string? LastName { get; set; }
        [Column("DATE_OF_BIRTH")] public DateTime? DateOfBirth { get; set; }
        [Column("EMAIL")] public string? Email { get; set; }
        [Column("PHONE")] public string? Phone { get; set; }
        [Column("NOTES")] public string? Notes { get; set; }
        [Column("ADDRESS_ADDRESS_ID")] public int AddressId { get; set; }

        [NotMapped] public string FullName => $"{FirstName} {LastName}";
        [NotMapped] public string? EmergencyContact { get; set; }
    }

    [Table("BOOKINGS")]
    public class Booking
    {
        [Key][Column("BOOKING_ID")] public int BookingId { get; set; }
        [Column("BOOKING_REF")] public string? BookingRef { get; set; }
        [Column("CREATED_DATE")] public DateTime CreatedDate { get; set; } = DateTime.Now;
        [Column("TOTAL_AMOUNT")] public decimal TotalAmount { get; set; } = 0;
        [Column("STATUS")] public string Status { get; set; } = "CONFIRMED";
        [Column("NOTES")] public string? Notes { get; set; }
        [Column("VISITORS_VISITOR_ID")] public int VisitorId { get; set; }
        [NotMapped] public string? CustomerName { get; set; }
    }

    [Table("BOOKING_ITEMS")]
    public class BookingItem
    {
        [Key][Column("BOOKING_ITEM_ID")] public int BookingItemId { get; set; }
        [Column("BOOKINGS_BOOKING_ID")] public int BookingId { get; set; }
        [Column("QUANTITY")] public int Quantity { get; set; } = 1;
        [Column("UNIT_PRICE")] public decimal UnitPrice { get; set; } = 0;
        [Column("TICKET_TYPES_TICKET_TYPE_ID")] public int TicketTypeId { get; set; }
        [ForeignKey("TicketTypeId")] public virtual TicketType? TicketType { get; set; }

        [NotMapped] public int? TicketId { get { return BookingItemId; } set { BookingItemId = value ?? 0; } }
        [NotMapped] public virtual Visitor? Visitor { get; set; }
        [NotMapped] public DateTime PurchaseDate { get; set; } = DateTime.Now;
        [NotMapped] public decimal PricePaid { get { return UnitPrice; } set { UnitPrice = value; } }
        [NotMapped] public string Status { get; set; } = "ACTIVE";
    }

    [Table("ATTRACTIONS")]
    public class Attraction
    {
        [Key][Column("ATTRACTION_ID")] public int AttractionId { get; set; }
        [Column("NAME")] public string Name { get; set; } = string.Empty;
        [Column("STATUS")] public string Status { get; set; } = "OPEN";
        [Column("SLIDE_TYPES_SLIDE_TYPE_ID")] public int? SlideTypeId { get; set; }
        [ForeignKey("SlideTypeId")] public virtual SlideType? SlideType { get; set; }
        [Column("PARK_AREAS_AREA_ID")] public int AreaId { get; set; }
        [ForeignKey("AreaId")] public virtual Pool? Pool { get; set; }

        [NotMapped] public int? ObjectId { get; set; }
        [NotMapped] public int? Capacity { get; set; }
        [NotMapped] public string? Notes { get; set; }
        [NotMapped] public string AttractionType { get; set; } = "Generic";

        [NotMapped] public decimal? LengthM { get; set; }
        [NotMapped] public decimal? HeightM { get; set; }
        [NotMapped] public int? MinHeightCm { get; set; }
        [NotMapped] public int? MaxWeightKg { get; set; }
        [NotMapped] public DateTime? InstallationDate { get; set; }
    }

    [Table("MEDIA")]
    public class Media
    {
        [Key][Column("MEDIA_ID")] public int MediaId { get; set; }
        [Column("FILE_NAME")] public string? FileName { get; set; }
        [Column("MEDIA_DATA")] public byte[]? MediaData { get; set; }
        [Column("MIME_TYPE")] public string? MimeType { get; set; }
        [Column("RELATED_TABLE")] public string RelatedTable { get; set; } = "ATTRACTIONS";
        [Column("RELATED_ID")] public int RelatedId { get; set; }
    }

    [Table("ADDRESS")]
    public class Address
    {
        [Key][Column("ADDRESS_ID")] public int AddressId { get; set; }
        [Column("STREET")] public string? Street { get; set; }
        [Column("HOUSE_NUMBER")] public string HouseNumber { get; set; } = string.Empty;
        [Column("POSTAL_CODES_POSTAL_CODE_ID")] public int PostalCodeId { get; set; }
        [NotMapped] public string City { get; set; } = string.Empty;
    }

    [Table("POSTAL_CODES")]
    public class PostalCode
    {
        [Key][Column("POSTAL_CODE_ID")] public int PostalCodeId { get; set; }
        [Column("CITY")] public string City { get; set; } = string.Empty;
        [Column("POSTAL_CODE")] public string Code { get; set; } = string.Empty;
    }

    [Table("TICKET_TYPES")]
    public class TicketType
    {
        [Key][Column("TICKET_TYPE_ID")] public int TicketTypeId { get; set; }
        [Column("NAME")] public string Name { get; set; } = string.Empty;
        [Column("DESCRIPTION")] public string? Description { get; set; }
        [Column("AGE_FROM")] public int? AgeMin { get; set; }
        [Column("AGE_TO")] public int? AgeMax { get; set; }
        [NotMapped] public int PriceId { get; set; }
    }

    [Table("PRICE_LIST")]
    public class PriceList
    {
        [Key][Column("PRICE_ID")] public int PriceId { get; set; }
        [Column("NAME")] public string Name { get; set; } = string.Empty;
        [Column("BASE_PRICE")] public decimal BasePrice { get; set; }
        [Column("DESCRIPTION")] public string? Description { get; set; }
        [Column("VALID_FROM")] public DateTime ValidFrom { get; set; } = DateTime.Now;
    }

    [Table("SLIDE_TYPES")]
    public class SlideType
    {
        [Key][Column("SLIDE_TYPE_ID")] public int SlideTypeId { get; set; }
        [Column("NAME")] public string Name { get; set; } = string.Empty;
        [Column("DESCRIPTION")] public string? Description { get; set; }
        [Column("DIFFICULTY")] public string? Difficulty { get; set; }
    }

    [Table("PARK_AREAS")]
    public class Pool
    {
        [Key][Column("AREA_ID")] public int PoolId { get; set; }
        [Column("NAME")] public string Name { get; set; } = string.Empty;
        [Column("CAPACITY")] public int Capacity { get; set; }
        [Column("INDOORS")] public string Indoors { get; set; } = "N";
        [NotMapped] public decimal DepthMin { get; set; }
        [NotMapped] public decimal DepthMax { get; set; }
        [NotMapped] public string? Notes { get; set; }
    }

    [Table("OPERATIONAL_LOGS")]
    public class MaintenanceRecord
    {
        [Key][Column("LOG_ID")] public int MaintenanceId { get; set; }
        [Column("LOG_TIMESTAMP")] public DateTime ReportDate { get; set; } = DateTime.Now;
        [Column("DESCRIPTION")] public string? ProblemDescription { get; set; }
        [Column("LOG_TYPE")] public string LogType { get; set; } = "MAINTENANCE";

        [Column("RELATED_TABLE")] public string? RelatedTable { get; set; }
        [Column("RELATED_ID")] public int? RelatedId { get; set; }
        [Column("STAFF_STAFF_ID")] public int? ReportedBy { get; set; }

        [ForeignKey("ReportedBy")] public virtual Staff? Staff { get; set; }

        [NotMapped] public int AttractionId { get; set; }
        [NotMapped] public virtual Attraction? Attraction { get; set; }
        [NotMapped] public string? ActionTaken { get; set; }
        [NotMapped] public DateTime? CompletedDate { get; set; }
        [NotMapped] public decimal Cost { get; set; }
    }

    [Table("PAYMENTS")]
    public class Payment
    {
        [Key][Column("PAYMENT_ID")] public int PaymentId { get; set; }
        [Column("AMOUNT")] public decimal Amount { get; set; }
        [Column("PAYMENT_METHOD")] public string? PaymentMethod { get; set; }
        [NotMapped] public int? BookingId { get; set; }
        [NotMapped] public virtual Booking? Booking { get; set; }
    }
}