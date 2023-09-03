using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class EngineerBehaviors : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    [SerializeField] UnityEngine.UI.Button ConnectBtn;
    [SerializeField]UnityEngine.UI.Text []PlayerName;
    [SerializeField]GameObject []PlayerImg;
    [SerializeField] GameObject ConnectPanel;
    [SerializeField] UnityEngine.UI.Text chatRoom;
    [SerializeField]GameObject []DeadPlayerImg;
    [SerializeField]GameObject CheckShootBtn;
    [SerializeField]GameObject ResetSelectedBtn;
    PlayerController _pc;
    GameManager _gm;

    public bool alreadyUsed;
    int connectedPlayerKey;//1~9
    public bool alreadySelect;
    void Start()
    {
        //一開始是白天所以要already used
        alreadyUsed = true;
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        _pc = GameObject.Find(PhotonNetwork.LocalPlayer.NickName + "(player)").GetComponent<PlayerController>();
        ConnectBtn.interactable = false;
        foreach(var kvp in PhotonNetwork.CurrentRoom.Players){
            PlayerName[kvp.Value.ActorNumber-1].text = kvp.Value.NickName;
            PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().sprite = GameObject.Find(kvp.Value.NickName + "(player)").GetComponent<SpriteRenderer>().sprite;
            Color color = PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().color;
            color.a = 1f;
            PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().color = color;
            PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().SetNativeSize();

        }
        connectedPlayerKey = -1;
        print("EngineerBehaviors");
    }
    void Update(){
        if(Input.GetKeyUp(KeyCode.Space) && _pc.allowMovement && ConnectBtn.interactable){//玩家沒有在其他狀態中且可使用技能時
            ShowSendPanel();
        }
    }
    public void ShowSendPanel(){
        if(ConnectPanel.activeInHierarchy){
            if(connectedPlayerKey!=-1){//將已經選擇的提示取消
                UnityEngine.UI.Image playerImg = ConnectPanel.transform.Find("Player" + connectedPlayerKey).gameObject
                                                .GetComponent<UnityEngine.UI.Image>();
                Color color = playerImg.color;
                color.a = 0f;
                playerImg.color = color;
            }
            ConnectPanel.SetActive(false);
            _pc.allowMovement = true;
            alreadySelect = false;//關掉重製
            connectedPlayerKey = -1;//關掉重製
        }
        else{
            _pc.allowMovement = false;//正在用電腦不能移動
            foreach(var kvp in PhotonNetwork.CurrentRoom.Players){
                if(_gm.playerMap[kvp.Value][0] == 0){ //已經死了，則顯示X，並且更新為倒下來的照片
                    PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().sprite = GameObject.Find(kvp.Value.NickName + "(player)").GetComponent<SpriteRenderer>().sprite;
                    DeadPlayerImg[kvp.Value.ActorNumber-1].SetActive(true);
                }
            } 
            ConnectPanel.SetActive(true);
            CheckShootBtn.SetActive(false);
            ResetSelectedBtn.SetActive(false);
        }
    }
    public void SelectedPlayer(int key){//1~9
        if(_gm.FindPlayerByKey(key) != null){//只是因為測試的時候沒有其他玩家會有bug暫時設置的
            if(_gm.playerMap[_gm.FindPlayerByKey(key)][0] == 1){//選擇的對象還活著
                if(!alreadySelect && _gm.playerMap[PhotonNetwork.LocalPlayer][0] == 1){//自己本人還沒選擇且還活著
                    //should play sound?
                    alreadySelect = true;
                    connectedPlayerKey = key;
                    UnityEngine.UI.Image playerImg =  ConnectPanel.transform.Find("Player" + connectedPlayerKey).gameObject.GetComponent<UnityEngine.UI.Image>();
                    Color color = playerImg.color;
                    color.a = 0.4f;
                    playerImg.color = color;
                    CheckShootBtn.SetActive(true);
                    ResetSelectedBtn.SetActive(true);
                }
            }
        }
    }
    public void ResetSelected(){
        CheckShootBtn.SetActive(false);
        ResetSelectedBtn.SetActive(false);
        alreadySelect = false;
        GameObject selectedPlayer;
        if(connectedPlayerKey!=-1){
            selectedPlayer = ConnectPanel.transform.Find("Player" + connectedPlayerKey).gameObject;
            UnityEngine.UI.Image playerImg = selectedPlayer.GetComponent<UnityEngine.UI.Image>();
            Color color = playerImg.color;
            color.a = 0f;
            playerImg.color = color;
        }       
        connectedPlayerKey = -1;
    }
    public void ConnectPC(){//晚上可以選擇某個人房間的電腦連線，1~9
        alreadyUsed = true;
        ConnectBtn.interactable = false;
        Photon.Realtime.Player player;
        player = _gm.FindPlayerByNickname(PlayerName[connectedPlayerKey-1].text);
        print("Clicked on " + player.NickName);
        ConnectPanel.SetActive(false);
        _pc.allowMovement = true;
        _gm.CallRpcEngineerConnected(connectedPlayerKey, PhotonNetwork.LocalPlayer.ActorNumber);
        string richText = "<color=#00FFFF>";
        richText += "\n成功與" + connectedPlayerKey + "號房間的電腦連上線";
        richText += "</color>";
        chatRoom.text += richText;
    }
    
    
}
