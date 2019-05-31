using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour
{

    BoxCollider2D collider;
    public LayerMask collisionMask;
    public CollisionInfo collisions;
    public float maxClimbAngle = 80;
    public float maxDescendAngle = 80;
    Origins origins;
    public float skinWidth = .015f;
    public int vRayCount = 4;
    public int hRayCount = 4;
    float vRaySpacing;
    float hRaySpacing;

    void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        CalcRaySpacing();
    }
    public void Move(Vector3 velocity)
    {

        UpdateOrigins();
        collisions.Reset();

        if (velocity.y < 0)
        {
            DescendSlope(ref velocity);
        }

        if (velocity.x != 0)
        {
            HorCollide(ref velocity);
        }
        if (velocity.y != 0)
        {
            VertCollide(ref velocity);
        }


        transform.Translate(velocity);

    }

    struct Origins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    public struct CollisionInfo
    {
        public bool globalCollision;
        public bool above, below, left, right;
        public bool hurtSurface;
        public bool climbingSlope;
        public bool descendingSlope;
        public bool breakableWall;
        public float slopeAngle, slopeAngleOld;


        public void Reset()
        {
            above = below = left = right = false;
            climbingSlope = false;
            descendingSlope = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;

        }

        public void Display()
        {
            Debug.Log("ABOVE: " + above + " BELOW: " + below + " LEFT: " + left + " RIGHT: " + right);
        }
    }
    void UpdateOrigins()
    {

        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        origins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        origins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        origins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        origins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);

    }

    void CalcRaySpacing()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        vRaySpacing = bounds.size.x / (vRayCount - 1);
        hRaySpacing = bounds.size.y / (hRayCount - 1);
    }



    void HorCollide(ref Vector3 velocity)
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);
        float dirX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;
        for (int i = 0; i < hRayCount; i++)
        {
            Vector2 rayOrigin = (dirX == -1) ? origins.bottomLeft : origins.bottomRight;

            if (collisions.below)
            {
                rayOrigin += (Vector2.up * hRaySpacing * i) + (Vector2.up * velocity.y * Time.deltaTime);
            }
            else
            {
                rayOrigin += (Vector2.up * hRaySpacing * i);
            }
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * rayLength * dirX);
            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                if (i == 0 && slopeAngle <= maxClimbAngle)
                {
                    float distanceToSlope = 0;
                    if (slopeAngle != collisions.slopeAngleOld)
                    {
                        distanceToSlope = hit.distance - skinWidth;
                        velocity.x -= distanceToSlope * dirX;
                    }
                    ClimbSlope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlope * dirX;
                }
                if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * dirX;
                    rayLength = hit.distance;

                    if (collisions.climbingSlope)
                    {
                        velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad * Mathf.Abs(velocity.x));
                    }

                    collisions.left = dirX == -1;
                    collisions.right = dirX == 1;
                }
            }
        }
    }
    void VertCollide(ref Vector3 velocity)
    {
        float dirY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for (int i = 0; i < vRayCount; i++)
        {
            Vector2 rayOrigin = (dirY == -1) ? origins.bottomLeft : origins.topLeft;
            rayOrigin += Vector2.right * (vRaySpacing * i + velocity.x);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * dirY, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * rayLength * dirY, Color.red);

            if (hit)
            {
                velocity.y = (hit.distance - skinWidth) * dirY;
                rayLength = hit.distance;

                if (collisions.climbingSlope)
                {
                    velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }
                collisions.above = dirY == 1;
                collisions.below = dirY == -1;
            }
        }

        if (collisions.climbingSlope)
        {
            float dirX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 rayOrigin = ((dirX == -1) ? origins.bottomLeft : origins.bottomRight) + Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisions.slopeAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * dirX;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }
    }
    void ClimbSlope(ref Vector3 velocity, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (velocity.y <= climbVelocityY)
        {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        }
    }

    void DescendSlope(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = directionX == -1 ? origins.bottomRight : origins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
            {
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= descendVelocityY;

                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true;
                    }
                }
            }
        }
    }
}
