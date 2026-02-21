using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;

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
    List<Keyword> Keywords
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

public class Keyword
{
    public string Value { get; private set; }
    
    [JsonConstructor]
    public Keyword(string value) => Value = value;
    
    public static Keyword MOROCCAN { get; } = new ("Marokkansk");
    public static Keyword SPANISH { get; } = new ("Spansk");
    public static Keyword FRENCH { get; } = new ("Fransk");
    public static Keyword ITALIAN { get; } = new ("Italiensk");
    public static Keyword ONE_POT { get; } = new ("One Pot");
    public static Keyword SLOW_COOKER { get; } = new ("Slow Cooker");
    public static Keyword VEGETARIAN { get; } = new ("Vegetar");
    public static Keyword FISH { get; } = new ("Fisk");
    public static Keyword CHICKEN { get; } = new ("Kylling");
    public static Keyword BEEF { get; } = new ("Oksekød");
    public static Keyword SOUP { get; } = new ("Suppe");
    public static Keyword PASTA { get; } = new ("Pasta");
    public static Keyword ASIAN { get; } = new ("Asiatisk");
}