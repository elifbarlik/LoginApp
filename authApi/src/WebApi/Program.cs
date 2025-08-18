using Application.Abstractions;
using Application.Auth;
using Application.Admin;
using Application.User;
using Infrastructure.Auth;
using Infrastructure.Admin;
using Infrastructure.User;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
	var connectionString = configuration.GetConnectionString("Default") ?? "Data Source=auth.db";
	options.UseSqlite(connectionString);
});

// DI registrations
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IRefreshTokenValidator, RefreshTokenValidator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IUserService, UserService>();

// JWT Auth
var jwtSection = configuration.GetSection("Jwt");
var secret = jwtSection["SecretKey"] ?? "dev-secret-change-me-please-1234567890";
var issuer = jwtSection["Issuer"] ?? "authapi";
var audience = jwtSection["Audience"] ?? "authapi.clients";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

builder.Services
	.AddAuthentication(options =>
	{
		options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	})
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = key,
			ValidateIssuer = true,
			ValidIssuer = issuer,
			ValidateAudience = true,
			ValidAudience = audience,
			ValidateLifetime = true,
			ClockSkew = TimeSpan.Zero
		};

		// Claim mapping ekle
		options.MapInboundClaims = false;
		options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Sub;
		options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;

		// Friendly auth errors for frontend alerts
		options.Events = new JwtBearerEvents
		{
			OnChallenge = context =>
			{
				context.HandleResponse();
				context.Response.StatusCode = 401;
				return context.Response.WriteAsJsonAsync(new { message = "Authentication required or token invalid" });
			},
			OnForbidden = context =>
			{
				context.Response.StatusCode = 403;
				return context.Response.WriteAsJsonAsync(new { message = "You do not have permission to perform this action" });
			}
		};
	});

builder.Services.AddAuthorization(options =>
{
	// Role-based policies
	options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
	options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
});

builder.Services.AddControllers()
	.ConfigureApiBehaviorOptions(options =>
	{
		options.InvalidModelStateResponseFactory = context =>
		{
			var errors = context.ModelState
				.Where(ms => ms.Value?.Errors.Count > 0)
				.Select(ms => new
				{
					field = ms.Key,
					messages = ms.Value!.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value" : e.ErrorMessage)
				});

			return new BadRequestObjectResult(new
			{
				message = "Validation failed",
				errors
			});
		};
	});

// Add CORS for frontend
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend", policy =>
	{
		policy.WithOrigins("http://localhost:3000", "http://localhost:4200", "http://localhost:8080")
			  .AllowAnyHeader()
			  .AllowAnyMethod()
			  .AllowCredentials();
	});
});

var app = builder.Build();

// Middleware
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// Global exception handling
app.UseExceptionHandler("/error");

// Return JSON for status code pages (e.g., 401/403/404)
app.UseStatusCodePages(async context =>
{
	var status = context.HttpContext.Response.StatusCode;
	var message = status switch
	{
		401 => "Authentication required or token invalid",
		403 => "You do not have permission to perform this action",
		404 => "Endpoint not found",
		_ => $"HTTP {status} error"
	};
	await context.HttpContext.Response.WriteAsJsonAsync(new { message });
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new { status = "Healthy", timestamp = DateTime.UtcNow });

app.Run();
