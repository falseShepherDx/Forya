using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;  // ← VFX Graph API

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("VFX Graph")]
    [SerializeField] private GameObject bloodVFXGraphPrefab;
    [SerializeField] private float vfxDuration = 3f;
    [Header("Audio")]
    [SerializeField] private AudioClip deathSfx;
    [SerializeField] private float destroyDelay = 5f;
    [Header("Physics")]
    [SerializeField] private MonoBehaviour movementScript;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private  Collider playerCollider;
    private AudioSource audioSource;
    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void OnDeath()
    {
        if (movementScript != null) movementScript.enabled = false;
        if (playerCollider != null)   playerCollider.enabled   = false;
        
        rb.isKinematic = false;
        rb.useGravity  = true;
        rb.constraints = RigidbodyConstraints.None;
        
        if (bloodVFXGraphPrefab != null)
        {
            GameObject go = Instantiate(
                bloodVFXGraphPrefab,
                transform.position + Vector3.up * 1f,
                Quaternion.identity
            );
            var vfx = go.GetComponent<VisualEffect>();
            if (vfx != null)
                vfx.Play(); 
            Destroy(go, vfxDuration);
        }
        if (deathSfx != null)
            audioSource.PlayOneShot(deathSfx);
        StartCoroutine(DestroyAfterDelay());
    }
    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}
