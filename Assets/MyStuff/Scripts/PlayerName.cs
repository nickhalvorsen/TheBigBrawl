using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerName : NetworkBehaviour
{
    public Text NameTag;
    public Text DamagePercent;
    private string playerName;

    private void Start()
    {
        playerName = PlayerPrefs.GetString("PlayerName");
    }

    public void UpdateDamagePercent(float percent)
    {
        CmdUpdateDamagePercent(percent);
    }

    [Command]
    private void CmdUpdateDamagePercent(float percent)
    {
        RpcUpdateDamagePercent(percent);
    }

    [ClientRpc]
    private void RpcUpdateDamagePercent(float percent)
    {

        if (percent >= 0.0f)
        {
            DamagePercent.text = ((int)percent).ToString() + "%";
        }
        else
        {
            DamagePercent.text = "";
        }
    }

    public void UpdateName()
    {
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
        playerName = name;
        NameTag.text = name;
        if (isLocalPlayer)
        {
            NameTag.text = "";
        }
    }
}
