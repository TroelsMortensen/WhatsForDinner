# WhatsForDinner – add a new recipe

## Agent checklist (do in order, then stop)
1. Read the photo(s) / URL with built-in vision only — no OCR tools.
2. Get next Id: `max(Id)` in `UI/wwwroot/recipes/index.json` (+1), unless user gave an Id.
3. Optionally open **one** recent similar recipe JSON for shape/conventions.
4. Write `UI/wwwroot/recipes/{Id}. {Title}.json`.
5. Append one object to `UI/wwwroot/recipes/index.json`.
6. If `UI/wwwroot/images/recipe-images/{Id}.jpg` exists: rename to `{Id}. {Title}.jpg`.
7. Stop. Do not polish further.

**Done when:** recipe JSON + index entry exist, image renamed (or `ImageUrl: null`), and you have not installed tools or searched the web.

## Your task
When I provide a **photo** of a recipe (and/or a **URL**):
1. **Extract** title, times, servings, ingredients, steps, notes, source.
2. **Create** the recipe JSON with the next Id (or the Id I specify).
3. **Update** `index.json`.
4. **Rename** the dish photo `{Id}.jpg` → `{Id}. {Title}.jpg` when present.

Prefer Agent mode. Aim for **few tool rounds**. Parallelize independent reads; then write.

## Accuracy policy
| Field | Bar |
|-------|-----|
| Title, quantities, units, steps, times, servings, ingredient groups | Match the source closely |
| Instructions | Faithful Danish HTML (`<p>`, optional `<h2>`/`<h3>`) |
| Keywords | Normalized tags; match style of recent recipes |
| Source | Exact site/book if known; else infer from recent same-book conversions |
| SubHeader, Note | Faithful meaning. If text is blurry: **one** best-effort Danish paraphrase is OK |

Do **not** spend extra turns perfecting SubHeader wording, punctuation, or OCR.

## Do not
- Install or run OCR (tesseract, EasyOCR, Windows OCR, etc.)
- Enhance/crop images for re-reading unless the first vision pass cannot read **ingredients or steps**
- Web-search the book/recipe unless Source cannot be inferred
- Read more than **one** example recipe unless the JSON shape is unclear
- Re-open Ask mode mid-task or explore the whole recipes folder
- Re-convert or edit unrelated recipes

## Language
- Recipe JSON text in **Danish** (titles may follow the source when that fits).
- Translate EN/DE sources into natural Danish.

## Filenames and paths
| What | Path / pattern |
|------|----------------|
| Recipe JSON | `UI/wwwroot/recipes/{Id}. {Title}.json` (space after the dot) |
| Image | `UI/wwwroot/images/recipe-images/{Id}. {Title}.jpg` |
| Index | `UI/wwwroot/recipes/index.json` |

- `ImageUrl`: relative, **no** leading `/`, e.g. `images/recipe-images/155. Chillied Beef with Chocolate.jpg`
- No dish image: `"ImageUrl": null` — do **not** rename any `*.jpg`
- Book-page photos in chat are for extraction only; the file to rename is `{Id}.jpg` in `recipe-images` (the food photo), not the chat attachment

## JSON structure (match `UI/Models/Models.cs`)
`Recipe`: `Id`, `Title`, `SubHeader`, `TotalMinutes`, `MinutesOfPreparation`, `NumberOfPersons`, `RecipeGroups`, `Instructions` (HTML string), `Note`, `ImageUrl`, `Source`, `Keywords`

`Ingredient`: `Quantity` (number/`null`, dot decimals), `Unit`, `Name`, `PreparationNote`

Valid JSON only; escape quotes in HTML as needed.

## Project conventions (Loui & Bearnaisen and similar)
- Footer like **Gryde** / **Pande** / **Fad** + FAQ box → `Source`: `"Loui & Bearnaisen"` (unless another source is clear)
- FAQ Q&A → `Note`: questions/answers separated by blank lines (`\n\n`)
- `TotalMinutes` = prep + cook when both are given
- Ranges (`2–3 spsk`) → mid quantity (e.g. `2.5`) + original range in `PreparationNote`
- Extra groups (**Topping**, **Servering**, etc.) → separate `RecipeGroups` entries
- Multi-page photos → one JSON merging all groups and instruction `<p>`s
- Keywords: short Danish tags (cuisine, protein, method, dish type) like recent index entries

## index.json
Append one object before `]`:
```json
{
  "Id": <same as recipe>,
  "Title": "<same as recipe Title>",
  "Keywords": [ "..." ],
  "TotalMinutes": <same as recipe TotalMinutes or null>
}
```
