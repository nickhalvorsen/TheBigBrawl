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

    private AudioSource _audioSource;
    public AudioClip[] _clips;

    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(int id)
    {
        if (id < 0 || id > _clips.Length)
        {
            return;
        }

        CmdSendServerSound(id);
    }

    // client sending to server
    [Command]
    void CmdSendServerSound(int id)
    {
        RpcSendSoundToClients(id);
    }

    // server sending to clients
    [ClientRpc]
    void RpcSendSoundToClients(int id)
    {
        // global sound
        //_audioSource.PlayOneShot(_clips[id]);

        // point sound
        AudioSource.PlayClipAtPoint(_clips[id], transform.position);



    }
}
