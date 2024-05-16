using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityPlane : GravitySource
{
    [SerializeField]
    float gravity = 9.81f;

    [SerializeField, Min(0f)]
    float range = 1f;

    [SerializeField]
    private MeshRenderer meshRenderer;

    public override Vector3 GetGravity(Vector3 position)
    {
        Vector3 up = transform.up;
        float distance = Vector3.Dot(up, position - transform.position);
        if (distance > range)
        {
            return Vector3.zero;
        }

        float g = -gravity;
        if (distance > 0f)
        {
            g *= 1f - distance / range;
        }
        return g * up;
    }

    void OnDrawGizmos()
    {
        Vector3 size = meshRenderer.bounds.size;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, size);

        Vector3 scale = transform.localScale;
        scale.y = range;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, scale);

        if (range > 0f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.up, size);
        }
    }


}
