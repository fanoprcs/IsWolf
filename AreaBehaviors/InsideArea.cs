using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class InsideArea : MonoBehaviourPunCallbacks
{
    [SerializeField] GameManager _gm;
    private NightMask _nm;
    void Start() {
        
        _nm = GameObject.Find("Mask").GetComponent<NightMask>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            print("in");
            if(GameObject.Find("Wolf")){
                GameObject.Find("Wolf").GetComponent<WolfBehaviors>().canBreak = false;
            }
            PlayerController _p = other.gameObject.GetComponent<PlayerController>();
            _p.canLock = true;//即使不是本地的player也會改動
            _p.canRing = false;
            _p.canPeek = true;
            _p.canWatch = true;
            
            if(_gm.gameStart)
                _gm.CallRpcSwitchArea(PhotonNetwork.LocalPlayer.NickName, false);
            if(!_gm.IsDayTime && other.gameObject.GetComponent<PhotonView>().IsMine){//本地的才會改動
                _nm.SwitchToInside();//夜晚遮罩的切換(包含切畫角色顯示、名字等)
            }
            
            
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            print("exit");
            if(GameObject.Find("Wolf")){
                GameObject.Find("Wolf").GetComponent<WolfBehaviors>().canBreak = true;
            }
            PlayerController _p = other.gameObject.GetComponent<PlayerController>();
            _p.canLock = false;//即使不是本地的player也會改動
            _p.canRing = true;
            _p.canPeek = false;
            _p.canWatch = false;
            if(_gm.gameStart)
                _gm.CallRpcSwitchArea(PhotonNetwork.LocalPlayer.NickName, true);
            if(!_gm.IsDayTime && other.gameObject.GetComponent<PhotonView>().IsMine){//本地的才會改動
                _nm.SwitchToOutside();//夜晚遮罩的切換(包含切畫角色顯示、名字等)
            }
        }
    }
}
