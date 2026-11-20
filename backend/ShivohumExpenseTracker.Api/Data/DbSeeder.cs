using Microsoft.EntityFrameworkCore;
using ShivohumExpenseTracker.Api.Data;
using ShivohumExpenseTracker.Api.Models.Entities;

namespace ShivohumExpenseTracker.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAdminAsync(AppDbContext db)
    {
        if (!await db.Users.AnyAsync())
        {
            db.Users.Add(new User
            {
                FullName = "Administrator",
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin",
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        if (await db.Transactions.AnyAsync())
        {
            return;
        }

        var admin = await db.Users.FirstAsync(u => u.Username == "admin");
        var labour = await db.Categories.FirstOrDefaultAsync(c => c.CategoryName == "Labour" && c.Type == "Expense");
        var clientPay = await db.Categories.FirstOrDefaultAsync(c => c.CategoryName == "CLIENT PAYMENT" && c.Type == "Income");
        var cash = await db.PaymentModes.FirstOrDefaultAsync(p => p.PaymentModeName == "Cash");
        var upi = await db.PaymentModes.FirstOrDefaultAsync(p => p.PaymentModeName == "UPI");
        var project = await db.Projects.FirstOrDefaultAsync(p => p.ProjectName.Contains("AMARBHAI"));
        var shivohum = await db.Accounts.FirstOrDefaultAsync(a => a.AccountName == "Shivohum Account");

        if (labour is null || clientPay is null || cash is null || upi is null || project is null)
        {
            return;
        }

        db.Transactions.AddRange(
            new Transaction
            {
                TransactionDate = DateTime.Today.AddDays(-3),
                Type = "Income",
                CategoryId = clientPay.CategoryId,
                Description = "Initial client payment",
                ProjectId = project.ProjectId,
                PaymentModeId = upi.PaymentModeId,
                Amount = 31000,
                AccountId = shivohum?.AccountId,
                CreatedBy = admin.UserId,
                CreatedOn = DateTime.UtcNow
            },
            new Transaction
            {
                TransactionDate = DateTime.Today.AddDays(-1),
                Type = "Expense",
                CategoryId = labour.CategoryId,
                Description = "Labour charges",
                ProjectId = project.ProjectId,
                PaymentModeId = cash.PaymentModeId,
                Amount = 4500,
                CreatedBy = admin.UserId,
                CreatedOn = DateTime.UtcNow
            });

        await db.SaveChangesAsync();
    }
}
