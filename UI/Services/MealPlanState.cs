using UI.Models;

namespace UI.Services;

/// <summary>
/// Scoped state that holds the generated meal plan so it persists across navigation.
/// </summary>
public class MealPlanState
{
    public IReadOnlyList<RecipeIndex> CurrentPlan { get; set; } = [];
}
