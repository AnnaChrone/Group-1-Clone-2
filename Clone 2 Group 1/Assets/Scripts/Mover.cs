using UnityEngine;

public class Mover : MonoBehaviour
{
    private float speed = 5f;
    private Vector3 moveDirection = Vector3.right;
    private float destroyBoundaryX = 25f;

    // --- ADD THESE TWO PUBLIC PROPERTIES ---
    public float CurrentSpeed => speed;
    public Vector3 CurrentDirection => moveDirection;
    // ---------------------------------------

    public void Initialize(float moveSpeed, Vector3 direction, float despawnDistance = 25f)
    {
        this.speed = moveSpeed;
        this.moveDirection = direction.normalized;
        this.destroyBoundaryX = despawnDistance;
    }

    private void Update()
    {
        // Move object smoothly in world space
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        // Despawn off-screen to save performance
        if (Mathf.Abs(transform.position.x) > destroyBoundaryX)
        {
            Destroy(gameObject);
        }
    }
}