using Mirror;
using System;
using System.Linq;
using System.Collections;
using UnityEngine;

public class Gem : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % 100 == 1)
        {
            if (gameObject.transform.position.y < -100)
            {
                gameObject.SetActive(false);
            }
        }
    }

    [Command]
    public void CmdRemove()
    {
        RpcRemove();
    }

    [ClientRpc]
    private void RpcRemove()
    {
        gameObject.SetActive(false);
    }
}