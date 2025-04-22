using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 3, -5);
    public float smoothSpeed = 5f;
    
    void Start()
    {
        // Se il target non è assegnato, cerca automaticamente il player
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("Camera target found automatically: " + target.name);
            }
            else
            {
                Debug.LogError("No player found with tag 'Player'! Camera won't follow anything.");
            }
        }
    }
    
    private void LateUpdate()
    {
        if (target == null) 
        {
            Debug.LogWarning("Camera target is null!");
            return;
        }
        
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
        transform.LookAt(target.position + Vector3.up);
    }
    
    // Metodo utile per reimpostare il target a runtime se necessario
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}