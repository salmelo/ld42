using UnityEngine;
using System.Collections;

public static class VectorExtensions 
{
    public static Vector3 WithZ(this Vector3 v, float z)
    {
        return new Vector3(v.x, v.y, z);
    }
}
