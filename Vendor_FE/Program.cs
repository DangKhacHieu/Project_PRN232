var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// ĐĂNG KÝ HTTP CLIENT ĐỂ GỌI API BE
builder.Services.AddHttpClient("BackendAPI", client =>
{
    // Lấy URL từ appsettings.json
    client.BaseAddress = new Uri(builder.Configuration["ApiUrls:BackendApiBaseUrl"]);
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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
