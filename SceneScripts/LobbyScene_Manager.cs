using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Text.RegularExpressions;
public class LobbyScene_Manager : MonoBehaviourPunCallbacks//目前設定只能有三個房間
{
    // Start is called before the first frame update
    [SerializeField] UnityEngine.UI.InputField inputRoomName; 
    [SerializeField] UnityEngine.UI.InputField inputPlayerName; 
    [SerializeField] UnityEngine.UI.Text []roomNameList; 
    [SerializeField] UnityEngine.UI.Text status;
    [SerializeField] UnityEngine.Sprite selected;
    [SerializeField] GameObject []roomListPanel;
    private string roomName = null;
    private string []roomNameArray = new string[10];
    void Start(){
        if(PhotonNetwork.IsConnected == true){
            if(PhotonNetwork.CurrentLobby == null)//當從room返回時候，雖然currentLobby還是一樣，但需要等待重新連線才能再度加入lobby，
                PhotonNetwork.JoinLobby();//馬上加入會出錯，因此上面要設null，這樣返回的時候才不會讀取到這行
        }
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
    }
    public override void OnConnectedToMaster(){
        PhotonNetwork.JoinLobby();//從room返回，重新連線後在載入大廳資訊
    }
    public override void OnJoinedLobby(){
        print("Lobby Joined");
    }
    public override void OnJoinedRoom(){
        print("Room Joined!");
        UnityEngine.SceneManagement.SceneManager.LoadScene("RoomScene");
    }
    public override void OnRoomListUpdate(List<Photon.Realtime.RoomInfo> roomList){
        int index = 0;
        foreach(Photon.Realtime.RoomInfo room in roomList){
            if(room.PlayerCount > 0){
                roomNameArray[index] = room.Name;
                roomListPanel[index].SetActive(true);
                roomNameList[index].text = "  房間名稱: " + addSpaceInWords(room.Name, 30) + "      房間人數: " + room.PlayerCount + "/9";
                index++;
            }
        }
       
    }
    public void OnClickJoinRoom(){
        if(roomName == null){
            roomName = inputRoomName.text;
        }
        PhotonNetwork.JoinRoom(roomName);
    }

    public void OnClickEnterName(){
        string playerName = GetPlayerName();
        string review_status = IsValidName(playerName);
        if(review_status == "valid"){
            if(CheckNickName(playerName)){
                PhotonNetwork.LocalPlayer.NickName = playerName;
                GameObject.Find("window").SetActive(false);
            }
            else{
                status.text = "這個名字已經有其他玩家使用了！";
            }
        }
        else{
            status.text = review_status;
        }
    }
    public void OnClickRoom(int index){
        for(int i = 0; i< 3; i++){
            if(index == i){   
                roomListPanel[i].GetComponent<UnityEngine.UI.Image>().sprite = selected;
                roomName = GetRoomName(index);
            }
            else{
                roomListPanel[i].GetComponent<UnityEngine.UI.Image>().sprite = null;
            }
        }
        
    }
    public void OnClickCreateRoom(){
        roomName = CreateRoomName();
        PhotonNetwork.CreateRoom(roomName);
        
    }
    private bool CheckNickName(string checkName){
        if (IsNameDuplicate(checkName)){
            return false;
        }
        return true;
    }
    private string IsValidName(string str){
        if(str.Length >= 6)
            return "字數過長，請輸入 1 到 6 個字元";
        else if(str.Length == 0)
            return "字數過短，請輸入 1 到 6 個字元";
        else if(!IsAllowedCharacters(str)){
            return "請勿輸入英文、中文、日文、數字、底線或破折號以外的字元";
        }

        return "valid";
    }
    public static bool IsAllowedCharacters(string input)
    {
        //允許字元：英文字母、中文字、數字、日文、底線、破折號
        string pattern = @"^[\p{L}\p{Nd}_-]+$";
        
        // 利用正則表達式檢查字串是否符合範圍
        return Regex.IsMatch(input, pattern);
    }
    
    // 檢查是否有其他玩家使用了相同的名字
    private bool IsNameDuplicate(string name)
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.NickName == name)
            {
                return true;
            }
        }
        return false;
    }
    private string CreateRoomName(){
        return inputRoomName.text;//Trim是過濾字元，預設會過濾空白
    }
    private string GetRoomName(int index){
        return roomNameArray[index];//Trim是過濾字元，預設會過濾空白
    }
    private string GetPlayerName(){
        return inputPlayerName.text.Trim();//Trim是過濾字元，預設會過濾空白
    }
   
    
    public void BacktoStartScene(){
        PhotonNetwork.Disconnect();
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
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
