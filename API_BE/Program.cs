using System.Text;
using System.Text.Json.Serialization;
using BLL.Services.Implementations;
using BLL.Services.Interfaces;
using DAL.Data;
using DAL.Repositories.Implementations;
using DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CloudinaryDotNet; // Thêm cái này
using API_BE.Helpers;   // Thêm cái này để lấy CloudinarySettings

var builder = WebApplication.CreateBuilder(args);

// CRITICAL: Disable ASP.NET Core's default JWT claim name remapping.
// Without this, claim names like "VendorId" and "role" get transformed into
// long Microsoft URI strings, making User.Claims.FirstOrDefault(c => c.Type == "VendorId") always return null.
System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

// --- 1. CONFIG CONTROLLERS & JSON ---
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- 2. CONFIG DATABASE ---
var connectionString = builder.Configuration.GetConnectionString("AppDbContext");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- 3. CONFIG CLOUDINARY (PHẦN BỊ THIẾU) ---
var cloudinarySettings = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
if (cloudinarySettings != null)
{
    var account = new Account(
        cloudinarySettings.CloudName,
        cloudinarySettings.ApiKey,
        cloudinarySettings.ApiSecret
    );
    var cloudinary = new Cloudinary(account);
    builder.Services.AddSingleton(cloudinary); // Đăng ký đối tượng Cloudinary vào DI Container
}

// --- 4. REGISTER REPOSITORIES & SERVICES (DI) ---
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMarketRepository, MarketRepository>();
builder.Services.AddScoped<IMarketService, MarketService>();
builder.Services.AddScoped<IVendorRepository, VendorRepository>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IVendorProfileRepository, VendorProfileRepository>();
builder.Services.AddScoped<IVendorProfileService, VendorProfileService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();

// Finance/Admin Dependencies
builder.Services.AddScoped<IUtilityReadingRepository, UtilityReadingRepository>();
builder.Services.AddScoped<IUtilityReadingService, UtilityReadingService>();
builder.Services.AddScoped<IFeeConfigRepository, FeeConfigRepository>();
builder.Services.AddScoped<IStallContractRepository, StallContractRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Quan trọng: PhotoService cần Cloudinary đã đăng ký ở mục 3
builder.Services.AddScoped<IPhotoService, PhotoService>();

// --- 5. CONFIG CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// --- 6. JWT AUTHENTICATION ---
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = jwtSection.GetValue<string>("Key") ?? string.Empty;
var issuer = jwtSection.GetValue<string>("Issuer") ?? string.Empty;
var audience = jwtSection.GetValue<string>("Audience") ?? string.Empty;
var keyBytes = Encoding.UTF8.GetBytes(key);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Đổi thành false nếu chạy local không có SSL certificate thật
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = !string.IsNullOrEmpty(issuer),
        ValidIssuer = issuer,
        ValidateAudience = !string.IsNullOrEmpty(audience),
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero, // Loại bỏ thời gian trễ mặc định của token
        RoleClaimType = "role"
    };
});

var app = builder.Build();

// --- 7. MIDDLEWARE PIPELINE ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Thứ tự cực kỳ quan trọng: Routing -> CORS -> Auth
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();