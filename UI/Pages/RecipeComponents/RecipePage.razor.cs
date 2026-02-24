using System.Globalization;
using Microsoft.AspNetCore.Components;
using UI.DataLoading;
using UI.Models;

namespace UI.Pages.RecipeComponents;

public partial class RecipePage : ComponentBase
{
    [Parameter] public int Id { get; set; }

    [Inject] private RecipeDataLoader RecipeDataLoader { get; set; } = default!;

    private Recipe? RecipeData { get; set; }
    private bool IsLoading { get; set; }
    private string? ErrorMessage { get; set; }
    private string PageTitle => RecipeData is null ? "Opskrift" : RecipeData.Title;

    protected override async Task OnParametersSetAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        RecipeData = null;

        try
        {
            RecipeIndex? recipeIndex = await RecipeDataLoader.FindRecipeIndexAsync(Id);
            if (recipeIndex is null)
            {
                ErrorMessage = "Opskriften blev ikke fundet.";
                return;
            }

            RecipeData = await RecipeDataLoader.LoadRecipeAsync(recipeIndex);
        }
        catch
        {
            ErrorMessage = "Der opstod en fejl ved indlæsning af opskriften.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static string FormatMinutes(int minutes)
    {
        if (minutes < 60)
        {
            return $"{minutes} MIN.";
        }

        if (minutes % 60 == 0)
        {
            int hours = minutes / 60;
            return $"{hours} TIME" + (hours > 1 ? "R" : "");
        }

        int wholeHours = minutes / 60;
        int remainingMinutes = minutes % 60;
        return $"{wholeHours} T {remainingMinutes} MIN.";
    }

    private static string FormatIngredient(Ingredient ingredient)
    {
        if (ingredient.Quantity is null)
        {
            return ingredient.Name;
        }

        string quantity = ingredient.Quantity.Value.ToString("0.##", CultureInfo.InvariantCulture);
        string unitPart = string.IsNullOrWhiteSpace(ingredient.Unit) ? string.Empty : $" {ingredient.Unit}";
        string notePart = string.IsNullOrWhiteSpace(ingredient.PreparationNote) ? string.Empty : $", {ingredient.PreparationNote}";

        return $"{quantity}{unitPart} {ingredient.Name}{notePart}";
    }
}