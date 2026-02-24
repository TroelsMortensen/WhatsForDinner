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
    private int? SelectedPersons { get; set; }
    private string PageTitle => RecipeData is null ? "Opskrift" : RecipeData.Title;

    protected override async Task OnParametersSetAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        RecipeData = null;
        SelectedPersons = null;

        try
        {
            RecipeIndex? recipeIndex = await RecipeDataLoader.FindRecipeIndexAsync(Id);
            if (recipeIndex is null)
            {
                ErrorMessage = "Opskriften blev ikke fundet.";
                return;
            }

            RecipeData = await RecipeDataLoader.LoadRecipeAsync(recipeIndex);
            if (RecipeData?.NumberOfPersons is > 0)
            {
                SelectedPersons = RecipeData.NumberOfPersons;
            }
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

    private void IncrementPersons()
    {
        if (!CanAdjustPersons())
        {
            return;
        }

        SelectedPersons = CurrentPersons() + 1;
    }

    private void DecrementPersons()
    {
        if (!CanAdjustPersons())
        {
            return;
        }

        SelectedPersons = Math.Max(1, CurrentPersons() - 1);
    }

    private bool CanAdjustPersons()
    {
        return RecipeData?.NumberOfPersons is > 0;
    }

    private int CurrentPersons()
    {
        if (SelectedPersons is > 0)
        {
            return SelectedPersons.Value;
        }

        return RecipeData?.NumberOfPersons is > 0 ? RecipeData.NumberOfPersons.Value : 1;
    }

    private double ScaleQuantity(double baseQuantity)
    {
        if (!CanAdjustPersons())
        {
            return baseQuantity;
        }

        int basePersons = RecipeData!.NumberOfPersons!.Value;
        int selectedPersons = CurrentPersons();
        return baseQuantity * selectedPersons / basePersons;
    }

    private string FormatIngredient(Ingredient ingredient)
    {
        if (ingredient.Quantity is null)
        {
            return ingredient.Name;
        }

        double scaledQuantity = ScaleQuantity(ingredient.Quantity.Value);
        string quantity = scaledQuantity.ToString("0.##", CultureInfo.InvariantCulture);
        string unitPart = string.IsNullOrWhiteSpace(ingredient.Unit) ? string.Empty : $" {ingredient.Unit}";
        string notePart = string.IsNullOrWhiteSpace(ingredient.PreparationNote) ? string.Empty : $", {ingredient.PreparationNote}";

        return $"{quantity}{unitPart} {ingredient.Name}{notePart}";
    }
}