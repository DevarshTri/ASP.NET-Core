var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/Home", async (context) =>
    {
        await context.Response.WriteAsync("Home Page...GET");
    });
    endpoints.MapPost("/Home", async (context) =>
    {
        await context.Response.WriteAsync("Home Page...POST");
    });
    endpoints.MapPut("/Home", async (context) =>
    {
        await context.Response.WriteAsync("Home Page...Put");
    });
    endpoints.MapDelete("/Home", async (context) =>
    {
        await context.Response.WriteAsync("Home Page...Delete");
    });
});

app.Run(async (HttpContext context) =>
{
    await context.Response.WriteAsync("Page Not Found");
});
//app.MapGet("/Home", () => "Hello World!-GET METHOD");
//app.MapPost("/Home", () => "Hello World!-POST METHOD");
//app.MapPut("/Home", () => "Hello World!-PUT METHOD");
//app.MapDelete("/Home", () => "Hello World!-DELETE METHOD");

//app.Map("/Home", () => "Hello Devarsh !");

app.Run();
