using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaParkManager.Models
{
    public class AquaParkContext : DbContext
    {
        // --- HR & Lidé ---
        public DbSet<User> Users { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<StaffRole> StaffRoles { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<StaffShift> StaffShifts { get; set; }
        public DbSet<Certification> Certifications { get; set; }
        public DbSet<Training> Training { get; set; }

        // --- SALES & Zákazníci ---
        public DbSet<Visitor> Visitors { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingItem> BookingItems { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
        public DbSet<PriceList> PriceLists { get; set; }
        public DbSet<PriceListItem> PriceListItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<LoyaltyPoint> LoyaltyPoints { get; set; }
        public DbSet<Waiver> Waivers { get; set; }
        public DbSet<ConcessionSale> ConcessionSales { get; set; }

        // --- FACILITIES & Logistika ---
        public DbSet<Pool> Pools { get; set; }
        public DbSet<Attraction> Attractions { get; set; }
        public DbSet<SlideType> SlideTypes { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<InventoryItem> Inventory { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Concession> Concessions { get; set; }
        public DbSet<SafetyStandard> SafetyStandards { get; set; }
        public DbSet<AttractionSafety> AttractionSafety { get; set; }
        public DbSet<MaintenancePart> MaintenanceParts { get; set; }
        public DbSet<WeatherCondition> WeatherConditions { get; set; }

        // --- SHARED ---
        public DbSet<Address> Address { get; set; }
        public DbSet<PostalCode> PostalCodes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // !!! ZDE SI UPRAVTE HESLO A PØIPOJENÍ !!!
            optionsBuilder.UseOracle("User Id=st72504;Password=Sejmutvojihoe106;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=fei-sql3.upceucebny.cz)(PORT=1521))(CONNECT_DATA=(SID=BDAS)))");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("USERS");
            modelBuilder.Entity<Staff>().ToTable("STAFF");
            modelBuilder.Entity<Role>().ToTable("ROLES");
            modelBuilder.Entity<StaffRole>().ToTable("STAFF_ROLES");
            modelBuilder.Entity<Shift>().ToTable("SHIFTS");
            modelBuilder.Entity<StaffShift>().ToTable("STAFF_SHIFTS");
            modelBuilder.Entity<Certification>().ToTable("CERTIFICATIONS");
            modelBuilder.Entity<Training>().ToTable("TRAINING");

            modelBuilder.Entity<Visitor>().ToTable("VISITORS");
            modelBuilder.Entity<Membership>().ToTable("MEMBERSHIPS");
            modelBuilder.Entity<Booking>().ToTable("BOOKINGS");
            modelBuilder.Entity<BookingItem>().ToTable("BOOKING_ITEMS");
            modelBuilder.Entity<TicketType>().ToTable("TICKET_TYPES");
            modelBuilder.Entity<PriceList>().ToTable("PRICE_LIST");
            modelBuilder.Entity<PriceListItem>().ToTable("PRICE_LIST_ITEMS");
            modelBuilder.Entity<Payment>().ToTable("PAYMENTS");
            modelBuilder.Entity<LoyaltyPoint>().ToTable("LOYALTY_POINTS");
            modelBuilder.Entity<Waiver>().ToTable("WAIVERS");
            modelBuilder.Entity<ConcessionSale>().ToTable("CONCESSION_SALES");

            modelBuilder.Entity<Pool>().ToTable("PARK_AREAS");
            modelBuilder.Entity<Attraction>().ToTable("ATTRACTIONS");
            modelBuilder.Entity<SlideType>().ToTable("SLIDE_TYPES");
            modelBuilder.Entity<MaintenanceRecord>().ToTable("OPERATIONAL_LOGS");
            modelBuilder.Entity<Media>().ToTable("MEDIA");
            modelBuilder.Entity<InventoryItem>().ToTable("INVENTORY");
            modelBuilder.Entity<Supplier>().ToTable("SUPPLIERS");
            modelBuilder.Entity<Concession>().ToTable("CONCESSIONS");
            modelBuilder.Entity<SafetyStandard>().ToTable("SAFETY_STANDARDS");
            modelBuilder.Entity<AttractionSafety>().ToTable("ATTRACTION_SAFETY_STANDARDS");
            modelBuilder.Entity<MaintenancePart>().ToTable("MAINTENANCE_PARTS");
            modelBuilder.Entity<WeatherCondition>().ToTable("WEATHER_CONDITIONS");

            modelBuilder.Entity<Address>().ToTable("ADDRESS");
            modelBuilder.Entity<PostalCode>().ToTable("POSTAL_CODES");
        }

        public override int SaveChanges() { return base.SaveChanges(); }
    }

    // --- Definice Entit ---
    public abstract class AuditableEntity
    {
        [Column("CREATED_AT")] public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Column("UPDATED_AT")] public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }

    [Table("USERS")] public class User : AuditableEntity { [Key][Column("USER_ID")] public int UserId { get; set; } [Column("USERNAME")] public string Username { get; set; } = ""; [Column("PASSWORD_HASH")] public string PasswordHash { get; set; } = ""; [Column("STAFF_STAFF_ID")] public int? StaffId { get; set; } }
    [Table("ADDRESS")] public class Address : AuditableEntity { [Key][Column("ADDRESS_ID")] public int AddressId { get; set; } [Column("STREET")] public string? Street { get; set; } [Column("HOUSE_NUMBER")] public string HouseNumber { get; set; } = ""; [Column("POSTAL_CODES_POSTAL_CODE_ID")] public int PostalCodeId { get; set; } [ForeignKey("PostalCodeId")] public virtual PostalCode? PostalCode { get; set; } }
    [Table("POSTAL_CODES")] public class PostalCode : AuditableEntity { [Key][Column("POSTAL_CODE_ID")] public int PostalCodeId { get; set; } [Column("CITY")] public string City { get; set; } = ""; [Column("POSTAL_CODE")] public string Code { get; set; } = ""; [Column("REGION")] public string Region { get; set; } = ""; [Column("COUNTRY")] public string Country { get; set; } = ""; }

    // HR
    [Table("STAFF")] public class Staff : AuditableEntity { [Key][Column("STAFF_ID")] public int StaffId { get; set; } [Column("FIRST_NAME")] public string FirstName { get; set; } = ""; [Column("LAST_NAME")] public string LastName { get; set; } = ""; [Column("EMAIL")] public string? Email { get; set; } [Column("ADDRESS_ADDRESS_ID")] public int AddressId { get; set; } [ForeignKey("AddressId")] public virtual Address? Address { get; set; } [NotMapped] public string FullName => $"{FirstName} {LastName}"; }
    [Table("ROLES")] public class Role : AuditableEntity { [Key][Column("ROLE_ID")] public int RoleId { get; set; } [Column("ROLE_NAME")] public string RoleName { get; set; } = ""; }
    [Table("STAFF_ROLES")] public class StaffRole : AuditableEntity { [Key][Column("STAFF_ROLE_ID")] public int StaffRoleId { get; set; } [Column("STAFF_STAFF_ID")] public int StaffId { get; set; } [Column("ROLES_ROLE_ID")] public int RoleId { get; set; } }
    [Table("SHIFTS")] public class Shift : AuditableEntity { [Key][Column("SHIFT_ID")] public int ShiftId { get; set; } [Column("SHIFT_NAME")] public string? ShiftName { get; set; } [Column("START_TIME")] public DateTime StartTime { get; set; } [Column("END_TIME")] public DateTime EndTime { get; set; } [NotMapped] public string TimeRange => $"{StartTime:HH:mm}-{EndTime:HH:mm}"; }
    [Table("STAFF_SHIFTS")] public class StaffShift : AuditableEntity { [Key][Column("STAFF_SHIFT_ID")] public int StaffShiftId { get; set; } [Column("STAFF_STAFF_ID")] public int StaffId { get; set; } [Column("SHIFTS_SHIFT_ID")] public int ShiftId { get; set; } [Column("SHIFT_DATE")] public DateTime ShiftDate { get; set; } [ForeignKey("StaffId")] public virtual Staff? Staff { get; set; } [ForeignKey("ShiftId")] public virtual Shift? Shift { get; set; } }
    [Table("CERTIFICATIONS")] public class Certification : AuditableEntity { [Key][Column("CERT_ID")] public int CertId { get; set; } [Column("STAFF_STAFF_ID")] public int StaffId { get; set; } [Column("CERT_NAME")] public string CertName { get; set; } = ""; [Column("EXPIRY_DATE")] public DateTime? ExpiryDate { get; set; } [ForeignKey("StaffId")] public virtual Staff? Staff { get; set; } }
    [Table("TRAINING")] public class Training : AuditableEntity { [Key][Column("TRAINING_ID")] public int TrainingId { get; set; } [Column("STAFF_STAFF_ID")] public int StaffId { get; set; } [Column("TRAINING_NAME")] public string TrainingName { get; set; } = ""; [ForeignKey("StaffId")] public virtual Staff? Staff { get; set; } }

    // SALES
    [Table("VISITORS")] public class Visitor : AuditableEntity { [Key][Column("VISITOR_ID")] public int VisitorId { get; set; } [Column("FIRST_NAME")] public string? FirstName { get; set; } [Column("LAST_NAME")] public string? LastName { get; set; } [Column("ADDRESS_ADDRESS_ID")] public int AddressId { get; set; } [ForeignKey("AddressId")] public virtual Address? Address { get; set; } [NotMapped] public string FullName => $"{FirstName} {LastName}"; }
    [Table("BOOKINGS")] public class Booking : AuditableEntity { [Key][Column("BOOKING_ID")] public int BookingId { get; set; } [Column("BOOKING_REF")] public string? BookingRef { get; set; } [Column("CREATED_DATE")] public DateTime CreatedDate { get; set; } = DateTime.Now; [Column("TOTAL_AMOUNT")] public decimal TotalAmount { get; set; } [Column("STATUS")] public string Status { get; set; } = "CONFIRMED"; [Column("VISITORS_VISITOR_ID")] public int VisitorId { get; set; } [ForeignKey("VisitorId")] public virtual Visitor? Visitor { get; set; } [NotMapped] public string? CustomerName => Visitor?.FullName; }
    [Table("BOOKING_ITEMS")] public class BookingItem : AuditableEntity { [Key][Column("BOOKING_ITEM_ID")] public int BookingItemId { get; set; } [Column("BOOKINGS_BOOKING_ID")] public int BookingId { get; set; } [Column("QUANTITY")] public int Quantity { get; set; } [Column("UNIT_PRICE")] public decimal UnitPrice { get; set; } [Column("TICKET_TYPES_TICKET_TYPE_ID")] public int TicketTypeId { get; set; } [ForeignKey("TicketTypeId")] public virtual TicketType? TicketType { get; set; } }
    [Table("TICKET_TYPES")] public class TicketType : AuditableEntity { [Key][Column("TICKET_TYPE_ID")] public int TicketTypeId { get; set; } [Column("NAME")] public string Name { get; set; } = ""; }
    [Table("PRICE_LIST")] public class PriceList : AuditableEntity { [Key][Column("PRICE_ID")] public int PriceId { get; set; } [Column("NAME")] public string Name { get; set; } = ""; [Column("VALID_FROM")] public DateTime ValidFrom { get; set; } }
    [Table("PRICE_LIST_ITEMS")] public class PriceListItem : AuditableEntity { [Key][Column("PRICE_LIST_ITEM_ID")] public int PriceListItemId { get; set; } [Column("PRICE_LIST_PRICE_ID")] public int PriceListId { get; set; } [Column("TICKET_TYPES_TICKET_TYPE_ID")] public int TicketTypeId { get; set; } [Column("UNIT_PRICE")] public decimal UnitPrice { get; set; } [Column("VALID_FROM")] public DateTime ValidFrom { get; set; } = DateTime.Now; [Column("VALID_TO")] public DateTime? ValidTo { get; set; } [ForeignKey("PriceListId")] public virtual PriceList? PriceList { get; set; } [ForeignKey("TicketTypeId")] public virtual TicketType? TicketType { get; set; } }
    [Table("PAYMENTS")] public class Payment : AuditableEntity { [Key][Column("PAYMENT_ID")] public int PaymentId { get; set; } [Column("AMOUNT")] public decimal Amount { get; set; } [Column("PAYMENT_METHOD")] public string? PaymentMethod { get; set; } [Column("BOOKINGS_BOOKING_ID")] public int? BookingId { get; set; } }
    [Table("MEMBERSHIPS")] public class Membership : AuditableEntity { [Key][Column("MEMBERSHIP_ID")] public int MembershipId { get; set; } [Column("VISITORS_VISITOR_ID")] public int VisitorId { get; set; } [Column("MEMBERSHIP_TYPE")] public string? MembershipType { get; set; } [Column("START_DATE")] public DateTime StartDate { get; set; } [Column("END_DATE")] public DateTime? EndDate { get; set; } [Column("STATUS")] public string Status { get; set; } = "ACTIVE"; [ForeignKey("VisitorId")] public virtual Visitor? Visitor { get; set; } }
    [Table("LOYALTY_POINTS")] public class LoyaltyPoint : AuditableEntity { [Key][Column("POINTS_ID")] public int PointsId { get; set; } [Column("VISITORS_VISITOR_ID")] public int VisitorId { get; set; } [Column("POINTS_EARNED")] public int Earned { get; set; } [Column("TIER")] public string Tier { get; set; } = "BRONZE"; [ForeignKey("VisitorId")] public virtual Visitor? Visitor { get; set; } }
    [Table("WAIVERS")] public class Waiver : AuditableEntity { [Key][Column("WAIVER_ID")] public int WaiverId { get; set; } [Column("VISITORS_VISITOR_ID")] public int VisitorId { get; set; } [Column("WAIVER_TYPE")] public string WaiverType { get; set; } = ""; [Column("SIGNED_DATE")] public DateTime SignedDate { get; set; } [ForeignKey("VisitorId")] public virtual Visitor? Visitor { get; set; } }
    [Table("CONCESSION_SALES")] public class ConcessionSale : AuditableEntity { [Key][Column("SALE_ID")] public int SaleId { get; set; } [Column("CONCESSIONS_CONCESSION_ID")] public int ConcessionId { get; set; } [Column("AMOUNT")] public decimal Amount { get; set; } [Column("SALE_DATE")] public DateTime SaleDate { get; set; } }

    // FACILITIES
    [Table("PARK_AREAS")] public class Pool : AuditableEntity { [Key][Column("AREA_ID")] public int PoolId { get; set; } [Column("NAME")] public string Name { get; set; } = ""; [Column("DESCRIPTION")] public string? Notes { get; set; } }
    [Table("ATTRACTIONS")] public class Attraction : AuditableEntity { [Key][Column("ATTRACTION_ID")] public int AttractionId { get; set; } [Column("NAME")] public string Name { get; set; } = ""; [Column("STATUS")] public string Status { get; set; } = "OPEN"; [Column("PARK_AREAS_AREA_ID")] public int AreaId { get; set; } [ForeignKey("AreaId")] public virtual Pool? Pool { get; set; } [Column("SLIDE_TYPES_SLIDE_TYPE_ID")] public int? SlideTypeId { get; set; } [ForeignKey("SlideTypeId")] public virtual SlideType? SlideType { get; set; } }
    [Table("SLIDE_TYPES")] public class SlideType : AuditableEntity { [Key][Column("SLIDE_TYPE_ID")] public int SlideTypeId { get; set; } [Column("NAME")] public string Name { get; set; } = ""; }
    [Table("OPERATIONAL_LOGS")] public class MaintenanceRecord : AuditableEntity { [Key][Column("LOG_ID")] public int MaintenanceId { get; set; } [Column("LOG_TIMESTAMP")] public DateTime ReportDate { get; set; } = DateTime.Now; [Column("DESCRIPTION")] public string? ProblemDescription { get; set; } [Column("LOG_TYPE")] public string LogType { get; set; } = "MAINTENANCE"; [Column("RELATED_TABLE")] public string? RelatedTable { get; set; } [Column("RELATED_ID")] public int? RelatedId { get; set; } [Column("STAFF_STAFF_ID")] public int? ReportedBy { get; set; } [ForeignKey("ReportedBy")] public virtual Staff? Staff { get; set; } }
    [Table("MEDIA")] public class Media : AuditableEntity { [Key][Column("MEDIA_ID")] public int MediaId { get; set; } [Column("FILE_NAME")] public string? FileName { get; set; } [Column("MEDIA_DATA")] public byte[]? MediaData { get; set; } [Column("RELATED_TABLE")] public string RelatedTable { get; set; } = "ATTRACTIONS"; [Column("RELATED_ID")] public int RelatedId { get; set; } }
    [Table("SUPPLIERS")] public class Supplier : AuditableEntity { [Key][Column("SUPPLIER_ID")] public int SupplierId { get; set; } [Column("NAME")] public string Name { get; set; } = ""; [Column("ADDRESS_ADDRESS_ID")] public int AddressId { get; set; } [ForeignKey("AddressId")] public virtual Address? Address { get; set; } }
    [Table("INVENTORY")] public class InventoryItem : AuditableEntity { [Key][Column("ITEM_ID")] public int ItemId { get; set; } [Column("NAME")] public string Name { get; set; } = ""; [Column("QUANTITY")] public int Quantity { get; set; } [Column("SUPPLIERS_SUPPLIER_ID")] public int? SupplierId { get; set; } [ForeignKey("SupplierId")] public virtual Supplier? Supplier { get; set; } }
    [Table("CONCESSIONS")] public class Concession : AuditableEntity { [Key][Column("CONCESSION_ID")] public int ConcessionId { get; set; } [Column("NAME")] public string Name { get; set; } = ""; [Column("STATUS")] public string Status { get; set; } = "OPEN"; [Column("PARK_AREAS_AREA_ID")] public int AreaId { get; set; } }
    [Table("SAFETY_STANDARDS")] public class SafetyStandard : AuditableEntity { [Key][Column("STANDARD_ID")] public int StandardId { get; set; } [Column("NAME")] public string Name { get; set; } = ""; [Column("DESCRIPTION")] public string? Description { get; set; } }
    [Table("ATTRACTION_SAFETY_STANDARDS")] public class AttractionSafety : AuditableEntity { [Key][Column("ASSOC_ID")] public int AssocId { get; set; } [Column("ATTRACTIONS_ATTRACTION_ID")] public int AttractionId { get; set; } [Column("SAFETY_STANDARDS_STANDARD_ID")] public int StandardId { get; set; } [ForeignKey("AttractionId")] public virtual Attraction? Attraction { get; set; } [ForeignKey("StandardId")] public virtual SafetyStandard? Standard { get; set; } }
    [Table("MAINTENANCE_PARTS")] public class MaintenancePart : AuditableEntity { [Key][Column("PART_ID")] public int PartId { get; set; } [Column("NAME")] public string Name { get; set; } = ""; [Column("COST")] public decimal Cost { get; set; } }
    [Table("WEATHER_CONDITIONS")] public class WeatherCondition : AuditableEntity { [Key][Column("WEATHER_ID")] public int WeatherId { get; set; } [Column("RECORD_DATE")] public DateTime RecordDate { get; set; } [Column("TEMPERATURE")] public decimal Temperature { get; set; } }
}