using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace AquaParkManager.Models
{
    public class AquaParkContext : DbContext
    {
        // Původní tabulky
        public DbSet<User> Users { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<StaffRole> StaffRoles { get; set; }
        public DbSet<Visitor> Visitors { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingItem> BookingItems { get; set; }
        public DbSet<Attraction> Attractions { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
        public DbSet<PriceList> PriceLists { get; set; }
        public DbSet<SlideType> SlideTypes { get; set; }
        public DbSet<Pool> Pools { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Address> Address { get; set; }
        public DbSet<PostalCode> PostalCodes { get; set; }
        public DbSet<Certification> Certifications { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<StaffShift> StaffShifts { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<Concession> Concessions { get; set; }
        public DbSet<ConcessionSale> ConcessionSales { get; set; }
        public DbSet<MedicalInfo> MedicalInfo { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<LoyaltyPoints> LoyaltyPoints { get; set; }
        public DbSet<OccupancyTracking> OccupancyTracking { get; set; }
        public DbSet<SafetyStandard> SafetyStandards { get; set; }
        public DbSet<AttractionSafetyStandard> AttractionSafetyStandards { get; set; }
        public DbSet<MaintenancePart> MaintenanceParts { get; set; }
        public DbSet<MaintenancePartCompat> MaintenancePartCompats { get; set; }
        public DbSet<PriceListItem> PriceListItems { get; set; }
        public DbSet<PriceListRule> PriceListRules { get; set; }
        public DbSet<PricingRule> PricingRules { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Training> Trainings { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Waiver> Waivers { get; set; }
        public DbSet<WeatherCondition> WeatherConditions { get; set; }
        public DbSet<BookingOverview> BookingOverviews { get; set; }
        public DbSet<DbObject> DbObjects { get; set; }
        public DbSet<AreaHierarchy> AreaHierarchies { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!string.IsNullOrWhiteSpace(App.ConnectionString))
            {
                optionsBuilder.UseOracle(App.ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mapování tabulek
            modelBuilder.Entity<User>().ToTable("USERS");
            modelBuilder.Entity<Staff>().ToTable("STAFF");
            modelBuilder.Entity<Role>().ToTable("ROLES");
            modelBuilder.Entity<StaffRole>().ToTable("STAFF_ROLES");
            modelBuilder.Entity<Visitor>().ToTable("VISITORS");
            modelBuilder.Entity<Booking>().ToTable("BOOKINGS");
            modelBuilder.Entity<BookingItem>().ToTable("BOOKING_ITEMS");
            modelBuilder.Entity<Attraction>().ToTable("ATTRACTIONS");
            modelBuilder.Entity<Media>().ToTable("MEDIA");
            modelBuilder.Entity<TicketType>().ToTable("TICKET_TYPES");
            modelBuilder.Entity<PriceList>().ToTable("PRICE_LIST");
            modelBuilder.Entity<SlideType>().ToTable("SLIDE_TYPES");
            modelBuilder.Entity<Pool>().ToTable("PARK_AREAS");
            modelBuilder.Entity<MaintenanceRecord>().ToTable("OPERATIONAL_LOGS");
            modelBuilder.Entity<Payment>().ToTable("PAYMENTS");
            modelBuilder.Entity<Address>().ToTable("ADDRESS");
            modelBuilder.Entity<PostalCode>().ToTable("POSTAL_CODES");
            modelBuilder.Entity<Certification>().ToTable("CERTIFICATIONS");
            modelBuilder.Entity<Shift>().ToTable("SHIFTS");
            modelBuilder.Entity<StaffShift>().ToTable("STAFF_SHIFTS");
            modelBuilder.Entity<Inventory>().ToTable("INVENTORY");
            modelBuilder.Entity<Concession>().ToTable("CONCESSIONS");
            modelBuilder.Entity<ConcessionSale>().ToTable("CONCESSION_SALES");
            modelBuilder.Entity<MedicalInfo>().ToTable("MEDICAL_INFO");
            modelBuilder.Entity<Membership>().ToTable("MEMBERSHIPS");
            modelBuilder.Entity<LoyaltyPoints>().ToTable("LOYALTY_POINTS");
            modelBuilder.Entity<OccupancyTracking>().ToTable("OCCUPANCY_TRACKING");
            modelBuilder.Entity<SafetyStandard>().ToTable("SAFETY_STANDARDS");
            modelBuilder.Entity<AttractionSafetyStandard>().ToTable("ATTRACTION_SAFETY_STANDARDS");
            modelBuilder.Entity<MaintenancePart>().ToTable("MAINTENANCE_PARTS");
            modelBuilder.Entity<MaintenancePartCompat>().ToTable("MAINTENANCE_PART_COMPAT");
            modelBuilder.Entity<PriceListItem>().ToTable("PRICE_LIST_ITEMS");
            modelBuilder.Entity<PriceListRule>().ToTable("PRICE_LIST_RULES");
            modelBuilder.Entity<PricingRule>().ToTable("PRICING_RULES");
            modelBuilder.Entity<Supplier>().ToTable("SUPPLIERS");
            modelBuilder.Entity<Training>().ToTable("TRAINING");
            modelBuilder.Entity<UserRole>().ToTable("USER_ROLES");
            modelBuilder.Entity<Waiver>().ToTable("WAIVERS");
            modelBuilder.Entity<WeatherCondition>().ToTable("WEATHER_CONDITIONS");
            modelBuilder.Entity<BookingOverview>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("V_BOOKING_OVERVIEW");
            });

            modelBuilder.Entity<DbObject>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("V_DB_OBJECTS");
            });

            modelBuilder.Entity<AreaHierarchy>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("V_AREA_HIERARCHY");
            });
        }
    


public override int SaveChanges()
        {
            // Ponech�no beze zm�ny (Audit log logic)
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            int? currentUserId = App.CurrentUser?.UserId;

            foreach (var entityEntry in entries)
            {
                if (entityEntry.State == EntityState.Added)
                {
                    var createdAtProp = entityEntry.Entity.GetType().GetProperty("CreatedAt");
                    if (createdAtProp != null && createdAtProp.CanWrite)
                        createdAtProp.SetValue(entityEntry.Entity, DateTime.Now);

                    if (currentUserId.HasValue)
                    {
                        var createdByProp = entityEntry.Entity.GetType().GetProperty("CreatedByUserId");
                        if (createdByProp != null && createdByProp.CanWrite)
                            createdByProp.SetValue(entityEntry.Entity, currentUserId.Value);
                    }
                }

                var updatedAtProp = entityEntry.Entity.GetType().GetProperty("UpdatedAt");
                if (updatedAtProp != null && updatedAtProp.CanWrite)
                    updatedAtProp.SetValue(entityEntry.Entity, DateTime.Now);

                if (currentUserId.HasValue)
                {
                    var updatedByProp = entityEntry.Entity.GetType().GetProperty("UpdatedByUserId");
                    if (updatedByProp != null && updatedByProp.CanWrite)
                        updatedByProp.SetValue(entityEntry.Entity, currentUserId.Value);
                }
            }

            return base.SaveChanges();
        }
    }

    public abstract class AuditableEntity
    {
        [Column("CREATED_AT")] public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Column("CREATED_BY_USER_ID")] public int? CreatedByUserId { get; set; }
        [Column("UPDATED_AT")] public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        [Column("UPDATED_BY_USER_ID")] public int? UpdatedByUserId { get; set; }
    }

    // --- Zbytek entit beze zm�n, pouze Attraction a MaintenanceRecord jsou o�ezan� ---

    [Table("USERS")]
    public class User : AuditableEntity
    {
        [Key][Column("USER_ID")] public int UserId { get; set; }
        [Column("USERNAME")] public string Username { get; set; } = string.Empty;
        [Column("PASSWORD_HASH")] public string PasswordHash { get; set; } = string.Empty;
        [Column("EMAIL")] public string Email { get; set; } = string.Empty;
        [Column("IS_ACTIVE")] public string IsActive { get; set; } = "Y";
        [Column("STAFF_STAFF_ID")] public int? StaffId { get; set; }
        [Column("VISITORS_VISITOR_ID")] public int? VisitorId { get; set; }
    }

    [Table("ADDRESS")]
    public class Address : AuditableEntity
    {
        [Key][Column("ADDRESS_ID")] public int AddressId { get; set; }
        [Column("STREET")] public string? Street { get; set; }
        [Column("HOUSE_NUMBER")] public string HouseNumber { get; set; } = string.Empty;
        [Column("POSTAL_CODES_POSTAL_CODE_ID")] public int PostalCodeId { get; set; }
        [ForeignKey("PostalCodeId")] public virtual PostalCode? PostalCode { get; set; }
    }

    [Table("POSTAL_CODES")]
    public class PostalCode : AuditableEntity
    {
        [Key][Column("POSTAL_CODE_ID")] public int PostalCodeId { get; set; }
        [Column("CITY")] public string City { get; set; } = string.Empty;
        [Column("POSTAL_CODE")] public string Code { get; set; } = string.Empty;
        [Column("REGION")] public string Region { get; set; } = string.Empty;
        [Column("COUNTRY")] public string Country { get; set; } = string.Empty;
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
        [Column("HIRE_DATE")] public DateTime HireDate { get; set; } = DateTime.Now;
        [Column("NOTES")] public string? Notes { get; set; }
        [Column("ACTIVE")] public string Active { get; set; } = "Y";

        [Column("ADDRESS_ADDRESS_ID")] public int AddressId { get; set; }
        [ForeignKey("AddressId")] public virtual Address? Address { get; set; }

        [NotMapped] public string FullName => $"{FirstName} {LastName}";
    }

    [Table("ROLES")]
    public class Role : AuditableEntity
    {
        [Key][Column("ROLE_ID")] public int RoleId { get; set; }
        [Column("ROLE_NAME")] public string RoleName { get; set; } = string.Empty;
        [Column("DESCRIPTION")] public string? Description { get; set; }
    }

    [Table("STAFF_ROLES")]
    public class StaffRole : AuditableEntity
    {
        [Key][Column("STAFF_ROLE_ID")] public int StaffRoleId { get; set; }
        [Column("STAFF_STAFF_ID")] public int StaffId { get; set; }
        [Column("ROLES_ROLE_ID")] public int RoleId { get; set; }
        [Column("ASSIGNED_DATE")] public DateTime AssignedDate { get; set; } = DateTime.Now;
    }

    [Table("VISITORS")]
    public class Visitor : AuditableEntity
    {
        [Key][Column("VISITOR_ID")] public int VisitorId { get; set; }
        [Column("FIRST_NAME")] public string? FirstName { get; set; }
        [Column("LAST_NAME")] public string? LastName { get; set; }
        [Column("DATE_OF_BIRTH")] public DateTime? DateOfBirth { get; set; }
        [Column("EMAIL")] public string? Email { get; set; }
        [Column("PHONE")] public string? Phone { get; set; }
        [Column("NOTES")] public string? Notes { get; set; }
        [Column("ADDRESS_ADDRESS_ID")] public int AddressId { get; set; }
        [Column("MEDICAL_INFO_MEDICAL_INFO_ID")] public int? MedicalInfoId { get; set; }
        [ForeignKey("AddressId")] public virtual Address? Address { get; set; }
        [ForeignKey("MedicalInfoId")] public virtual MedicalInfo? MedicalInfo { get; set; }
        [NotMapped] public string FullName => $"{FirstName} {LastName}";
    }

    [Table("BOOKINGS")]
    public class Booking : AuditableEntity
    {
        [Key][Column("BOOKING_ID")] public int BookingId { get; set; }
        [Column("BOOKING_REF")] public string? BookingRef { get; set; }
        [Column("CREATED_DATE")] public DateTime CreatedDate { get; set; } = DateTime.Now;
        [Column("TOTAL_AMOUNT")] public decimal TotalAmount { get; set; } = 0;
        [Column("STATUS")] public string Status { get; set; } = "CONFIRMED";
        [Column("NOTES")] public string? Notes { get; set; }
        [Column("VISITORS_VISITOR_ID")] public int VisitorId { get; set; }
        [ForeignKey("VisitorId")] public virtual Visitor? Visitor { get; set; }
        [NotMapped] public string? CustomerName => Visitor?.FullName;
    }

    [Table("BOOKING_ITEMS")]
    public class BookingItem : AuditableEntity
    {
        [Key][Column("BOOKING_ITEM_ID")] public int BookingItemId { get; set; }
        [Column("BOOKINGS_BOOKING_ID")] public int BookingId { get; set; }
        [Column("QUANTITY")] public int Quantity { get; set; } = 1;
        [Column("UNIT_PRICE")] public decimal UnitPrice { get; set; } = 0;
        [Column("TICKET_TYPES_TICKET_TYPE_ID")] public int TicketTypeId { get; set; }
        [ForeignKey("TicketTypeId")] public virtual TicketType? TicketType { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("LINE_TOTAL")] public decimal LineTotal { get; set; }
    }

    [Table("ATTRACTIONS")]
    public class Attraction : AuditableEntity
    {
        [Key][Column("ATTRACTION_ID")] public int AttractionId { get; set; }
        [Column("NAME")] public string Name { get; set; } = string.Empty;
        [Column("STATUS")] public string Status { get; set; } = "OPEN";

        [Column("SLIDE_TYPES_SLIDE_TYPE_ID")] public int? SlideTypeId { get; set; }
        [ForeignKey("SlideTypeId")] public virtual SlideType? SlideType { get; set; }

        [Column("PARK_AREAS_AREA_ID")] public int AreaId { get; set; }
        [ForeignKey("AreaId")] public virtual Pool? Pool { get; set; }

        // Odstran�ny neexistuj�c� sloupce (Capacity, Notes, Dimensions...)
    }

    [Table("MEDIA")]
    public class Media : AuditableEntity
    {
        [Key][Column("MEDIA_ID")] public int MediaId { get; set; }
        [Column("FILE_NAME")] public string? FileName { get; set; }
        [Column("MEDIA_DATA")] public byte[]? MediaData { get; set; }
        [Column("MIME_TYPE")] public string? MimeType { get; set; }
        [Column("RELATED_TABLE")] public string RelatedTable { get; set; } = "ATTRACTIONS";
        [Column("RELATED_ID")] public int RelatedId { get; set; }
    }

    [Table("TICKET_TYPES")]
    public class TicketType : AuditableEntity
    {
        [Key][Column("TICKET_TYPE_ID")] public int TicketTypeId { get; set; }
        [Column("NAME")] public string Name { get; set; } = string.Empty;
        [Column("DESCRIPTION")] public string? Description { get; set; }
        [Column("AGE_FROM")] public int? AgeMin { get; set; }
        [Column("AGE_TO")] public int? AgeMax { get; set; }
    }

    [Table("PRICE_LIST")]
    public class PriceList : AuditableEntity
    {
        [Key][Column("PRICE_ID")] public int PriceId { get; set; }
        [Column("NAME")] public string Name { get; set; } = string.Empty;
        [Column("BASE_PRICE")] public decimal BasePrice { get; set; }
        [Column("DESCRIPTION")] public string? Description { get; set; }
        [Column("VALID_FROM")] public DateTime ValidFrom { get; set; } = DateTime.Now;
    }

    [Table("SLIDE_TYPES")]
    public class SlideType : AuditableEntity
    {
        [Key][Column("SLIDE_TYPE_ID")] public int SlideTypeId { get; set; }
        [Column("NAME")] public string Name { get; set; } = string.Empty;
        [Column("DESCRIPTION")] public string? Description { get; set; }
        [Column("DIFFICULTY")] public string? Difficulty { get; set; }
    }

    [Table("PARK_AREAS")]
    public class Pool : AuditableEntity
    {
        [Key][Column("AREA_ID")] public int PoolId { get; set; }
        [Column("NAME")] public string Name { get; set; } = string.Empty;
        [Column("CAPACITY")] public int Capacity { get; set; }
        [Column("INDOORS")] public string Indoors { get; set; } = "N";
        [Column("DESCRIPTION")] public string? Notes { get; set; }
    }

    [Table("OPERATIONAL_LOGS")]
    public class MaintenanceRecord : AuditableEntity
    {
        [Key][Column("LOG_ID")] public int MaintenanceId { get; set; }
        [Column("LOG_TIMESTAMP")] public DateTime ReportDate { get; set; } = DateTime.Now;
        [Column("DESCRIPTION")] public string? ProblemDescription { get; set; }
        [Column("LOG_TYPE")] public string LogType { get; set; } = "MAINTENANCE";

        [Column("RELATED_TABLE")] public string? RelatedTable { get; set; }
        [Column("RELATED_ID")] public int? RelatedId { get; set; }
        [Column("STAFF_STAFF_ID")] public int? ReportedBy { get; set; }

        [ForeignKey("ReportedBy")] public virtual Staff? Staff { get; set; }

        // Odstran�ny neexistuj�c� sloupce (Cost, ActionTaken, CompletedDate)
    }

    [Table("PAYMENTS")]
    public class Payment : AuditableEntity
    {
        [Key][Column("PAYMENT_ID")] public int PaymentId { get; set; }
        [Column("AMOUNT")] public decimal Amount { get; set; }
        [Column("PAYMENT_METHOD")] public string? PaymentMethod { get; set; }
        [Column("BOOKINGS_BOOKING_ID")] public int? BookingId { get; set; }
        [Column("STATUS")] public string Status { get; set; } = "COMPLETED";
        [Column("REFUND_AMOUNT")] public decimal RefundAmount { get; set; } = 0;
        [Column("REFUND_REASON")] public string? RefundReason { get; set; }
        [Column("REFERENCE")] public string? Reference { get; set; }
        [ForeignKey("BookingId")] public virtual Booking? Booking { get; set; }
    }

    // ==================== Nove Veci ====================

    [Table("CERTIFICATIONS")]
    public class Certification : AuditableEntity
    {
        [Key][Column("CERT_ID")] public int CertId { get; set; }
        [Column("STAFF_STAFF_ID")] public int StaffId { get; set; }
        [Column("CERT_NAME")] public string CertName { get; set; } = string.Empty;
        [Column("ISSUED_BY")] public string? IssuedBy { get; set; }
        [Column("ISSUED_DATE")] public DateTime? IssuedDate { get; set; }
        [Column("EXPIRY_DATE")] public DateTime? ExpiryDate { get; set; }
        [Column("VERIFICATION_STATUS")] public string VerificationStatus { get; set; } = "PENDING";
        [Column("UPLOADED_DOCUMENT")] public byte[]? UploadedDocument { get; set; }
        [Column("DOCUMENT_MIME_TYPE")] public string? DocumentMimeType { get; set; }
        [Column("NOTES")] public string? Notes { get; set; }
        [ForeignKey("StaffId")] public virtual Staff? Staff { get; set; }
        [NotMapped] public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.Now;
        [NotMapped] public bool IsExpiringSoon => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.Now.AddDays(30) && !IsExpired;
    }

    [Table("SHIFTS")]
    public class Shift
    {
        [Key][Column("SHIFT_ID")] public int ShiftId { get; set; }
        [Column("SHIFT_NAME")] public string? ShiftName { get; set; }
        [Column("START_TIME")] public DateTime StartTime { get; set; }
        [Column("END_TIME")] public DateTime EndTime { get; set; }
        [Column("NOTES")] public string? Notes { get; set; }
    }

    [Table("STAFF_SHIFTS")]
    public class StaffShift
    {
        [Key][Column("STAFF_SHIFT_ID")] public int StaffShiftId { get; set; }
        [Column("STAFF_STAFF_ID")] public int StaffId { get; set; }
        [Column("SHIFTS_SHIFT_ID")] public int ShiftId { get; set; }
        [Column("SHIFT_DATE")] public DateTime ShiftDate { get; set; }
        [Column("ASSIGNED_AT")] public DateTime AssignedAt { get; set; } = DateTime.Now;

        [ForeignKey("StaffId")] public virtual Staff? Staff { get; set; }
        [ForeignKey("ShiftId")] public virtual Shift? Shift { get; set; }
    }

    [Table("INVENTORY")]
    public class Inventory : AuditableEntity
    {
        [Key][Column("ITEM_ID")] public int ItemId { get; set; }
        [Column("NAME")] public string Name { get; set; } = string.Empty;
        [Column("SUPPLIERS_SUPPLIER_ID")] public int? SupplierId { get; set; }
        [Column("QUANTITY")] public int Quantity { get; set; } = 0;
        [Column("UNIT_PRICE")] public decimal? UnitPrice { get; set; }
        [Column("CATEGORY")] public string? Category { get; set; }
        [Column("MIN_STOCK")] public int MinStock { get; set; } = 0;
        [Column("NOTES")] public string? Notes { get; set; }
        [ForeignKey("SupplierId")] public virtual Supplier? Supplier { get; set; }
        [NotMapped] public bool IsLowStock => Quantity <= MinStock;
    }

    [Table("SUPPLIERS")]
    public class Supplier : AuditableEntity
    {
        [Key][Column("SUPPLIER_ID")] public int SupplierId { get; set; }
        [Column("NAME")] public string Name { get; set; } = string.Empty;
        [Column("CONTACT_EMAIL")] public string? ContactEmail { get; set; }
        [Column("CONTACT_PHONE")] public string? ContactPhone { get; set; }
        [Column("ADDRESS_ADDRESS_ID")] public int AddressId { get; set; }
        [Column("NOTES")] public string? Notes { get; set; }
        [ForeignKey("AddressId")] public virtual Address? Address { get; set; }
    }

    [Table("CONCESSIONS")]
    public class Concession : AuditableEntity
    {
        [Key][Column("CONCESSION_ID")] public int ConcessionId { get; set; }
        [Column("PARK_AREAS_AREA_ID")] public int AreaId { get; set; }
        [Column("NAME")] public string Name { get; set; } = string.Empty;
        [Column("TYPE")] public string? Type { get; set; }
        [Column("STATUS")] public string Status { get; set; } = "OPEN";
        [Column("NOTES")] public string? Notes { get; set; }
        [ForeignKey("AreaId")] public virtual Pool? Area { get; set; }
    }

    [Table("CONCESSION_SALES")]
    public class ConcessionSale : AuditableEntity
    {
        [Key][Column("SALE_ID")] public int SaleId { get; set; }
        [Column("CONCESSIONS_CONCESSION_ID")] public int ConcessionId { get; set; }
        [Column("BOOKINGS_BOOKING_ID")] public int? BookingId { get; set; }
        [Column("INVENTORY_ITEM_ID")] public int? ItemId { get; set; }
        [Column("QUANTITY")] public int? Quantity { get; set; }
        [Column("SALE_DATE")] public DateTime SaleDate { get; set; } = DateTime.Now;
        [Column("AMOUNT")] public decimal? Amount { get; set; }
        [ForeignKey("ConcessionId")] public virtual Concession? Concession { get; set; }
        [ForeignKey("BookingId")] public virtual Booking? Booking { get; set; }
        [ForeignKey("ItemId")] public virtual Inventory? Item { get; set; }
    }

    [Table("MEDICAL_INFO")]
    public class MedicalInfo : AuditableEntity
    {
        [Key][Column("MEDICAL_INFO_ID")] public int MedicalInfoId { get; set; }
        [Column("MEDICAL_CONDITIONS")] public string? MedicalConditions { get; set; }
        [Column("SWIMMING_ABILITY")] public string SwimmingAbility { get; set; } = string.Empty;
        [Column("EMERGENCY_CONTACT")] public string EmergencyContact { get; set; } = string.Empty;
    }

    [Table("MEMBERSHIPS")]
    public class Membership : AuditableEntity
    {
        [Key][Column("MEMBERSHIP_ID")] public int MembershipId { get; set; }
        [Column("VISITORS_VISITOR_ID")] public int VisitorId { get; set; }
        [Column("MEMBERSHIP_TYPE")] public string? MembershipType { get; set; }
        [Column("START_DATE")] public DateTime StartDate { get; set; } = DateTime.Now;
        [Column("END_DATE")] public DateTime? EndDate { get; set; }
        [Column("RECURRING")] public string Recurring { get; set; } = "N";
        [Column("STATUS")] public string Status { get; set; } = "ACTIVE";
        [ForeignKey("VisitorId")] public virtual Visitor? Visitor { get; set; }
        [NotMapped] public bool IsActive => Status == "ACTIVE" && (!EndDate.HasValue || EndDate.Value > DateTime.Now);
    }

    [Table("LOYALTY_POINTS")]
    public class LoyaltyPoints : AuditableEntity
    {
        [Key][Column("POINTS_ID")] public int PointsId { get; set; }
        [Column("VISITORS_VISITOR_ID")] public int VisitorId { get; set; }
        [Column("POINTS_EARNED")] public int PointsEarned { get; set; } = 0;
        [Column("POINTS_REDEEMED")] public int PointsRedeemed { get; set; } = 0;
        [Column("LAST_ACTIVITY_DATE")] public DateTime? LastActivityDate { get; set; }
        [Column("TIER")] public string Tier { get; set; } = "BRONZE";
        [ForeignKey("VisitorId")] public virtual Visitor? Visitor { get; set; }
        [NotMapped] public int AvailablePoints => PointsEarned - PointsRedeemed;
    }

    [Table("OCCUPANCY_TRACKING")]
    public class OccupancyTracking : AuditableEntity
    {
        [Key][Column("OCCUPANCY_ID")] public int OccupancyId { get; set; }
        [Column("PARK_AREAS_AREA_ID")] public int AreaId { get; set; }
        [Column("RECORDED_TIME")] public DateTime RecordedTime { get; set; } = DateTime.Now;
        [Column("VISITOR_COUNT")] public int VisitorCount { get; set; }
        [Column("CAPACITY_PERCENTAGE")] public int? CapacityPercentage { get; set; }
        [ForeignKey("AreaId")] public virtual Pool? Area { get; set; }
    }

    [Table("SAFETY_STANDARDS")]
    public class SafetyStandard : AuditableEntity
    {
        [Key][Column("STANDARD_ID")] public int StandardId { get; set; }
        [Column("NAME")] public string Name { get; set; } = string.Empty;
        [Column("REGULATORY_BODY")] public string? RegulatoryBody { get; set; }
        [Column("DESCRIPTION")] public string? Description { get; set; }
        [Column("REQUIRED_FREQUENCY")] public string? RequiredFrequency { get; set; }
    }

    [Table("ATTRACTION_SAFETY_STANDARDS")]
    public class AttractionSafetyStandard : AuditableEntity
    {
        [Key][Column("ASSOC_ID")] public int AssocId { get; set; }
        [Column("ATTRACTIONS_ATTRACTION_ID")] public int AttractionId { get; set; }
        [Column("SAFETY_STANDARDS_STANDARD_ID")] public int StandardId { get; set; }
        [Column("NOTES")] public string? Notes { get; set; }
        [ForeignKey("AttractionId")] public virtual Attraction? Attraction { get; set; }
        [ForeignKey("StandardId")] public virtual SafetyStandard? Standard { get; set; }
    }

    [Table("MAINTENANCE_PARTS")]
    public class MaintenancePart : AuditableEntity
    {
        [Key][Column("PART_ID")] public int PartId { get; set; }
        [Column("NAME")] public string Name { get; set; } = string.Empty;
        [Column("PART_NUMBER")] public string? PartNumber { get; set; }
        [Column("SUPPLIERS_SUPPLIER_ID")] public int? SupplierId { get; set; }
        [Column("COST")] public decimal? Cost { get; set; }
        [Column("LEAD_TIME_DAYS")] public int? LeadTimeDays { get; set; }
        [ForeignKey("SupplierId")] public virtual Supplier? Supplier { get; set; }
    }

    [Table("MAINTENANCE_PART_COMPAT")]
    public class MaintenancePartCompat : AuditableEntity
    {
        [Key][Column("COMPAT_ID")] public int CompatId { get; set; }
        [Column("PARTS_PART_ID")] public int PartId { get; set; }
        [Column("ATTRACTIONS_ATTRACTION_ID")] public int AttractionId { get; set; }
        [Column("NOTES")] public string? Notes { get; set; }
        [ForeignKey("PartId")] public virtual MaintenancePart? Part { get; set; }
        [ForeignKey("AttractionId")] public virtual Attraction? Attraction { get; set; }
    }

    [Table("PRICE_LIST_ITEMS")]
    public class PriceListItem : AuditableEntity
    {
        [Key][Column("PRICE_LIST_ITEM_ID")] public int PriceListItemId { get; set; }
        [Column("PRICE_LIST_PRICE_ID")] public int PriceId { get; set; }
        [Column("TICKET_TYPES_TICKET_TYPE_ID")] public int TicketTypeId { get; set; }
        [Column("UNIT_PRICE")] public decimal UnitPrice { get; set; }
        [Column("VALID_FROM")] public DateTime ValidFrom { get; set; } = DateTime.Now;
        [Column("VALID_TO")] public DateTime? ValidTo { get; set; }
        [ForeignKey("PriceId")] public virtual PriceList? PriceList { get; set; }
        [ForeignKey("TicketTypeId")] public virtual TicketType? TicketType { get; set; }
    }

    [Table("PRICING_RULES")]
    public class PricingRule : AuditableEntity
    {
        [Key][Column("RULE_ID")] public int RuleId { get; set; }
        [Column("NAME")] public string Name { get; set; } = string.Empty;
        [Column("RULE_TYPE")] public string? RuleType { get; set; }
        [Column("APPLY_FROM")] public DateTime? ApplyFrom { get; set; }
        [Column("APPLY_TO")] public DateTime? ApplyTo { get; set; }
        [Column("MULTIPLIER")] public decimal Multiplier { get; set; } = 1.00m;
        [Column("CONDITIONS")] public string? Conditions { get; set; }
    }

    [Table("PRICE_LIST_RULES")]
    public class PriceListRule : AuditableEntity
    {
        [Key][Column("PRICE_LIST_RULE_ID")] public int PriceListRuleId { get; set; }
        [Column("PRICE_LIST_PRICE_ID")] public int PriceId { get; set; }
        [Column("PRICING_RULES_RULE_ID")] public int RuleId { get; set; }
        [ForeignKey("PriceId")] public virtual PriceList? PriceList { get; set; }
        [ForeignKey("RuleId")] public virtual PricingRule? Rule { get; set; }
    }

    [Table("TRAINING")]
    public class Training : AuditableEntity
    {
        [Key][Column("TRAINING_ID")] public int TrainingId { get; set; }
        [Column("STAFF_STAFF_ID")] public int StaffId { get; set; }
        [Column("TRAINER_STAFF_ID")] public int? TrainerStaffId { get; set; }
        [Column("ROLES_ROLE_ID")] public int? RoleId { get; set; }
        [Column("CERTIFICATIONS_CERT_ID")] public int? CertId { get; set; }
        [Column("TRAINING_NAME")] public string TrainingName { get; set; } = string.Empty;
        [Column("COMPLETION_DATE")] public DateTime? CompletionDate { get; set; }
        [Column("NOTES")] public string? Notes { get; set; }
        [ForeignKey("StaffId")] public virtual Staff? Staff { get; set; }
        [ForeignKey("TrainerStaffId")] public virtual Staff? Trainer { get; set; }
        [ForeignKey("RoleId")] public virtual Role? Role { get; set; }
        [ForeignKey("CertId")] public virtual Certification? Certification { get; set; }
    }

    [Table("USER_ROLES")]
    public class UserRole : AuditableEntity
    {
        [Key][Column("USER_ROLE_ID")] public int UserRoleId { get; set; }
        [Column("USERS_USER_ID")] public int UserId { get; set; }
        [Column("ROLES_ROLE_ID")] public int RoleId { get; set; }
        [Column("ASSIGNED_AT")] public DateTime AssignedAt { get; set; } = DateTime.Now;
        [ForeignKey("UserId")] public virtual User? User { get; set; }
        [ForeignKey("RoleId")] public virtual Role? Role { get; set; }
    }

    [Table("WAIVERS")]
    public class Waiver : AuditableEntity
    {
        [Key][Column("WAIVER_ID")] public int WaiverId { get; set; }
        [Column("VISITORS_VISITOR_ID")] public int VisitorId { get; set; }
        [Column("WAIVER_TYPE")] public string WaiverType { get; set; } = string.Empty;
        [Column("SIGNED_DATE")] public DateTime SignedDate { get; set; } = DateTime.Now;
        [Column("EXPIRY_DATE")] public DateTime? ExpiryDate { get; set; }
        [Column("DIGITAL_SIGNATURE")] public string? DigitalSignature { get; set; }
        [Column("IP_ADDRESS")] public string? IpAddress { get; set; }
        [Column("VERSION")] public int Version { get; set; } = 1;
        [ForeignKey("VisitorId")] public virtual Visitor? Visitor { get; set; }
        [NotMapped] public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.Now;
    }

    [Table("WEATHER_CONDITIONS")]
    public class WeatherCondition : AuditableEntity
    {
        [Key][Column("WEATHER_ID")] public int WeatherId { get; set; }
        [Column("PARK_AREAS_AREA_ID")] public int? AreaId { get; set; }
        [Column("RECORD_DATE")] public DateTime RecordDate { get; set; } = DateTime.Now;
        [Column("TEMPERATURE")] public decimal? Temperature { get; set; }
        [Column("WEATHER_TYPE")] public string? WeatherType { get; set; }
        [Column("UV_INDEX")] public int? UvIndex { get; set; }
        [Column("WIND_SPEED_KMH")] public decimal? WindSpeedKmh { get; set; }
        [Column("PRECIPITATION_MM")] public decimal? PrecipitationMm { get; set; }
        [Column("OPERATIONAL_STATUS")] public string OperationalStatus { get; set; } = "NORMAL";
        [ForeignKey("AreaId")] public virtual Pool? Area { get; set; }
    }
}