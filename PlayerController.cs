using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class PlayerController : MonoBehaviourPunCallbacks
{
    private Animator animator;
    public float moveSpeed = 3f;
    public Careers playerCareer;
    GameManager _gm;
    PhotonView _pv;
    private int nowWalkingDir;// 0 = null, 1= top, 2 = left, 3 = down, 4 = right
    public bool allowMovement = true;
    public bool canLock;//此變數用來管理玩家是否可以鎖門
    public bool canRing;//此變數用來管理玩家是否可以按門鈴
    public bool canPeek;//此變數用來管理玩家是否可以偷看門外
    public bool canWatch;//此變數用來管理玩家是否可以查看門外Z
    public UnityEngine.UI.Text nickName;
    void Start()
    {
        _pv = GetComponent<PhotonView>();
        this.gameObject.name = _pv.Owner.NickName + "(player)";
        
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        //不能直接寫: playerCareer= _gm.playerCareer 因為即使是別人那邊創造的角色，依然是調用我這邊的script生成，要從gamemanager抓資料
        playerCareer = (Careers)_gm.playerMap[gameObject.GetComponent<PhotonView>().Owner][1];
        if(!gameObject.GetComponent<PhotonView>().IsMine)
            Destroy(gameObject.GetComponent<Rigidbody2D>());
        else{//如果該生成的玩家為自己則以自己玩家位置為聽眾
            this.gameObject.AddComponent<AudioListener>();
        }
        canLock = false;
        canRing = true;
        canPeek = false;
        canWatch = false;
        nickName.GetComponent<UnityEngine.UI.Text>().text = _pv.Owner.NickName;
        animator = GetComponent<Animator>();
    }
    void Update(){
        
    }
    void FixedUpdate(){
        if(gameObject.GetComponent<PhotonView>().IsMine && _gm.playerMap[PhotonNetwork.LocalPlayer][0] == 1 && allowMovement){
            Move();
            //Shoot();
        }
        if(allowMovement == false){//修正 限制allowMovement時產生的延遲問題
            photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_right", false);
            photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_down", false);
            photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_up", false);
            photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_left", false);
        }
    }
    void Move(){
        if(Input.GetKey(KeyCode.W) && (nowWalkingDir == 1 || nowWalkingDir == 0)){
            nowWalkingDir = 1;
            transform.Translate(0, moveSpeed * Time.deltaTime, 0);
            if(animator.GetBool("walk_right")){
                animator.SetBool("walk_right", false);
                photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_right", false);
            }
            if(animator.GetBool("walk_down")){
                animator.SetBool("walk_down", false);
                photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_down", false);
            }
            if(animator.GetBool("walk_left")){
                animator.SetBool("walk_left", false);
                photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_left", false);
            }
            animator.SetBool("walk_up", true);
            photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_up", true);
        }
        else if(Input.GetKey(KeyCode.A) && (nowWalkingDir == 2 || nowWalkingDir == 0)){
            nowWalkingDir = 2;
            if(animator.GetBool("walk_up")){
                animator.SetBool("walk_up", false);
                photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_up", false);
            }
            if(animator.GetBool("walk_down")){
                animator.SetBool("walk_down", false);
                photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_down", false);
            }
            if(animator.GetBool("walk_right")){
                animator.SetBool("walk_right", false);
                photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_right", false);
            }
            animator.SetBool("walk_left", true);
            photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_left", true);
            transform.Translate(-1 * moveSpeed * Time.deltaTime, 0, 0);  
        }
        else if(Input.GetKey(KeyCode.S) && (nowWalkingDir == 3 || nowWalkingDir == 0)){
            nowWalkingDir = 3;
            transform.Translate(0, -1 * moveSpeed * Time.deltaTime, 0);
            if(animator.GetBool("walk_right")){
                animator.SetBool("walk_right", false);
                photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_right", false);
            }
            if(animator.GetBool("walk_up")){
                animator.SetBool("walk_up", false);
                photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_up", false);
            }
            if(animator.GetBool("walk_left")){
                animator.SetBool("walk_left", false);
                photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_left", false);
            }
            animator.SetBool("walk_down", true);
            photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_down", true);
        }
        else if(Input.GetKey(KeyCode.D) && (nowWalkingDir == 4 || nowWalkingDir == 0)){
            nowWalkingDir = 4;
            if(animator.GetBool("walk_up")){
                animator.SetBool("walk_up", false);
                photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_up", false);
            }
            if(animator.GetBool("walk_down")){
                animator.SetBool("walk_down", false);
                photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_down", false);
            }
            if(animator.GetBool("walk_left")){
                animator.SetBool("walk_left", false);
                photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_left", false);
            }
            animator.SetBool("walk_right", true);
            photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_right", true);
            transform.Translate(moveSpeed * Time.deltaTime, 0, 0); 
        }
        else{
            nowWalkingDir = 0;
            if(animator.GetBool("walk_right")){
                animator.SetBool("walk_right", false);
                photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_right", false);
            }
            else if(animator.GetBool("walk_up")){
                animator.SetBool("walk_up", false);
                photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_up", false);
            }
            else if(animator.GetBool("walk_down")){
                animator.SetBool("walk_down", false);
                photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_down", false);
            }
            else if(animator.GetBool("walk_left")){
                animator.SetBool("walk_left", false);
                photonView.RPC("SyncAnimationState", RpcTarget.Others, "walk_left", false);
            }
        }
    }
    
    [PunRPC]
    private void SyncAnimationState(string stateName, bool value)
    {
        // 在其他客戶端上，同步動畫狀態
        if(animator)
            animator.SetBool(stateName, value);
    }
    
}
