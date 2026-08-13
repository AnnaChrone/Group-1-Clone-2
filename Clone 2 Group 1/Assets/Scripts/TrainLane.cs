using System.Collections;
using UnityEngine;

public class TrainLane : MonoBehaviour
{
    [Header("Train Settings")]
    [SerializeField] private GameObject trainPrefab;
    [SerializeField] private GameObject warningLightObject;
    [SerializeField] private float trainSpeed = 30f;
    [SerializeField] private float warningDuration = 1.5f;
    [SerializeField] private float minCooldown = 5f;
    [SerializeField] private float maxCooldown = 12f;
    [SerializeField] private bool moveRight = true;

    private void Start()
    {
        if (warningLightObject != null)
        {
            warningLightObject.SetActive(false);
        }
        StartCoroutine(TrainCycle());
    }

    private IEnumerator TrainCycle()
    {
        while (true)
        {
            float cooldown = Random.Range(minCooldown, maxCooldown);
            yield return new WaitForSeconds(cooldown);

            if (warningLightObject != null)
            {
                warningLightObject.SetActive(true);
            }

            yield return new WaitForSeconds(warningDuration);

            if (warningLightObject != null)
            {
                warningLightObject.SetActive(false);
            }

            SpawnTrain();
        }
    }

    private void SpawnTrain()
    {
        float startX = moveRight ? -25f : 25f;
        Vector3 spawnPos = new Vector3(startX, transform.position.y + 0.5f, transform.position.z);
        Vector3 dir = moveRight ? Vector3.right : Vector3.left;

        GameObject train = Instantiate(trainPrefab, spawnPos, Quaternion.identity, transform);

        if (!moveRight)
        {
            train.transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        Mover mover = train.GetComponent<Mover>();
        if (mover != null)
        {
            mover.Initialize(trainSpeed, dir, 30f);
        }
    }
}