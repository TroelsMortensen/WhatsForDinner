using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace UI.Models;

public record Recipe(
    int Id,
    string Title,
    string? SubHeader,
    int? TotalMinutes,
    int? MinutesOfPreparation,
    int? NumberOfPersons,
    List<RecipeGroup> RecipeGroups,
    MarkupString Instructions,
    string Note,
    string? ImageUrl,
    string? Source,
    List<string> Keywords
);

public record RecipeGroup(
    string Title,
    List<Ingredient> Ingredients
);

public record Ingredient(
    double? Quantity,
    string? Unit,
    string Name,
    string? PreparationNote
);

public record RecipeIndex(int Id, string Title, List<string> Keywords);

public static class Keyword
{
    public static IReadOnlyList<string> All { get; } = typeof(Keyword)
        .GetProperties(BindingFlags.Public | BindingFlags.Static)
        .Where(p => p.PropertyType == typeof(string) && p.Name != nameof(All))
        .Select(p => p.GetValue(null) as string)
        .Where(v => !string.IsNullOrWhiteSpace(v))
        .Cast<string>()
        .ToList()
        .AsReadOnly();

    public static string MOROCCAN { get; } = "Marokkansk";
    public static string SPANISH { get; } = "Spansk";
    public static string FRENCH { get; } = "Fransk";
    public static string ITALIAN { get; } = "Italiensk";
    public static string ONE_POT { get; } = "One Pot";
    public static string SLOW_COOKER { get; } = "Slow Cooker";
    public static string VEGETARIAN { get; } = "Vegetar";
    public static string FISH { get; } = "Fisk";
    public static string CHICKEN { get; } = "Kylling";
    public static string BEEF { get; } = "Oksekød";
    public static string SOUP { get; } = "Suppe";
    public static string PASTA { get; } = "Pasta";
    public static string ASIAN { get; } = "Asiatisk";
    public static string AIRFRYER { get; } = "Air Fryer";
}