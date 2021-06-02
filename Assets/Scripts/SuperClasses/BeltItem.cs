using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeltItem : Movable
{
    /// <summary>
    /// Direction the belt item is going
    /// </summary>
    public Vector2Int direction;

    /// <summary>
    /// belt the item is actively on
    /// </summary>
    public Belt currentBelt;

    /// <summary>
    /// Ingredient the beltItem represents
    /// </summary>
    public Ingredient ingredient;

    /// <summary>
    /// Name of the recipe the ingredient was produced from
    /// </summary>
    public string recipeName;
    /// <summary>
    /// On creation, move on top of the belt.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        AttemptMove<Impassible>(direction.x,direction.y);
    }

    /// <summary>
    /// Check if the game object is not on top of something
    /// </summary>
    private void FixedUpdate()
    {
        RaycastHit2D[] hit = Physics2D.RaycastAll(transform.position, Vector2.zero);
        if (hit.Length ==1)
        {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// Attempt a movement in the given direction
    /// </summary>
    /// <typeparam name="T">What the object is checking for collision with</typeparam>
    /// <param name="x">X component of the direction of the move</param>
    /// <param name="y">Y component of the direction of the move</param>
    protected override void AttemptMove<T>(int x, int y)
    {
        //Whether the item hit something
        RaycastHit2D[] hits;
        Vector2 destination;
        bool targetBeltEmpty = true;
        bool beltDetected = false;

        //Attempt to move the belt item
        bool destinationUnoccupied= Move(x, y, out hits, out destination);
        //If the belt item is blocked by something
        if (destinationUnoccupied == false)
        {
            //Check each element 
            foreach (RaycastHit2D hit in hits)
            {

                //if collided with another BeltItem, prevent movement
                if (hit.collider.GetComponent<BeltItem>())
                {
                    Debug.Log(gameObject.name+ " No belt item");
                    OnCantMove<BeltItem>(hit.collider.gameObject.GetComponent<BeltItem>());
                    targetBeltEmpty = false;
                }
                if (hit.collider.GetComponent<Belt>())
                {
                    Debug.Log(gameObject.name + " Valid Belt");
                    beltDetected = true;
                    currentBelt = hit.collider.GetComponent<Belt>();
                }
            }
            //if belt exists, and isn't full, begin movement
            if (beltDetected && targetBeltEmpty)
            {

                StartCoroutine(SmoothMovement(destination));
            }
        }
    }
    //Accessor for Attempt Move
    public void MoveToDestination(Vector2 direction)
    {
        AttemptMove<Impassible>((int)direction.x, (int)direction.y);
    }
    /// <summary>
    /// Controls the action that occurs if the object cant move
    /// </summary>
    /// <typeparam name="T">Type of object collided with</typeparam>
    /// <param name="component">Type of object collided with</param>
    protected override void OnCantMove<T>(T component)
    {
        // Currently impassible doesn't do anything besides
        //block collision
        if (component.GetComponent<BeltItem>() != null)
        {
            BeltItem collided = component as BeltItem;
        }
    }
}

