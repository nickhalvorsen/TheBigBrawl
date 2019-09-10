using Mirror;
using UnityEngine;

public class ForceSync : NetworkBehaviour
{
    public float clapEffectRadius = 4.0f;
    public float clapEffectForce = 2.0f;

    public void PlayForce()
    {
        CmdServerForce();
    }

    // client sending to server
    [Command]
    void CmdServerForce()
    {
        RpcTriggerForceOnClients();
    }

    // server sending to clients
    [ClientRpc]
    void RpcTriggerForceOnClients()
    {
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, clapEffectRadius);

        var me = GetComponent<Rigidbody>();

        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null && rb.GetInstanceID() != me.GetInstanceID())
            {
                rb.AddExplosionForce(clapEffectForce, explosionPos, clapEffectRadius, .5F, ForceMode.Impulse);
            }
        }
    }
}