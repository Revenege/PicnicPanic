using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * <summary>
 * Movable handles the movement of any and actors capable of movement. 
 * This class is based up the tutorial found at https://learn.unity.com/tutorial/unit-mechanics?uv=5.x&projectId=5c514a00edbc2a0020694718#5c7f8528edbc2a002053b6f0
 * </summary>
 * 
 */ 
public abstract class Movable : MonoBehaviour
{
    /// <summary> 
    /// Length of time for a single time movement, in seconds.  
    /// </summary>
    public float moveTime = 0.1f;
    ///<summary> 
    ///Layer movement will be checked on
    ///</summary>
    public LayerMask collisionMask;

    ///<summary> 
    ///Collision box around the actor
    ///</summary>
    private BoxCollider2D boxCollider;

    ///<summary>
    ///2d Physics body of the actor
    ///</summary>
    private Rigidbody2D rigidBody;
    /// <summary>
    /// offset of building or actor from the anchor point of a tile
    /// </summary>
    private const float GRID_OFFSET = 0.5f;

    /// <summary> 
    /// Inverse function of moveTime 
    /// </summary>
    protected float inverseMoveTime;

    /// <summary>
    /// 
    /// </summary>
    public bool isMoving;

    // Start is called before the first frame update. Overridable
    protected virtual void Start()
    {
        //creating local instants of our colliders
        boxCollider = GetComponent<BoxCollider2D>();
        rigidBody = GetComponent<Rigidbody2D>();

        //getting the inverse of the movetime
        inverseMoveTime = 1f / moveTime;
        
    }

    /**
     * <summary>
     * Handles the movement of a movable object between the movable objects current location
     * and the location of the next tile
     * </summary>
     * <param name="x"> X coordinate of the direction of the movement</param>
     * <param name="y"> Y coordinate of the direction of the movement</param>
     * <param name="ray"> Ray for handling hit detection </param>
     * <param name="destination">destination for the attempted movement</param>
     * <returns>True if collision was not blocked</returns>
     */
    protected bool Move (int x, int y, out RaycastHit2D[] ray, out Vector2 destination)
    {

        //exit condition
        //Getting the start and end point of the move, based on passed movement
        Vector2 currentPosition = transform.position;
        Vector2 endPosition = currentPosition + new Vector2 (x, y);
        RaycastHit2D[] terrainRay;
        destination = endPosition;

        //If the actor is offset from the grid, realign them
        if (endPosition.x % GRID_OFFSET != 0)
        {
            endPosition.x = Mathf.Floor(endPosition.x) + GRID_OFFSET;
        }

        if (endPosition.y % GRID_OFFSET != 0)
        {
            endPosition.y = Mathf.Floor(endPosition.y) + GRID_OFFSET;
        }
        //preventing collision with self
        boxCollider.enabled = false;

        //Cast a ray between the current location and the destination, checking on the collision layer
        ray = Physics2D.LinecastAll(currentPosition, endPosition, collisionMask);
        terrainRay = Physics2D.LinecastAll(currentPosition, endPosition);
        //re-enable collision
        boxCollider.enabled = true;
        //If object is already moving, do not allow movement until they have stopped. 
        if (isMoving == true)
        {
            return false;
        }


        //If the ray did not collide, movement was sucessful
        if (ray.Length == 0)
        {

            foreach(RaycastHit2D hit in terrainRay)
            {
                Debug.Log(hit.transform.name);
                if(hit.transform.name == "Tilemap")
                {
                    Debug.Log(terrainRay.Length);
                    return false;
                }
            }
            return true;
        }
        else
        {
            //Check each element and see if its placed
            foreach (RaycastHit2D hit in ray)
            {
                //check if that object was a placeable structure
                if (hit.collider.GetComponent<PlaceableStructure>() != null)
                {
                    //check if the placeable structure actually has been placed
                    if (hit.collider.GetComponent<PlaceableStructure>().placed == true)
                    {
                        //if it has, prevent movement
                        return false; 
                    }
                }
                else if(hit.collider.GetComponent<ImpassibleStructure>() != null)
                {
                    return false;
                }
                else if(hit.collider.GetComponent<GatherableResource>() != null)
                {
                    return false;
                }
            }
            
        }
        //movement failed
        return true;
    }

    /**
     * <summary>
     * Coroutine that ensures movement is smooth betwene tiles
     * </summary>
     *
     */
    protected IEnumerator SmoothMovement (Vector3 destination)
    {
        isMoving = true;
        //gets the distance between current location and the destination. 
        float remainingDistaince = (transform.position - destination).sqrMagnitude;

        Debug.Log(destination.ToString());

        //Move the object towards its destination
        //Epsilon used to prevent Zeno's Paradox of Movement
        while (remainingDistaince > float.Epsilon)
        {
            //pick a position between current one 
            Vector3 newPosition = Vector3.MoveTowards(rigidBody.position, destination, inverseMoveTime * Time.deltaTime);

            //Set the position of the rigid body to the designated locaiton
            rigidBody.MovePosition(newPosition);

            //recalculate the distance and continue
            remainingDistaince = (transform.position - destination).sqrMagnitude;
            yield return null;
        }
        isMoving = false;

    }

    /**
     * <summary> 
     * Attempt to move the player, and on a failure, handle with OnCantMove based on what type of object was collided with
     * </summary>
     * <param name="x"> X coordinate of the destination tile</param>
     * <param name="y"> Y coordinate of the destination tile</param>
     */
    protected virtual void AttemptMove<T> (int x, int y)
        where T : Component
    {
        RaycastHit2D[] collisions;
        Vector2 destination;
        bool canMove = Move(x, y, out collisions,out destination);

        //If nothing was hit, don't continue
        if (canMove)
        {
            return;
        }

        //Determine what the actor hit
        foreach (RaycastHit2D collision in collisions)
        {
            T hitComponent = collision.transform.GetComponent<T>();

            Debug.Log(hitComponent.ToString());

            if (!canMove && hitComponent != null)
                OnCantMove(hitComponent);
        }
    }

    /**
     * <summary>
     * Handles if the movable actor is blocked
     * </summary>
     */
    protected abstract void OnCantMove<T>(T component)
        where T : Component;
}
