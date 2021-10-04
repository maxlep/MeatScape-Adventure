using System;
using Shapes;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class ConeEmitter : MonoBehaviour
{
    [SerializeField] private float LifetimeMin = 5f;
    [SerializeField] private float LifetimeMax = 5f;
    [SerializeField] private float ConeAngle = 0f;
    [SerializeField] private float InitialSpeedMin = 50f;
    [SerializeField] private float InitialSpeedMax = 50f;
    [SerializeField] private float GravityFactor = 0f;
    private bool isDirectionLocalSpace = true;
    [SerializeField] private Vector3 ConeDirection = Vector3.up;
    [SerializeField] private AnimationCurve MovementCurve;

    public UnityEvent OnDeath;

    private float StartTime = Mathf.Infinity;
    private float percentLifetime;
    private float gizmoConeHeight = 10f;
    private float initialSpeed;
    private float lifeTime;
    private Vector3 dir;
    private Vector3 velocity;
    private bool isDead = true;

    public void Initialize()
    {
        isDead = !gameObject.activeSelf;
        StartTime = Time.time;
        dir = GetDirectionFromCone();
        velocity = initialSpeed * dir;
        initialSpeed = Random.Range(InitialSpeedMin, InitialSpeedMax);
        lifeTime = Random.Range(LifetimeMin, LifetimeMax);
        percentLifetime = 0f;
    }

    private void OnDisable()
    {
        isDead = true;
    }

    private void Update()
    {
        if (isDead) return;
        
        percentLifetime = (Time.time - StartTime) / lifeTime;

        if (percentLifetime > 1f)
        {
            OnDeath.Invoke();
            isDead = true;
        }
        else
        {
            //Move
            velocity += Physics.gravity * (GravityFactor * Time.deltaTime * Time.deltaTime);
            transform.position += velocity * Time.deltaTime;
        }
    }

    private Vector3 GetDirectionFromCone()
    {
        float coneRadius = Mathf.Tan(ConeAngle * Mathf.Deg2Rad) * gizmoConeHeight;
        Vector2 horizontalOffset = Random.insideUnitCircle * coneRadius;
        Vector3 endPoint = transform.position + new Vector3(horizontalOffset.x, gizmoConeHeight, horizontalOffset.y);
        return (endPoint - transform.position).normalized;
    }

    private void OnDrawGizmos()
    {
        //Draw cone for spread
        Vector3 dir = (isDirectionLocalSpace) ? 
            transform.TransformDirection(ConeDirection.normalized) :
            ConeDirection.normalized;
        Quaternion lookRotation = Quaternion.LookRotation(-dir);

        float coneRadius = Mathf.Tan(ConeAngle * Mathf.Deg2Rad) * gizmoConeHeight;
        Vector3 conePosition = transform.position + dir * gizmoConeHeight;
        
        Draw.Cone(ShapesBlendMode.Transparent, ThicknessSpace.Meters, conePosition, lookRotation,
            coneRadius, gizmoConeHeight, false, new Color(1f, 1f, 0f, .3f));
        
        
        //Draw line and cone tip for direction
        Color directionColor = new Color(150f/255f, 50f/255f, 220f/255f, 1f);
        float lineLength = 13f;
        Draw.LineThickness = .2f;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + (lineLength * dir);
        float rad = .5f;
        float coneHeight = 1.75f;
        Vector3 coneOffset = -dir * (coneHeight * .5f);
        Quaternion coneRot = Quaternion.LookRotation(dir);

        Draw.Line(startPos, endPos, directionColor);
        if (!Mathf.Approximately(dir.magnitude, 0f)) 
            Draw.Cone(endPos + coneOffset,
                coneRot, rad, coneHeight, true, directionColor);
    }
}
