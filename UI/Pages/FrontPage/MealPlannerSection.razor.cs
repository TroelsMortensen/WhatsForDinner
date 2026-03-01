using Microsoft.AspNetCore.Components;
using UI.Models;

namespace UI.Pages.FrontPage;

public partial class MealPlannerSection : ComponentBase
{
    [Parameter] public IReadOnlyList<RecipeIndex> AllRecipes { get; set; } = [];
    [Parameter] public bool IsLoading { get; set; }

    private int? PlannerRecipeCount { get; set; } = 5;
    private IReadOnlyList<RecipeIndex> WeeklyPlanItems { get; set; } = [];
    private string? WeeklyPlanErrorMessage { get; set; }

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

        if (PlannerRecipeCount > AllRecipes.Count)
        {
            WeeklyPlanErrorMessage = $"Der findes kun {AllRecipes.Count} opskrifter. Vælg et lavere antal.";
            return;
        }

        WeeklyPlanItems = AllRecipes
            .OrderBy(_ => Random.Shared.Next())
            .Take(PlannerRecipeCount.Value)
            .ToList();
    }
}
