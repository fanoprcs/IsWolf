using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
public class HunterBehaviors : MonoBehaviourPunCallbacks/*, UnityEngine.EventSystems.IPointerClickHandler*/
{
    [SerializeField]UnityEngine.UI.Button ShootBtn;
    [SerializeField]GameObject ShootPanel;
    [SerializeField]UnityEngine.UI.Text []PlayerName;
    [SerializeField]GameObject []PlayerImg;
    [SerializeField]GameObject []DeadPlayerImg;
    [SerializeField]GameObject CheckShootBtn;
    [SerializeField]GameObject ResetSelectedBtn;
    GameManager _gm;
    PlayerController _pc;
    public bool alreadyUsed = false;
    int shootPlayerKey;//1~9
    public bool alreadySelect;
    void Start()
    {
        
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        _pc = GameObject.Find(PhotonNetwork.LocalPlayer.NickName + "(player)").GetComponent<PlayerController>();
        foreach(var kvp in PhotonNetwork.CurrentRoom.Players){
            PlayerName[kvp.Value.ActorNumber-1].text = kvp.Value.NickName;
            PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().sprite = GameObject.Find(kvp.Value.NickName + "(player)").GetComponent<SpriteRenderer>().sprite;
            Color color = PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().color;
            color.a = 1f;
            PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().color = color;
            PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().SetNativeSize();
            
        }
        shootPlayerKey = -1;
        print("HunterBehaviors");
    }
    void Update(){
        if(Input.GetKeyUp(KeyCode.Space) && _pc.allowMovement && ShootBtn.interactable){//玩家沒有在其他狀態中且可使用技能時
            ShowShootPanel();
        }
    }
    public void ShowShootPanel(){
        if(ShootPanel.activeInHierarchy){//關閉
            if(shootPlayerKey!=-1){//將已經選擇的提示取消
                UnityEngine.UI.Image playerImg = ShootPanel.transform.Find("Player" + shootPlayerKey).gameObject
                                                .GetComponent<UnityEngine.UI.Image>();
                Color color = playerImg.color;
                color.a = 0f;
                playerImg.color = color;
            }
            ShootPanel.SetActive(false);
            alreadySelect = false;//關掉重製
            shootPlayerKey = -1;//關掉重製
        }
        else{//開啟
            foreach(var kvp in PhotonNetwork.CurrentRoom.Players){
                if(_gm.playerMap[kvp.Value][0] == 0){ //已經死了，則顯示X，並且更新為倒下來的照片
                    PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().sprite = GameObject.Find(kvp.Value.NickName + "(player)").GetComponent<SpriteRenderer>().sprite;
                    DeadPlayerImg[kvp.Value.ActorNumber-1].SetActive(true);
                }
            }
            ShootPanel.SetActive(true);
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
                    shootPlayerKey = key;
                    UnityEngine.UI.Image playerImg =  ShootPanel.transform.Find("Player" + shootPlayerKey).gameObject.GetComponent<UnityEngine.UI.Image>();
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
        if(shootPlayerKey!=-1){
            selectedPlayer = ShootPanel.transform.Find("Player" + shootPlayerKey).gameObject;
            UnityEngine.UI.Image playerImg = selectedPlayer.GetComponent<UnityEngine.UI.Image>();
            Color color = playerImg.color;
            color.a = 0f;
            playerImg.color = color;
        }       
        shootPlayerKey = -1;
    }
    public void Shoot(){
        alreadyUsed = true;
        ShootBtn.interactable = false;
        CheckShootBtn.SetActive(false);
        ResetSelectedBtn.SetActive(false);
        UnityEngine.UI.Image playerImg =  ShootPanel.transform.Find("Player" + shootPlayerKey).gameObject.GetComponent<UnityEngine.UI.Image>();
        Color color = playerImg.color;
        color.a = 0f;
        playerImg.color = color;
        print(PhotonNetwork.LocalPlayer.NickName + " vote " + _gm.FindPlayerByKey(shootPlayerKey).NickName + ".");
        print("Shoot on " + PlayerName[shootPlayerKey-1].text);
        ShootPanel.SetActive(false);
        //animator play
        _gm.CallRpcPlayAudioOnPlayer(PhotonNetwork.LocalPlayer.NickName, 0);//audioCode = 0
        _gm.CallRpcIsDead(PlayerName[shootPlayerKey-1].text, 1);//被獵人殺死表示1
    }  
}
