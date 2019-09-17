using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [SyncVar]
    private GameState _gameState;

    [SyncVar]
    private int _playersInArena;

    private const float PostGameDuration = 5;
    private float _postGameTimer;

    public GameState GameState => _gameState;
    public int PlayersInArena => _playersInArena;

    private void Start()
    {
        _gameState = GameState.Waiting;
    }

    private void Update()
    {
        
    }

    public void PlayerEnteredArena()
    {
        _playersInArena++;

        if (_playersInArena > 1)
        {
            _gameState = GameState.InProgress;
        }
    }

    public void PlayerLeftArena()
    {
        _playersInArena--;

        if (PlayersInArena <= 1)
        {
            _gameState = GameState.PostGame;
            _postGameTimer = PostGameDuration;
        }
    }
}

public enum GameState
{
    Waiting,
    InProgress,
    PostGame
}