using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Player game object. 
/// </summary>
public class Player : Movable
{
    #region Propeties
    /// <summary>
    /// Players animation controller
    /// </summary>
    private Animator animator;
  //  Vector2 lookDirection = new Vector2(1, 0);
    /// <summary>
    /// X cordinate of the player on the grid. 
    /// </summary>
    protected int xCord;
    /// <summary>
    /// Y cordinate of the player on the grid. 
    /// </summary>
    protected int yCord;

    /// <summary>
    /// Range the player is allowed to interact with objects
    /// </summary>
    public const int INTERACTION_RANGE = 3;

    /// <summary>
    /// Last object the player interacted with
    /// </summary>
    /// 
    public RaycastHit2D lastObjectClicked;

    /// <summary>
    /// Check for if the player currently has a user interface open
    /// </summary>
    public bool canInteract = true;
    #endregion

    #region Unity Fuctions
    ///<summary>
    /// Enable all properties of a movable game object, as well as start the player animator
    /// </summary> 
    protected override void Start()
    {
        animator = GetComponent <Animator>();
        base.Start();
    }

    /**  
     * <summary> 
     *  Check every frame for player actions
     * </summary>
     */
    private void Update()
    {

        #region User input 

        //DIRECTIONAL CONTROLS

        //If the user is pressing the left or right directions, get what directions
        //Only check once the button has pressed, so that one press = one movement
        if (Input.GetButtonDown("Horizontal"))
        {
            xCord = (int)(Input.GetAxisRaw("Horizontal"));
            animator.SetFloat("Move X", xCord);
            animator.SetFloat("Move Y", 0);
        }
        else
        {
            xCord = 0;
        }

        //If the user is pressing the up or down directions, get what directions
        //Only check once the button has pressed, so that one press = one movement
        if (Input.GetButtonDown("Vertical"))
        {
            yCord = (int)(Input.GetAxisRaw("Vertical"));
            animator.SetFloat("Move Y", yCord);
            animator.SetFloat("Move X", 0);
        }
        else
        {
            yCord = 0;
        }

  


        //If two directions are held, prefer horizontal movement
        if (xCord != 0)
        {
            yCord = 0;
        }

        //if the player is attempting to move, move them accordingly
        if (xCord != 0 || yCord != 0)
        {

            AttemptMove<Impassible>(xCord, yCord);

        }

        if (gameObject.GetComponentInChildren<MouseManager>().isObjectSelected)
        {
            
            canInteract = false;
        }

        #endregion

        #region UI checks

        //If a UI is open, check how far the player is from the object
        if (!canInteract)
        {
          //  try
     //       {
                if (lastObjectClicked == true)
                {
                    //If the player is too far from the object, close the UI
                    if (Vector2.Distance(lastObjectClicked.collider.bounds.center, transform.position) > INTERACTION_RANGE)
                    {
                        
                            lastObjectClicked.collider.GetComponentInChildren<AutoGatherUI>(true).HideUI();
                        this.canInteract = true;
                     }
                }
            //}catch
   //         {
                //if the player rapidly spams delete, supress errorsa and allow interaction
   //             this.canInteract = true;
   //             Debug.Log("Object alredy destroyed");
    //        }
        }
        #endregion



    }
    #endregion
    #region Player Movement
    /**
     * <summary>
     *  Handles what happens if the player moves, or collides with terrain or structures
     * </summary>
     * <param name="x"> X coordinate of the destination tile</param>
     * <param name="y"> Y coordinate of the destination tile</param>
     */
    protected override void AttemptMove<T>(int xCord, int yCord)
    {
        //Whether the player hit something
        RaycastHit2D[] hits;
        Vector2 destination;

        //Attempt to the player and save the result
        bool successfulMove = Move(xCord, yCord, out hits, out destination);

        //If the player was able to move
        if (successfulMove)
        {
            //Move the player
            StartCoroutine(SmoothMovement(destination));
            //Play the sound effect
            SoundManager.instance.PlayMoveSound();
        }
        else
        {
            //If the player was clicking move too rapidly ignore the input
            if (hits.Length != 0)
            {
                //check if anything they hit was impassible
                foreach (RaycastHit2D hit in hits)
                {
                    //If the player hit something impassible, pass to OnCantMove
                    if (hit.collider.gameObject.GetComponent<Impassible>() == true)
                    {
                        OnCantMove<Impassible>(hit.collider.gameObject.GetComponent<Impassible>());
                        break;
                    }
                }
            }
        }
    }

    /**
     * <summary> 
     * Handles event if player is unable to move.
     * </summary>
     * <param name="component">Type of object player collided with</param>
     * 
     */
    protected override void OnCantMove<T>(T component)
    {
        //Play the bump sound effect. Currently impassible doesn't do anything besides
        //block collision
        if (component.GetComponent<Impassible>() != null)
        {
            Impassible collided = component as Impassible;
            SoundManager.instance.PlayBumpSound();
        }
    }
    #endregion
    /// <summary>
    /// set the players can interact property to true if the UI is closed via a clickEvent
    /// </summary>
    public void OnClickUiClose()
    {
        canInteract = true;
    }

}
