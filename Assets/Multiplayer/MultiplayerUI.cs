using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;

public class MultiplayerUI : MonoBehaviour {
    public Button hostBtn;
    public Button clientBtn;
    public Button restartBtn;
    public TMP_InputField ipInput;


    void DisableButtons(){
        hostBtn.gameObject.SetActive(false);
        clientBtn.gameObject.SetActive(false);
        ipInput.gameObject.SetActive(false);
    }

    void Awake() {
        hostBtn.onClick.AddListener( () =>{
            DisableButtons();
            var transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            transport.SetConnectionData(ipInput.text, 7777, "0.0.0.0");
            NetworkManager.Singleton.StartHost();            
        });
        clientBtn.onClick.AddListener( () =>{
            DisableButtons();   
            var transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            transport.SetConnectionData(ipInput.text, 7777);
            NetworkManager.Singleton.StartClient();            
        });
        restartBtn.onClick.AddListener( () => {
            // Reload scene
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
    }
}
