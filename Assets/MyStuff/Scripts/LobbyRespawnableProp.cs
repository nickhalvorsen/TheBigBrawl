using Mirror;
using UnityEngine;

public class LobbyRespawnableProp : NetworkBehaviour
{
    private Vector3 initialPosition;
    private Quaternion originalRotation;
    public GameObject respawnWhenTouches;

    // Start is called before the first frame update
    void Start()
	{
        if (GetComponent<Rigidbody>() == null)
        {
            Debug.LogWarning("Rigidbody component is missing on GameObject: " + gameObject.name);
        }
        if (respawnWhenTouches == null)
        {
            Debug.LogWarning("RespawnWhenTouches param is null on GameObject: " + gameObject.name);
        }

        initialPosition = transform.position;
        originalRotation = transform.rotation;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == respawnWhenTouches.name)
        {
            Invoke(nameof(Respawn), 1);
        }
    }
    private void Respawn()
    {
        var rb = GetComponent<Rigidbody>();
        rb.velocity = new Vector3(0, 0, 0);
        rb.angularVelocity = new Vector3(0, 0, 0);

        transform.position = initialPosition + new Vector3(0, .5f, 0);
        transform.rotation = originalRotation;
        this.gameObject.SetActive(true);
    }
}