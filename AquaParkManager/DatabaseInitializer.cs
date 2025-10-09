using AquaParkManager.Models;
using Microsoft.EntityFrameworkCore;

namespace AquaParkManager
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            using var context = new AquaParkContext();
            
            // Ensure database is created
            context.Database.EnsureCreated();
            
            // Check if data already exists
            if (context.Staff.Any())
                return;

            // Add sample data
            AddSampleData(context);
        }

        private static void AddSampleData(AquaParkContext context)
        {
            // Add roles
            var lifeguardRole = new Role { RoleName = "Lifeguard", Description = "Responsible for pool safety" };
            var maintenanceRole = new Role { RoleName = "Maintenance", Description = "Equipment and facility maintenance" };
            var managerRole = new Role { RoleName = "Manager", Description = "General management duties" };
            
            context.Roles.AddRange(lifeguardRole, maintenanceRole, managerRole);
            context.SaveChanges();

            // Add staff
            var staff1 = new Staff 
            { 
                FirstName = "John", 
                LastName = "Smith", 
                Email = "john.smith@aquapark.com", 
                Phone = "555-0101", 
                JobTitle = "Senior Lifeguard",
                HireDate = DateTime.Now.AddYears(-2),
                Active = "Y"
            };
            
            var staff2 = new Staff 
            { 
                FirstName = "Sarah", 
                LastName = "Johnson", 
                Email = "sarah.johnson@aquapark.com", 
                Phone = "555-0102", 
                JobTitle = "Maintenance Technician",
                HireDate = DateTime.Now.AddYears(-1),
                Active = "Y"
            };

            var staff3 = new Staff 
            { 
                FirstName = "Mike", 
                LastName = "Davis", 
                Email = "mike.davis@aquapark.com", 
                Phone = "555-0103", 
                JobTitle = "Park Manager",
                HireDate = DateTime.Now.AddYears(-3),
                Active = "Y"
            };

            context.Staff.AddRange(staff1, staff2, staff3);
            context.SaveChanges();

            // Add staff roles
            context.StaffRoles.AddRange(
                new StaffRole { StaffId = staff1.StaffId, RoleId = lifeguardRole.RoleId },
                new StaffRole { StaffId = staff2.StaffId, RoleId = maintenanceRole.RoleId },
                new StaffRole { StaffId = staff3.StaffId, RoleId = managerRole.RoleId }
            );

            // Add slide types
            var tubeSlide = new SlideType { Name = "Tube Slide", Difficulty = "Medium", Description = "Enclosed tube water slide" };
            var bodySlide = new SlideType { Name = "Body Slide", Difficulty = "Low", Description = "Open body water slide" };
            var speedSlide = new SlideType { Name = "Speed Slide", Difficulty = "High", Description = "High-speed water slide" };

            context.SlideTypes.AddRange(tubeSlide, bodySlide, speedSlide);
            context.SaveChanges();

            // Add pools
            var mainPool = new Pool 
            { 
                Name = "Main Pool", 
                DepthMin = 1.2m, 
                DepthMax = 2.5m, 
                Capacity = 100, 
                Indoors = "N",
                Notes = "Main swimming pool with diving area"
            };

            var kidsPool = new Pool 
            { 
                Name = "Kids Pool", 
                DepthMin = 0.3m, 
                DepthMax = 0.8m, 
                Capacity = 50, 
                Indoors = "N",
                Notes = "Shallow pool for children"
            };

            var wavePool = new Pool 
            { 
                Name = "Wave Pool", 
                DepthMin = 0.5m, 
                DepthMax = 1.8m, 
                Capacity = 200, 
                Indoors = "N",
                Notes = "Pool with wave generation system"
            };

            context.Pools.AddRange(mainPool, kidsPool, wavePool);
            context.SaveChanges();

            // Add slides
            var slide1 = new Slide 
            { 
                Name = "Thunder Tube", 
                SlideTypeId = tubeSlide.SlideTypeId, 
                LengthM = 120m, 
                HeightM = 15m, 
                MinHeightCm = 120, 
                MaxWeightKg = 100,
                Status = "OPEN",
                PoolId = mainPool.PoolId,
                InstallationDate = DateTime.Now.AddYears(-1)
            };

            var slide2 = new Slide 
            { 
                Name = "Splash Mountain", 
                SlideTypeId = bodySlide.SlideTypeId, 
                LengthM = 80m, 
                HeightM = 10m, 
                MinHeightCm = 100, 
                MaxWeightKg = 120,
                Status = "OPEN",
                PoolId = mainPool.PoolId,
                InstallationDate = DateTime.Now.AddMonths(-6)
            };

            context.Slides.AddRange(slide1, slide2);
            context.SaveChanges();

            // Add attractions
            var attraction1 = new Attraction 
            { 
                Name = "Main Pool Area", 
                AttractionType = "POOL", 
                ObjectId = mainPool.PoolId, 
                Status = "OPEN", 
                Capacity = 100 
            };

            var attraction2 = new Attraction 
            { 
                Name = "Thunder Tube Slide", 
                AttractionType = "SLIDE", 
                ObjectId = slide1.SlideId, 
                Status = "OPEN", 
                Capacity = 1 
            };

            var attraction3 = new Attraction 
            { 
                Name = "Wave Pool", 
                AttractionType = "WAVE_POOL", 
                ObjectId = wavePool.PoolId, 
                Status = "OPEN", 
                Capacity = 200 
            };

            context.Attractions.AddRange(attraction1, attraction2, attraction3);
            context.SaveChanges();

            // Add price list
            var adultPrice = new PriceList 
            { 
                Name = "Adult Day Pass", 
                Description = "Full day access for adults", 
                BasePrice = 25.00m, 
                ValidFrom = DateTime.Now.AddYears(-1) 
            };

            var childPrice = new PriceList 
            { 
                Name = "Child Day Pass", 
                Description = "Full day access for children", 
                BasePrice = 15.00m, 
                ValidFrom = DateTime.Now.AddYears(-1) 
            };

            context.PriceLists.AddRange(adultPrice, childPrice);
            context.SaveChanges();

            // Add ticket types
            var adultTicket = new TicketType 
            { 
                Name = "Adult Day Pass", 
                Description = "Full day access for adults (18+)", 
                PriceId = adultPrice.PriceId, 
                AgeMin = 18 
            };

            var childTicket = new TicketType 
            { 
                Name = "Child Day Pass", 
                Description = "Full day access for children (3-17)", 
                PriceId = childPrice.PriceId, 
                AgeMin = 3, 
                AgeMax = 17 
            };

            context.TicketTypes.AddRange(adultTicket, childTicket);
            context.SaveChanges();

            // Add sample visitors
            var visitor1 = new Visitor 
            { 
                FirstName = "Alice", 
                LastName = "Brown", 
                DateOfBirth = new DateTime(1990, 5, 15), 
                Email = "alice.brown@email.com", 
                Phone = "555-0201" 
            };

            var visitor2 = new Visitor 
            { 
                FirstName = "Tommy", 
                LastName = "Wilson", 
                DateOfBirth = new DateTime(2015, 8, 22), 
                Email = "tommy.wilson@email.com", 
                Phone = "555-0202" 
            };

            context.Visitors.AddRange(visitor1, visitor2);
            context.SaveChanges();

            // Add sample tickets
            var ticket1 = new Ticket 
            { 
                TicketTypeId = adultTicket.TicketTypeId, 
                VisitorId = visitor1.VisitorId, 
                PurchaseDate = DateTime.Now.AddDays(-1), 
                ValidFrom = DateTime.Now, 
                ValidTo = DateTime.Now.AddDays(1), 
                PricePaid = 25.00m, 
                Status = "ACTIVE" 
            };

            var ticket2 = new Ticket 
            { 
                TicketTypeId = childTicket.TicketTypeId, 
                VisitorId = visitor2.VisitorId, 
                PurchaseDate = DateTime.Now.AddDays(-1), 
                ValidFrom = DateTime.Now, 
                ValidTo = DateTime.Now.AddDays(1), 
                PricePaid = 15.00m, 
                Status = "ACTIVE" 
            };

            context.Tickets.AddRange(ticket1, ticket2);
            context.SaveChanges();

            // Add sample booking
            var booking1 = new Booking 
            { 
                BookingRef = "BK001", 
                CustomerName = "Alice Brown", 
                CreatedDate = DateTime.Now.AddDays(-1), 
                TotalAmount = 40.00m, 
                Status = "CONFIRMED" 
            };

            context.Bookings.Add(booking1);
            context.SaveChanges();

            // Add booking items
            context.BookingItems.AddRange(
                new BookingItem { BookingId = booking1.BookingId, TicketId = ticket1.TicketId, Quantity = 1, UnitPrice = 25.00m },
                new BookingItem { BookingId = booking1.BookingId, TicketId = ticket2.TicketId, Quantity = 1, UnitPrice = 15.00m }
            );

            context.SaveChanges();
        }
    }
}
