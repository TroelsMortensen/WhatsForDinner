using Microsoft.AspNetCore.Components;
using UI.DataLoading;
using UI.Models;

namespace UI.Pages.FrontPage;

public partial class Home : ComponentBase
{
    [Inject] private RecipeDataLoader RecipeDataLoader { get; set; } = null!;

    private IReadOnlyList<HomeRecipeListItem> RecipeItems { get; set; } = [];
    private bool IsLoading { get; set; }
    private string? ErrorMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        RecipeItems = [];

        try
        {
            IReadOnlyList<RecipeIndex> indices = await RecipeDataLoader.GetAllRecipeIndicesAsync();
            RecipeItems = indices
                .Select(index => new HomeRecipeListItem(index.Id, index.Title))
                .OrderBy(item => item.Id)
                .ToList();
        }
        catch
        {
            ErrorMessage = "Kunne ikke indlæse opskrifter.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private sealed record HomeRecipeListItem(int Id, string Title);
}
