using System.Security.Claims;
using FinancialManagerApi.Data;
using FinancialManagerApi.Models;
using FinancialManagerApi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagerApi.Routes;

public static class UserRoutes
{
    public static void MapUserRoutes(this WebApplication app)
    {
        var userRoutes = app.MapGroup("users");

        userRoutes.MapGet("/me", [Authorize] async (AppDbContext context, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
                return Results.Unauthorized();

            var currentUser = await context.Users.FindAsync(parsedUserId);

            if (currentUser == null)
                return Results.NotFound(new { message = "User not found." });

            return Results.Ok(new
            {
                currentUser.Id,
                currentUser.Name,
                currentUser.Email,
                currentUser.Transactions
            });
        });

        userRoutes.MapPost("/signup", async (AddUserRequest? request, AppDbContext context) =>
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return Results.BadRequest("All fields are required.");

            if (!PasswordValidator.IsValidPassword(request.Password))
                return Results.BadRequest("Invalid password.");

            var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
                return Results.Conflict("A user with this email already exists.");

            var passwordHasher = new PasswordHasher<User>();
            var hashedPassword = passwordHasher.HashPassword(null, request.Password);

            var newUser = new User(request.Name, request.Email, hashedPassword);
            context.Users.Add(newUser);

            await context.SaveChangesAsync();
            return Results.Created($"/users/{newUser.Id}", newUser);
        });

        userRoutes.MapPost("/login", async (LoginUserRequest request, AppDbContext context, HttpContext httpContext) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return Results.BadRequest("All fields are required.");

            var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser == null)
                return Results.NotFound("A user with this email was not found.");

            if (!PasswordValidator.VerifyPassword(existingUser, existingUser.Password, request.Password))
                return Results.Unauthorized();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, existingUser.Id.ToString()),
                new(ClaimTypes.Name, existingUser.Name),
                new(ClaimTypes.Email, existingUser.Email)
            };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync("Cookies", principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            });

            return Results.Ok(new { message = "Login successful" });
        });

        userRoutes.MapPut("/password", [Authorize] async (ChangePasswordRequest request, AppDbContext context, ClaimsPrincipal user) =>
        {
            if (!PasswordValidator.IsValidPassword(request.NewPassword))
                return Results.BadRequest("The new password does not meet the required complexity.");

            var userIdFromToken = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdFromToken) || !Guid.TryParse(userIdFromToken, out var parsedUserId))
                return Results.Unauthorized();

            var existingUser = await context.Users.FindAsync(parsedUserId);

            if (existingUser == null)
                return Results.NotFound("User not found.");

            var passwordHasher = new PasswordHasher<User>();
            existingUser.Password = passwordHasher.HashPassword(existingUser, request.NewPassword);

            await context.SaveChangesAsync();
            return Results.Ok("Password updated successfully.");
        });

        userRoutes.MapDelete("/logout", [Authorize] async (HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync("Cookies");
            return Results.Ok(new { message = "User logged out successfully." });
        });

        userRoutes.MapDelete("/delete-account", [Authorize] async (AppDbContext context, HttpContext httpContext) =>
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
                return Results.Unauthorized();

            var user = await context.Users.FindAsync(parsedUserId);
            if (user == null)
                return Results.NotFound(new { message = "User account not found." });

            context.Users.Remove(user);
            await context.SaveChangesAsync();

            await httpContext.SignOutAsync("Cookies");
            return Results.Ok(new { message = "Account deleted and user logged out successfully." });
        });
    }
}
