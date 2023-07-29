using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class RoomManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    [SerializeField] UnityEngine.UI.Text textRoomName;
    [SerializeField] UnityEngine.UI.Text playerRoom;
    [SerializeField] UnityEngine.UI.Text chatRoom;
    [SerializeField] UnityEngine.UI.Button startGame;
    [SerializeField] UnityEngine.UI.InputField inputChatMessage; 
    private string chatSb;
    void Start(){
        if(PhotonNetwork.CurrentRoom == null)
            UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
        else{
            textRoomName.text = addSpaceInWords(PhotonNetwork.CurrentRoom.Name, 30);
            chatSb = "";
            startGame.interactable = PhotonNetwork.IsMasterClient;//設定是否可以互動(這樣表示出發按鈕只有master有權)
            UpdatePlayerList();
        }
    }
    public override void OnLeftRoom(){
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGame.interactable = PhotonNetwork.IsMasterClient;//換房主時
    }
    // Update is called once per frame
    public void UpdatePlayerList(){
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach(var playerName in PhotonNetwork.CurrentRoom.Players){
            sb.AppendLine("-> " + addSpaceInWords(playerName.Value.NickName, 30));
        }
        playerRoom.text = sb.ToString();
    }
    public override void OnPlayerEnteredRoom(Player player){
        UpdatePlayerList();
    }
    public override void OnPlayerLeftRoom(Player player){
        UpdatePlayerList();
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        chatSb += "\n" + targetPlayer.NickName + ": " + (string)changedProps["chatRoom"];
        chatRoom.text = chatSb;
    }
    public void OnClickChat(){
        string chatMessage = GetChatMessage();
        if(IsValidMessage(chatMessage)){
            ExitGames.Client.Photon.Hashtable table = new ExitGames.Client.Photon.Hashtable();
            table.Add("chatRoom", chatMessage);
            PhotonNetwork.LocalPlayer.SetCustomProperties(table);
        }
        inputChatMessage.text = "";
    }
    public void OnClickStartGame(){
        if(PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("MapScene");
    }
    public void OnClickLeftRoom(){
        PhotonNetwork.LeaveRoom();
    }
    private string GetChatMessage(){
        return inputChatMessage.text.Trim();//Trim是過濾字元，預設會過濾空白
    }
    private bool IsValidMessage(string str){//言論審查
        return true;
    }
    private string addSpaceInWords(string str, int space){
        if(str.Length == 0)
            return " ";
        string newStr = "";
        newStr += str[0];
        for(int i = 1; i < str.Length; i++){
            newStr += "<size=" + space +"> </size>" + str[i];
        }
        return newStr;
    }
}
