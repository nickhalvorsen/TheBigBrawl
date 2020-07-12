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
        // Do not apply force if a player was not hit by clap
        var colliders = Physics.OverlapSphere(explosionPos, ClapEffectRadius);

        foreach (var collider in colliders)
        {
            if (!ShouldApplyClapEffectToThisObject(collider, explosionPos, forceOwnerInstanceId))
            {
                continue;
            }

            TriggerClapForceOnObject(collider, explosionPos, forceOwnerInstanceId);
        }
    }

    private void TriggerClapForceOnObject(Collider collider, Vector3 explosionPos, int forceOwnerInstanceId)
    {
        var localPlayer = GetLocalPlayer();

        var hitRigidbody = collider.GetComponent<Rigidbody>();

        var forceMultiplier = 1f;

        // if the tag is Player we know it's the local player (earlier, it was filtered)
        if (collider.gameObject.tag == "Player")
        {
            var slapPower = GetSlapPower(explosionPos, hitRigidbody.transform.position);
            localPlayer.gameObject.GetComponent<PlayerGameRules>().PlayerWasSlapped(slapPower);
            forceMultiplier += localPlayer.gameObject.GetComponent<PlayerGameRules>()._isInArena ? 0.5f : 0f;
            // set the isgrounded manually rather than waiting for the next frame to check.
            // if this is not set manually like this, on this frame, the vThirdPersonMotor or something 
            // will override the x and z velocity
            localPlayer.GetComponent<vThirdPersonController>().isGrounded = false;

            var playerCurrentDamage = localPlayer.gameObject.GetComponent<PlayerGameRules>().DamagePercent;
            forceMultiplier += forceMultiplier * playerCurrentDamage / 50.0f;
        }

        var baseClapForceVector = GetClapForceVector(explosionPos, hitRigidbody.transform.position, forceMultiplier);
       

        hitRigidbody.AddForce(baseClapForceVector, ForceMode.Impulse);
    }

    private bool ShouldApplyClapEffectToThisObject(Collider collider, Vector3 explosionPos, int forceOwnerInstanceId)
    {
        var localPlayer = GetLocalPlayer();
        // Do not calculate force on this player if they were the clapper
        var myInstanceId = localPlayer.GetInstanceID();

        // Only apply forces for this local player
        if (collider.gameObject.tag == "Player")
        {
            if (collider.gameObject.GetInstanceID() != myInstanceId)
            {
                return false;
            }
            if (collider.gameObject.GetInstanceID() == myInstanceId && forceOwnerInstanceId == myInstanceId)
            {
                return false;
            }
        }

        if (collider.GetComponent<Rigidbody>() == null)
        {
            return false;
        }

        var colliderClosestPoint = collider.ClosestPoint(explosionPos);
        var distance = GetDistance(explosionPos, colliderClosestPoint);
        if (distance > ClapEffectRadius)
        {
            return false;
        }

        return true;
    }

    private Vector3 GetClapForceVector(Vector3 explosionPosition, Vector3 rigidbodyPosition, float forceMultiplier)
    {
        var distance = GetDistance(explosionPosition, rigidbodyPosition);
        if (distance < 0.5)
        {
            distance = 0.5f;
        }

        float forceMagnitude = forceMultiplier * ClapEffectForce * (1.0f / Mathf.Pow(distance / ClapEffectRadius, 2));
        var horizontalForceDirection = (rigidbodyPosition - explosionPosition).normalized;
        horizontalForceDirection.y = 0;
        var horizForceComponent = horizontalForceDirection * forceMagnitude;
        var vertForceComponent = new Vector3(0, 1, 0) * forceMagnitude * 3;
        var force = horizForceComponent + vertForceComponent;
        return force;
    }

    private float GetDistance(Vector3 v1, Vector3 v2)
    {
        return (v1 - v2).magnitude;
    }

    private float GetSlapPower(Vector3 explosionPosition, Vector3 rigidbodyPosition)
    {
        var distance = GetDistance(explosionPosition, rigidbodyPosition);
        // this is a number between 0 and 1 that gauges how powerful the slap was
        // 1 = maximum power, 0 = minimum power
        return 1.0f - Mathf.Pow(distance / ClapEffectRadius, 2);
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