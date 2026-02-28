using System.Text.Json;
using UI.Models;

namespace Tests.RecipeData;

public class RecipeJsonToModelsTests
{
    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    [Fact]
    public void Deserialize_Recipe_FromMinimalJson_PopulatesIdAndTitle()
    {
        Recipe recipe = DeserializeRecipe(MinimalRecipeJson);

        Assert.Equal(1, recipe.Id);
        Assert.Equal("Kold Grøntsagssuppe", recipe.Title);
    }

    [Fact]
    public void Deserialize_Recipe_WithRecipeGroups_PopulatesGroupCountAndFirstTitle()
    {
        Recipe recipe = DeserializeRecipe(RecipeWithGroupsJson);

        Assert.Equal(2, recipe.RecipeGroups.Count);
        Assert.Equal("Ingredienser", recipe.RecipeGroups[0].Title);
    }

    [Fact]
    public void Deserialize_RecipeGroup_WithIngredients_PopulatesIngredientCountAndFirstName()
    {
        Recipe recipe = DeserializeRecipe(RecipeWithGroupedIngredientsJson);
        RecipeGroup group = recipe.RecipeGroups[0];

        Assert.Equal(2, group.Ingredients.Count);
        Assert.Equal("rødløg", group.Ingredients[0].Name);
    }

    [Fact]
    public void Deserialize_Ingredient_WithAllFields_PopulatesQuantityUnitNamePreparationNote()
    {
        Recipe recipe = DeserializeRecipe(RecipeWithCompleteIngredientJson);
        Ingredient ingredient = recipe.RecipeGroups[0].Ingredients[0];

        Assert.Equal(0.5, ingredient.Quantity);
        Assert.Equal("tsk", ingredient.Unit);
        Assert.Equal("fennikelfrø", ingredient.Name);
        Assert.Equal("knust", ingredient.PreparationNote);
    }

    [Fact]
    public void Deserialize_Ingredient_WithNullQuantityAndUnit_PreservesNulls()
    {
        Recipe recipe = DeserializeRecipe(RecipeWithNullQuantityAndUnitJson);
        Ingredient ingredient = recipe.RecipeGroups[0].Ingredients[0];

        Assert.Null(ingredient.Quantity);
        Assert.Null(ingredient.Unit);
    }

    [Fact]
    public void Deserialize_Recipe_WithKeywords_PopulatesKeywordsCountAndFirstValue()
    {
        Recipe recipe = DeserializeRecipe(RecipeWithKeywordsJson);

        Assert.Equal(2, recipe.Keywords.Count);
        Assert.Equal("Suppe", recipe.Keywords[0]);
    }

    [Fact]
    public void Deserialize_Recipe_WithInstructions_PopulatesMarkupString()
    {
        Recipe recipe = DeserializeRecipe(RecipeWithInstructionsJson);

        Assert.Equal("<p>Test</p>", recipe.Instructions.Value);
    }

    [Fact]
    public void Deserialize_Recipe_WithSource_PopulatesSource()
    {
        Recipe recipe = DeserializeRecipe(RecipeWithSourceJson);

        Assert.Equal("Madens Magi, side 42", recipe.Source);
    }

    [Fact]
    public void Deserialize_FullRecipeJson_Succeeds()
    {
        Recipe recipe = DeserializeRecipe(FullRecipeJson);

        Assert.Equal(3, recipe.RecipeGroups.Count);
    }

    private static Recipe DeserializeRecipe(string json)
    {
        return JsonSerializer.Deserialize<Recipe>(json, SerializerOptions)
               ?? throw new InvalidOperationException("Recipe JSON deserialized to null.");
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new MarkupStringJsonConverter());
        return options;
    }

    private const string MinimalRecipeJson = """
                                             {
                                               "Id": 1,
                                               "Title": "Kold Grøntsagssuppe",
                                               "SubHeader": null,
                                               "TotalMinutes": null,
                                               "MinutesOfPreparation": null,
                                               "NumberOfPersons": null,
                                               "RecipeGroups": [],
                                               "Instructions": "",
                                               "Note": "",
                                               "ImageUrl": null,
                                               "Source": null,
                                               "Keywords": []
                                             }
                                             """;

    private const string RecipeWithGroupsJson = """
                                                {
                                                  "Id": 1,
                                                  "Title": "Groups",
                                                  "SubHeader": null,
                                                  "TotalMinutes": 10,
                                                  "MinutesOfPreparation": 5,
                                                  "NumberOfPersons": 2,
                                                  "RecipeGroups": [
                                                    { "Title": "Ingredienser", "Ingredients": [] },
                                                    { "Title": "Til servering", "Ingredients": [] }
                                                  ],
                                                  "Instructions": "",
                                                  "Note": "",
                                                  "ImageUrl": null,
                                                  "Source": null,
                                                  "Keywords": []
                                                }
                                                """;

    private const string RecipeWithGroupedIngredientsJson = """
                                                           {
                                                             "Id": 1,
                                                             "Title": "Grouped ingredients",
                                                             "SubHeader": null,
                                                             "TotalMinutes": 10,
                                                             "MinutesOfPreparation": 5,
                                                             "NumberOfPersons": 2,
                                                             "RecipeGroups": [
                                                               {
                                                                 "Title": "Ingredienser",
                                                                 "Ingredients": [
                                                                   { "Quantity": 1, "Unit": "stk", "Name": "rødløg", "PreparationNote": null },
                                                                   { "Quantity": 2, "Unit": "dl", "Name": "bouillon", "PreparationNote": null }
                                                                 ]
                                                               }
                                                             ],
                                                             "Instructions": "",
                                                             "Note": "",
                                                             "ImageUrl": null,
                                                             "Source": null,
                                                             "Keywords": []
                                                           }
                                                           """;

    private const string RecipeWithCompleteIngredientJson = """
                                                            {
                                                              "Id": 1,
                                                              "Title": "Complete ingredient",
                                                              "SubHeader": null,
                                                              "TotalMinutes": 10,
                                                              "MinutesOfPreparation": 5,
                                                              "NumberOfPersons": 2,
                                                              "RecipeGroups": [
                                                                {
                                                                  "Title": "Ingredienser",
                                                                  "Ingredients": [
                                                                    { "Quantity": 0.5, "Unit": "tsk", "Name": "fennikelfrø", "PreparationNote": "knust" }
                                                                  ]
                                                                }
                                                              ],
                                                              "Instructions": "",
                                                              "Note": "",
                                                              "ImageUrl": null,
                                                              "Source": null,
                                                              "Keywords": []
                                                            }
                                                            """;

    private const string RecipeWithNullQuantityAndUnitJson = """
                                                             {
                                                               "Id": 1,
                                                               "Title": "Null quantity and unit",
                                                               "SubHeader": null,
                                                               "TotalMinutes": 10,
                                                               "MinutesOfPreparation": 5,
                                                               "NumberOfPersons": 2,
                                                               "RecipeGroups": [
                                                                 {
                                                                   "Title": "Ingredienser",
                                                                   "Ingredients": [
                                                                     { "Quantity": null, "Unit": null, "Name": "salt og peber", "PreparationNote": null }
                                                                   ]
                                                                 }
                                                               ],
                                                               "Instructions": "",
                                                               "Note": "",
                                                               "ImageUrl": null,
                                                               "Source": null,
                                                               "Keywords": []
                                                             }
                                                             """;

    private const string RecipeWithKeywordsJson = """
                                                  {
                                                    "Id": 1,
                                                    "Title": "Keywords",
                                                    "SubHeader": null,
                                                    "TotalMinutes": 10,
                                                    "MinutesOfPreparation": 5,
                                                    "NumberOfPersons": 2,
                                                    "RecipeGroups": [],
                                                    "Instructions": "",
                                                    "Note": "",
                                                    "ImageUrl": null,
                                                    "Source": null,
                                                    "Keywords": [
                                                      "Suppe",
                                                      "Vegetar"
                                                    ]
                                                  }
                                                  """;

    private const string RecipeWithInstructionsJson = """
                                                      {
                                                        "Id": 1,
                                                        "Title": "Instructions",
                                                        "SubHeader": null,
                                                        "TotalMinutes": 10,
                                                        "MinutesOfPreparation": 5,
                                                        "NumberOfPersons": 2,
                                                        "RecipeGroups": [],
                                                        "Instructions": "<p>Test</p>",
                                                        "Note": "",
                                                        "ImageUrl": null,
                                                        "Source": null,
                                                        "Keywords": []
                                                      }
                                                      """;

    private const string RecipeWithSourceJson = """
                                                {
                                                  "Id": 1,
                                                  "Title": "Source",
                                                  "SubHeader": null,
                                                  "TotalMinutes": 10,
                                                  "MinutesOfPreparation": 5,
                                                  "NumberOfPersons": 2,
                                                  "RecipeGroups": [],
                                                  "Instructions": "",
                                                  "Note": "",
                                                  "ImageUrl": null,
                                                  "Source": "Madens Magi, side 42",
                                                  "Keywords": []
                                                }
                                                """;

    private const string FullRecipeJson = """
                                          {
                                            "Id": 1,
                                            "Title": "Kold Grøntsagssuppe",
                                            "SubHeader": null,
                                            "TotalMinutes": 120,
                                            "MinutesOfPreparation": 25,
                                            "NumberOfPersons": 4,
                                            "RecipeGroups": [
                                              {
                                                "Title": "Ingredienser",
                                                "Ingredients": [
                                                  { "Quantity": 0.5, "Unit": null, "Name": "rødløg", "PreparationNote": "finthakket" },
                                                  { "Quantity": 1, "Unit": "fed", "Name": "hvidløg", "PreparationNote": null }
                                                ]
                                              },
                                              {
                                                "Title": "Croutoner",
                                                "Ingredients": [
                                                  { "Quantity": 2, "Unit": "skiver", "Name": "hvedebrød", "PreparationNote": null }
                                                ]
                                              },
                                              {
                                                "Title": "Til servering",
                                                "Ingredients": [
                                                  { "Quantity": 2, "Unit": "spsk", "Name": "olivenolie", "PreparationNote": null }
                                                ]
                                              }
                                            ],
                                            "Instructions": "<h2>Fremgangsmåde</h2><p>Sauter løg og hvidløg ...</p><h3>Croutoner</h3><p>Skær brødet ...</p><h3>Servering</h3><p>Server suppen ...</p>",
                                            "Note": "Grøntsagerne skal ikke skæres fint.",
                                            "ImageUrl": null,
                                            "Source": null,
                                            "Keywords": [
                                              "Suppe",
                                              "Vegetar"
                                            ]
                                          }
                                          """;
}