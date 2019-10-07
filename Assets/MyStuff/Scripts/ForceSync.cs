using Invector.CharacterController;
using Mirror;
using System.Linq;
using UnityEngine;

public class ForceSync : NetworkBehaviour
{
    public const float ClapEffectRadius = 4.0f;
    public const float ClapEffectForce = 0.25f;

    public void PlayForce()
    {
        var explosionPos = GetClapEffectOrigin();
        var forceOwnerInstanceId = gameObject.GetInstanceID();

        CmdServerForce(explosionPos, forceOwnerInstanceId);
    }

    // client sending to server
    [Command]
    void CmdServerForce(Vector3 explosionPos, int forceOwnerInstanceId)
    {
        RpcTriggerForceOnClients(explosionPos, forceOwnerInstanceId);
    }

    // server sending to clients
    [ClientRpc]
    void RpcTriggerForceOnClients(Vector3 explosionPos, int forceOwnerInstanceId)
    {
        var localPlayer = GetLocalPlayer();

        // Do not calculate force for this player if this player clapped
        var myInstanceId = localPlayer.GetInstanceID();

        Debug.Log("Triggering flap force effect on THIS PLAYER");

        // Do not apply force if this player was not hit by clap
        var colliders = Physics.OverlapSphere(explosionPos, ClapEffectRadius);

        foreach (var collider in colliders)
        {
            // Only apply forces for this local player
            if (collider.gameObject.tag == "Player")
            {
                if (collider.gameObject.GetInstanceID() != myInstanceId)
                {
                    continue;
                }
                if (collider.gameObject.GetInstanceID() == myInstanceId && forceOwnerInstanceId == myInstanceId)
                {
                    continue;
                }
            }

            var hitRigidbody = collider.GetComponent<Rigidbody>();

            if (hitRigidbody == null)
            {
                continue;
            }

            var distance = (hitRigidbody.transform.position - explosionPos).magnitude;
            if (distance > ClapEffectRadius)
            {
                continue;
            }


            if (distance < 0.5)
            {
                distance = 0.5f;
            }

            // this is a number between 0 and 1 that gauges how powerful the slap was
            // 1 = maximum power, 0 = minimum power
            float slapPower = 1.0f - Mathf.Pow(distance / ClapEffectRadius, 2);

            var arenaForce = 1.0f;

            // if it's a player we know it's the local player 
            if (collider.gameObject.tag == "Player")
            {
                localPlayer.gameObject.GetComponent<PlayerGameRules>().PlayerWasSlapped(slapPower);
                arenaForce = localPlayer.gameObject.GetComponent<PlayerGameRules>()._isInArena ? 1.5f : 1.0f;
                // set the isgrounded manually rather than waiting for the next frame to check.
                // if this is not set manually like this, on this frame, the vThirdPersonMotor or something 
                // will override the x and z velocity
                localPlayer.GetComponent<vThirdPersonController>().isGrounded = false;
            }

            float forceMagnitude = ClapEffectForce * (1.0f / Mathf.Pow(distance / ClapEffectRadius, 2));
            forceMagnitude *= arenaForce;// extra base clap power if they are in the round
            var horizontalForceDirection = (localPlayer.transform.position - explosionPos).normalized;
            horizontalForceDirection.y = 0;

            var playerCurrentDamage = localPlayer.gameObject.GetComponent<PlayerGameRules>().DamagePercent;
            forceMagnitude += forceMagnitude * playerCurrentDamage / 100.0f;

            var horizForceComponent = horizontalForceDirection * forceMagnitude;
            var vertForceComponent = new Vector3(0, 0, 0);
            
            if (collider.gameObject.tag == "Player")
            {
                vertForceComponent = new Vector3(0, 1, 0) * forceMagnitude * 3;
            }

            var force = horizForceComponent + vertForceComponent;

            hitRigidbody.AddForce(force, ForceMode.Impulse);
        }
    }

    private GameObject GetLocalPlayer()
    {
        return GameObject
            .FindGameObjectsWithTag("Player")
            .FirstOrDefault(IsLocalPlayer);
    }

    private bool IsLocalPlayer(GameObject go)
    {
        return go.GetComponent<vThirdPersonController>().isLocalPlayer;
    }

    private Vector3 GetClapEffectOrigin()
    {
        // in front of the character, where the hands meet
        return gameObject.transform.position + gameObject.transform.forward * .75f;
    }
}