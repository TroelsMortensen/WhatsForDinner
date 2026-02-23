using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using UI.Models;

namespace UI.Pages.RecipeComponents;

public partial class RecipePage : ComponentBase
{
    private static IReadOnlyList<RecipeIndex> recipeFileNames = [];

    [Parameter] public int Id { get; set; }

    [Inject] private HttpClient HttpClient { get; set; } = default!;

    [Inject] private JsonSerializerOptions JsonSerializerOptions { get; set; } = default!;

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
            RecipeIndex? recipeFileName = await FindRecipeFileNameAsync(Id);
            if (recipeFileName is null)
            {
                return;
            }

            string recipePath = $"recipes/{Uri.EscapeDataString(recipeFileName.Id)}";
            RecipeData = await HttpClient.GetFromJsonAsync<Recipe>(recipePath, JsonSerializerOptions);
        }
        catch
        {
            ErrorMessage = "Der opstod en fejl ved indlæsning af opskriften.";
        }

        IsLoading = false;
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

    private async Task<RecipeIndex?> FindRecipeFileNameAsync(int recipeId)
    {
        IReadOnlyList<RecipeIndex> indices = await GetAllRecipeFileNamesAsync();
        string expectedPrefix = recipeId.ToString(CultureInfo.InvariantCulture) + ".";

        return indices.FirstOrDefault(fileName =>
            fileName.Id.EndsWith(".json", StringComparison.OrdinalIgnoreCase) &&
            fileName.Id.StartsWith(expectedPrefix, StringComparison.Ordinal));
    }

    private async Task<IReadOnlyList<RecipeIndex>> GetAllRecipeFileNamesAsync()
    {
        if (recipeFileNames.Any())
        {
            return recipeFileNames;
        }

        List<RecipeIndex>? fileNames = await HttpClient.GetFromJsonAsync<List<RecipeIndex>>("recipes/index.json");
        recipeFileNames = fileNames ?? [];
        return recipeFileNames;
    }
}