using Invector.CharacterController;
using Mirror;
using UnityEngine;

public class PlayerGameRules : NetworkBehaviour
{
    private GameManager _gameManager;
    private vThirdPersonController _vThirdPersonController;
    private PlayerName _playerName;
    private AudioSync _audioSync;
    private int _money;

    public float DamagePercent;
    private bool _isInArena = false;

    // Start is called before the first frame update
    void Start()
    {
        _money = PlayerPrefs.GetInt("Money");
        DamagePercent = 0;
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _vThirdPersonController = this.gameObject.GetComponent<vThirdPersonController>();
        _audioSync = this.gameObject.GetComponent<AudioSync>();
        _playerName = this.gameObject.GetComponent<PlayerName>();
    }

    private void Update()
    {
        // if player is in arena, set % text to visible
    }

    public void EnteredArena()
    {
        if (_isInArena)
        {
            return;
        }

        _isInArena = true;
        CmdPlayerHasEnteredArena();
    }

    public void Died()
    {
        if (!_isInArena)
        {
            return;
        }

        _isInArena = false;
        CmdPlayerHasDied();
    }

    public void PlayerWasSlapped(float slapPower)
    {
        if (!_isInArena)
        {
            return;
        }

        _vThirdPersonController.DamagePercent += slapPower * 5;
        _playerName.UpdateDamagePercent(_vThirdPersonController.DamagePercent);
    }

    private void OnGUI()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        GUI.Label(new Rect(10, 10, 200, 20), "Players in arena: " + _gameManager.PlayersInArena.ToString());
        GUI.Label(new Rect(10, 20, 200, 20), "Game state: " + _gameManager.GameState.ToString());
    }

    public void PickedUpMoney()
    {
        _money++;
        PlayerPrefs.SetInt("Money", _money);
        PlayerPrefs.Save();
    }

    // client sending to server
    [Command]
    private void CmdPlayerHasEnteredArena()
    {
        _gameManager.PlayerEnteredArena();
        RpcPlayerHasEnteredArena();
    }

    // server sending to clients
    [ClientRpc]
    private void RpcPlayerHasEnteredArena()
    {
        DamagePercent = 0;
        _audioSync.PlayWorldSound(AudioSync.EnterArenaSound);
        _playerName.UpdateDamagePercent(DamagePercent);
    }

    [Command]
    private void CmdPlayerHasDied()
    {
        _gameManager.PlayerDied();
        RpcPlayerHasDied();
    }

    [ClientRpc]
    private void RpcPlayerHasDied()
    {
        _playerName.UpdateDamagePercent(-1.0f);
    }
}
