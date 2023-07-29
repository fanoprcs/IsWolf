using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class NightMask : MonoBehaviourPunCallbacks//這個腳本只有本地端會執行
{
    [SerializeField]GameObject InsideMask;
    [SerializeField]GameObject OutsideMask;
    private GameManager _gm;
    void Start(){
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
    public void SwitchToInside(){//進入室內
        _gm.CallRpcSwitchAnimatorNightmaskBool(PhotonNetwork.LocalPlayer.NickName, false);
        foreach(var kvp in PhotonNetwork.CurrentRoom.Players){
            if(PhotonNetwork.LocalPlayer != kvp.Value && _gm.playerMap[kvp.Value][0] == 1){//如果還活著且不是自己
                GameObject player = GameObject.Find(kvp.Value.NickName+ "(player)");
                if(player.GetComponent<PlayerController>().canRing){//如果玩家在外面
                    player.GetComponent<Animator>().SetBool("night_mask", true);
                }
                
            }
        }
        
        print("SwitchToInside");
    }
    public void SwitchToOutside(){//離開室內
        _gm.CallRpcSwitchAnimatorNightmaskBool(PhotonNetwork.LocalPlayer.NickName, true);
        foreach(var kvp in PhotonNetwork.CurrentRoom.Players){//傳送玩家
            if(PhotonNetwork.LocalPlayer != kvp.Value){//如果還活著
                GameObject player = GameObject.Find(kvp.Value.NickName+ "(player)");
                if(player.GetComponent<PlayerController>().canRing){//如果玩家在外面
                    player.GetComponent<Animator>().SetBool("night_mask", false);
                }
                
            }
        }
        print("SwitchToOutside");
    }
    
    public void SwitchToDay(){
        foreach(var kvp in PhotonNetwork.CurrentRoom.Players){
            if(PhotonNetwork.LocalPlayer != kvp.Value){//如果不是自己
                GameObject.Find(kvp.Value.NickName+ "(player)").GetComponent<Animator>().SetBool("night_mask", false);
            }
        }
        print("SwitchToDay");
        
    }
    
}
