﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    public GameObject splashPrefab;

    private AudioSync _audioSync;
    private float timeSinceObjectCreated = 0;

    void Start()
    {
        _audioSync = GetComponent<AudioSync>();
    }

    void Update()
    {
        timeSinceObjectCreated += Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (timeSinceObjectCreated < 1)
        {
            return;
        }
        
        // Create splash particle effect
        var splash = Instantiate(splashPrefab, other.transform.position, new Quaternion(.25f,-.25f,.25f,.25f));
        Destroy(splash, 2000);

        // play splash sound
        _audioSync.PlayWorldSound(Sounds.Splash, other.transform.position);
    }

    // Delete objects that have passed below the bottom of the collider 
    // (this is y=-50 or something like that)
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.StartsWith("arena"))
        {
            return;
        }

        if (other.gameObject.transform.position.y > -10)
        {
            return;
        }

        if (other.gameObject.tag != "Player"
            && other.gameObject.tag != "ChessBoard")
        {
            other.gameObject.SetActive(false);
        }
    }

    private static class Sounds
    {
        public static int Splash = 0;
    }
}
