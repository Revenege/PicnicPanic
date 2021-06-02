using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Apple tree game object. Handles scripting relating to apple trees
/// </summary>
public class AppleTree : GatherableResource
{
    private void Awake()
    {
        resourceName = "AppleTree";
    }
}
