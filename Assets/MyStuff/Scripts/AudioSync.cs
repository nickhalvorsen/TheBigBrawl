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
        PlayWorldSound(id, transform.position);
    }

    public void PlayWorldSound(int id, Vector3 position)
    {
        if (id < 0 || id > _clips.Length)
        {
            return;
        }

        CmdSendServerSound(id, position);
    }

    [Command]
    void CmdSendServerSound(int id, Vector3 position)
    {
        RpcSendSoundToClients(id, position);
    }
    
    [ClientRpc]
    void RpcSendSoundToClients(int id, Vector3 position)
    {
        AudioSource.PlayClipAtPoint(_clips[id], position);
    }
}