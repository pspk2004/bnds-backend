using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public static class SeedData
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Memberships.AnyAsync())
            return;

        // Seed Memberships
        var freeMembership = new Membership { Name = "Free", MaxAds = 1, MaxBids = 5, MaxFeaturedAds = 0, Price = 0, DurationMonths = 6 };
        var silverMembership = new Membership { Name = "Silver", MaxAds = 5, MaxBids = 30, MaxFeaturedAds = 1, Price = 499, DurationMonths = 6 };
        var goldMembership = new Membership { Name = "Gold", MaxAds = 20, MaxBids = 100, MaxFeaturedAds = 5, Price = 999, DurationMonths = 6 };
        var platinumMembership = new Membership { Name = "Platinum", MaxAds = -1, MaxBids = -1, MaxFeaturedAds = -1, Price = 1999, DurationMonths = 12 };

        context.Memberships.AddRange(freeMembership, silverMembership, goldMembership, platinumMembership);
        await context.SaveChangesAsync();

        // Seed Categories
        var electronics = new Category { Name = "Electronics", ImageUrl = "https://images.unsplash.com/photo-1498049794561-7780e7231661" };
        var vehicles = new Category { Name = "Vehicles", ImageUrl = "https://images.unsplash.com/photo-1549317661-bd32c8ce0afa" };
        var furniture = new Category { Name = "Furniture", ImageUrl = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc" };
        var fashion = new Category { Name = "Fashion", ImageUrl = "https://images.unsplash.com/photo-1445205170230-053b83016050" };
        var sports = new Category { Name = "Sports", ImageUrl = "https://images.unsplash.com/photo-1461896836934-bd45ba862e8e" };
        var books = new Category { Name = "Books", ImageUrl = "https://images.unsplash.com/photo-1495446815901-a7297e633e8d" };

        context.Categories.AddRange(electronics, vehicles, furniture, fashion, sports, books);
        await context.SaveChangesAsync();

        // Seed Admin
        var admin = new User
        {
            Name = "Admin",
            Email = "admin@bidsnap.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = "Admin",
            MembershipId = platinumMembership.Id,
            MembershipExpiry = DateTime.UtcNow.AddYears(10),
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        // Seed Users
        var alice = new User { Name = "Alice Johnson", Email = "alice@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"), Role = "User", MembershipId = freeMembership.Id, MembershipExpiry = DateTime.UtcNow.AddMonths(6), CreatedAt = DateTime.UtcNow.AddDays(-30) };
        var bob = new User { Name = "Bob Smith", Email = "bob@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"), Role = "User", MembershipId = silverMembership.Id, MembershipExpiry = DateTime.UtcNow.AddMonths(6), CreatedAt = DateTime.UtcNow.AddDays(-25) };
        var charlie = new User { Name = "Charlie Brown", Email = "charlie@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"), Role = "User", MembershipId = goldMembership.Id, MembershipExpiry = DateTime.UtcNow.AddMonths(6), CreatedAt = DateTime.UtcNow.AddDays(-20) };
        var diana = new User { Name = "Diana Prince", Email = "diana@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"), Role = "User", MembershipId = platinumMembership.Id, MembershipExpiry = DateTime.UtcNow.AddMonths(12), CreatedAt = DateTime.UtcNow.AddDays(-15) };
        var eve = new User { Name = "Eve Wilson", Email = "eve@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"), Role = "User", MembershipId = freeMembership.Id, MembershipExpiry = DateTime.UtcNow.AddMonths(6), CreatedAt = DateTime.UtcNow.AddDays(-10) };

        context.Users.AddRange(alice, bob, charlie, diana, eve);
        await context.SaveChangesAsync();

        // Seed Products
        var product1 = new Product { Title = "iPhone 15 Pro Max", Description = "Brand new iPhone 15 Pro Max 256GB, sealed box.", CategoryId = electronics.Id, SellerId = alice.Id, StartingPrice = 80000, CurrentPrice = 80000, BidEndTime = DateTime.UtcNow.AddDays(7), CreatedAt = DateTime.UtcNow.AddDays(-2) };
        var product2 = new Product { Title = "MacBook Air M2", Description = "MacBook Air M2 2023, 8GB RAM, 256GB SSD, excellent condition.", CategoryId = electronics.Id, SellerId = bob.Id, StartingPrice = 65000, CurrentPrice = 65000, BidEndTime = DateTime.UtcNow.AddDays(5), CreatedAt = DateTime.UtcNow.AddDays(-3) };
        var product3 = new Product { Title = "Honda City 2020", Description = "Honda City 2020 model, petrol, 25000 km driven, single owner.", CategoryId = vehicles.Id, SellerId = charlie.Id, StartingPrice = 750000, CurrentPrice = 750000, BidEndTime = DateTime.UtcNow.AddDays(14), CreatedAt = DateTime.UtcNow.AddDays(-5) };
        var product4 = new Product { Title = "L-Shaped Sofa Set", Description = "Premium L-shaped sofa set, 7 seater, fabric, grey color.", CategoryId = furniture.Id, SellerId = diana.Id, StartingPrice = 25000, CurrentPrice = 25000, BidEndTime = DateTime.UtcNow.AddDays(10), CreatedAt = DateTime.UtcNow.AddDays(-1) };
        var product5 = new Product { Title = "Nike Air Jordan 1", Description = "Nike Air Jordan 1 Retro High OG, Size 10, brand new.", CategoryId = fashion.Id, SellerId = alice.Id, StartingPrice = 12000, CurrentPrice = 12000, BidEndTime = DateTime.UtcNow.AddDays(3), IsFeatured = true, CreatedAt = DateTime.UtcNow.AddDays(-1) };
        var product6 = new Product { Title = "Cricket Bat - MRF Genius", Description = "MRF Genius Grand Edition English Willow Cricket Bat.", CategoryId = sports.Id, SellerId = bob.Id, StartingPrice = 8000, CurrentPrice = 8000, BidEndTime = DateTime.UtcNow.AddDays(6), CreatedAt = DateTime.UtcNow.AddDays(-4) };
        var product7 = new Product { Title = "Harry Potter Complete Set", Description = "Harry Potter complete 7 book box set, hardcover edition.", CategoryId = books.Id, SellerId = charlie.Id, StartingPrice = 3500, CurrentPrice = 3500, BidEndTime = DateTime.UtcNow.AddDays(8), CreatedAt = DateTime.UtcNow.AddDays(-2) };
        var product8 = new Product { Title = "Samsung Galaxy S24 Ultra", Description = "Samsung Galaxy S24 Ultra 512GB, Titanium Black.", CategoryId = electronics.Id, SellerId = diana.Id, StartingPrice = 90000, CurrentPrice = 90000, BidEndTime = DateTime.UtcNow.AddDays(4), IsFeatured = true, CreatedAt = DateTime.UtcNow };
        var product9 = new Product { Title = "Royal Enfield Classic 350", Description = "Royal Enfield Classic 350, 2022 model, 8000 km, Signals edition.", CategoryId = vehicles.Id, SellerId = eve.Id, StartingPrice = 150000, CurrentPrice = 150000, BidEndTime = DateTime.UtcNow.AddDays(12), CreatedAt = DateTime.UtcNow.AddDays(-6) };
        var product10 = new Product { Title = "Study Table with Chair", Description = "Ergonomic study table with adjustable chair, wooden finish.", CategoryId = furniture.Id, SellerId = alice.Id, StartingPrice = 7000, CurrentPrice = 7000, BidEndTime = DateTime.UtcNow.AddDays(9), CreatedAt = DateTime.UtcNow.AddDays(-3) };

        context.Products.AddRange(product1, product2, product3, product4, product5, product6, product7, product8, product9, product10);
        await context.SaveChangesAsync();

        // Seed Bids
        context.Bids.AddRange(
            new Bid { ProductId = product1.Id, UserId = bob.Id, Amount = 82000, Time = DateTime.UtcNow.AddDays(-1) },
            new Bid { ProductId = product1.Id, UserId = charlie.Id, Amount = 85000, Time = DateTime.UtcNow.AddHours(-12) },
            new Bid { ProductId = product2.Id, UserId = charlie.Id, Amount = 67000, Time = DateTime.UtcNow.AddDays(-2) },
            new Bid { ProductId = product2.Id, UserId = diana.Id, Amount = 70000, Time = DateTime.UtcNow.AddDays(-1) },
            new Bid { ProductId = product3.Id, UserId = alice.Id, Amount = 770000, Time = DateTime.UtcNow.AddDays(-3) },
            new Bid { ProductId = product5.Id, UserId = charlie.Id, Amount = 13000, Time = DateTime.UtcNow.AddHours(-6) },
            new Bid { ProductId = product5.Id, UserId = diana.Id, Amount = 14500, Time = DateTime.UtcNow.AddHours(-3) },
            new Bid { ProductId = product6.Id, UserId = alice.Id, Amount = 8500, Time = DateTime.UtcNow.AddDays(-2) },
            new Bid { ProductId = product8.Id, UserId = bob.Id, Amount = 92000, Time = DateTime.UtcNow.AddHours(-8) },
            new Bid { ProductId = product9.Id, UserId = charlie.Id, Amount = 155000, Time = DateTime.UtcNow.AddDays(-4) }
        );
        await context.SaveChangesAsync();

        // Update current prices based on highest bids
        product1.CurrentPrice = 85000;
        product2.CurrentPrice = 70000;
        product3.CurrentPrice = 770000;
        product5.CurrentPrice = 14500;
        product6.CurrentPrice = 8500;
        product8.CurrentPrice = 92000;
        product9.CurrentPrice = 155000;

        await context.SaveChangesAsync();
    }
}
