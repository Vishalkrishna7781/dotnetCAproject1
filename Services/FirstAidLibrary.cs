using Sthairya.Models;

namespace Sthairya.Services;

/// <summary>Offline first-aid content, stored in code so it needs no internet or database.</summary>
public class FirstAidLibrary
{
    public IReadOnlyList<FirstAidTopic> All { get; } = new List<FirstAidTopic>
    {
        new("heat", "Heat stroke",
            "Very high temperature, fast pulse, confusion, hot dry or very sweaty skin.",
            new[] {
                "Move the person to shade or a cool place.",
                "Remove extra clothing.",
                "Cool the body with wet cloths, fanning, or cool water on neck, armpits and groin.",
                "If fully awake, give small sips of cool water.",
                "Do not give anything by mouth if they are drowsy or confused." }),
        new("hypoxia", "Low oxygen / breathing difficulty",
            "Low SpO2, fast breathing, blue lips, dizziness.",
            new[] {
                "Move to fresh air, away from smoke, dust or fumes.",
                "Sit upright and loosen tight clothing.",
                "Encourage slow, steady breaths.",
                "Use the person's prescribed inhaler or oxygen if they have one.",
                "Recheck the reading after a few minutes and seek help if it stays low." }),
        new("shock", "Shock after injury or dehydration",
            "Low blood pressure, fast weak pulse, pale cold skin, dizziness.",
            new[] {
                "Lay the person down. Raise the legs a little if there is no leg or spine injury.",
                "Press firmly on any bleeding with a clean cloth.",
                "Keep them warm with a blanket or coat.",
                "Do not give food or drink if serious injury is possible.",
                "Stay with them and keep checking breathing." }),
        new("general-critical", "Other critical readings",
            "Very high or low heart rate, breathing rate or temperature.",
            new[] {
                "Call for help and keep the person still and calm.",
                "Check for breathing and responsiveness.",
                "If they are not breathing normally and you are trained, start CPR.",
                "Show responders their blood group, allergies and conditions." }),
        new("monitor", "Warning level: rest and recheck",
            "An unusual pattern that is not yet an emergency.",
            new[] {
                "Stop activity and rest in a cool, safe place.",
                "Drink water if conscious and not injured.",
                "Recheck vitals in 5 to 10 minutes.",
                "If readings get worse or symptoms appear, treat it as critical." }),
    };
}
