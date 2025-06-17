using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Security.Cryptography;

public class Cannon : NetworkBehaviour
{
    public GameObject cannonBallPrefab;
    public Transform firePoint;
    public float shootForce = 20f;
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [Header("Timing")]
    [SerializeField] private float fireDelay = 3f;
    [Header("VFX & SFX")]
    [SerializeField] private GameObject fireVFX;
    [SerializeField] private GameObject sinkVFX;
    [SerializeField] private AudioClip fireSfx;
    [SerializeField] private AudioClip sinkSFX;
    [SerializeField] private Transform bubbleTransform;
    
    
    
    private AudioSource audioSource;


    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        if (!animator) animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        StartCoroutine(CannonRoutine());
    }
    IEnumerator CannonRoutine()
    {
        while (true)
        {
            animator.Play("Rise");
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
            yield return new WaitForSeconds(fireDelay);
            animator.Play("FireAndSink");
            yield return new WaitForSeconds(1.1f); 
            //OnSinkComplete();
        }
    }
    public void OnCannonFire()
    {
        if (!IsServer) return;
        GameObject ball = Instantiate(cannonBallPrefab, firePoint.position, Quaternion.identity);
        ball.GetComponent<NetworkObject>().Spawn(true);
        ball.GetComponent<Rigidbody>().AddForce(firePoint.forward * shootForce, ForceMode.Impulse);

        if (fireVFX)
            Destroy(Instantiate(fireVFX, firePoint.position, Quaternion.identity), 2f);
        if (fireSfx)
            audioSource.PlayOneShot(fireSfx);
    }
    public void OnSinkComplete()
    {
        Debug.Log("ON SINK ON SINK ON SINK !!!");
        if (!IsServer) return;
       
        if (sinkVFX)
            Destroy(Instantiate(sinkVFX, bubbleTransform.position, Quaternion.identity), 2f);
        if(sinkSFX)
            audioSource.PlayOneShot(sinkSFX);
       Invoke(nameof(SinkCompleted),2f);
    }

    private void SinkCompleted()
    {
        GetComponent<NetworkObject>().Despawn(true);
    }
}