# WhatsForDinner – add a new recipe
## Your task
You are helping with the **WhatsForDinner** project . When I provide **a photo of a recipe** (and/or a **URL**), you must:
1. **Extract** all relevant data (title, times, servings, ingredients, steps, notes, source).
2. **Create** a new recipe JSON file with the **next available Id** (I may specify it, or you determine it by reading `UI/wwwroot/recipes/index.json` and using `max(Id) + 1`).
3. **Update** `UI/wwwroot/recipes/index.json` with a new entry for this recipe.
4. **Rename the image file** (if there is an image) to match the convention below. Image files are by default named with just an Id, of type jpg, located in the `UI/wwwroot/images/recipe-images` folder. You must rename the file to match the convention below.

## Language
- Recipe text in JSON must be in **Danish** (titles may follow the source, e.g. English book titles, when that fits better).
- If the source is in English/German, **translate** into natural Danish.
## Filenames and paths
| What | Path / pattern |
|------|----------------|
| Recipe JSON | `UI/wwwroot/recipes/{Id}. {Title}.json` (space after the dot, same as existing files) |
| Image | `UI/wwwroot/images/recipe-images/{Id}. {Title}.jpg` |
| Index | `UI/wwwroot/recipes/index.json` |
- **ImageUrl** in JSON must be **relative with no leading `/`**, e.g. `images/recipe-images/155. Chillied Beef with Chocolate.jpg`.
- If there is **no** image: set `"ImageUrl": null` and **do not** rename any `*.jpg`.
## JSON structure (must match `UI/Models/Models.cs`)
`Recipe` has:
- `Id` (int)
- `Title` (string)
- `SubHeader` (string or `null`)
- `TotalMinutes` (int or `null`) – total time if known
- `MinutesOfPreparation` (int or `null`)
- `NumberOfPersons` (int or `null`)
- `RecipeGroups` – list of `{ "Title": "...", "Ingredients": [ ... ] }`
- `Instructions` – **HTML string** in one or more `<p>`, optionally `<h2>` / `<h3>` for sections (the app uses `MarkupString`)
- `Note` (string – can be `""` if nothing)
- `ImageUrl` (string or `null`)
- `Source` (string or `null`) – e.g. website or book title
- `Keywords` – list of strings
`Ingredient`:
- `Quantity` – number or `null` (use a dot as decimal separator in JSON, e.g. `0.5`)
- `Unit` – string or `null` (e.g. `"g"`, `"spsk"`, `"dl"`, `"fed"`)
- `Name` – ingredient name
- `PreparationNote` – string or `null` (prep / comment)
Use **valid JSON** (escape quotes in HTML where needed).
## index.json
Append **one** object to the array (before the closing `]`), using the same shape as existing entries:
```json
{
  "Id": <same as recipe>,
  "Title": "<same as recipe Title>",
  "Keywords": [ "...", "..." ],
  "TotalMinutes": <same as recipe TotalMinutes or null>
}
```

