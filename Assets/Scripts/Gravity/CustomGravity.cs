using System.Collections.Generic;
using UnityEngine;

public static class CustomGravity 
{
    static List<GravitySource> sources = new List<GravitySource>();
    public static Vector3 GetGravity(Vector3 position)
    {
        Vector3 gravity = Vector3.zero;
        for (int i = 0; i < sources.Count; i++)
        {
            gravity += sources[i].GetGravity(position);
        }
        return gravity;
    }

    public static Vector3 GetUpAxis(Vector3 position)
    {
        Vector3 gravity = Vector3.zero;
        for (int i = 0; i < sources.Count; i++)
        {
            gravity += sources[i].GetGravity(position);
        }
        return -gravity.normalized;
    }

    public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
    {
        Vector3 gravity = Vector3.zero;
        for (int i = 0; i < sources.Count; i++)
        {
            gravity += sources[i].GetGravity(position);
        }
        upAxis = -gravity.normalized;
        return gravity;
    }

    public static void Register(GravitySource source)
    {
        Debug.Assert(
            !sources.Contains(source),
            "Duplicate registration of gravity source!", source
        );
        sources.Add(source);
    }

    public static void Unregister(GravitySource source)
    {
        Debug.Assert(
            sources.Contains(source),
            "Unregistration of unknown gravity source!", source
        );
        sources.Remove(source);
    }


}
