using Mirror;
using UnityEngine;

public class ForceSync : NetworkBehaviour
{
    public const float ClapEffectRadius = 4.0f;
    public const float ClapEffectForce = 0.2f;

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
        Vector3 explosionPos = GetClapEffectOrigin();
        Collider[] colliders = Physics.OverlapSphere(explosionPos, ClapEffectRadius);

        var me = GetComponent<Rigidbody>();

        foreach (Collider hit in colliders)
        {
            Rigidbody hitPlayer = hit.GetComponent<Rigidbody>();

            if (hitPlayer != null && hitPlayer.GetInstanceID() != me.GetInstanceID())
            {
                // old method, quite garbage
                //hitPlayer.AddExplosionForce(clapEffectForce, explosionPos, clapEffectRadius, .5F, ForceMode.Impulse);

                var distance = (hitPlayer.position - explosionPos).magnitude;
                if (distance > ClapEffectRadius)
                {
                    continue;
                }


                if (distance < 0.5)
                {
                    distance = 0.5f;
                }

                float forceMagnitude = ClapEffectForce * (1.0f / Mathf.Pow(distance / ClapEffectRadius, 2));
                var horizontalForceDirection = (hitPlayer.position - explosionPos).normalized;
                horizontalForceDirection.y = 0;

                hitPlayer.AddForce(horizontalForceDirection * forceMagnitude, ForceMode.Impulse);
                hitPlayer.AddForce(new Vector3(0, 1, 0) * forceMagnitude * 3, ForceMode.Impulse);
            }
        }
    }

    private Vector3 GetClapEffectOrigin()
    {
        // in front of the character, where the hands meet
        return transform.position + transform.forward * .75f;
    }
}