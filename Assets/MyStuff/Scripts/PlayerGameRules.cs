using Mirror;
using UnityEngine;

public class PlayerGameRules : NetworkBehaviour
{
    private bool _isInArena = false;
    private GameManager GameManager => GameObject.Find("GameManager").GetComponent<GameManager>();

    // Start is called before the first frame update
    void Start()
    {
    }

    private void Update()
    {

    }

    public void EnteredArena()
    {
        if (_isInArena)
        {
            return;
        }

        _isInArena = true;
        CmdPlayerHasEnteredArena();
    }

    public void Died()
    {
        if (!_isInArena)
        {
            return;
        }

        _isInArena = false;
        CmdPlayerHasLeftArena();
    }

    private void OnGUI()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        GUI.Label(new Rect(10, 10, 200, 20), "Players in arena: " + GameManager.PlayersInArena.ToString());
    }

    // client sending to server
    [Command]
    private void CmdPlayerHasEnteredArena()
    {
        GameManager.PlayerEnteredArena();
        RpcPlayerHasEnteredArena();
    }

    // server sending to clients
    [ClientRpc]
    private void RpcPlayerHasEnteredArena()
    {
        var a = GetComponent<AudioSync>();
        a.PlaySound(AudioSync.EnterArenaSound);
    }

    [Command]
    private void CmdPlayerHasLeftArena()
    {
        GameManager.PlayerLeftArena();
        RpcPlayerHasLeftArena();
    }

    [ClientRpc]
    private void RpcPlayerHasLeftArena()
    {
    }
}
