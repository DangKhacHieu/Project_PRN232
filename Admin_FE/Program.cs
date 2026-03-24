
var builder = WebApplication.CreateBuilder(args);
// 1. ??ng ký d?ch v? HttpClient c? b?n (C?n thi?t ?? resolve IHttpClientFactory)

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient();

// 2. ??ng ký HttpClient có tên "MyAPI" v?i BaseAddress (nh? ?ã bàn ? các b??c tr??c)
builder.Services.AddHttpClient("MyAPI", client =>
{
    // L?y URL t? appsettings.json ho?c ghi tr?c ti?p
    client.BaseAddress = new Uri("https://localhost:7169/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=MarketMap}/{action=Index}/{id?}");

app.Run();
