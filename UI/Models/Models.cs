using Microsoft.AspNetCore.Components;

namespace UI.Models;

record Recipe(
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

record RecipeGroup(
    string Title,
    List<Ingredient> Ingredients,
    MarkupString Instructions
);

record Ingredient(
    double? Quantity,
    string? Unit,
    string Name,
    string? PreparationNote
);

class Keyword
{
    public string Value { get; private set; }
    
    private Keyword(string value) => Value = value;
    
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