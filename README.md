# Sthairya - Health Companion (ASP.NET Core MVC, .NET 8)

## Run
    dotnet run
Open http://localhost:5080 (binds to localhost only).

## Structure
- Controllers/HomeController.cs   pages (Index, FirstAid, Privacy, Architecture)
- Controllers/AssessController.cs POST /api/assess (JSON)
- Models/                         VitalsReading, Assessment, FirstAidTopic, DashboardViewModel
- Services/RiskEngine.cs          validation, thresholds, Normal/Warning/Critical, IAnomalyModel
- Services/FirstAidLibrary.cs     offline first-aid content
- Views/                          Razor views + shared layout
- wwwroot/                        site.css and JavaScript

Thresholds are illustrative and need clinical validation. Not a medical device.
