using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

/// <summary>
/// represents a collection of recipes, used for generating recipe book from XML file
/// </summary>
public class Recipes : Recipe
{
    /// <summary>
    /// Recipe Book containing all recipes in the game
    /// </summary>
    private List<Recipe> recipeBook = new List<Recipe>();

    /// <summary>
    /// Data within the xml file for recipes
    /// </summary>
    IEnumerable<XElement> xmlData;

    /// <summary>
    /// Generate the recipe book from XML
    /// </summary>
    public void GenerateRecipeBook()
    {
        //Load XML file into memory
        xmlData = from all in XElement.Load(GameManager.instance.dataPath + "/StreamingAssets/Data/recipes.xml").Elements("Recipe")
                  select all;
        //For each recipe object in XML file, store its information
        foreach (XElement recipe in xmlData)
        {
            //temporary incredient list
            Dictionary<int, Ingredient> ingredientsToAdd = new Dictionary<int, Ingredient>();
            //Query for ingredient members
            var ingredientsQuery = recipe.Descendants()
                .Where(ingredients => ingredients.Name.LocalName == ("Ingredients")).Elements("Ingredient");

            int counter = 0;
                //adding each ingredient to the temporary ingredient list
            foreach (var ingredientValue in ingredientsQuery)
            {
                //Creating the ingredient 
                Ingredient ingredientToAdd = new Ingredient()
                {
                    ingredientName = ingredientValue.Element("IngredientName").Value.Trim(),
                    amountNeeded = float.Parse(ingredientValue.Element("AmountNeeded").Value.Trim())
                };

                ingredientsToAdd.Add(counter,ingredientToAdd);
                counter++;
            }
            //Adding static information from the xml file to the recipe object
                Recipe recipeToAdd = new Recipe()
                {
                    recipeName = recipe.Element("Name").Value.Trim(),
                    numberPerMinute = float.Parse(recipe.Element("NumberPerMinute").Value.Trim()),
                    //by default false
                    isAvailable = false,
                    //adding generated ingredient list from previous step to the recipe
                    ingredients = ingredientsToAdd
                };

            //Calculate how many seconds it takes to make one item and add to the the recipe book
            recipeToAdd.secondsPerItem = 60f / recipeToAdd.numberPerMinute;
            recipeBook.Add(recipeToAdd);
        }
    }

    /// <summary>
    /// Search the recipe book for a recipe based upon that recipe's name
    /// </summary>
    /// <param name="recipeName">recipe being searched for</param>
    /// <returns>Recipe with the desired ingredient</returns>
    public Recipe FindRecipeByName(string recipeName)
    {
        Recipe selected = new Recipe();
        //Due to nested Lists, need to manually search rather than use .find

        //for each recipe in the recipe book
        foreach (Recipe recipe in recipeBook)
        {
            //search for an ingredient with the same name as the requested one
            if (recipe.recipeName.Equals(recipeName))
            {
                selected = recipe;
            }
        }
        return selected;
    }

    /// <summary>
    /// Search the recipe book for a recipe based upon an ingredient in that recipe
    /// </summary>
    /// <param name="ingredientName">ingredient being searched for</param>
    /// <returns>Recipe with the desired ingredient</returns>
    public Recipe FindRecipeByIngredient(string ingredientName)
    {
        Recipe selected = new Recipe();
        //Due to nested Lists, need to manually search rather than use .find

        //for each recipe in the recipe book
        foreach (Recipe recipe in recipeBook)
        {
            //for each ingredient in the recipe
            foreach (KeyValuePair<int, Ingredient> ingredient in recipe.ingredients)
            {
                Debug.Log(ingredientName);
                //search for an ingredient with the same name as the requested one
                if (ingredient.Value.ingredientName.Equals(ingredientName))
                {
                    selected = recipe;
                }
            }
        }
        return selected;
    }




}
