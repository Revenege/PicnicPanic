using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A structure that is placeable by the player
/// </summary>
public class PlaceableStructure : ImpassibleStructure
{
    /// <summary>
    /// Whether the object has been placed
    /// </summary>
    public bool placed;

    /// <summary>
    /// Direction the placeable structure's output is facing
    /// </summary>
    public Vector2 outputDirection;

    /// <summary>
    /// Direction the placeable structure's input is facing
    /// </summary>
    public Vector2 inputDirection;
}
