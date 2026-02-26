using Microsoft.AspNetCore.Components;
using UI.DataLoading;
using UI.Models;

namespace UI.Pages.FrontPage;

public partial class Home : ComponentBase
{
    [Inject] private RecipeDataLoader RecipeDataLoader { get; set; } = null!;

    private IReadOnlyList<HomeRecipeListItem> RecipeItems { get; set; } = [];
    private IReadOnlyList<HomeRecipeListItem> WeeklyPlanItems { get; set; } = [];
    private int? PlannerRecipeCount { get; set; } = 5;
    private bool IsLoading { get; set; }
    private string? ErrorMessage { get; set; }
    private string? WeeklyPlanErrorMessage { get; set; }

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

    private void GenerateWeeklyPlan()
    {
        WeeklyPlanItems = [];
        WeeklyPlanErrorMessage = null;

        if (IsLoading)
        {
            WeeklyPlanErrorMessage = "Vent med at lave madplan til opskrifterne er indlæst.";
            return;
        }

        if (PlannerRecipeCount is null || PlannerRecipeCount < 1)
        {
            WeeklyPlanErrorMessage = "Vælg et antal opskrifter på mindst 1.";
            return;
        }

        if (PlannerRecipeCount > RecipeItems.Count)
        {
            WeeklyPlanErrorMessage = $"Der findes kun {RecipeItems.Count} opskrifter. Vælg et lavere antal.";
            return;
        }

        WeeklyPlanItems = RecipeItems
            .OrderBy(_ => Random.Shared.Next())
            .Take(PlannerRecipeCount.Value)
            .ToList();
    }

    private sealed record HomeRecipeListItem(int Id, string Title);
}
