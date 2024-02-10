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
        initialPosition = transform.position;
        originalRotation = transform.rotation;
    }

    public void Respawn()
    {
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.velocity = new Vector3(0, 0, 0);
            rb.angularVelocity = new Vector3(0, 0, 0);
        }

        transform.position = initialPosition + new Vector3(0, .5f, 0);
        transform.rotation = originalRotation;
        this.gameObject.SetActive(true);
    }

	// Update is called once per frame
	void Update()
	{

	}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == respawnWhenTouches.name)
        {
            Invoke(nameof(Respawn), 1);
        }
    }
}