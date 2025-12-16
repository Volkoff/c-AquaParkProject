using System.ComponentModel.DataAnnotations.Schema;

namespace AquaParkManager.Models
{
    // Mapuje pohled V_BOOKING_OVERVIEW
    public class BookingOverview
    {
        [Column("BOOKING_ID")]
        public int BookingId { get; set; } // ID sice v modelu být musí pro mapování, ale v GUI ho skryjeme

        [Column("BOOKING_REF")]
        public string? BookingRef { get; set; }

        [Column("VISITOR_NAME")]
        public string? VisitorName { get; set; }

        [Column("BOOKING_STATUS")]
        public string? Status { get; set; }

        [Column("TOTAL_AMOUNT")]
        public decimal TotalAmount { get; set; }

        [Column("TOTAL_PAID")]
        public decimal TotalPaid { get; set; }
    }
}