using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    [SerializeField] private Transform treePrefab;

    public HashSet<int> Init(float z)
    {
        //Places obstacle at location given
        transform.position = new Vector3(0,0,z);

        //ensures obstacles outside of game area
        HashSet<int> locations = new() { -6, 6 };

        //populates terrain with trees
        int numTrees = Random.Range(1, 5);

        for (int i = 0; i < numTrees; i++)
        {
            Transform tree = Instantiate(treePrefab, transform);
            int xPos = Random.Range(-5, 6);
            tree.position = new Vector3(xPos, 0.2f, z);
            locations.Add(xPos);
        }
        return locations; //This stores the locations of all the trees, which will be used by the character controller to stop movement through them
    }
}
