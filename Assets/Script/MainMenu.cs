using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    public Button singleButton;
    public Button multiButton;
    void Awake() {
        singleButton.onClick.AddListener( () => {
            SceneManager.LoadScene("level1");            
        });
        multiButton.onClick.AddListener( () => {
            //Debug.Log("Multiplayer not ready yet"); 
            SceneManager.LoadScene("Multiplayer");            
        });
    }

}
