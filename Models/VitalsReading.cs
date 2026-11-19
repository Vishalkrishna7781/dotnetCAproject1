namespace Sthairya.Models;

/// <summary>One set of vitals, sent by the browser to /api/assess.</summary>
public record VitalsReading(
    double HeartRate,
    double Spo2,
    double Temperature,
    double Systolic,
    double Diastolic,
    double RespRate,
    string Activity);   // "rest", "walking" or "exercise"
