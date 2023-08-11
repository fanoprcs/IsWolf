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
    public bool canWatch;//此變數用來管理玩家是否可以查看門外
    private UnityEngine.UI.InputField inputField;//要跟筆記本的狀態做連結，因為當筆記本再輸入字的時候玩家不能移動
    private UnityEngine.UI.InputField enterMsg;//要跟筆記本的狀態做連結，因為當筆記本再輸入字的時候玩家不能移動
    public UnityEngine.UI.Text nickName;
    void Start()
    {
        _pv = GetComponent<PhotonView>();
        this.gameObject.name = _pv.Owner.NickName + "(player)";
        inputField = GameObject.Find("InputField(msg)").GetComponent<UnityEngine.UI.InputField>();
        enterMsg = GameObject.Find("Button(Chat)").GetComponent<UnityEngine.UI.InputField>();
        inputField.onValueChanged.AddListener(OnInputValueChanged);
        inputField.onEndEdit.AddListener(OnInputEndEdit);
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        //不能直接寫: playerCareer= _gm.playerCareer 因為即使是別人那邊創造的角色，依然是調用我這邊的script生成，要從gamemanager抓資料
        playerCareer = (Careers)_gm.playerMap[gameObject.GetComponent<PhotonView>().Owner][1];
        if(!gameObject.GetComponent<PhotonView>().IsMine)
            Destroy(gameObject.GetComponent<Rigidbody2D>());
        else{//以玩家位置為聽眾
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
    private void OnInputValueChanged(string value)//用來限制當打字時角色不能移動
    {
        if(value!="")//因為當button發送時，value變成"" 
            allowMovement = false;
        else
            allowMovement = true;
    }

    private void OnInputEndEdit(string value)
    {
        allowMovement = true;
    }
    [PunRPC]
    private void SyncAnimationState(string stateName, bool value)
    {
        // 在其他客戶端上，同步動畫狀態
        if(animator)
            animator.SetBool(stateName, value);
    }
    /*private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if(collision.transform.position.y < this.transform.position.y){
                this.GetComponent<SpriteRenderer>().sortingOrder = 9;
                print("Me order 0");
            }
            else{
                this.GetComponent<SpriteRenderer>().sortingOrder = 11;
                print("Me order 11");
            }
        }
    }*/
    /*private void Shoot(){
        float ScreenRatePosX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
        float ScreenRatePosY = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;
        if(Input.GetMouseButton(0) && shootFrequency >= shootCoolDown){
            float relatedPosX = ScreenRatePosX - transform.position.x;
            float relatedPosY = ScreenRatePosY - transform.position.y;
            if(relatedPosX != 0f || relatedPosY != 0f){
                float rate = (Mathf.Abs(relatedPosX) > Mathf.Abs(relatedPosY)) ? Mathf.Abs(relatedPosX / 0.1f) : Mathf.Abs(relatedPosY / 0.1f);     
                Vector3 offset = new Vector3(relatedPosX/rate, relatedPosY/rate, 0);
                GameObject bullet = PhotonNetwork.Instantiate("Bullet", transform.position + offset, Quaternion.identity);
                float rotateAngle = computeAngle(relatedPosX, relatedPosY);
                bullet.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotateAngle));
                bullet.GetComponent<Rigidbody2D>().velocity = transform.TransformDirection(new Vector2(relatedPosX, relatedPosY).normalized *bulletSpeed);
                shootFrequency = 0f;
            }
        }
        shootFrequency += Time.deltaTime;
    }
    private float computeAngle(float relatedPosX, float relatedPosY){
        if(relatedPosX == 0f)
            return (relatedPosY > 0f) ? 90f : 270f;
        if(relatedPosY == 0f)
            return (relatedPosX > 0f) ? 0f : 180f;
        float angle = Mathf.Atan(relatedPosY/relatedPosX) * 180 /Mathf.PI;
        if(relatedPosY > 0)
            return (relatedPosX > 0) ? angle : angle + 180f;
        else
            return (relatedPosX > 0) ? angle + 360f : angle + 180f;
    }
    private void UpdateHpBar(){
        hpBar.transform.localScale = new Vector3(hp/3f, hpBar.transform.localScale.y, hpBar.transform.localScale.z);
    }
    public void Hurt(int damage, Player player){
        ExitGames.Client.Photon.Hashtable table = new ExitGames.Client.Photon.Hashtable();
        hp -= damage;
        table.Add("hp", hp);
        player.SetCustomProperties(table);
        if(hp <= 0){
            Dead();
        }

    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if(targetPlayer == gameObject.GetComponent<PhotonView>().Owner){
            print(targetPlayer.NickName + " update");
            if(changedProps["hp"] != null)
                hp = (int)changedProps["hp"];
            UpdateHpBar();
        
    }*/
}
