using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AutoCraft : PlaceableStructure
{
    /// <summary>
    /// The recipe book, containing all recipes in the game
    /// DEPRICATED
    /// </summary>
    private List<Recipe> recipeBook = new List<Recipe>();

    /// <summary>
    /// Current recipe that the gatherer is collecting
    /// </summary>
    private Recipe activeRecipe;

    /// <summary>
    /// Local instance of the recipes class
    /// </summary>
    private Recipes recipes;

    private float[] ingredientInventoryAmounts;

    private float inventoryAmount;

    /// <summary>
    /// Current invenotry of the gatherer. 
    /// </summary>
    //private float inventory = 0f;

    /// <summary>
    /// Maximium amount the gatherer can hold
    /// </summary>
    private const float MAX_INVENTORY = 50f;
    /// <summary>
    /// Prefab's directory for placement
    /// </summary>
    private const string PREFAB_DIRECTORY = "Prefabs/";

    /// <summary>
    /// Upper bound on number of ingredients
    /// </summary>
    private int totalIngredients;

    private float timer = 0;

    /// <summary>
    /// Building ouput defaults to left
    private static readonly Vector2 INPUT_FACING_0_DEGREE = new Vector2(-1, 0);

    //Get the center of the object
    private Vector2 boxCenter;
    //Get the size of the object
    private Vector2 boxSize;

    /// <summary>
    /// layer collision should be checked on
    /// </summary>
    [SerializeField]
    private LayerMask collisionMask;

    [SerializeField]
    private GameObject exit;
    [SerializeField]
    private GameObject input1;
    [SerializeField]
    private GameObject input2;

    protected void Awake()
    {
        inputDirection = INPUT_FACING_0_DEGREE;
        recipes = GameManager.recipes;
        activeRecipe = recipes.FindRecipeByName("AppleJuice");
        totalIngredients = 1;
        ingredientInventoryAmounts = new float[totalIngredients];
        System.Array.Clear(ingredientInventoryAmounts,0, ingredientInventoryAmounts.Length);
    }

    // Update is called once per frame
    void Update()
    {
        if (placed)
        {
            CheckForValidBeltItem();
            BeltItemInsideCrafter();
            GenerateGameObject();
            if(inventoryAmount > 0)
            {
                InstantitateItem();

            }
        }
    }



    private void CheckForValidBeltItem()
    {
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        RaycastHit2D[] input1Ready = Physics2D.RaycastAll(input1.transform.position, inputDirection, 1);
        RaycastHit2D[] input2Ready = Physics2D.RaycastAll(input2.transform.position, inputDirection, 1);
        gameObject.GetComponent<BoxCollider2D>().enabled = true;
        InputHitDetection(input1Ready);
        InputHitDetection(input2Ready);
    }

    private void BeltItemInsideCrafter()
    {
        boxCenter = gameObject.GetComponent<SpriteRenderer>().bounds.center;
        boxSize = gameObject.GetComponent<SpriteRenderer>().bounds.extents;
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0, collisionMask);
        gameObject.GetComponent<BoxCollider2D>().enabled = true;
        foreach(Collider2D hit in hits)
        {
            foreach (KeyValuePair<int, Ingredient> ingredient in activeRecipe.ingredients)
            {
                Debug.Log(ingredient.Value.ingredientName);
                if (hit.transform.GetComponent<BeltItem>() != null)
                {
                    if (hit.transform.GetComponent<BeltItem>().recipeName == ingredient.Value.ingredientName)
                    {
                        ingredientInventoryAmounts[ingredient.Key]++;
                        Destroy(hit.gameObject);
                    }
                }
            }
        }
    }

    private void InputHitDetection(RaycastHit2D[] hits)
    {
        if(hits.Length > 0)
        {
            Collider2D belt = null;
            Collider2D beltItem = null;
            bool validItem = false;
            bool validBelt = false;

            foreach(RaycastHit2D hit in hits)
            {
                if (hit.transform.GetComponent<Belt>() != null)
                {
                    if(-hit.transform.GetComponent<Belt>().outputDirection == inputDirection)
                    {
                        validBelt = true;
                        belt = hit.collider;
                    }
                }
                if (hit.transform.GetComponent<BeltItem>() != null)
                {
                    foreach (KeyValuePair<int, Ingredient> ingredient in activeRecipe.ingredients)
                    {
                        Debug.Log(ingredient.Value.ingredientName);
                        if (hit.transform.GetComponent<BeltItem>().recipeName == ingredient.Value.ingredientName)
                        {
                                validItem = true;
                                beltItem = hit.collider;
                        }
                    }
                }
            }
            //Belt is facing the input, and is occupied by a valid input for the recipe
            if(validBelt && validItem)
            {
                foreach (KeyValuePair<int, Ingredient> ingredient in activeRecipe.ingredients)
                {
                    if (ingredientInventoryAmounts[ingredient.Key] < MAX_INVENTORY && ingredient.Value.ingredientName.Equals(beltItem.GetComponent<BeltItem>().recipeName))
                    {
                        beltItem.GetComponent<BeltItem>().MoveToDestination(-inputDirection);
                    }
                }
            }
        }
    }

    public void GenerateGameObject()
    {
        int allItemsValid = 0;
        if(activeRecipe != null)
        {
            for(int i=0; i < totalIngredients;i++)
            {
                if (ingredientInventoryAmounts[i] > 0)
                {
                    Ingredient ingredientToCheck;
                    activeRecipe.ingredients.TryGetValue(i, out ingredientToCheck);
                    if( ingredientToCheck.amountNeeded<= ingredientInventoryAmounts[i])
                    {
                        allItemsValid++;  
                    }
                }
            }
        }
        if (allItemsValid == totalIngredients)
        {

            GenerateItemTimer();

        }

    }
    private void GenerateItemTimer()
    {
        if (activeRecipe != null)
        {
            if (inventoryAmount < MAX_INVENTORY)
            {
                timer += Time.deltaTime;
                if (timer >= activeRecipe.secondsPerItem)
                {
                    inventoryAmount += 1f;
                    //gatherUI.UpdateResourceAmount(inventory);
                    timer = 0f;
                }
            }
        }
    }

    private void InstantitateItem()
    {
        //check in front of the output port
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        RaycastHit2D[] hits = Physics2D.RaycastAll(exit.transform.position, outputDirection, 1, collisionMask);
        gameObject.GetComponent<BoxCollider2D>().enabled = true;
        bool beltFound = false;
        bool beltOccupied = false;
        //If the output port has something in front of it
        if (hits.Length != 0)
        {
            //check whats infront of it
            foreach (RaycastHit2D hit in hits)
            {
                //Check if its a belt
                if (hit.collider.GetComponent<Belt>())
                {
                    //check if the belt is on the ground
                    if (hit.collider.GetComponent<Belt>().placed)
                    {
                        //set that the belt exists
                        beltFound = true;

                    }
                }
                //check if the belt is occupied
                if (hit.collider.GetComponent<BeltItem>())
                {
                    beltOccupied = true;
                }

            }
            //If their is a belt and the belt is not occupied
            if (beltFound && !beltOccupied)
            {
                //create a new belt item
                GameObject output = Instantiate(Resources.Load(PREFAB_DIRECTORY + activeRecipe.recipeName)) as GameObject;
                //If the output was successfuly created
                if (output.transform != null)
                {
                    //set direction and position of belt item, and update the inventory
                    output.transform.position = exit.transform.position;
         //           gatherUI.UpdateResourceAmount(inventory);
                    output.GetComponent<BeltItem>().direction = new Vector2Int((int)outputDirection.x, (int)outputDirection.y);
                    Debug.Log(output.GetComponent<BeltItem>().direction.ToString());
                    activeRecipe.ingredients.TryGetValue(0, out output.GetComponent<BeltItem>().ingredient);
                    output.GetComponent<BeltItem>().recipeName = activeRecipe.recipeName;
                    for (int i = 0; i < totalIngredients; i++)
                    {
                        if (ingredientInventoryAmounts[i] > 0)
                        {
                            Ingredient ingredientToCheck;
                            activeRecipe.ingredients.TryGetValue(i, out ingredientToCheck);
                            if (ingredientToCheck.amountNeeded <= ingredientInventoryAmounts[i])
                            {
                                ingredientInventoryAmounts[i] -= ingredientToCheck.amountNeeded;
                                inventoryAmount--;  
                                Debug.Log(ingredientInventoryAmounts[i]);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("Invalid belt item output, check file path");
                }
            }
        }
    }
}
