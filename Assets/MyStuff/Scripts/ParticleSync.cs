using Mirror;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSync : NetworkBehaviour
{
    private ParticleSystem _particleSystem;

    // Start is called before the first frame update
    void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    public void PlayParticle()
    {
        CmdServerParticle();
    }

    // client sending to server
    [Command]
    void CmdServerParticle()
    {
        RpcTriggerParticleOnClients();
    }

    // server sending to clients
    [ClientRpc]
    void RpcTriggerParticleOnClients()
    {
        _particleSystem.Play();
    }
}