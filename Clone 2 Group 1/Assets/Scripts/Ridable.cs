using UnityEngine;

[RequireComponent(typeof(Mover))]
public class Ridable : MonoBehaviour
{
    private Mover mover;

    private void Awake()
    {
        mover = GetComponent<Mover>();
    }

    // Calculates velocity vector so the Player can match log speed
    public Vector3 GetMovementVelocity()
    {
        if (mover != null)
        {
            return mover.CurrentDirection * mover.CurrentSpeed;
        }
        return Vector3.zero;
    }
}