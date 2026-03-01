using Microsoft.AspNetCore.Components;
using UI.Models;

namespace UI.Pages.FrontPage;

public partial class RecipeListSection : ComponentBase
{
    [Parameter] public IReadOnlyList<RecipeIndex> Recipes { get; set; } = [];
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public string? ErrorMessage { get; set; }

    private readonly HashSet<string> _selectedKeywords = [];
    private readonly HashSet<string> _selectedTimeRanges = [];
    private IReadOnlyList<string>? _availableKeywords;

    private static readonly IReadOnlyList<TimeRange> TimeRanges =
    [
        new TimeRange("under-30", "Under 30 min", MaxExclusive: 30),
        new TimeRange("30-60", "30-60 min", MinInclusive: 30, MaxInclusive: 60),
        new TimeRange("over-60", "Over 60 min", MinExclusive: 60),
    ];

    protected override void OnParametersSet()
    {
        if (Recipes.Count > 0 && _availableKeywords is null)
        {
            _availableKeywords = Recipes
                .SelectMany(r => r.Keywords ?? [])
                .Distinct()
                .OrderBy(k => k)
                .ToList();
        }
    }

    private IReadOnlyList<RecipeIndex> FilteredRecipes
    {
        get
        {
            var byKeywords = _selectedKeywords.Count == 0
                ? Recipes
                : Recipes.Where(r => _selectedKeywords.All(k => (r.Keywords ?? []).Contains(k))).ToList();

            if (_selectedTimeRanges.Count == 0)
            {
                return byKeywords;
            }

            return byKeywords.Where(r =>
            {
                if (r.TotalMinutes is not { } minutes)
                {
                    return false;
                }

                return _selectedTimeRanges.Any(rangeId =>
                {
                    var range = TimeRanges.First(t => t.Id == rangeId);
                    if (range.MinInclusive is { } min && minutes < min)
                        return false;
                    if (range.MinExclusive is { } minEx && minutes <= minEx)
                        return false;
                    if (range.MaxInclusive is { } max && minutes > max)
                        return false;
                    if (range.MaxExclusive is { } maxEx && minutes >= maxEx)
                        return false;
                    return true;
                });
            }).ToList();
        }
    }

    private bool HasActiveFilters => _selectedKeywords.Count > 0 || _selectedTimeRanges.Count > 0;

    private void ToggleKeyword(string keyword)
    {
        if (_selectedKeywords.Contains(keyword))
        {
            _selectedKeywords.Remove(keyword);
        }
        else
        {
            _selectedKeywords.Add(keyword);
        }

        StateHasChanged();
    }

    private void ToggleTimeRange(string rangeId)
    {
        if (_selectedTimeRanges.Contains(rangeId))
        {
            _selectedTimeRanges.Remove(rangeId);
        }
        else
        {
            _selectedTimeRanges.Add(rangeId);
        }

        StateHasChanged();
    }

    private sealed record TimeRange(
        string Id,
        string Label,
        int? MinInclusive = null,
        int? MinExclusive = null,
        int? MaxInclusive = null,
        int? MaxExclusive = null);
}
