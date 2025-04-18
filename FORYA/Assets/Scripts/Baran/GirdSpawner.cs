using UnityEngine;
using Unity.Netcode;
public class GirdSpawner : NetworkBehaviour
{
    public GameObject cubePrefab;
    public int gridSize;
    public float spacing;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        //GridDesigner();
    }

    public void GridDesigner()
    {
        for (int x = -3; x < gridSize-3; x++)
        {
           for (int z = 0; z < gridSize; z++)
            {
                Vector3 pos = new Vector3(x* spacing, -3, z*spacing);
                GameObject cube = Instantiate(cubePrefab,pos,Quaternion.Euler(-90,0,0));
               
                cube.GetComponent<NetworkObject>().Spawn();
                cube.transform.SetParent(this.transform);
            }
        }
    }
}
