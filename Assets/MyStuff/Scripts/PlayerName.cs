using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerName : NetworkBehaviour
{
    public Text NameTag;
    private bool nameInitialized = false;
    private string playerName;

    private void Start()
    {
        playerName = PlayerPrefs.GetString("PlayerName");
    }

    public void UpdateName()
    {
        if (!nameInitialized)
        {
            //playerName = 
        }

        CmdUpdateName(playerName);
    }

    [Command]
    private void CmdUpdateName(string name)
    {
        RpcUpdateName(name);
    }

    [ClientRpc]
    private void RpcUpdateName(string name)
    {

        NameTag.text = name;
        if (isLocalPlayer)
        {
            NameTag.text = "";
        }
    }
}
