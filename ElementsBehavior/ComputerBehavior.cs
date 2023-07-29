using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ComputerBehavior : MonoBehaviourPunCallbacks
{
    [SerializeField] UnityEngine.Sprite computerOn;
    [SerializeField] UnityEngine.Sprite computerOff;
    [SerializeField] UnityEngine.Sprite computerMsg;
    [SerializeField]GameObject ComputerPanel;
    [SerializeField]GameObject GamePanel;
    [SerializeField]GameObject MsgPanel;
    [SerializeField]GameObject ComputerBtn;
    [SerializeField]GameObject newMsgIcon;
    [SerializeField]GameObject game;
    [SerializeField]GameObject newMsg;
    [SerializeField] UnityEngine.UI.Button EngineerBtn;
    public UnityEngine.UI.Text chatRoom;
    [SerializeField] UnityEngine.UI.InputField inputChatMessage;
    [SerializeField] UnityEngine.UI.Button closeBtn;
    [SerializeField] UnityEngine.UI.Button chatBtn;
    
    PlayerController _pc;
    GameManager _gm;
    private bool status = false;
    public float radius;
    public int computerKey;
    public int mode = 0;//白天要記得變回0
    public int skinMode = 1;
    public int connectedKey = 0;
    
    public bool belongsToEngineer = false;//是否該電腦屬於工程師
    void Start()
    {
        radius = 0.5f;
        skinMode = 1;
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(skinMode == 1){
            GetComponent<SpriteRenderer>().sprite = computerOff;
            newMsgIcon.SetActive(false);
        }
        else if(skinMode == 2){
            GetComponent<SpriteRenderer>().sprite = computerOn;
            newMsgIcon.SetActive(false);
        }
        else if(skinMode == 3){
            GetComponent<SpriteRenderer>().sprite = computerMsg;
            newMsgIcon.SetActive(true);
        }
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);
        bool alter = false;
        
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject.name.Contains("(player)"))
            {
                PhotonView pv = collider.gameObject.GetComponent<PhotonView>();
                if(pv.IsMine &&_gm.playerMap[pv.Owner][0] == 1){//還活著
                    alter = true;
                }
            }
        }
        if(alter){
            ComputerBtn.SetActive(true);
            if(belongsToEngineer){
                if(GameObject.Find("Engineer").GetComponent<EngineerBehaviors>().alreadyUsed == false){
                    EngineerBtn.interactable = true;
                }
            }
            if(!status){
                ComputerBtn.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
                ComputerBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(ShowContent);
            }
            status = true;
        }
        else{
            if(status){
                if(belongsToEngineer){
                    EngineerBtn.interactable = false;
                }
                ComputerBtn.SetActive(false);
                status = false;
            }
        }
    }
    public void ShowContent(){
        _pc = GameObject.Find(PhotonNetwork.LocalPlayer.NickName + "(player)").GetComponent<PlayerController>();
        if(ComputerPanel.activeInHierarchy){
            ComputerPanel.SetActive(false);
            if(skinMode != 3){
                _gm.CallRpcPcSwitchStatus(computerKey, 1);
            }
            _pc.allowMovement = true;
        }
        else{
            _pc.allowMovement = false;
            ComputerPanel.SetActive(true);
            if(skinMode != 3){
                _gm.CallRpcPcSwitchStatus(computerKey, 2);
                //newMsgIcon.SetActive(false);
            }
            else
                newMsgIcon.SetActive(true);
            game.GetComponent<UnityEngine.EventSystems.EventTrigger>().triggers.Clear();
            var pointerClick = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerClick.eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick;
            pointerClick.callback.AddListener((data) => ToLittleGame());
            game.GetComponent<UnityEngine.EventSystems.EventTrigger>().triggers.Add(pointerClick);

            if(mode == 1){//當晚有被連上線
                newMsg.SetActive(true);
                newMsg.GetComponent<UnityEngine.EventSystems.EventTrigger>().triggers.Clear();
                pointerClick = new UnityEngine.EventSystems.EventTrigger.Entry();
                pointerClick.eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick;
                pointerClick.callback.AddListener((data) => MsgRoom());
                newMsg.GetComponent<UnityEngine.EventSystems.EventTrigger>().triggers.Add(pointerClick);
            }
            else{
                newMsg.SetActive(false);
            }
        }
    }
    public void ToLittleGame(){
        print("Game");
    }
    public void MsgRoom(){
        print("Msg with Computer" + connectedKey);
        _gm.CallRpcPcSwitchStatus(computerKey, 2);
        //newMsgIcon.SetActive(false);
        closeBtn.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
        closeBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnClickClose);
        chatBtn.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
        chatBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnClickChat);
        MsgPanel.SetActive(true);
    }
    public void OnClickChat(){
        string chatMessage = GetChatMessage();
        if(IsValidMessage(chatMessage)){
            _gm.CallRpcSendMsg(chatMessage, connectedKey, computerKey);
        }
        inputChatMessage.text = "";
    }
    public void OnClickClose(){
        MsgPanel.SetActive(false);
        _gm.CallRpcPcSwitchStatus(computerKey, 2);
        //newMsgIcon.SetActive(false);
    }
    
    private string GetChatMessage(){
        return inputChatMessage.text.Trim();//Trim是過濾字元，預設會過濾空白
    }
    private bool IsValidMessage(string str){//言論審查
        return true;
    }
    
}
