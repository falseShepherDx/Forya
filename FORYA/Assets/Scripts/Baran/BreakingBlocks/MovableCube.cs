using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class MovableCube : NetworkBehaviour
{

    private Rigidbody rb;
    private Animator animator;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
       
    }

    private void Start()
    {
        GridDropper.instance.allCubes.Add(this);
    }

    public void EnablePhsyics()
    {
        animator.SetBool("isSelected", false);
        rb.isKinematic = false;
        rb.useGravity = true;
        GridDropper.instance.allCubes.Remove(this);
        StartCoroutine(ReturnPos());
    }


    IEnumerator ReturnPos()
    {
        yield return new WaitForSecondsRealtime(4f);

        rb.isKinematic = true;
        rb.useGravity = false;
        transform.position = new Vector3(transform.position.x, -3 ,transform.position.z);

        GridDropper.instance.allCubes.Add(this);

    }
    public void Select()
    {
        animator.SetBool("isSelected", true);
    }

    private void OnDisable()
    {
        GridDropper.instance.allCubes.Remove(this);
    }
}
