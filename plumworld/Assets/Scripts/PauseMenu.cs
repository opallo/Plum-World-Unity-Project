using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour {
    [SerializeField] public GameObject pauseMenuUI;
    [SerializeField] private bool paused = false;
    void Update () {
        if (Input.GetKeyDown (KeyCode.Joystick1Button7)) {
            ToggleMenu (paused);
        }
    }

    public void ToggleMenu (bool _toggle) {
        paused = !paused;
        pauseMenuUI.SetActive (_toggle);
        Time.timeScale = paused ? 1 : 0;
        AudioListener.pause = paused ? true : false;
    }

}