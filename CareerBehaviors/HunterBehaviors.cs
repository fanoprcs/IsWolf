using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
public class HunterBehaviors : MonoBehaviourPunCallbacks/*, UnityEngine.EventSystems.IPointerClickHandler*/
{
    [SerializeField] UnityEngine.UI.Button ShootBtn;
    [SerializeField]GameObject ShootPanel;
    [SerializeField]UnityEngine.UI.Text []PlayerName;
    [SerializeField]GameObject []PlayerImg;
    [SerializeField]GameObject []DeadPlayerImg;
    GameManager _gm;
    public bool alreadyUsed = false;
    void Start()
    {
        
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        foreach(var kvp in PhotonNetwork.CurrentRoom.Players){
            PlayerName[kvp.Value.ActorNumber-1].text = kvp.Value.NickName;
            PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().sprite = GameObject.Find(kvp.Value.NickName + "(player)").GetComponent<SpriteRenderer>().sprite;
            Color color = PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().color;
            color.a = 1f;
            PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().color = color;
            PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().SetNativeSize();
            
        }
        print("HunterBehaviors");
    }
    public void ShowShootPanel(){
        foreach(var kvp in PhotonNetwork.CurrentRoom.Players){
            if(_gm.playerMap[kvp.Value][0] == 0){ //已經死了，則顯示圖案
                DeadPlayerImg[kvp.Value.ActorNumber-1].SetActive(true);
            }
        }
        if(ShootPanel.activeInHierarchy){
        ShootPanel.SetActive(false);
        }
        else{
            ShootPanel.SetActive(true);
        }
    }
    public void Shoot(int key){
        alreadyUsed = true;
        ShootBtn.interactable = false;
        Photon.Realtime.Player player;
        player = _gm.FindPlayerByNickname(PlayerName[key-1].text);
        print("Clicked on " + player.NickName);
        ShootPanel.SetActive(false);
        //animator play
        _gm.CallRpcIsDead(player.NickName);
    }

}
