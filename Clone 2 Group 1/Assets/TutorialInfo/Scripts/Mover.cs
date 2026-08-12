using UnityEngine;

public class Mover : MonoBehaviour
{
    private float moveSpeed;
    private Vector3 moveDirection;
    private float destroyXBound = 25f; // Distance off-screen to despawn

    public void Initialize(float speed, Vector3 direction)
    {
        this.moveSpeed = speed;
        this.moveDirection = direction.normalized;
    }

    void Update()
    {
        // Move in world space
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

        // Self-despawn when far enough off-screen to avoid clutter
        if (Mathf.Abs(transform.position.x) > destroyXBound)
        {
            Destroy(gameObject); // Or return to Member 1's object pool
        }
    }
}