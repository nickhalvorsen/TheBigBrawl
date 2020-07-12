using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuRunner : MonoBehaviour
{
    private Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();

        if (_animator != null)
        {
            _animator.SetFloat("InputVertical", 1.5f);
            _animator.Play("Free Movement");

        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * (150 * Time.deltaTime));

    }
}
