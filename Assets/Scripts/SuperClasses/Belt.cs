using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Belt game object that moves resources between structures
/// </summary>
public class Belt : PlaceableStructure
{
    private void Update()
    {
        //if the belt is on the ground
        if (placed)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(gameObject.transform.position, Vector2.zero);
            //if something is on the belt, at its center
            if (hits.Length > 1)
            {
                foreach(RaycastHit2D hit in hits)
                {
                    if (hit.transform.GetComponent<BeltItem>())
                    {
                        hit.transform.GetComponent<BoxCollider2D>().enabled = false;
                        gameObject.GetComponent<BoxCollider2D>().enabled = false;
                        RaycastHit2D destinationHit = Physics2D.Raycast(gameObject.transform.position, outputDirection,1);
                        hit.transform.GetComponent<BoxCollider2D>().enabled = true;
                        gameObject.GetComponent<BoxCollider2D>().enabled = true;
                        //If the ray hit something
                        if (destinationHit.transform != null)
                        {
                            //if the item at the location is a belt
                            if (destinationHit.collider.GetComponent<Belt>())
                            {
                                //if the destination belt is placed
                                if (destinationHit.collider.GetComponent<Belt>().placed == true)
                                {
                                    //if the beltItem isn't moving
                                    if (!hit.collider.GetComponent<BeltItem>().isMoving)
                                    {
                                        Debug.Log(outputDirection.ToString());
                                        //Move the beltItem
                                        hit.transform.GetComponent<BeltItem>().MoveToDestination(outputDirection);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
