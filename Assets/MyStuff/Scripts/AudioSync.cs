using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSync : NetworkBehaviour
{
    public const int SlapSound = 0;
    public const int DeathSound = 1;
    public const int EnterArenaSound = 2;
    public const int PickupMoneySound = 3;


    public const int RoundEndBell = 0;
    public const int WinRiff1 = 1;
    public const int WinRiff2 = 2;
    public const int WinRiff3 = 3;

    private AudioSource _audioSource;
    public AudioClip[] _clips;

    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlaySoundForUserOnly(int id)
    {
        _audioSource.PlayOneShot(_clips[id]);
    }

    public void PlayWorldSound(int id)
    {
        if (id < 0 || id > _clips.Length)
        {
            return;
        }

        CmdSendServerSound(id);
    }

    [Command]
    void CmdSendServerSound(int id)
    {
        RpcSendSoundToClients(id);
    }
    
    [ClientRpc]
    void RpcSendSoundToClients(int id)
    {
        AudioSource.PlayClipAtPoint(_clips[id], transform.position);
    }
}
