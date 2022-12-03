using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {
    public GameObject enemies;
    public string nextLevel;
    public GameObject enemyText;
    public GameObject pauseMenu;
    public GameObject winText;
    public GameObject deadText;

    // Update is called once per frame
    void Update() {
        if ((Input.GetKeyDown("escape") || Input.GetKeyDown("space")) && !winText.activeSelf && !deadText.activeSelf) {
            if (pauseMenu.activeInHierarchy) {
                pauseMenu.SetActive(false);
                Time.timeScale = 1;
            }
            else {
                // Pause game
                pauseMenu.SetActive(true);
                Time.timeScale = 0;
            }
        }

        if(enemyText != null)
            enemyText.GetComponent<TextMeshProUGUI>().text = enemies.transform.childCount.ToString() + " enemies";
        if(enemies != null && enemies.transform.childCount == 0 ){
            winText.SetActive(true);
            deadText.SetActive(false);
        }
    }

    public void ContinueClicked() {
        // Continue game
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }

    public void NextLevelClicked() {
        SceneManager.LoadScene(nextLevel);
        Time.timeScale = 1;
    }

    public void RestartClicked() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
    }
}
