using System.Collections.Generic;
using UnityEngine;


public class PlacementResult
{
    public List<Vector2> path;

    public bool valid;


    public PlacementResult(
        List<Vector2> path,
        bool valid)
    {
        this.path = path;
        this.valid = valid;
    }
}