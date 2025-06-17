using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;  

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("Movement&Physics")]
     private MonoBehaviour movementScript; 
     private Collider playerCol;      
     private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        movementScript = GetComponent<PlayerMovement_B>();
    }

    public void OnDeath()
    {
        Debug.Log("Player Death");
        if (movementScript != null) movementScript.enabled = false;
        if (playerCol != null) playerCol.enabled = false;
        
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
            
        }
    }
}
