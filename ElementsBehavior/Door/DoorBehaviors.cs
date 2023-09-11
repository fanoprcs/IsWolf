using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class DoorBehaviors : MonoBehaviourPunCallbacks
{
    //public GameObject PeekBtn;偷看功能改成直接在房內就可以看到黑影
    private float radius;
    [SerializeField]GameObject BreakDoorBtn;
    [SerializeField]GameObject DoorBtn;
    [SerializeField]GameObject LockBtn;
    [SerializeField]GameObject UnlockBtn;
    public GameObject BellBtn;
    [SerializeField] UnityEngine.UI.Text chatRoom;//用notebook來顯示偷看到的人是誰
    public GameObject CheckBtn;
    PlayerController _pc;
    GameManager _gm;
    private bool status = false;
    public bool whetherLock = false;
    public bool whetherOpen = false;
    public int doorKey;// 0 ~ n
    public bool canCheck;//有人按門鈴時候，關門可以查看，一旦開門就不能查看，此變數表示該們可以被查看
    public int CheckPlayerId;
    private bool isWolf = false;
    public bool successBreakDoor;
    // Start is called before the first frame update
    void Start()
    {
        radius = 0.5f;
        whetherOpen = false;
        canCheck = false;
        //player = GameObject.Find(PhotonNetwork.LocalPlayer.NickName + "(player)");
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    //如果門沒鎖就可以打開，只有主人才能鎖住自己房間的門
    private void Update() {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);
        bool alter = false;
        GameObject me = null;
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject.name.Contains("(player)"))
            {
                PhotonView pv = collider.gameObject.GetComponent<PhotonView>();
                if(pv.IsMine &&_gm.playerMap[pv.Owner][0] == 1){//還活著
                    alter = true;
                    me = collider.gameObject;
                    if(_gm.playerMap[pv.Owner][1] == (int)Careers.wolf){
                        isWolf = true;
                    }
                }
            }
        }
        if(alter){//角色靠近門旁邊
            DoorBtn.SetActive(true);
            if(Input.GetKeyUp(KeyCode.E)){
                SwitchDoor();
            }
            if(Input.GetKeyUp(KeyCode.R) && LockBtn.activeInHierarchy){
                LockDoor();
            }
            else if(Input.GetKeyUp(KeyCode.R) && UnlockBtn.activeInHierarchy){
                UnlockDoor();
            }
            if(!whetherOpen){//角色靠近門旁邊，關門時
                if(isWolf){//是狼人，且能破門的話
                    WolfBehaviors _w = GameObject.Find("Wolf").GetComponent<WolfBehaviors>();
                    if(_w.canBreak && !_w.alreadyBreak){//是否已經使用破門和是否在可以破門的區域
                        successBreakDoor = true;
                        BreakDoorBtn.SetActive(true);
                        BreakDoorBtn.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
                        BreakDoorBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(BreakDoor);
                    }
                    else{
                        BreakDoorBtn.SetActive(false);
                    }
                }
                if(me.GetComponent<PlayerController>().canLock){//是否在可以破門的區域
                    if(!whetherLock){//如果沒鎖門
                        LockBtn.SetActive(true);
                        UnlockBtn.SetActive(false);
                        LockBtn.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
                        LockBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(LockDoor);
                    }
                    else{//如果是鎖門
                        UnlockBtn.SetActive(true);
                        LockBtn.SetActive(false);
                        UnlockBtn.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
                        UnlockBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(UnlockDoor);
                    }
                }
                if(me.GetComponent<PlayerController>().canRing){//是否在可以按門鈴的區域
                    BellBtn.SetActive(true);
                    BellBtn.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
                    BellBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(RingTheBell);
                }
                else{
                    BellBtn.SetActive(false);
                }
                //if(me.GetComponent<PlayerController>().canPeek){//是否在可以偷窺的區域
                    //PeekBtn.SetActive(true);
                    //PeekBtn.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
                    //PeekBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(PeekPlayer);
                //}
                //else{
                //    PeekBtn.SetActive(false);
                //}
                if(this.canCheck && me.GetComponent<PlayerController>().canWatch){//是否在可以查看誰按了門鈴的區域
                    CheckBtn.SetActive(true);
                    CheckBtn.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
                    CheckBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(CheckPlayer);
                }
                else{
                    CheckBtn.SetActive(false);
                }
            }
            else{//角色靠近門旁邊，開門時
                if(isWolf){
                    BreakDoorBtn.SetActive(false);
                }
                LockBtn.SetActive(false);
                UnlockBtn.SetActive(false);
                //PeekBtn.SetActive(false);
                CheckBtn.SetActive(false);
                BellBtn.SetActive(false);
            }
            
            if(!status){//剛靠近門的時候設置listener，status避免反覆添加listner吃效能
                
                DoorBtn.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
                DoorBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(SwitchDoor);
            }
            status = true;
        }
        else{//角色離開門旁邊
            if(status){//status避免反覆設置吃效能
                if(isWolf){
                    BreakDoorBtn.SetActive(false);
                }
                DoorBtn.SetActive(false);
                BellBtn.SetActive(false);
                //PeekBtn.SetActive(false);
                CheckBtn.SetActive(false);
                LockBtn.SetActive(false);
                UnlockBtn.SetActive(false);
                status = false;
            }
        }
    }
    public void SwitchDoor(){
        if(!whetherOpen){
            if(!whetherLock)
                _gm.CallRpcDoorSwitchStatus(doorKey, true);
            else{
                string richText = "\n<color=#" + ColorUtility.ToHtmlStringRGB(Color.blue) + ">";
                richText += "門已上鎖";
                richText += "</color>";
                chatRoom.text += richText;
                //門鎖住的音效
                print("已經上鎖");
            }
        }
        else{
            _gm.CallRpcDoorSwitchStatus(doorKey, false);
        }
    }
    
    public void LockDoor(){
        GameObject player =  GameObject.Find(PhotonNetwork.LocalPlayer.NickName + "(player)");
        StartCoroutine(_gm.GenerateProgressBar(player.transform.position.x, player.transform.position.y + 1.4f, 1f, false, doorKey,() =>
        {
            Debug.Log("進度條填滿");
            if(!whetherOpen){
                print("Lock Door");
                _gm.CallRpcDoorSwitchLock(doorKey, true);
            }
            else{
                print("Lock failed");
            }
            LockBtn.SetActive(false);
            
        }));
        
    }
    public void UnlockDoor(){
        GameObject player =  GameObject.Find(PhotonNetwork.LocalPlayer.NickName + "(player)");
        StartCoroutine(_gm.GenerateProgressBar(player.transform.position.x, player.transform.position.y + 1.45f, 1f, false, doorKey,() =>
        {
            Debug.Log("進度條填滿");
            if(!whetherOpen){
                print("Unlock Door");
                _gm.CallRpcDoorSwitchLock(doorKey, false);
            }
            else{
                print("Unlock failed");
            }
            UnlockBtn.SetActive(false);  
        }));
        
    }
    public void RingTheBell(){
        print("ring the bell");
        _gm.CallRpcRingTheBell(doorKey, PhotonNetwork.LocalPlayer.ActorNumber);//這邊會通知所有人這扇門是由誰按下了門鈴，所有人都可以得到CheckPlayerId
        
    }

    public void CheckPlayer(){
        canCheck = false;
        print("Player " + CheckPlayerId + " ring the bell");
        string richText = "\n<color=#" + ColorUtility.ToHtmlStringRGB(Color.blue) + ">";
        richText += "Player " + CheckPlayerId + " ring the bell";
        richText += "</color>";
        chatRoom.text += richText;
    }
    public void BreakDoor(){
        GameObject player =  GameObject.Find(PhotonNetwork.LocalPlayer.NickName + "(player)");
        bool isBreakDoor = true;
        StartCoroutine(_gm.GenerateProgressBar(player.transform.position.x, player.transform.position.y + 1.45f, 10f, isBreakDoor, doorKey, () =>
        {
            if(successBreakDoor){//成功破門的話才觸發
                _gm.CallRpcDoorSwitchStatus(doorKey, true);
                GameObject.Find("Wolf").GetComponent<WolfBehaviors>().alreadyBreak = true;//破完門後才確定成功破門
            }
        }));
    }

}
