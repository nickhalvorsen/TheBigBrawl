using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Elevator : NetworkBehaviour
{
    private static class Sounds
    {
        public const int CountdownBeep = 0;
        public const int CountdownFinalBeep = 1;
        public const int DoorMoving = 2;
        public const int DoorContact = 3;
        public const int Moving = 4;
    }

    private const int PlayersToTriggerStart = 1;
    private const int CountdownSeconds = 4;
    private const float ElevatorWallOpenCloseSpeed = 2; // this many seconds it takes 
    private const float MoveToDestinationVelocity = 5;
    private const float PauseAtDestinationDuration = 2;

    private enum ElevatorState
    {
        WaitingForPlayers,
        CountingDown,
        ClosingBackDoor,
        MovingToDestination,
        PausingAtDestination,
        OpeningAtDestination,
        PausingAfterOpen,
        MovingToStart,
    }

    [SyncVar]
    private ElevatorState _state;
    [SyncVar]
    private Vector3 _startPos;
    [SyncVar]
    private Vector3 _endPos;

    private GameObject _arenaDoor;
    private GameObject _entranceDoor;

    private GameObject[] _textDisplays;

    private float _startCountdown;
    private int _lastStartCountdownBeep;
    private float _pausingAtDestinationTimer;

    private GameManager GameManager;
    private AudioSync _audioSync;

    private Rigidbody _rigidBody;

    // Start is called before the first frame update
    void Start()
    {
        this.GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        if (isServer)
        {
            _textDisplays = GameObject.FindGameObjectsWithTag("Elevator text");
            _arenaDoor = GameObject.FindGameObjectWithTag("Elevator arena door");
            _entranceDoor = GameObject.FindGameObjectWithTag("Elevator back door");
            _audioSync = this.gameObject.GetComponent<AudioSync>();
            _rigidBody = this.gameObject.GetComponent<Rigidbody>();
            _state = ElevatorState.WaitingForPlayers;
            _startPos = this.gameObject.transform.position;
            _endPos = this.gameObject.transform.position + new Vector3(0, 0, -20);
        }
    }

    private void OnGUI()
    {
       // GUI.Label(new Rect(50, 250, 100, 100), GetPlayersInElevator() + " players in elevator");
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
        switch (_state)
        {
            case ElevatorState.WaitingForPlayers:
                UpdateWaitingForPlayers();
                break;
            case ElevatorState.CountingDown:
                UpdateCountingDown();
                break;
            case ElevatorState.ClosingBackDoor:
                UpdateClosingBackDoor();
                break;
            case ElevatorState.MovingToDestination:
                UpdateMovingToDestination();
                break;
            case ElevatorState.PausingAtDestination:
                UpdatePausingAtDestination();
                break;
            case ElevatorState.OpeningAtDestination:
                UpdateOpeningAtDestination();
                break;
        }

        UpdateTextDisplays();
    }

    private void UpdateWaitingForPlayers()
    {
        if (ShouldTriggerCountdown())
        {
            _state = ElevatorState.CountingDown;
            _startCountdown = CountdownSeconds;
            _lastStartCountdownBeep = -1;
        }
    }

    private bool ShouldTriggerCountdown()
    {
        return GameManager.GameState == GameState.Waiting && GetPlayersInElevator() >= PlayersToTriggerStart;
    }

    private void UpdateCountingDown()
    {
        if (GetPlayersInElevator() < PlayersToTriggerStart)
        {
            _state = ElevatorState.WaitingForPlayers;
        }

        _startCountdown -= Time.deltaTime;

        if ((int)_startCountdown != _lastStartCountdownBeep)
        {
            _lastStartCountdownBeep = (int)_startCountdown;

            if ((int)_startCountdown == 0)
            {
                _audioSync.PlaySound(Sounds.CountdownFinalBeep);
            }
            else
            {
                _audioSync.PlaySound(Sounds.CountdownBeep);
            }
        }

        if (_startCountdown < 0)
        {
            _audioSync.PlaySound(Sounds.DoorMoving);
            _state = ElevatorState.ClosingBackDoor;
        }
    }

    private void UpdateClosingBackDoor()
    {
        if (_entranceDoor.transform.localScale.y >= 1.0f)
        {
            _audioSync.PlaySound(Sounds.DoorContact);
            _audioSync.PlaySound(Sounds.Moving);
            _state = ElevatorState.MovingToDestination;
            return;
        }

        var scale = _entranceDoor.transform.localScale;
        var newYScale = scale.y + (1 / ElevatorWallOpenCloseSpeed) * Time.deltaTime;
        _entranceDoor.transform.localScale = new Vector3(scale.x, newYScale, scale.z);

        var newYPosition = -.5f + newYScale / 2; // -.5 is the default y position for the closed door
        _entranceDoor.transform.localPosition = new Vector3(_entranceDoor.transform.localPosition.x, newYPosition, _entranceDoor.transform.localPosition.z); 
    }

    private void UpdateMovingToDestination()
    {
        if (Vector3.Distance(this.transform.position, _endPos) < .1)
        {
            _rigidBody.velocity = new Vector3(0, 0, 0);
            this._state = ElevatorState.PausingAtDestination;
            _pausingAtDestinationTimer = PauseAtDestinationDuration;
            return;
        }

        _rigidBody.velocity = (_endPos - _startPos).normalized * MoveToDestinationVelocity;
        _rigidBody.MovePosition( _rigidBody.position + _rigidBody.velocity * Time.deltaTime);
    }

    private void UpdatePausingAtDestination()
    {
        if (_pausingAtDestinationTimer <= 0)
        {
            _state = ElevatorState.OpeningAtDestination;
            _audioSync.PlaySound(Sounds.DoorMoving);
            return;
        }

        _pausingAtDestinationTimer -= Time.deltaTime;
    }

    private void UpdateOpeningAtDestination()
    {
        if (_arenaDoor.transform.localScale.y <= 0f)
        {
            _audioSync.PlaySound(Sounds.DoorContact);
            _state = ElevatorState.PausingAfterOpen;
            return;
        }

        var scale = _arenaDoor.transform.localScale;
        var newYScale = scale.y - (1 / ElevatorWallOpenCloseSpeed) * Time.deltaTime;
        _arenaDoor.transform.localScale = new Vector3(scale.x, newYScale, scale.z);

        var newYPosition = 0 + (1-newYScale) / 2; // -.5 is the default y position for the closed door
        _arenaDoor.transform.localPosition = new Vector3(_arenaDoor.transform.localPosition.x, newYPosition, _arenaDoor.transform.localPosition.z);
    }

    private void UpdateTextDisplays()
    {
        foreach (var o in _textDisplays)
        {
            o.GetComponent<TextMesh>().text = GetStatusText();
        }
    }

    private string GetStatusText()
    {
        switch (_state)
        {
            case ElevatorState.WaitingForPlayers:
                return $"Waiting for {PlayersToTriggerStart - GetPlayersInElevator()} more players";
            case ElevatorState.CountingDown:
                return $"Starting game in {(int)_startCountdown}";
            default:
                //return "";
                return _state.ToString();
        }
    }
}