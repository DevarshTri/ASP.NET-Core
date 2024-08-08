var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
var app = builder.Build();

// Convention based Routing

//app.MapGet("/", () => "Hello World!");

//app.MapDefaultControllerRoute();

app.MapControllerRoute(
    name: "Default",
    pattern: "{controller=Home}/{action=About}/{id?}");

// Attribute based Routing

app.MapControllers();

app.Run();
