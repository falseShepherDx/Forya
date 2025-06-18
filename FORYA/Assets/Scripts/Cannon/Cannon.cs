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
    private Vector3 targetDirection;
    private AudioSource audioSource;

    public void SetTarget(Vector3 targetPos)
    {
        targetDirection = (targetPos - firePoint.position).normalized;
    }
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        if (!animator) animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        StartCoroutine(CannonRoutine());
    }
    IEnumerator CannonRoutine()
    {
            animator.Play("Rise");
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
            yield return new WaitForSeconds(fireDelay);
            animator.Play("FireAndSink");
            yield return new WaitForSeconds(1.1f); 
            //OnSinkComplete();
    }
    public void OnCannonFire()
    {
        if (!IsServer) return;
        GameObject ball = Instantiate(cannonBallPrefab, firePoint.position, Quaternion.identity);
        var netObj = ball.GetComponent<NetworkObject>();
        netObj.Spawn(true);
        ball.GetComponent<Rigidbody>().AddForce(targetDirection * shootForce, ForceMode.Impulse);

        if (fireVFX)
            Destroy(Instantiate(fireVFX, firePoint.position, Quaternion.identity), 2f);
        if (fireSfx)
            audioSource.PlayOneShot(fireSfx);
    }
    public void OnSinkComplete()
    {
        
        if (!IsServer) return;
       
        if (sinkVFX)
            Destroy(Instantiate(sinkVFX, bubbleTransform.position, Quaternion.identity), 1f);
        if(sinkSFX)
            audioSource.PlayOneShot(sinkSFX);
       Invoke(nameof(SinkCompleted),0.1f);
    }

    private void SinkCompleted()
    {
        GetComponent<NetworkObject>().Despawn(true);
    }
  
}