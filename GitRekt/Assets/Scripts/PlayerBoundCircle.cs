using UnityEngine;
using System.Collections;

public class PlayerBoundCircle : PlayerBound {
    public float radius = 10f;

    protected override Vector2 UpdatePosition(Vector2 aPos, float aRadius) {
        Vector2 center = transform.position;
        Vector2 delta = aPos - center;

        float distanceSqr = delta.sqrMagnitude;
        if(distanceSqr > radius*radius) {
            Vector2 dir = delta/Mathf.Sqrt(distanceSqr);
            aPos = center + dir*(radius - aRadius);
        }

        return aPos;
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}