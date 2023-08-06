using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class setting : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject Option;
    [SerializeField] GameObject KeyboardOptionPanel;
    [SerializeField] GameObject QuitLobbyPanel;
    [SerializeField] GameObject QuitGamePanel;
    private MusicPlayer musicManager;
    [SerializeField] AudioClip lobbyBackgroundMusic;
    // Update is called once per frame
    void Update()
    {
       if (Input.GetKeyDown(KeyCode.Escape)) {
            if(Option.activeInHierarchy)
                Option.SetActive(false);
            else
                Option.SetActive(true);
        } 
    }
    public void BackToGame(){
        Option.SetActive(false);
    }
    public void ShowKeyboardOptionPanel(){
        KeyboardOptionPanel.SetActive(true);
    }
    public void ShowQuitLobbyPanel(){
        QuitLobbyPanel.SetActive(true);
    }
    public void ShowQuitGamePanel(){
        QuitGamePanel.SetActive(true);
    }
    public void BackToOption(){
        KeyboardOptionPanel.SetActive(false);
        QuitLobbyPanel.SetActive(false);
        QuitGamePanel.SetActive(false);
    }
    public void BackToLooby(){
        musicManager = FindObjectOfType<MusicPlayer>();
        musicManager.GetComponent<AudioSource>().clip = lobbyBackgroundMusic;
        musicManager.GetComponent<AudioSource>().Play();
        PhotonNetwork.LeaveRoom();
    }
    public void QuitGame(){
        Application.Quit();
    }
}
