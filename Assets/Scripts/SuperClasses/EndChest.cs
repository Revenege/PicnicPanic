using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game object representive of the end of the level
/// </summary>
public class EndChest : ImpassibleStructure
{
    public List<Recipe> endRequirementRecipes;
   // List<float> inventory;
    public List<float> endRequirementAmount;
    /// <summary>
    /// The recipe book, containing all recipes in the game
    /// </summary>
  //  private List<Recipe> recipeBook = new List<Recipe>();
    /// <summary>
    /// Gather  range of the end chest
    /// </summary>
    private readonly Vector2 INPUT_GATHER_RANGE = Vector2.one *2;
    /// <summary>
    ///Get the center of the object
    /// </summarys>
    private Vector2 boxCenter;
    /// <summary>
    ///  Get the size of the object
    /// </summary>
    private Vector2 boxSize;

    /// <summary>
    /// Deprecated
    /// </summary>
   // private Recipes recipes;

    /// <summary>
    /// testing gadget range
    /// </summary>
    private bool m_Started;


    /// <summary>
    /// Flag for if the level has been completed
    /// </summary>
    public bool levelComplete; 
    /// <summary>
    /// layer collision should be checked on
    /// </summary>
    [SerializeField]
    private LayerMask collisionMask;


    private void Awake()
    {
        //Get the size and center of the game object
        boxCenter = gameObject.GetComponent<SpriteRenderer>().bounds.center; 
        boxSize = gameObject.GetComponent<SpriteRenderer>().bounds.extents;
        //setting default flags
        //recipes = new Recipes();
        m_Started = true;
        levelComplete = false;
    }

    private void Start()
    {
        //gather the win conditions from the managers, but only once the 
        //manager is finished
        endRequirementRecipes = GameManager.instance.winCondition;
        endRequirementAmount = GameManager.instance.endRequirementAmount;
    }

    private void Update()
    {
        BeltItemHitDetetection();
        InputHitDetection();
    }

    /// <summary>
    /// Check if a game object is intersecting the end chest
    /// </summary>
    private void BeltItemHitDetetection()
    {
        //check under the object
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0, collisionMask);
        if (hits.Length != 0)
        {
            //for each item under the object
            foreach (Collider2D hit in hits)
            {
                //if its a belt item
                if (hit.transform.GetComponent<BeltItem>())
                {
                    //check each item in the end requirements
                    for (int recipe = 0; recipe < endRequirementRecipes.Count; recipe++)
                    {
                        //if the belt item matches the end requirement
                        if (hit.transform.GetComponent<BeltItem>().recipeName == endRequirementRecipes[recipe].recipeName)
                        {
                            //if the end requirement  amount is still greater than zero
                            if (endRequirementAmount[recipe] > 0)
                            {
                                //decrement the amount
                                endRequirementAmount[recipe]--;
                                Debug.Log(endRequirementAmount[recipe]);
                            }
                            //destroy the game object and check if we've won
                            Destroy(hit.gameObject);
                            CheckForWin();
                        }
                    }
   
                }
            }
       }
    }
   /// <summary>
   /// Check if the inputs of the endchest are occupied
   /// </summary>
    private void InputHitDetection()
    {
        //game objects near the end chest
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize+INPUT_GATHER_RANGE, 0,collisionMask);
        //belt item we are currently on
        Collider2D beltItem = new Collider2D();
        //flags for validity of item and belt the item is on
        bool validItem = false;
        bool validBelt = false;
        //foreach object we collided with
        foreach (Collider2D hit in hits)
        {
            //check if its a belt
            if (hit.transform.GetComponent<Belt>())
            {
                //check that the belt is actually placed
                if (hit.transform.GetComponent<Belt>().placed)
                {
                    Debug.Log("Belt in range");
                    //check if the belt is occupied
                    Vector2 beltCenter = hit.transform.GetComponent<SpriteRenderer>().bounds.center;
                    Vector2 beltRadius = hit.transform.GetComponent<SpriteRenderer>().bounds.extents;
                    hit.transform.GetComponent<Belt>().enabled = false;
                    beltItem = Physics2D.OverlapBox(beltCenter, beltRadius, 0, collisionMask);
                    hit.transform.GetComponent<Belt>().enabled = true;
                    if (beltItem.transform.GetComponent<BeltItem>() != null)
                    {
                        Debug.Log("Belt Item on belt");
                        Vector2 beltItemCenter = beltItem.transform.GetComponent<SpriteRenderer>().bounds.center;
                        //Checking if the chest is in front of the belt
                        hit.transform.GetComponent<Belt>().enabled = false;
                        beltItem.enabled = false;
                        RaycastHit2D[] destinationHits = Physics2D.RaycastAll(beltItemCenter, hit.transform.GetComponent<Belt>().outputDirection, 1);
                        hit.transform.GetComponent<Belt>().enabled = true;
                        beltItem.enabled = true;
                        //check each object hit by the belt
                        foreach (RaycastHit2D destination in destinationHits)
                        {
                            //if we find the belt is pointing at the game object
                            if (destination.transform.GetComponent<EndChest>())
                            {
                                //check each end requirement and see if we have a match
                                foreach (Recipe recipe in endRequirementRecipes)
                                {
                                    if (beltItem.transform.GetComponent<BeltItem>().recipeName == recipe.recipeName)
                                    {
                                        //move the game object to its destination
                                        beltItem.GetComponent<BeltItem>().MoveToDestination(hit.transform.GetComponent<Belt>().outputDirection);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check if the player has won
    /// </summary>
    private void CheckForWin()
    {
        //get the number of requiremenets and there amounts
        int numberOfRequirements = endRequirementRecipes.Count;
        Debug.Log(numberOfRequirements);
        int endRequirementAmountAmount = endRequirementAmount.Count;
        Debug.Log(endRequirementAmountAmount);
        int numberOfRequirementsMet=0;
        //check if the number of requirements to be met is equal to the number of requirements met
        for (int i=0; i < numberOfRequirements; i++)
        {
            //increament the amount of met requirements when the amount is equal to zero
            if (endRequirementAmount[i] <=0)
            {
                numberOfRequirementsMet++;
            }
            //advance the next level when all requirements have been met
            if(numberOfRequirementsMet == numberOfRequirements)
            {
                GameManager.instance.AdvanceLevel();
            }
        }
    }

    /// <summary>
    /// DEBUG 
    /// draws the rectangle showing the hit range of the end chest
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
        if (m_Started)
            //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
            Gizmos.DrawWireCube(boxCenter, boxSize+INPUT_GATHER_RANGE);
    }


}
