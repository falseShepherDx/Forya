using UnityEngine;
using Unity.Netcode;

public class GirdSpawner : NetworkBehaviour
{
    public GameObject[] cubePrefabs; // Birden fazla prefab tanýmý için array
    public int gridSize;
    public float spacing;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        //GridDesigner(); // Otomatik çaðýrmak istemiyorsan yorumlu býrak
    }

    public void GridDesigner()
    {
        for (int x = -3; x < gridSize - 3; x++)
        {
            Debug.Log("created");
            for (int z = 0; z < gridSize; z++)
            {
                Vector3 pos = new Vector3(x * spacing, -3, z * spacing);
                GameObject selectedPrefab = cubePrefabs[Random.Range(0, cubePrefabs.Length)];

                GameObject cube = Instantiate(selectedPrefab, pos, Quaternion.Euler(-90, 0, 0));
                cube.GetComponent<NetworkObject>().Spawn();
                cube.transform.SetParent(this.transform);
            }
        }
    }
}
