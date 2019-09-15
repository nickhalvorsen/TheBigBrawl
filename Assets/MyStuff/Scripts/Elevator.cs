using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : NetworkBehaviour
{
    [SyncVar]
    private Vector3 _startPos;
    [SyncVar]
    private Vector3 _endPos;

    // Start is called before the first frame update
    void Start()
    {
        if (isServer)
        {
            _startPos = this.gameObject.transform.position;
            _endPos = this.gameObject.transform.position + new Vector3(0, 0, 40);
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(50, 250, 100, 100), GetPlayersInElevator() + " players in elevator");
    }

    public int GetPlayersInElevator()
    {
        var playersInElevator = 0;

        var players = GameObject.FindGameObjectsWithTag("Player");

        foreach (var player in players)
        {
            if (PlayerIsInElevator(player))
            {
                playersInElevator++;
            }
        }

        return playersInElevator;
    }

    private bool PlayerIsInElevator(GameObject player)
    {
        var ceiling = GameObject.Find("Elevator ceiling");
        var floor = GameObject.Find("Elevator floor");

        var corner1 = ceiling.transform.position + ceiling.transform.lossyScale/2;
        var corner2 = floor.transform.position - floor.transform.lossyScale/2;

        var pos = player.gameObject.transform.position;

        return pos.z < corner1.z && pos.y < corner1.y && pos.x < corner1.x
            && pos.z > corner2.z && pos.y > corner2.y && pos.x > corner2.x;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
