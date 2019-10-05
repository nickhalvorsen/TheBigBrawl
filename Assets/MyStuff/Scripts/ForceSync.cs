using Invector.CharacterController;
using Mirror;
using System.Linq;
using UnityEngine;

public class ForceSync : NetworkBehaviour
{
    public const float ClapEffectRadius = 4.0f;
    public const float ClapEffectForce = 0.2f;

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
        if (myInstanceId == forceOwnerInstanceId)
        {
            return;
        }

        Debug.Log("Triggering flap force effect on THIS PLAYER");

        // Do not apply force if this player was not hit by clap
        var colliders = Physics.OverlapSphere(explosionPos, ClapEffectRadius);
        if (!colliders.Any(c => c.GetComponent<Rigidbody>() != null && c.GetComponent<Rigidbody>().gameObject.GetInstanceID() == myInstanceId))
        {
            return;
        }


        Debug.Log("This player was actually hit by the fog!! ");

        var distance = (localPlayer.transform.position - explosionPos).magnitude;
        if (distance > ClapEffectRadius)
        {
            return;
        }


        if (distance < 0.5)
        {
            distance = 0.5f;
        }

        var playerRigidBody = localPlayer.GetComponent<Rigidbody>();

        // this is a number between 0 and 1 that gauges how powerful the slap was
        // 1 = maximum power, 0 = minimum power
        float slapPower = 1.0f - Mathf.Pow(distance / ClapEffectRadius, 2);
        localPlayer.gameObject.GetComponent<PlayerGameRules>().PlayerWasSlapped(slapPower);

        float forceMagnitude = ClapEffectForce * (1.0f / Mathf.Pow(distance / ClapEffectRadius, 2));
        var horizontalForceDirection = (localPlayer.transform.position - explosionPos).normalized;
        horizontalForceDirection.y = 0;

        var playerCurrentDamage = localPlayer.gameObject.GetComponent<PlayerGameRules>().DamagePercent;
        forceMagnitude += forceMagnitude * playerCurrentDamage / 100.0f;

        var horizForceComponent = horizontalForceDirection * forceMagnitude;
        var vertForceComponent = new Vector3(0, 1, 0) * forceMagnitude * 3;
        var force = horizForceComponent + vertForceComponent;

        playerRigidBody.AddForce(force, ForceMode.Impulse);

        // set the isgrounded manually rather than waiting for the next frame to check.
        // if this is not set manually like this, on this frame, the vThirdPersonMotor or something 
        // will override the x and z velocity
        localPlayer.GetComponent<vThirdPersonController>().isGrounded = false;
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