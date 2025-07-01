using UnityEngine;

public class DebugGhostSystem : MonoBehaviour
{
    public Transform playerTransform;          // Actual predicted position (client-side)
    public Vector3 serverReconciledPosition;   // Last server reconcile position

    private GameObject clientGhost;
    private GameObject serverGhost;

    void Start()
    {
        // Create ghost objects
        clientGhost = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        clientGhost.transform.localScale = Vector3.one * 0.3f;
        clientGhost.GetComponent<Renderer>().material.color = Color.green;
        clientGhost.name = "ClientGhost (Predicted)";

        serverGhost = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        serverGhost.transform.localScale = Vector3.one * 0.3f;
        serverGhost.GetComponent<Renderer>().material.color = Color.red;
        serverGhost.name = "ServerGhost (Reconciled)";
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Update ghost positions
        clientGhost.transform.position = playerTransform.position + Vector3.up * 1f; // Green = client prediction
        serverGhost.transform.position = serverReconciledPosition + Vector3.up * 1.5f; // Red = server correction
    }

    public void UpdateServerPosition(Vector3 serverPos)
    {
        serverReconciledPosition = serverPos;
    }
}
