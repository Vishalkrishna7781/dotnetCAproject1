using Sthairya.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC: controllers + Razor views.
builder.Services.AddControllersWithViews();

// Dependency injection. Singletons are fine: these services hold no per-user state.
builder.Services.AddSingleton<IAnomalyModel, HeuristicAnomalyModel>(); // swap for an ONNX model later
builder.Services.AddSingleton<RiskEngine>();
builder.Services.AddSingleton<FirstAidLibrary>();

// Local-only server: listen on loopback unless configured otherwise.
if (string.IsNullOrEmpty(builder.Configuration["urls"]) &&
    string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.UseUrls("http://localhost:5080");
}

var app = builder.Build();

// Security headers: nothing is loaded from third-party origins.
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    ctx.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; img-src 'self' data:; style-src 'self'; script-src 'self'";
    await next();
});

app.UseStaticFiles();   // serves wwwroot (css, js)
app.UseRouting();

// Routing: "/" -> Home/Index, "/FirstAid" -> Home/FirstAid, "/Privacy" -> Home/Privacy ...
app.MapControllerRoute(
    name: "default",
    pattern: "{action=Index}/{id?}",
    defaults: new { controller = "Home" });

app.MapControllers();   // attribute-routed API controller (api/assess)

app.Run();
