using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StartScene_Manager : MonoBehaviourPunCallbacks//加上PunCallbacks 才能連結伺服器
{
   public void OnClickStart(){
        PhotonNetwork.AutomaticallySyncScene = true; //一開始這樣設定，加入房間後，房主進行切換場景時，大家會一起進去遊戲畫面(進入遊戲後可能要再調整)
        PhotonNetwork.ConnectUsingSettings();
        print("Press Start");
   }
   public override void OnConnectedToMaster(){
        print("Connected!");
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
   }
   public void QuitGame(){
        Application.Quit();
        
    }
}
