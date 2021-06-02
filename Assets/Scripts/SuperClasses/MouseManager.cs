using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * Mouse manager covers all player interaction involving the mouse. 
 * This includes selecting a building, placing a building, deleting a building, and interacting with buildings 
 */

/// <summary>
/// Handles all interactions involving the players mouse.
/// </summary>
public class MouseManager : MonoBehaviour
{
    /// <summary>
    /// Prefab game object for the AutoGatherer
    /// </summary>
    [SerializeField]
    private GameObject autoGather;

    /// <summary>
    /// Prefab game object for the AutoCrafter
    /// </summary>
    [SerializeField]
    private GameObject autoCraft;

    /// <summary>
    /// Prefab game object for the belt
    /// </summary>
    [SerializeField]
    private GameObject belt;

    /// <summary>
    /// layer collision should be checked on
    /// </summary>
    [SerializeField]
    private LayerMask collisionMask;

    /// <summary>
    /// Object the player is currently considering placing
    /// </summary>
    private GameObject objectSelected;

    /// <summary>
    /// Whether the player has an object in "hover" mode
    /// </summary>
    public bool isObjectSelected;

    /// <summary>
    /// Object Transparency for "hover" mode
    /// </summary>
    private static Color transparent = new Color(1f, 1f, 1f, 0.4f);

    /// <summary>
    /// Standard Object colour when leaving "hover" mode
    /// </summary>
    private static Color solid = new Color(1f, 1f, 1f, 1f);

    /// <summary>
    /// Player object associated with this mouse manager
    /// </summary>
    [SerializeField]
    private Player player;

    /// <summary>
    /// Max number of gathering buildings per resource node
    /// </summary>
    public const float MAX_GATHERERS = 1f;

    /// <summary>
    /// range the player is allowed to place structure
    /// </summary>
    public const float PLACEMENT_RANGE = 3f;


    private float rotationDirection;



    /// <summary>
    /// Building ouput defaults to Right
    /// </summary>
    private static readonly Vector2 OUTPUT_FACING_0_DEGREES = new Vector2(1,0);


    /// <summary>
    /// Building input defaults to Right
    /// </summary>
    private static readonly Vector2 INPUT_FACING_0_DEGREES = new Vector2(-1, 0);

    /// <summary>
    /// Building ouput facing at 90 degrees is Down
    /// </summary>
    private static readonly Vector2 OUTPUT_FACING_90_DEGREES = new Vector2(0, 1);

    /// <summary>
    /// Building input facing at 90 degrees is Down
    /// </summary>
    private static readonly Vector2 INPUT_FACING_90_DEGREES = new Vector2(0, -1);

    /// <summary>
    /// Building ouput facing at 180 degrees is Left
    /// </summary>
    private static readonly Vector2 OUTPUT_FACING_180_DEGREES = new Vector2(-1, 0);

    /// <summary>
    /// Building input facing at 180 degrees is Left
    /// </summary>
    private static readonly Vector2 INPUT_FACING_180_DEGREES = new Vector2(1, 0);

    /// <summary>
    /// Building ouput facing at 180 degrees is up
    /// </summary>
    private static readonly  Vector2 OUTPUT_FACING_270_DEGREES = new Vector2(0, -1);

    /// <summary>
    /// Building input facing at 180 degrees is up
    /// </summary>
    private static readonly Vector2 INPUT_FACING_270_DEGREES = new Vector2(0, 1);

    /// <summary>
    /// Direction of the output vent
    /// </summary>
    public Vector2 outputDirection = OUTPUT_FACING_0_DEGREES;

    /// <summary>
    /// Direction of the input vent
    /// </summary>
    public Vector2 inputDirection = INPUT_FACING_0_DEGREES;

    /// <summary>
    /// Called on script load
    /// </summary>
    private void Awake()
    {
        outputDirection = OUTPUT_FACING_0_DEGREES;
        inputDirection = INPUT_FACING_0_DEGREES;
        isObjectSelected = false;   
    }



    /// <summary>
    /// Called every frame
    /// </summary>
    private void Update()
    {
        GetInput();
        if (objectSelected != null)
        {
            FollowMouse();
            FinalizePlaceStructure();
        }
    }

    /// <summary>
    /// Gets the user input
    /// </summary>
    private void GetInput()
    {
        //Handle player interaction ONLY if an object isn't selected
        if(Input.GetButtonDown("Interact") && !isObjectSelected)
        {
            Interact();
        }

        //Handle the player rotating an object only if an object is selected
        if (Input.GetButtonDown("Rotate") && isObjectSelected)
        {
            Rotate();
        }

        //Handle removing an object only if an object isnt selected
        if (Input.GetButtonDown("Remove") && !isObjectSelected)
        {
            Debug.Log("Destroying requested");
            RemoveStructure();
        }

        //Handle all building placement logic
        if (Input.GetButtonDown("Building1"))
        {
            AutoGatherPlacement();
        }
        else if (Input.GetButtonDown("Building2"))
        {
            AutoCraftPlacement();
        }
        else if (Input.GetButtonDown("Building3"))
        {
            BeltPlacement();
        }
        

    }

    /// <summary>
    /// Rotate an object to be placed about the z access
    /// </summary>
    protected void Rotate()
    {
        rotationDirection = Input.GetAxisRaw("Rotate");
        //Rotate the object about the y access
        objectSelected.transform.Rotate(0,0,-90f*rotationDirection, Space.Self);
        if (objectSelected.GetComponentInChildren<UI>(true)!=null)
        {
            //Rotate its UI about the y access in the opposite direction so it always stays vertical
            objectSelected.GetComponentInChildren<UI>(true).gameObject.transform.Rotate(0, 0, -90f * -rotationDirection, Space.Self);

            //Get the y component of the top edge of the object
            float topEdgeY = (objectSelected.GetComponent<SpriteRenderer>().bounds.center.y + objectSelected.GetComponent<SpriteRenderer>().bounds.size.y);
            //Apply the objects offset
            topEdgeY += objectSelected.GetComponent<BoxCollider2D>().offset.y;
            //Get the x component of the top edge of the object
            float topEdgeX = (objectSelected.GetComponent<SpriteRenderer>().bounds.center.x + objectSelected.GetComponent<SpriteRenderer>().bounds.size.x);
            //adjust the X axis of the UI by half the width of its background element
            topEdgeX -= (objectSelected.GetComponentInChildren<UI>(true).transform.Find("BackGround").GetComponent<RectTransform>().sizeDelta.x) / 2;
            //Apply its new position
            objectSelected.GetComponentInChildren<UI>(true).gameObject.transform.position = new Vector2(topEdgeX, topEdgeY);
        }
        SetFacing(objectSelected.transform.rotation.eulerAngles.z);
        Debug.Log(objectSelected.transform.rotation.eulerAngles.z);
        Debug.Log(outputDirection.ToString());
    }

    /// <summary>
    /// Set the direction of inputs and outputs based on the angle of the structure
    /// </summary>
    /// <param name="angle"></param>
    protected void SetFacing(float angle)
    {
        switch(angle){
            case 0f:
                outputDirection = OUTPUT_FACING_0_DEGREES;
                inputDirection = INPUT_FACING_0_DEGREES;
                break;
            case 90f:
                outputDirection = OUTPUT_FACING_90_DEGREES;
                inputDirection = INPUT_FACING_90_DEGREES;
                break;
            case 180f:
                outputDirection = OUTPUT_FACING_180_DEGREES;
                inputDirection = INPUT_FACING_180_DEGREES;
                break;
            case 270f:
                outputDirection = OUTPUT_FACING_270_DEGREES;
                inputDirection = INPUT_FACING_270_DEGREES;
                break;

        }
    }

    /// <summary>
    /// Player Interaction with existing structures
    /// </summary>
    protected void Interact()
    {

        //Get the position of the players mouse using the the 2d grid
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Convert to a 2D point
        Vector2 mousePosition2D = new Vector2(mousePosition.x, mousePosition.y);
        //Check to see if the player clicked on something
        RaycastHit2D hit = Physics2D.Raycast(mousePosition2D, mousePosition2D, 0, collisionMask);
        if (hit.collider != null)
        {

            //Check to see if the player clicked on an interactable actor
            if (hit.collider.tag == "Interactable")
            {
                //Check to see if the player is in interaction range
                float distanceToObject = Vector2.Distance(hit.collider.bounds.center, player.transform.position);
                Debug.Log(distanceToObject.ToString());
                if (distanceToObject <= Player.INTERACTION_RANGE)
                {
                    //If an interface isn't already open
                    if (player.canInteract)
                    {

                        //Check what they clicked

                        //Auto Gatherer
                        if (hit.collider.GetComponent<AutoGather>())
                        {
                            //Set what they clicked on
                            player.lastObjectClicked = hit;

                            //Display the UI for the AutoGatherer
                            hit.collider.GetComponentInChildren<AutoGatherUI>(true).DisplayUI();
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Handles placement of a belt
    /// </summary>
    private void BeltPlacement()
    {
        //If an object is selected, destroy it if its an autogatherer
        if (objectSelected != null)
        {
            Destroy(objectSelected);
            if (objectSelected.GetComponent<Belt>())
            {

                isObjectSelected = false;
                gameObject.GetComponentInParent<Player>().canInteract = true;
            }
            else
            {
                //Otherwise, create a new blet, and make it tranparent
                isObjectSelected = true;
                objectSelected = Instantiate(belt);
                objectSelected.GetComponent<PlaceableStructure>().placed = false;
                SetFacing(objectSelected.transform.rotation.eulerAngles.z);
                objectSelected.GetComponent<SpriteRenderer>().color = transparent;
            }
        }
        else
        {
            //Otherwise, create a new auto autogatherer, and make it tranparent
            isObjectSelected = true;
            objectSelected = Instantiate(belt);
            objectSelected.GetComponent<PlaceableStructure>().placed = false;
            SetFacing(objectSelected.transform.rotation.eulerAngles.z);
            objectSelected.GetComponent<SpriteRenderer>().color = transparent;
        }
    }

    /// <summary>
    /// Placement logic for the auto gatherer
    /// </summary>
    private void AutoGatherPlacement()
    {
        //If an object is selected, destroy it if its an autogatherer
        if (objectSelected != null)
        {
            Destroy(objectSelected);
            if (objectSelected.GetComponent<AutoGather>())
            {
                isObjectSelected = false;
                gameObject.GetComponentInParent<Player>().canInteract = true;
            }
            else
            {
                //Otherwise, create a new auto autogatherer, and make it tranparent
                isObjectSelected = true;
                objectSelected = Instantiate(autoGather);
                objectSelected.GetComponent<PlaceableStructure>().placed = false;
                SetFacing(objectSelected.transform.rotation.eulerAngles.z);
                objectSelected.GetComponent<SpriteRenderer>().color = transparent;
            }
        }
        else
        {
            //Otherwise, create a new auto autogatherer, and make it tranparent
            isObjectSelected = true;
            objectSelected = Instantiate(autoGather);
            objectSelected.GetComponent<PlaceableStructure>().placed = false;
            SetFacing(objectSelected.transform.rotation.eulerAngles.z);
            objectSelected.GetComponent<SpriteRenderer>().color = transparent;
        }
    }
    /// <summary>
    /// Handles logic for placing autocrafters
    /// </summary>
    private void AutoCraftPlacement()
    {
        //If an object is selected, destroy it if its an autogatherer
        if (objectSelected != null)
        {
            Destroy(objectSelected);
            if (objectSelected.GetComponent<AutoCraft>())
            {

                isObjectSelected = false;
                gameObject.GetComponentInParent<Player>().canInteract = true;
            }
            else
            {
                //Otherwise, create a new auto autogatherer, and make it tranparent
                isObjectSelected = true;
                objectSelected = Instantiate(autoCraft);
                objectSelected.GetComponent<PlaceableStructure>().placed = false;
                SetFacing(objectSelected.transform.rotation.eulerAngles.z);
                objectSelected.GetComponent<SpriteRenderer>().color = transparent;
            }
        }
        else
        {
            //Otherwise, create a new auto autogatherer, and make it tranparent
            isObjectSelected = true;
            objectSelected = Instantiate(autoCraft);
            objectSelected.GetComponent<PlaceableStructure>().placed = false;
            SetFacing(objectSelected.transform.rotation.eulerAngles.z);
            objectSelected.GetComponent<SpriteRenderer>().color = transparent;
        }
    }

    /// <summary>
    ///     Destroy the structure at the players mouse
    /// </summary>
    private void RemoveStructure()
    {
        //Get the position of the players mouse using the the 2d grid
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Convert to a 2D point
        Vector2 mousePosition2D = new Vector2(mousePosition.x, mousePosition.y);
        mousePosition2D.Set(Mathf.Floor(mousePosition2D.x) + 0.5f, Mathf.Floor(mousePosition2D.y) + 0.5f);
        //Check to see if the player clicked within there interaction range
        float distanceToPlayer = Vector2.Distance(mousePosition2D, gameObject.GetComponentInParent<Player>().transform.position);

        //If plyers mouse is within placement range
        Debug.Log(distanceToPlayer.ToString());
        if (PLACEMENT_RANGE >= distanceToPlayer)
        {
            Debug.Log("Object to destroy in range");
            //if there is something below the mouse
            Collider2D[] hits = Physics2D.OverlapBoxAll(mousePosition2D,(new Vector2(0.5f, 0.5f)) ,0f);
            if(hits.Length != 0)
            {
                //check if any item at the mouse click is destroyable
                foreach (Collider2D hit in hits)
                {
                    //If the object below the mouse is a placeable structure
                    if (hit.transform.GetComponent<PlaceableStructure>() == true)
                    {
                        //Destroy all game objects at the mouse location
                        Debug.Log("Destroying object");
                        foreach(Collider2D objectToDestroy in hits)
                        {
                            Debug.Log("Destroying first object");
                            Destroy(objectToDestroy.gameObject);
                        }
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Have the building to be placed follow the players mouse
    /// </summary>
    private void FollowMouse()
    {
        //Get the position of the players mouse using the  grid
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Convert to a 2D point
        Vector2 mousePosition2D = new Vector2(mousePosition.x, mousePosition.y);

        //move the selected object to the players mouse
        objectSelected.transform.position = mousePosition2D;
        //Snap the object to the grid
        objectSelected.transform.position = new Vector2(Mathf.Floor(mousePosition2D.x) + 0.5f, Mathf.Floor(mousePosition2D.y)+0.5f);
    }

    /// <summary>
    /// Additional hitDetection rules specific to a auto gather
    /// </summary>
    /// <returns>Boolean, true if placment would be successful</returns>
    private bool GathererPlacementHitDetection()
    {
        bool successfulPlacment = false;
        Debug.Log("Placing Gatherer");
        //Check if there is a resource within range to be collected
        //First create a vector that represents the area to check around the object based on gather range
        Vector2 gatherSizeVector = new Vector2(objectSelected.GetComponent<AutoGather>().GATHER_RANGE, objectSelected.GetComponent<AutoGather>().GATHER_RANGE);

        //Than, get he extents, the radius, of the object
        Vector2 gatherRadiusVector = objectSelected.GetComponent<SpriteRenderer>().bounds.extents;
        //The size of bounding box is thus the gathersize, plus the radius of the object
        Vector2 resourceCheckDistance = gatherSizeVector + gatherRadiusVector;  

        //Finally we check using the center of the object as the center of the check. This should allow placement of a structure of any size
        Collider2D[] resourcesInRange = Physics2D.OverlapBoxAll(objectSelected.GetComponent<SpriteRenderer>().bounds.center, resourceCheckDistance, 0, collisionMask);

        //Check through all objects within range
        foreach (Collider2D resourceCollider in resourcesInRange)
        {
            //If there is a collectable resource in range
            if (resourceCollider.GetComponent<GatherableResource>() == true)
            {
                Debug.Log("Resource Found");
                //Check if there is already a gatherer  
                Collider2D[] gatherersInRange = Physics2D.OverlapBoxAll(resourceCollider.transform.position,gatherSizeVector, 0, collisionMask);
                float numberOfGathers = 0f;
                foreach (Collider2D gathererCollider in gatherersInRange)
                {
                    //If there is a gather, increment the counter
                    if (gathererCollider.GetComponent<AutoGather>())
                    {
                        numberOfGathers++;
                    }
                    //If there is more gatherers than allowed, placement fails
                    if (numberOfGathers > MAX_GATHERERS)
                    {
                        successfulPlacment = false;
                        break;
                    }
                    else
                    {
                        successfulPlacment = true;
                    }
                }
            }
        }

        return successfulPlacment;
    }
    /// <summary>
    /// Detects whether it is possible to place a structure. 
    /// </summary>
    /// <returns>Booolean. True if placement would be successful</returns>
    private bool PlacementHitDetection()
    {
        bool successfulPlacment = false;
        //Get the position of the players mouse using the the 2d grid
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Convert to a 2D point
        Vector2 mousePosition2D = new Vector2(mousePosition.x, mousePosition.y);
        //Measure distance from the center of the tile
        mousePosition2D = new Vector2(Mathf.Floor(mousePosition2D.x) + 0.5f, Mathf.Floor(mousePosition2D.y) + 0.5f);
        //Check to see if the player clicked within there interaction range
        float distanceToPlayer = Vector2.Distance(mousePosition2D, gameObject.GetComponentInParent<Player>().transform.position);
        //If 
        if (PLACEMENT_RANGE >= distanceToPlayer)
        {
            Debug.Log("In range");
            //preventing collision with self
            objectSelected.GetComponent<BoxCollider2D>().enabled = false;

            //Get the center of the object
            Vector2 boxCenter = objectSelected.GetComponent<SpriteRenderer>().bounds.center;
            //Get the size of the object
            Vector2 boxSize = objectSelected.GetComponent<SpriteRenderer>().bounds.extents;
            //check under the object
            Collider2D[] collision = Physics2D.OverlapBoxAll(boxCenter, boxSize,0, collisionMask);
            Collider2D[] terrainCollision = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0);
            //re-enable collision
            objectSelected.GetComponent<BoxCollider2D>().enabled = true;
            Debug.Log(collision.Length);
            //If placing in an unoccupied space, allow placement
            if (collision.Length == 0)
            {
                if (terrainCollision.Length == 0)
                {
                    Debug.Log("Space Unoccupied");
                    //If were placing an auto gather, check additional restrictions
                    if (objectSelected.GetComponent<AutoGather>() == true)
                    {
                        successfulPlacment = GathererPlacementHitDetection();
                    }
                    else
                    {
                        //Otherwise place the building
                        successfulPlacment = true;
                    }
                }
            }
        }

        return successfulPlacment;
    }

    /// <summary>
    /// If placement would be succesful, release the object
    /// </summary>
    private void FinalizePlaceStructure()
    {
        //if the player presses the interaction button
        if (Input.GetButtonDown("Interact"))
        {
            //check if placement would be successful
            bool successfulPlacment = PlacementHitDetection();
   
            //If placement is possible, place the building
            if (successfulPlacment)
            {
                //let go of the item
                Debug.Log("Placement Successful");
                GameObject floatingObject = Instantiate(objectSelected);
                objectSelected.GetComponent<SpriteRenderer>().color = solid;
                objectSelected.GetComponent<PlaceableStructure>().placed = true;
                if (objectSelected.GetComponent<PlaceableStructure>().outputDirection != null)
                {
                    objectSelected.GetComponent<PlaceableStructure>().outputDirection = outputDirection;
                }
                if(objectSelected.GetComponent<PlaceableStructure>().inputDirection != null)
                {
                    objectSelected.GetComponent<PlaceableStructure>().inputDirection = inputDirection;
                }
                //Drop the game object
                objectSelected = null;
                objectSelected = floatingObject;
                
            }
        }
    }
}
