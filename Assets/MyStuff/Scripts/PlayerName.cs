using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;

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
            DamagePercent.color = GetDamageTextColor(percent); // for some reason this has no effect.
        }
        else
        {
            DamagePercent.text = "";
        }
    }

    private Color GetDamageTextColor(float percent)
    {
        var maxColorR = 117;
        var maxColorG = 22;
        var maxColorB = 15;

        var maxColorAtPercent = 200.0f;

        var percentOfMaxColor = percent / maxColorAtPercent;

        var r = (255 - (255 - maxColorR) * Math.Min(percentOfMaxColor, 1));
        var g = (255 - (255 - maxColorG) * Math.Min(percentOfMaxColor, 1));
        var b = (255 - (255 - maxColorB) * Math.Min(percentOfMaxColor, 1));

        return new Color(r/255.0f, g/255.0f, b/255.0f);
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
