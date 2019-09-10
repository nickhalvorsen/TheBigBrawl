 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class MainMenu : MonoBehaviour
{
    public InputField PlayerNameField;

    // Start is called before the first frame update
    void Start()
    {
        var se = new InputField.SubmitEvent();
        se.AddListener(NameUpdated);
        PlayerNameField.onEndEdit = se;

        PlayerNameField.text = PlayerPrefs.GetString("PlayerName");
    }

    private void NameUpdated(string name)
    {
        PlayerPrefs.SetString("PlayerName", name);
        PlayerPrefs.Save();
    }

    private void OnGUI()
    {
        if ((NetworkServer.active || NetworkClient.active) && this.gameObject.activeSelf)
        {
            this.gameObject.SetActive(false);
        }

        if (!(NetworkClient.active || NetworkClient.active) && !this.gameObject.activeSelf)
        {
            this.gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
