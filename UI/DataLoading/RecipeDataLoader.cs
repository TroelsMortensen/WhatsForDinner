using System.Net.Http.Json;
using System.Text.Json;
using UI.Models;

namespace UI.DataLoading;

public class RecipeDataLoader(HttpClient httpClient, JsonSerializerOptions jsonSerializerOptions)
{
    private const int RecipeCacheLimit = 5;

    private IReadOnlyList<RecipeIndex> _recipeIndices = [];
    private readonly Queue<Recipe> _recipeCacheQueue = new();

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
        return indices.FirstOrDefault(index => index.Id == recipeId);
    }

    public async Task<Recipe?> LoadRecipeAsync(RecipeIndex recipeIndex)
    {
        Recipe? recipe = TryGetFromCache(recipeIndex.Id);
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
        _recipeCacheQueue.Enqueue(loadedRecipe);

        while (_recipeCacheQueue.Count > RecipeCacheLimit)
        {
            _ = _recipeCacheQueue.Dequeue();
        }
    }

    private async Task<Recipe?> TryLoadRecipe(RecipeIndex recipeIndex)
    {
        string fileName = $"{recipeIndex.Id}. {recipeIndex.Title}.json";
        string recipePath = $"recipes/{Uri.EscapeDataString(fileName)}";
        Recipe? loadedRecipe = await httpClient.GetFromJsonAsync<Recipe>(recipePath, jsonSerializerOptions);
        return loadedRecipe;
    }

    private Recipe? TryGetFromCache(int requestedRecipeId)
    {
        return _recipeCacheQueue.FirstOrDefault(recipe => recipe.Id == requestedRecipeId);
    }
}