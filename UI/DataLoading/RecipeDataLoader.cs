using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using UI.Models;

namespace UI.DataLoading;

public class RecipeDataLoader(HttpClient httpClient, JsonSerializerOptions jsonSerializerOptions)
{
    private const int RecipeCacheLimit = 5;

    private IReadOnlyList<RecipeIndex> _recipeIndices = [];
    private readonly Queue<Recipe> recipeCacheQueue = new();

    public async Task<IReadOnlyList<RecipeIndex>> GetAllRecipeIndicesAsync()
    {
        if (_recipeIndices.Any())
        {
            return _recipeIndices;
        }

        List<RecipeIndex>? indices =
            await httpClient.GetFromJsonAsync<List<RecipeIndex>>("recipes/index.json", jsonSerializerOptions);

        _recipeIndices = indices ?? [];
        return _recipeIndices;
    }

    public async Task<RecipeIndex?> FindRecipeIndexAsync(int recipeId)
    {
        IReadOnlyList<RecipeIndex> indices = await GetAllRecipeIndicesAsync();
        string expectedPrefix = recipeId.ToString(CultureInfo.InvariantCulture) + ".";

        return indices.FirstOrDefault(index =>
            index.Id.EndsWith(".json", StringComparison.OrdinalIgnoreCase) &&
            index.Id.StartsWith(expectedPrefix, StringComparison.Ordinal));
    }

    public async Task<Recipe?> LoadRecipeAsync(RecipeIndex recipeIndex)
    {
        int? requestedRecipeId = TryGetRecipeIdFromIndexFileName(recipeIndex.Id);
        if (!requestedRecipeId.HasValue)
        {
            return null;
        }

        Recipe? recipe = TryGetFromCache(requestedRecipeId.Value);
        if (recipe is not null)
        {
            return recipe;
        }

        Recipe? loadedRecipe = await TryLoadRecipe(recipeIndex);
        if (loadedRecipe is null)
        {
            return null;
        }

        CacheRecipe(loadedRecipe);

        return loadedRecipe;
    }

    private void CacheRecipe(Recipe loadedRecipe)
    {
        recipeCacheQueue.Enqueue(loadedRecipe);

        while (recipeCacheQueue.Count > RecipeCacheLimit)
        {
            _ = recipeCacheQueue.Dequeue();
        }
    }

    private async Task<Recipe?> TryLoadRecipe(RecipeIndex recipeIndex)
    {
        string recipePath = $"recipes/{Uri.EscapeDataString(recipeIndex.Id)}";
        Recipe? loadedRecipe = await httpClient.GetFromJsonAsync<Recipe>(recipePath, jsonSerializerOptions);
        return loadedRecipe;
    }

    private Recipe? TryGetFromCache(int requestedRecipeId)
    {
        return recipeCacheQueue.FirstOrDefault(recipe => recipe.Id == requestedRecipeId);
    }

    private static int? TryGetRecipeIdFromIndexFileName(string indexFileName)
    {
        int separator = indexFileName.IndexOf('.');
        if (separator <= 0)
        {
            return null;
        }

        string prefix = indexFileName[..separator];
        return int.TryParse(prefix, NumberStyles.Integer, CultureInfo.InvariantCulture, out int recipeId)
            ? recipeId
            : null;
    }
}