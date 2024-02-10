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

    private static int PlayersToTriggerStart => GameManager.PlayersNeededToStartGame;
    private const int CountdownSeconds = 4;
    private const float ElevatorWallOpenCloseSpeed = 2; // this many seconds it takes 
    private const float MoveToDestinationVelocity = 25;
    private const float PauseAtDestinationDuration = 2;
    private const float TimeToPauseAfterOpen = 4;
    private const float TimeToOpenBottom = 6;


    private enum ElevatorState
    {
        WaitingForPlayers,
        CountingDown,
        ClosingBackDoor,
        MovingToDestination,
        PausingAtDestination,
        OpeningAtDestination,
        PausingAfterOpen,
        OpeningBottomAtDestination,
        ClosingBottomAtDestination,
        MovingToStart,
        ClosingArenaDoorAtStart,
        OpeningBackDoorAtStart
    }

    [SyncVar]
    private ElevatorState _state;
    [SyncVar]
    private Vector3 _startPos;
    [SyncVar]
    private Vector3 _endPos;

    private GameObject _arenaDoor;
    private GameObject _entranceDoor;
    private GameObject _bottomPlatform;
    private GameObject[] _textDisplays;
    
    private float _startCountdown;
    private int _lastStartCountdownBeep;
    private float _timer;

    private GameManager GameManager;
    private AudioSync _audioSync;
    private Rigidbody _rigidBody;
    private Rigidbody _floorRigidBody;

    // Start is called before the first frame update
    void Start()
    {
        _textDisplays = GameObject.FindGameObjectsWithTag("Elevator text");

        if (isServer)
        {
            this.GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            _arenaDoor = GameObject.FindGameObjectWithTag("Elevator arena door");
            _entranceDoor = GameObject.FindGameObjectWithTag("Elevator back door");
            _bottomPlatform = GameObject.Find("Elevator floor");
            _audioSync = this.gameObject.GetComponent<AudioSync>();
            _rigidBody = this.gameObject.GetComponent<Rigidbody>();
            _state = ElevatorState.WaitingForPlayers;
            _startPos = this.gameObject.transform.position;
            _endPos = this.gameObject.transform.position + new Vector3(0, 0, -20);
        }
    }

    private void OnGUI()
    {
       // GUI.Label(new Rect(50, 250, 100, 100), GetPlaye;rsInElevator() + " players in elevator");
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
        if (!isServer)
        {
            return;
        }

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
            case ElevatorState.PausingAfterOpen:
                UpdatePausingAfterOpen();
                break;
            case ElevatorState.OpeningBottomAtDestination:
                UpdateOpeningBottomAtDestination();
                break;
            case ElevatorState.ClosingBottomAtDestination:
                UpdateClosingBottomAtDestination();
                break;
            case ElevatorState.MovingToStart:
                UpdateMovingToStart();
                break;
            case ElevatorState.ClosingArenaDoorAtStart:
                UpdateClosingArenaDoorAtStart();
                break;
            case ElevatorState.OpeningBackDoorAtStart:
                UpdateOpeningBackDoorAtStart();
                break;
        }

        RpcUpdateTextDisplays(GetStatusText());
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
                _audioSync.PlayWorldSound(Sounds.CountdownFinalBeep);
            }
            else
            {
                _audioSync.PlayWorldSound(Sounds.CountdownBeep);
            }
        }

        if (_startCountdown < 0)
        {
            BeginClosingBackDoor();
        }
    }

    private void BeginClosingBackDoor()
    {
        _audioSync.PlayWorldSound(Sounds.DoorMoving);
        _state = ElevatorState.ClosingBackDoor;
    }

    private void UpdateClosingBackDoor()
    {
        if (_entranceDoor.transform.localScale.y >= 1.0f)
        {
            BeginMovingToDestination();
            return;
        }

        var scale = _entranceDoor.transform.localScale;
        var newYScale = scale.y + (1 / ElevatorWallOpenCloseSpeed) * Time.deltaTime;
        _entranceDoor.transform.localScale = new Vector3(scale.x, newYScale, scale.z);

        var newYPosition = -.5f + newYScale / 2; // -.5 is the default y position for the closed door
        _entranceDoor.transform.localPosition = new Vector3(_entranceDoor.transform.localPosition.x, newYPosition, _entranceDoor.transform.localPosition.z); 
    }

    private void BeginMovingToDestination()
    {
        _audioSync.PlayWorldSound(Sounds.DoorContact);
        _audioSync.PlayWorldSound(Sounds.Moving);
        _state = ElevatorState.MovingToDestination;
    }

    private void UpdateMovingToDestination()
    {
        if (Vector3.Distance(this.transform.position, _endPos) < .1)
        {
            BeginPausingAtDestination();
            return;
        }

        var velocity = (_endPos - _startPos).normalized * MoveToDestinationVelocity;
        _rigidBody.MovePosition( _rigidBody.position + velocity * Time.deltaTime);

        //var players = GameObject.FindGameObjectsWithTag("Player");
        //foreach (var player in players)
        //{
        //    if (PlayerIsInElevator(player))
        //    {
        //        var rb = player.GetComponent<Rigidbody>();
        //        rb.velocity = (_endPos - _startPos).normalized * MoveToDestinationVelocity;
        //        rb.MovePosition(rb.position + rb.velocity * Time.deltaTime);
        //    }
        //}
    }

    private void BeginPausingAtDestination()
    {
        _rigidBody.velocity = new Vector3(0, 0, 0);
        this._state = ElevatorState.PausingAtDestination;
        _timer = PauseAtDestinationDuration;
    }

    private void UpdatePausingAtDestination()
    {
        if (_timer <= 0)
        {
            BeginOpeningAtDestination();
            return;
        }

        _timer -= Time.deltaTime;
    }

    private void BeginOpeningAtDestination()
    {
        _state = ElevatorState.OpeningAtDestination;
        _audioSync.PlayWorldSound(Sounds.DoorMoving);
    }

    private void UpdateOpeningAtDestination()
    {
        if (_arenaDoor.transform.localScale.y <= 0f)
        {
            BeginPausingAfterOpen();
            return;
        }

        var scale = _arenaDoor.transform.localScale;
        var newYScale = scale.y - (1 / ElevatorWallOpenCloseSpeed) * Time.deltaTime;
        _arenaDoor.transform.localScale = new Vector3(scale.x, newYScale, scale.z);

        var newYPosition = 0 + (1-newYScale) / 2; // -.5 is the default y position for the closed door
        _arenaDoor.transform.localPosition = new Vector3(_arenaDoor.transform.localPosition.x, newYPosition, _arenaDoor.transform.localPosition.z);
    }

    private void BeginPausingAfterOpen()
    {
        _audioSync.PlayWorldSound(Sounds.DoorContact);
        _state = ElevatorState.PausingAfterOpen;
        _timer = TimeToPauseAfterOpen;
    }

    private void UpdatePausingAfterOpen()
    {
        if (_timer <= 0)
        {
            BeginOpeningBottomAtDestination();
            return;
        }

        _timer -= Time.deltaTime;
    }

    private void BeginOpeningBottomAtDestination()
    {
        _state = ElevatorState.OpeningBottomAtDestination;
        _timer = TimeToOpenBottom;
        _audioSync.PlayWorldSound(Sounds.DoorMoving);
        // IDK why this is necessary instead of adding in the unity editor. 
        _floorRigidBody = _bottomPlatform.AddComponent<Rigidbody>();
        _floorRigidBody.isKinematic = true;
    }

    private void UpdateOpeningBottomAtDestination()
    {
        if (_timer <= 0)
        {
            BeginClosingBottomAtDestination();
            return;
        }

        _timer -= Time.deltaTime;

        var velocity = new Vector3(0, 0, 20);
        _floorRigidBody.MovePosition(_floorRigidBody.position + velocity * Time.deltaTime);
    }

    private void BeginClosingBottomAtDestination()
    {
        _state = ElevatorState.ClosingBottomAtDestination;
        _timer = TimeToOpenBottom;
        _audioSync.PlayWorldSound(Sounds.DoorMoving);
    }

    private void UpdateClosingBottomAtDestination()
    {
        if (_timer <= 0)
        {
            BeginMovingToStart();
            return;
        }

        _timer -= Time.deltaTime;

        _floorRigidBody.MovePosition(_floorRigidBody.position + new Vector3(0, 0, -20) * Time.deltaTime);
    }

    private void BeginMovingToStart()
    {
        _state = ElevatorState.MovingToStart;
        _audioSync.PlayWorldSound(Sounds.DoorMoving);
        Destroy(_floorRigidBody);
    }

    private void UpdateMovingToStart()
    {
        if (Vector3.Distance(this.transform.position, _startPos) < .1)
        {
            BeginClosingArenaDoorAtStart();
            return;
        }

        var velocity = (_startPos - _endPos).normalized * MoveToDestinationVelocity;
        _rigidBody.MovePosition(_rigidBody.position + velocity * Time.deltaTime);
    }

    private void BeginClosingArenaDoorAtStart()
    {
        _rigidBody.velocity = new Vector3(0, 0, 0);
        this._state = ElevatorState.ClosingArenaDoorAtStart;
    }

    private void UpdateClosingArenaDoorAtStart()
    {
        if (_arenaDoor.transform.localScale.y >= 1f)
        {
            BeginOpeningBackDoorAtStart();
            return;
        }

        var scale = _arenaDoor.transform.localScale;
        var newYScale = scale.y + (1 / ElevatorWallOpenCloseSpeed) * Time.deltaTime;
        _arenaDoor.transform.localScale = new Vector3(scale.x, newYScale, scale.z);

        var newYPosition = 0 + (1 - newYScale) / 2; // -.5 is the default y position for the closed door
        _arenaDoor.transform.localPosition = new Vector3(_arenaDoor.transform.localPosition.x, newYPosition, _arenaDoor.transform.localPosition.z);
    }

    private void BeginOpeningBackDoorAtStart()
    {
        _audioSync.PlayWorldSound(Sounds.DoorContact);
        _audioSync.PlayWorldSound(Sounds.DoorMoving);
        _state = ElevatorState.OpeningBackDoorAtStart;
    }

    private void UpdateOpeningBackDoorAtStart()
    {
        if (_entranceDoor.transform.localScale.y <= 0.0f)
        {
            BeginWaitingForPlayers();
            return;
        }

        var scale = _entranceDoor.transform.localScale;
        var newYScale = scale.y - (1 / ElevatorWallOpenCloseSpeed) * Time.deltaTime;
        _entranceDoor.transform.localScale = new Vector3(scale.x, newYScale, scale.z);

        var newYPosition = -.5f + newYScale / 2; // -.5 is the default y position for the closed door
        _entranceDoor.transform.localPosition = new Vector3(_entranceDoor.transform.localPosition.x, newYPosition, _entranceDoor.transform.localPosition.z);
    }

    private void BeginWaitingForPlayers()
    {
        _audioSync.PlayWorldSound(Sounds.DoorContact);
        _state = ElevatorState.WaitingForPlayers;
    }

    [ClientRpc]
    private void RpcUpdateTextDisplays(string text)
    {
        if (_textDisplays == null)
            return;

        foreach (var o in _textDisplays)
        {
            o.GetComponent<TextMesh>().text = text;
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
            case ElevatorState.PausingAfterOpen:
            case ElevatorState.OpeningBottomAtDestination:
            case ElevatorState.ClosingBottomAtDestination:
                return "Jump down!";
            default:
                return "";
                //return _state.ToString();
        }
    }
}