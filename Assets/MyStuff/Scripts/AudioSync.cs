using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSync : NetworkBehaviour
{
    public AudioClip[] _clips;

    private AudioSource _audioSource;

    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlaySoundForUserOnly(int id)
    {
        if (id < 0 || id > _clips.Length)
        {
            return;
        }

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