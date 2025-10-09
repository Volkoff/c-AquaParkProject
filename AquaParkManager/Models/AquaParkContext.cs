using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaParkManager.Models
{
    public class AquaParkContext : DbContext
    {
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<StaffRole> StaffRoles { get; set; }
        public DbSet<Certification> Certifications { get; set; }
        public DbSet<Pool> Pools { get; set; }
        public DbSet<SlideType> SlideTypes { get; set; }
        public DbSet<Slide> Slides { get; set; }
        public DbSet<Attraction> Attractions { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<PriceList> PriceLists { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
        public DbSet<Visitor> Visitors { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingItem> BookingItems { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<StaffShift> StaffShifts { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Update this connection string to match your database
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=AquaParkDB;Trusted_Connection=true;MultipleActiveResultSets=true");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure decimal precision
            modelBuilder.Entity<Pool>().Property(p => p.DepthMin).HasPrecision(5, 2);
            modelBuilder.Entity<Pool>().Property(p => p.DepthMax).HasPrecision(5, 2);
            modelBuilder.Entity<Slide>().Property(s => s.LengthM).HasPrecision(6, 2);
            modelBuilder.Entity<Slide>().Property(s => s.HeightM).HasPrecision(6, 2);
            modelBuilder.Entity<MaintenanceRecord>().Property(m => m.Cost).HasPrecision(12, 2);
            modelBuilder.Entity<PriceList>().Property(p => p.BasePrice).HasPrecision(12, 2);
            modelBuilder.Entity<TicketType>().Property(t => t.DurationHours).HasPrecision(5, 2);
            modelBuilder.Entity<Ticket>().Property(t => t.PricePaid).HasPrecision(12, 2);
            modelBuilder.Entity<Booking>().Property(b => b.TotalAmount).HasPrecision(12, 2);
            modelBuilder.Entity<BookingItem>().Property(b => b.UnitPrice).HasPrecision(12, 2);
            modelBuilder.Entity<Payment>().Property(p => p.Amount).HasPrecision(12, 2);

            // Configure indexes
            modelBuilder.Entity<Attraction>().HasIndex(a => a.AttractionType);
            modelBuilder.Entity<Schedule>().HasIndex(s => new { s.AttractionId, s.StartDatetime, s.EndDatetime });
            modelBuilder.Entity<Ticket>().HasIndex(t => t.Status);
        }
    }

    public class Staff
    {
        [Key]
        public int StaffId { get; set; }
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
        [MaxLength(200)]
        public string? Email { get; set; }
        [MaxLength(30)]
        public string? Phone { get; set; }
        public DateTime HireDate { get; set; } = DateTime.Now;
        [MaxLength(1)]
        public string Active { get; set; } = "Y";
        [MaxLength(100)]
        public string? JobTitle { get; set; }
        public string? Notes { get; set; }

        public virtual ICollection<StaffRole> StaffRoles { get; set; } = new List<StaffRole>();
        public virtual ICollection<Certification> Certifications { get; set; } = new List<Certification>();
        public virtual ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
        public virtual ICollection<StaffShift> StaffShifts { get; set; } = new List<StaffShift>();
    }

    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        [Required]
        [MaxLength(100)]
        public string RoleName { get; set; } = string.Empty;
        [MaxLength(4000)]
        public string? Description { get; set; }

        public virtual ICollection<StaffRole> StaffRoles { get; set; } = new List<StaffRole>();
    }

    public class StaffRole
    {
        [Key]
        public int StaffRoleId { get; set; }
        public int StaffId { get; set; }
        public int RoleId { get; set; }
        public DateTime AssignedDate { get; set; } = DateTime.Now;

        [ForeignKey("StaffId")]
        public virtual Staff Staff { get; set; } = null!;
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;
    }

    public class Certification
    {
        [Key]
        public int CertId { get; set; }
        public int StaffId { get; set; }
        [Required]
        [MaxLength(200)]
        public string CertName { get; set; } = string.Empty;
        [MaxLength(200)]
        public string? IssuedBy { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        [MaxLength(2000)]
        public string? Notes { get; set; }

        [ForeignKey("StaffId")]
        public virtual Staff Staff { get; set; } = null!;
    }

    public class Pool
    {
        [Key]
        public int PoolId { get; set; }
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;
        [Required]
        public decimal DepthMin { get; set; }
        [Required]
        public decimal DepthMax { get; set; }
        [Required]
        public int Capacity { get; set; }
        [MaxLength(1)]
        public string Indoors { get; set; } = "N";
        [MaxLength(2000)]
        public string? Notes { get; set; }

        public virtual ICollection<Slide> Slides { get; set; } = new List<Slide>();
    }

    public class SlideType
    {
        [Key]
        public int SlideTypeId { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(20)]
        public string? Difficulty { get; set; }
        [MaxLength(2000)]
        public string? Description { get; set; }

        public virtual ICollection<Slide> Slides { get; set; } = new List<Slide>();
    }

    public class Slide
    {
        [Key]
        public int SlideId { get; set; }
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        public int SlideTypeId { get; set; }
        public decimal? LengthM { get; set; }
        public decimal? HeightM { get; set; }
        public int? MinHeightCm { get; set; }
        public int? MaxWeightKg { get; set; }
        [MaxLength(20)]
        public string Status { get; set; } = "OPEN";
        public int? PoolId { get; set; }
        public DateTime? InstallationDate { get; set; }
        [MaxLength(2000)]
        public string? Notes { get; set; }

        [ForeignKey("SlideTypeId")]
        public virtual SlideType SlideType { get; set; } = null!;
        [ForeignKey("PoolId")]
        public virtual Pool? Pool { get; set; }
    }

    public class Attraction
    {
        [Key]
        public int AttractionId { get; set; }
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [MaxLength(50)]
        public string AttractionType { get; set; } = string.Empty;
        public int? ObjectId { get; set; }
        [MaxLength(20)]
        public string Status { get; set; } = "OPEN";
        public int? Capacity { get; set; }
        [MaxLength(2000)]
        public string? Notes { get; set; }

        public virtual ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public virtual ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
    }

    public class MaintenanceRecord
    {
        [Key]
        public int MaintenanceId { get; set; }
        public int AttractionId { get; set; }
        public int? ReportedBy { get; set; }
        public DateTime ReportDate { get; set; } = DateTime.Now;
        [MaxLength(4000)]
        public string? ProblemDescription { get; set; }
        [MaxLength(4000)]
        public string? ActionTaken { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal Cost { get; set; } = 0;

        [ForeignKey("AttractionId")]
        public virtual Attraction Attraction { get; set; } = null!;
        [ForeignKey("ReportedBy")]
        public virtual Staff? Staff { get; set; }
    }

    public class Schedule
    {
        [Key]
        public int ScheduleId { get; set; }
        public int AttractionId { get; set; }
        public DateTime StartDatetime { get; set; }
        public DateTime EndDatetime { get; set; }
        [MaxLength(20)]
        public string Status { get; set; } = "OPEN";
        [MaxLength(2000)]
        public string? Notes { get; set; }

        [ForeignKey("AttractionId")]
        public virtual Attraction Attraction { get; set; } = null!;
    }

    public class PriceList
    {
        [Key]
        public int PriceId { get; set; }
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(2000)]
        public string? Description { get; set; }
        [MaxLength(10)]
        public string Currency { get; set; } = "EUR";
        [Required]
        public decimal BasePrice { get; set; }
        public DateTime ValidFrom { get; set; } = DateTime.Now;
        public DateTime? ValidTo { get; set; }
        [MaxLength(1)]
        public string Active { get; set; } = "Y";

        public virtual ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
    }

    public class TicketType
    {
        [Key]
        public int TicketTypeId { get; set; }
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(2000)]
        public string? Description { get; set; }
        public int PriceId { get; set; }
        public decimal? DurationHours { get; set; }
        public int? AgeMin { get; set; }
        public int? AgeMax { get; set; }

        [ForeignKey("PriceId")]
        public virtual PriceList PriceList { get; set; } = null!;
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }

    public class Visitor
    {
        [Key]
        public int VisitorId { get; set; }
        [MaxLength(120)]
        public string? FirstName { get; set; }
        [MaxLength(120)]
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        [MaxLength(200)]
        public string? Email { get; set; }
        [MaxLength(30)]
        public string? Phone { get; set; }
        [MaxLength(200)]
        public string? EmergencyContact { get; set; }
        [MaxLength(2000)]
        public string? Notes { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public virtual ICollection<Membership> Memberships { get; set; } = new List<Membership>();
    }

    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }
        public int TicketTypeId { get; set; }
        public int? VisitorId { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        [Required]
        public decimal PricePaid { get; set; }
        [MaxLength(20)]
        public string Status { get; set; } = "ACTIVE";

        [ForeignKey("TicketTypeId")]
        public virtual TicketType TicketType { get; set; } = null!;
        [ForeignKey("VisitorId")]
        public virtual Visitor? Visitor { get; set; }
        public virtual ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
    }

    public class Booking
    {
        [Key]
        public int BookingId { get; set; }
        [MaxLength(30)]
        public string? BookingRef { get; set; }
        [MaxLength(200)]
        public string? CustomerName { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; } = 0;
        [MaxLength(20)]
        public string Status { get; set; } = "CONFIRMED";
        [MaxLength(2000)]
        public string? Notes { get; set; }

        public virtual ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

    public class BookingItem
    {
        [Key]
        public int BookingItemId { get; set; }
        public int BookingId { get; set; }
        public int? AttractionId { get; set; }
        public int? TicketId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; } = 0;

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; } = null!;
        [ForeignKey("AttractionId")]
        public virtual Attraction? Attraction { get; set; }
        [ForeignKey("TicketId")]
        public virtual Ticket? Ticket { get; set; }
    }

    public class Membership
    {
        [Key]
        public int MembershipId { get; set; }
        public int VisitorId { get; set; }
        [MaxLength(100)]
        public string? MembershipType { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; }
        [MaxLength(1)]
        public string Recurring { get; set; } = "N";
        [MaxLength(20)]
        public string Status { get; set; } = "ACTIVE";

        [ForeignKey("VisitorId")]
        public virtual Visitor Visitor { get; set; } = null!;
    }

    public class Shift
    {
        [Key]
        public int ShiftId { get; set; }
        [MaxLength(100)]
        public string? ShiftName { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        [MaxLength(2000)]
        public string? Notes { get; set; }

        public virtual ICollection<StaffShift> StaffShifts { get; set; } = new List<StaffShift>();
    }

    public class StaffShift
    {
        [Key]
        public int StaffShiftId { get; set; }
        public int StaffId { get; set; }
        public int ShiftId { get; set; }
        public DateTime ShiftDate { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.Now;

        [ForeignKey("StaffId")]
        public virtual Staff Staff { get; set; } = null!;
        [ForeignKey("ShiftId")]
        public virtual Shift Shift { get; set; } = null!;
    }

    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }
        public int? BookingId { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        [Required]
        public decimal Amount { get; set; }
        [MaxLength(50)]
        public string? PaymentMethod { get; set; }
        [MaxLength(200)]
        public string? Reference { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking? Booking { get; set; }
    }
}
