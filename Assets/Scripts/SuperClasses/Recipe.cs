using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Stores information related to crafting recipes and gathering recipes
/// </summary>
public class Recipe 
{
    /// <summary>
    /// Amount of the item generated per minute if requirements are met
    /// </summary>
    public float numberPerMinute { get; set; }
    /// <summary>
    /// Amount of seconds required to generate 1 item
    /// </summary>
    public float secondsPerItem { get; set; }

    /// <summary>
    /// name of the recipe
    /// </summary>
    public string recipeName { get; set; } 

    /// <summary>
    /// Ingredients in the recipe. 
    /// </summary>
    public Dictionary<int,Ingredient> ingredients { get; set; }

    /// <summary>
    /// Check if the recipe has been unlocked
    /// </summary>
    public bool isAvailable { get; set; }

    /// <summary>
    /// Override of ToString. Ouputs a nicely formated string with recipe infromation
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string output;
        output = "recipeName: " + recipeName + "\n " +
            "Number Per minute: " + numberPerMinute + "\n " +
            "Seconds Per Item: " + secondsPerItem + "\n " +
            "isAvailable: " + isAvailable + "\n ";
        //Looping over ingredient list
        foreach (KeyValuePair<int, Ingredient> kvp in ingredients)
        {
            output += "Ingredient: " + kvp.Value.ingredientName + "\n"
                    + "AmountNeeded: " + kvp.Value.amountNeeded.ToString() + "\n";
        }
        return output;
    }

}
