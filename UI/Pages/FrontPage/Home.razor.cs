using Microsoft.AspNetCore.Components;
using UI.DataLoading;
using UI.Models;

namespace UI.Pages.FrontPage;

public partial class Home : ComponentBase
{
    [Inject] private RecipeDataLoader RecipeDataLoader { get; set; } = null!;

    private IReadOnlyList<RecipeIndex> RecipeIndices { get; set; } = [];
    private bool IsLoading { get; set; }
    private string? ErrorMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        RecipeIndices = [];

        try
        {
            IReadOnlyList<RecipeIndex> indices = await RecipeDataLoader.GetAllRecipeIndicesAsync();
            RecipeIndices = indices.OrderBy(index => index.Id).ToList();
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
}
