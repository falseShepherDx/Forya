using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GridDropper : NetworkBehaviour
{
    public static GridDropper instance;
    public List<MovableCube> allCubes;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);    
        }
    }
    void Update()
    {
        if(!IsServer) { return; }

        if (Input.GetKeyDown(KeyCode.F))
        {
            DropRandomCubes(10);
        }
    }

    public void DropRandomCubes(int amount)
    {
        if (allCubes.Count < amount) return;
        
        List<MovableCube> shuffled = allCubes.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < amount; i++)
        {
            
            shuffled[i].Select();
        }
    }
}
