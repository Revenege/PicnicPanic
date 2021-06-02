using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Autogather game object. USed for gathering a resource node automatically
/// </summary>
public class AutoGather : PlaceableStructure
{

    /// <summary>
    /// Current recipe that the gatherer is collecting
    /// </summary>
    private Recipe activeRecipe;

    /// <summary>
    /// Local instance of the recipes class
    /// </summary>
    private Recipes recipes;

    /// <summary>
    /// Current invenotry of the gatherer. 
    /// </summary>
    private float inventory = 0f;

    /// <summary>
    /// Maximium amount the gatherer can hold
    /// </summary>
    private const float MAX_INVENTORY = 50f;

    private const string PREFAB_DIRECTORY = "Prefabs/";

    /// <summary>
    /// layer collision should be checked on
    /// </summary>
    [SerializeField]
    private LayerMask collisionMask;



    /// <summary>
    /// Timer tracking when a item will be generated
    /// </summary>
    private float timer = 0f;

    [SerializeField]
    private GameObject exit;

    AutoGatherUI gatherUI;
   // LayerMask uI;

    /// <summary>
    /// Range at which a resource is capable of being gathered from
    /// </summary>
    public readonly float  GATHER_RANGE = 1f;

    /// <summary>
    /// Constructor for the AutoGather. Creates a recipe book
    /// </summary>
    private void Awake()
    {
        gatherUI = gameObject.GetComponentInChildren<AutoGatherUI>(true);
        recipes = GameManager.recipes;
    }


    /// <summary>
    /// Generate an item on a fixed schedule rather than every frame to save cycles
    /// </summary>
    private void FixedUpdate()
    {
        //If the AutoGather isn't in placement mode
        if (placed)
        {
            if (activeRecipe != null)
            {
                GenerateItem();
            }
            else
            {
                CheckForResource();
            }

            //If the gather has items stored, check for output
            if (inventory >=1)
            {
                InstantitateItem();
            }
        }
    }


    /// <summary>
    /// Check if the gatherer is next to a collectable resource. If it is, set the active recipe to be that resoruce
    /// </summary>
    private void CheckForResource()
    {

        //Check to see if their is a nearby resource
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position,new Vector2(GATHER_RANGE,GATHER_RANGE),0);

        foreach(Collider2D resourceCollider in colliders)
        {
            Debug.Log("Recipe selected");
            if (resourceCollider.GetComponent<GatherableResource>() == true)
            {
                activeRecipe = new Recipe();

                activeRecipe = recipes.FindRecipeByIngredient(resourceCollider.GetComponent<GatherableResource>().resourceName);
            }
        }

    }

    /// <summary>
    /// Generate an item automatically
    /// </summary>
    private void GenerateItem()
    {
        if (activeRecipe != null)
        {
            if (inventory < MAX_INVENTORY)
            {
                timer += Time.deltaTime;
                if (timer >= activeRecipe.secondsPerItem)
                {
                    inventory += 1f;
                    gatherUI.UpdateResourceAmount(inventory);
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
                Debug.Log(activeRecipe.recipeName);
                //create a new belt item
                GameObject output = Instantiate(Resources.Load(PREFAB_DIRECTORY + activeRecipe.recipeName)) as GameObject;
                //If the output was successfuly created
                if (output.transform != null)
                {
                    Debug.Log("aaa");
                    //set direction and position of belt item, and update the inventory
                    output.transform.position = exit.transform.position;
                    inventory -= 1;
                    gatherUI.UpdateResourceAmount(inventory);
                    output.GetComponent<BeltItem>().direction = new Vector2Int((int)outputDirection.x, (int)outputDirection.y);
                    activeRecipe.ingredients.TryGetValue(0, out output.GetComponent<BeltItem>().ingredient);
                    output.GetComponent<BeltItem>().recipeName = activeRecipe.recipeName;
                }
                else
                {
                    Debug.LogError("Invalid belt item output, check file path");
                }
            }
        }
    }
}
