var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

//app.MapGet("/", () => "Hello World!");
//app.Run(async (context) =>
//{
//    await context.Response.WriteAsync("Welcome to ASP.NET Core 8");
//});
app.Use(async (context,Next) =>
{
    await context.Response.WriteAsync("Welcome to ASP.NET Core 8 \n");
    await Next(context);
});
app.Run(async (context) =>
{
    await context.Response.WriteAsync("Devarsh Trivedi");
});
app.Run();
