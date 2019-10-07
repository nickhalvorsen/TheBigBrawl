using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public const int PlayersNeededToStartGame = 1;
    private const float PostGameDuration = 5;
    public int RewardGemsMin = 10;
    public int RewardGemsMax = 20;
    public GameObject GemPrefab;

    [HideInInspector]
    public GameState GameState => _gameState;
    [HideInInspector]
    public int PlayersInArena => _playersInArena;

    [SyncVar]
    private GameState _gameState;

    [SyncVar]
    private int _playersInArena;

    private AudioSync _audioSync;
    private System.Random _random;
    private float _postGameTimer;

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
            GenerateGems();
        }
    }

    private void GenerateGems()
    {
        var gemsToGive = _random.Next(RewardGemsMin, RewardGemsMax);
        for (var i = 0; i < gemsToGive; i++)
        {
            GenerateGem(GetRandomGemPosition(), UnityEngine.Random.rotation);
        }
    }

    [Command]
    private void GenerateGem(Vector3 position, Quaternion rotation)
    {
        GenerateGemClientRpc(position, rotation);
    }

    [ClientRpc]
    private void GenerateGemClientRpc(Vector3 position, Quaternion rotation)
    {
        Instantiate(GemPrefab, position, rotation);
    }

    [Command]
    private void BeginPostGame()
    {
        BeginPostGameClientRpc();
    }

    [ClientRpc]
    private void BeginPostGameClientRpc()
    {
        _audioSync.PlayWorldSound(Sounds.RoundEnd);
        var winRiffId = _random.Next(1, 3);
        _audioSync.PlayWorldSound(winRiffId);
        _gameState = GameState.PostGame;
        _postGameTimer = PostGameDuration;
    }

    private Vector3 GetRandomGemPosition()
    {
        var x = _random.Next(-20, 20);
        var y = _random.Next(45, 55);
        var z = _random.Next(-102, -66);

        return new Vector3(x, y, z);
    }

    private void OnPlayerDisconnected(NetworkIdentity player) 
    {
        
    }

    public static class Sounds
    {
        public const int RoundEnd = 0;
        public const int WinRiff1 = 1;
        public const int WinRiff2 = 2;
        public const int WinRiff3 = 3;
    }
}

public enum GameState
{
    Waiting,
    InProgress,
    PostGame
}