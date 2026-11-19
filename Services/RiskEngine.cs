using Sthairya.Models;

namespace Sthairya.Services;

/// <summary>
/// Abstraction for the AI model. Replace HeuristicAnomalyModel with an ONNX Runtime
/// implementation (QNN execution provider on Snapdragon, CPU fallback) when a trained model exists.
/// </summary>
public interface IAnomalyModel
{
    /// <summary>Returns an anomaly score from 0 (normal) to 1 (highly abnormal).</summary>
    double Score(VitalsReading r);
}

/// <summary>Placeholder model: a smooth score from how far each vital is from its normal range.</summary>
public class HeuristicAnomalyModel : IAnomalyModel
{
    public double Score(VitalsReading r)
    {
        var maxHr = r.Activity == "exercise" ? 160 : r.Activity == "walking" ? 110 : 95;
        var dev =
            Dist(r.HeartRate, 55, maxHr, 30) +
            Dist(r.Spo2, 95, 100, 5) * 1.5 +
            Dist(r.Temperature, 36.1, 37.5, 1.5) +
            Dist(r.Systolic, 100, 130, 40) +
            Dist(r.RespRate, 12, 20, 8);
        return 1.0 / (1.0 + Math.Exp(-(dev - 1.5) * 2.0));
    }

    private static double Dist(double v, double lo, double hi, double scale)
    {
        if (v < lo) return (lo - v) / scale;
        if (v > hi) return (v - hi) / scale;
        return 0;
    }
}

/// <summary>
/// Combines model output with fixed safety thresholds. The model can raise a level but never
/// lower one, so a model error cannot hide a dangerous reading.
/// THRESHOLDS ARE ILLUSTRATIVE and must be validated by medical professionals before real use.
/// </summary>
public class RiskEngine
{
    private readonly IAnomalyModel _model;
    public RiskEngine(IAnomalyModel model) => _model = model;

    public string? Validate(VitalsReading r)
    {
        if (!Between(r.HeartRate, 20, 250)) return "Heart rate out of valid range (20-250).";
        if (!Between(r.Spo2, 50, 100)) return "SpO2 out of valid range (50-100).";
        if (!Between(r.Temperature, 28, 45)) return "Temperature out of valid range (28-45).";
        if (!Between(r.Systolic, 40, 260)) return "Systolic pressure out of valid range (40-260).";
        if (!Between(r.Diastolic, 20, 160)) return "Diastolic pressure out of valid range (20-160).";
        if (!Between(r.RespRate, 4, 60)) return "Respiratory rate out of valid range (4-60).";
        if (r.Activity is not ("rest" or "walking" or "exercise")) return "Activity must be rest, walking or exercise.";
        return null;
    }

    public Assessment Assess(VitalsReading r)
    {
        var findings = new List<Finding>();
        var hrWarnHi = r.Activity == "exercise" ? 160 : r.Activity == "walking" ? 120 : 110;
        var hrCritHi = r.Activity == "exercise" ? 185 : r.Activity == "walking" ? 150 : 140;

        Check(findings, "Heart rate", r.HeartRate, $"{r.HeartRate:0} bpm",
            critLo: 40, warnLo: 50, warnHi: hrWarnHi, critHi: hrCritHi);
        Check(findings, "SpO2", r.Spo2, $"{r.Spo2:0}%",
            critLo: 90, warnLo: 94, warnHi: double.MaxValue, critHi: double.MaxValue);
        Check(findings, "Temperature", r.Temperature, $"{r.Temperature:0.0} °C",
            critLo: 35, warnLo: 36, warnHi: 38, critHi: 39.5);
        Check(findings, "Blood pressure", r.Systolic, $"{r.Systolic:0}/{r.Diastolic:0} mmHg",
            critLo: 80, warnLo: 90, warnHi: 160, critHi: 180);
        Check(findings, "Respiratory rate", r.RespRate, $"{r.RespRate:0} /min",
            critLo: 8, warnLo: 10, warnHi: 24, critHi: 30);

        var level = "Normal";
        if (findings.Any(f => f.Level == "Warning")) level = "Warning";
        if (findings.Any(f => f.Level == "Critical")) level = "Critical";

        var score = _model.Score(r);
        if (level == "Normal" && score >= 0.6)
        {
            level = "Warning";
            findings.Add(new Finding("AI model", "Warning",
                $"Combined pattern looks unusual (score {score:0.00}) even though each value is in range."));
        }

        // Rule-of-thumb patterns that matter in disasters.
        if (r.Temperature >= 39.5 && r.HeartRate > 120)
            findings.Add(new Finding("Pattern", "Critical", "High temperature with fast heart rate: possible heat stroke."));
        if (r.Systolic < 90 && r.HeartRate > 110)
            findings.Add(new Finding("Pattern", level == "Critical" ? "Critical" : "Warning",
                "Low blood pressure with fast heart rate: possible shock or dehydration."));
        if (findings.Any(f => f.Level == "Critical")) level = "Critical";

        return new Assessment(level, Math.Round(score, 3), findings, GuidanceFor(level, r));
    }

    private static string GuidanceFor(string level, VitalsReading r) => level switch
    {
        "Critical" => r.Spo2 < 90 ? "hypoxia"
                    : r.Temperature >= 39.5 ? "heat"
                    : r.Systolic < 90 ? "shock"
                    : "general-critical",
        "Warning" => "monitor",
        _ => "none"
    };

    private static void Check(List<Finding> list, string vital, double v, string shown,
        double critLo, double warnLo, double warnHi, double critHi)
    {
        if (v < critLo || v > critHi)
            list.Add(new Finding(vital, "Critical", $"{shown} is dangerously {(v < critLo ? "low" : "high")}."));
        else if (v < warnLo || v > warnHi)
            list.Add(new Finding(vital, "Warning", $"{shown} is {(v < warnLo ? "below" : "above")} the normal range."));
    }

    private static bool Between(double v, double lo, double hi) => !double.IsNaN(v) && v >= lo && v <= hi;
}
