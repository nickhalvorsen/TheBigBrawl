using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SyncVar]
    public int PlayersInArena;

    //public void AddPlayerToArena()
    //{
    //    PlayersInArena++;
    //}

    //public void SubtractPlayerFromArena()
    //{
    //    PlayersInArena--;
    //}
}
