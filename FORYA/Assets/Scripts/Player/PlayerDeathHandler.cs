using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;  // ← VFX Graph API

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("Movement&Physics")]
    [SerializeField] private MonoBehaviour movementScript; 
     private Collider playerCol;      
    [SerializeField] private Rigidbody rb;               
    [Header("Blood VFX")]
    [SerializeField] private GameObject[] bloodVFXVariants;
    [SerializeField] private float vfxDuration = 3f;
    [Header("Death SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] deathSfxVariants;
    
    [SerializeField] private float destroyDelay = 5f;       

    public void OnDeath()
    {
        
        if (movementScript != null) movementScript.enabled = false;
        if (playerCol != null) playerCol.enabled = false;

        
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
        }

       
        if (bloodVFXVariants != null && bloodVFXVariants.Length > 0)
        {
            var prefab = bloodVFXVariants[Random.Range(0, bloodVFXVariants.Length)];
            var go = Instantiate(prefab, transform.position + Vector3.up * 1f, Quaternion.identity);
            var vfx = go.GetComponent<VisualEffect>();
            vfx?.Play();
            Destroy(go, vfxDuration);
        }
        
        if (deathSfxVariants != null && deathSfxVariants.Length > 0 && audioSource != null)
        {
            var clip = deathSfxVariants[Random.Range(0, deathSfxVariants.Length)];
            audioSource.PlayOneShot(clip);
        }
        Destroy(gameObject, destroyDelay);
    }
}
