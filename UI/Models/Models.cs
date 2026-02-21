using Microsoft.AspNetCore.Components;

namespace UI.Models;

record Recipe(
    int Id,
    string Title,
    string? SubHeader,
    int? TotalHours,
    int? TotalMinutes,
    int? HoursOfPreparation,
    int? MinutesOfPreparation,
    int? Persons,
    List<RecipeGroup> RecipeGroups,
    MarkupString Instructions,
    string Note,
    string? ImageUrl
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