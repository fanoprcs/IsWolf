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
    PlayerController _pc;
    GameManager _gm;
    private int localKey;
    public bool alreadyUsed;
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
            if(kvp.Value == PhotonNetwork.LocalPlayer)
                localKey = kvp.Value.ActorNumber;
        }
        print("EngineerBehaviors");
    }
    void Update(){
        if(Input.GetKeyUp(KeyCode.Space) && _pc.allowMovement && ConnectBtn.interactable){//玩家沒有在其他狀態中且可使用技能時
            ShowSendPanel();
        }
    }
    public void ShowSendPanel(){
        if(ConnectPanel.activeInHierarchy){
            ConnectPanel.SetActive(false);
            _pc.allowMovement = true;
        }
        else{
            
            _pc.allowMovement = false;
            ConnectPanel.SetActive(true);
        }
    }
    public void ConnectPC(int key){//晚上可以選擇某個人房間的電腦連線
        alreadyUsed = true;
        ConnectBtn.interactable = false;
        Photon.Realtime.Player player;
        player = _gm.FindPlayerByNickname(PlayerName[key-1].text);
        print("Clicked on " + player.NickName);
        ConnectPanel.SetActive(false);
        _pc.allowMovement = true;
        _gm.CallRpcEngineerConnected(key, localKey);
        string richText = "<color=#00FFFF>";
        richText += "\n成功與" + key + "號房間的電腦連上線";
        richText += "</color>";
        chatRoom.text += richText;
    }
    
    
}
