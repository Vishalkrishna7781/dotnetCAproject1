using Microsoft.AspNetCore.Mvc;
using Sthairya.Models;
using Sthairya.Services;

namespace Sthairya.Controllers;

/// <summary>Serves the pages. Each action picks a view and hands it the data it needs.</summary>
public class HomeController : Controller
{
    private readonly FirstAidLibrary _firstAid;

    // The framework supplies FirstAidLibrary (dependency injection).
    public HomeController(FirstAidLibrary firstAid) => _firstAid = firstAid;

    public IActionResult Index() => View(DashboardViewModel.Create());   // Views/Home/Index.cshtml
    public IActionResult FirstAid() => View(_firstAid.All);              // Views/Home/FirstAid.cshtml
    public IActionResult Privacy() => View();
    public IActionResult Architecture() => View();
}
