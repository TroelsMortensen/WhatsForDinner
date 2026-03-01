using Microsoft.AspNetCore.Components;
using UI.Models;

namespace UI.Pages.FrontPage;

public partial class RecipeListSection : ComponentBase
{
    [Parameter] public IReadOnlyList<RecipeIndex> Recipes { get; set; } = [];
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public string? ErrorMessage { get; set; }
}
