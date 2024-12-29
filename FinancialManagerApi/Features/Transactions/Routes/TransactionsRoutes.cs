using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FinancialManagerApi.Data;
using FinancialManagerApi.Models;
using FinancialManagerApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagerApi.Routes
{
    public static class TransactionsRoutes
    {
        public static void MapTransactionRoutes(this WebApplication app)
        {
            var transactionRoutes = app.MapGroup("/users/{userId:guid}/transactions");
            
            transactionRoutes.MapGet("/", [Authorize] async (Guid userId, AppDbContext context, ClaimsPrincipal user) =>
            {
                var authenticatedUserId = GetUserId(user);
                if (authenticatedUserId == null || authenticatedUserId != userId) return Results.Unauthorized();

                var transactions = await context.Transactions
                    .Where(t => t.UserId == userId)
                    .ToListAsync();

                return transactions.Count != 0 ? Results.Ok(transactions) : Results.NotFound("No transactions found.");
            });
            
            transactionRoutes.MapGet("/{id:guid}", [Authorize] async (Guid userId, Guid id, AppDbContext context, ClaimsPrincipal user) =>
            {
                var authenticatedUserId = GetUserId(user);
                if (authenticatedUserId == null || authenticatedUserId != userId) return Results.Unauthorized();

                var transaction = await context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                return transaction != null
                    ? Results.Ok(transaction)
                    : Results.NotFound("Transaction not found.");
            });
            
            transactionRoutes.MapPost("/", [Authorize] async (
                Guid userId,
                [FromBody] AddTransactionRequest createRequest,
                AppDbContext context,
                ClaimsPrincipal user) =>
            {
                var authenticatedUserId = GetUserId(user);
                if (authenticatedUserId == null || authenticatedUserId != userId) return Results.Unauthorized();

                // Create a new transaction using the request data
                var transaction = new Transaction
                {
                    UserId = userId,
                    Amount = createRequest.Amount,
                    Description = createRequest.Description,
                    Type = createRequest.Type,
                    Category = createRequest.Category,
                    Date = DateTime.UtcNow // Set the current date for the transaction
                };

                context.Transactions.Add(transaction);
                await context.SaveChangesAsync();

                return Results.Created($"/users/{userId}/transactions/{transaction.Id}", transaction);
            });

            
            transactionRoutes.MapPut("/{id:guid}", [Authorize] async (
                Guid userId,
                [FromBody] UpdateTransactionRequest updateRequest,
                AppDbContext context,
                ClaimsPrincipal user,
                Guid id,
                [FromServices] ITokenBlacklistService blacklistService) =>
            {
                var authenticatedUserId = GetUserId(user);
                if (authenticatedUserId == null || authenticatedUserId != userId) return Results.Unauthorized();

                if (await blacklistService.IsUserBlacklisted(userId)!) return Results.Unauthorized();

                var transaction = await context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (transaction == null) return Results.NotFound("Transaction not found or not owned by the user.");

                transaction.Amount = updateRequest.Amount;
                transaction.Description = updateRequest.Description;
                transaction.Date = updateRequest.Date;

                await context.SaveChangesAsync();

                return Results.Ok(transaction);
            });
            
            transactionRoutes.MapDelete("/{id:guid}", [Authorize] async (Guid userId, Guid id, AppDbContext context, ClaimsPrincipal user) =>
            {
                var authenticatedUserId = GetUserId(user);
                if (authenticatedUserId == null || authenticatedUserId != userId) return Results.Unauthorized();

                var transaction = await context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (transaction == null) return Results.NotFound("Transaction not found.");

                context.Transactions.Remove(transaction);
                await context.SaveChangesAsync();

                return Results.Ok("Transaction deleted successfully.");
            });
        }

        private static Guid? GetUserId(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
        }
    }
}
