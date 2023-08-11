using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class WolfBehaviors : MonoBehaviourPunCallbacks
{
    [SerializeField] UnityEngine.UI.Button killBtn;
    private float killRadius;
    string targetPlayerName;
    GameManager _gm;
    GameObject player;
    public bool canKilled;//是否可以殺人
    public bool canBreak;//是否再可以破門的區域(門外、門內)
    public bool alreadyBreak;//當晚是否已經破壞房門
    void Start()
    {
        killRadius = 0.75f;
        player = GameObject.Find(PhotonNetwork.LocalPlayer.NickName + "(player)");
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        killBtn.interactable = false;
        canKilled = true;
        canBreak = true;
        alreadyBreak = true;//一開始是白天不能 break
        print("WolfBehaviors");
    }
    void Update()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.transform.position, killRadius);
        float minDistance = Mathf.Infinity;
        GameObject nearestObject = null;
        foreach (Collider2D collider in colliders)
        {
            
            if (collider.gameObject.name.Contains("(player)"))
            {
                if(collider.gameObject.GetComponent<PlayerController>().playerCareer != Careers.wolf
                && _gm.playerMap[collider.gameObject.GetComponent<PhotonView>().Owner][0] == 1){//不是狼且活著
                    float distance = Vector3.Distance(transform.position, collider.gameObject.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestObject = collider.gameObject;
                    }  
                }
            }
        }
        if (nearestObject != null && canKilled){
            if(Input.GetKeyUp(KeyCode.Space)){
                killPlayer();
            }
            targetPlayerName = nearestObject.GetComponent<PhotonView>().Owner.NickName;
            killBtn.interactable = true;
        }
        else{
            killBtn.interactable = false;
        }
    }
    public void killPlayer(){
        _gm.CallRpcIsDead(targetPlayerName, 0);
    }
    
}
