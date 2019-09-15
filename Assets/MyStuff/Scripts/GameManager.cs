using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SyncVar]
    private int _playersInArena;

    public int PlayersInArena => _playersInArena;

    public void PlayerEnteredArena()
    {
        _playersInArena++;
    }

    public void PlayerLeftArena()
    {
        _playersInArena--;
    }
}
