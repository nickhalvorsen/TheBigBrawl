using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public const int PlayersNeededToStartGame = 2;
    
    public int RewardGems = 10;
    public GameObject GemPrefab;

    [SyncVar]
    private GameState _gameState;

    [SyncVar]
    private int _playersInArena;

    private AudioSync _audioSync;
    private System.Random _random;

    private const float PostGameDuration = 5;
    private float _postGameTimer;

    public GameState GameState => _gameState;
    public int PlayersInArena => _playersInArena;

    private void Start()
    {
        _audioSync = GetComponent<AudioSync>();
        _gameState = GameState.Waiting;
        _random = new System.Random();
    }

    private void Update()
    {
        switch (_gameState)
        {
            case GameState.PostGame:
                UpdatePostGame();
                break;
        }
    }

    private void UpdatePostGame()
    {
        _postGameTimer -= Time.deltaTime;

        if (_postGameTimer <= 0)
        {
            BeginWaitingForNewGame();
        }
    }

    private void BeginWaitingForNewGame()
    {
        _gameState = GameState.Waiting;
    }

    public void PlayerEnteredArena()
    {
        _playersInArena++;

        if (_playersInArena >= PlayersNeededToStartGame)
        {
            _gameState = GameState.InProgress;
        }
    }

    public void PlayerDied()
    {
        _playersInArena--;

        if (PlayersInArena < PlayersNeededToStartGame)
        {
            BeginPostGame();
        }
    }

    // todo make this into a client rpc 
    private void BeginPostGame()
    {
        _audioSync.PlayWorldSound(AudioSync.RoundEndBell);
        var winRiffId = _random.Next(1, 3);
        _audioSync.PlayWorldSound(winRiffId);
        _gameState = GameState.PostGame;
        _postGameTimer = PostGameDuration;

        for (var i = 0; i < RewardGems; i++)
        {
            Instantiate(GemPrefab, GetRandomGemPosition(), Quaternion.identity);
        }

    }

    private Vector3 GetRandomGemPosition()
    {
        var x = _random.Next(-20, 20);
        var y = 50;
        var z = _random.Next(-102, -66);

        return new Vector3(x, y, z);
    }
}

public enum GameState
{
    Waiting,
    InProgress,
    PostGame
}