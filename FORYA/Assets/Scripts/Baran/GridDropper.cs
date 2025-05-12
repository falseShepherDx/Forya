using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class GridDropper : NetworkBehaviour
{
    public static GridDropper instance;
    public List<MovableCube> allCubes;

    [SerializeField] float time,timeGap,dropTimer;
    [SerializeField] int dropAmount;
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


   
    void DropLoop()
    {
        dropTimer += Time.deltaTime;
        if (dropTimer >= time)
        {
            DropRandomCubes(dropAmount);
            dropAmount = dropAmount + 2;
            time = time - timeGap;

            dropTimer = 0;  
        }
    }
    void Update()                                   //Update içinde yapmak yerie Coroutine ile zamaný yönetmek çok daha verimli olacak
    {
        if (IsServer)
        {
            DropLoop();

            if (Input.GetKeyDown(KeyCode.F))
            {
                DropRandomCubes(10);
            }
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
