using BLL.Services.Implementations;
using BLL.Services.Interfaces;
using DAL.Data;
using DAL.Repositories.Implementations;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
	// Ngắt vòng lặp vô tận khi convert Entity sang JSON
	options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("AppDbContext");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<IMarketRepository, MarketRepository>();
builder.Services.AddScoped<IMarketService, MarketService>();
builder.Services.AddScoped<IVendorRepository, VendorRepository>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin()
			  .AllowAnyMethod()
			  .AllowAnyHeader();
	});
});

var app = builder.Build();




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting(); // 1. BỔ SUNG DÒNG NÀY: Phải có Routing trước để hệ thống biết API đang đi đâu

app.UseCors("AllowAll"); // 2. CORS PHẢI NẰM NGAY ĐÂY: Dưới Routing và trên Authorization

app.UseAuthorization();

app.MapControllers();

app.Run();
