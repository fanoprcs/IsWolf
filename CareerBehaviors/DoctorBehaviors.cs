using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
public class DoctorBehaviors : MonoBehaviourPunCallbacks
{
    [SerializeField] UnityEngine.UI.Button checkBtn;
    [SerializeField] UnityEngine.UI.Text chatRoom;
    GameManager _gm;
    GameObject player;
    private float checkRadius;
    string targetPlayerName;
    void Start()
    {
        checkRadius = 0.75f;
        player = GameObject.Find(PhotonNetwork.LocalPlayer.NickName + "(player)");
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        checkBtn.interactable = false;
        print("DoctorBehaviors");
    }

    // Update is called once per frame
    void Update()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.transform.position, checkRadius);
        float minDistance = Mathf.Infinity;
        GameObject nearestObject = null;
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject.name.Contains("(player)"))
            {
                if(_gm.playerMap[collider.gameObject.GetComponent<PhotonView>().Owner][0] == 0){
                    float distance = Vector3.Distance(transform.position, collider.gameObject.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestObject = collider.gameObject;
                    }  
                }
            }
        }
        if (nearestObject != null){   
            if(Input.GetKeyUp(KeyCode.Space)){
                CheckPlayer();
            }
            targetPlayerName = nearestObject.GetComponent<PhotonView>().Owner.NickName;
            checkBtn.interactable = true;
        }
        else{
            checkBtn.interactable = false;
        }
    }
    public void CheckPlayer(){
        print( "check " + targetPlayerName);
        StartCoroutine(_gm.GenerateProgressBar(player.transform.position.x, player.transform.position.y + 1f, 5f, false,() =>
        {
            Debug.Log("進度條填滿");
            if(_gm.playerMap[_gm.FindPlayerByNickname(targetPlayerName)][1] == (int)Careers.wolf){
                string richText = "<color=#" + ColorUtility.ToHtmlStringRGB(Color.red) + ">";
                richText += "\n該玩家為: 狼人";
                richText += "</color>";
                chatRoom.text += richText;
                //播放狼的圖案(gameManager下的動畫)
            }
            else{
                string richText = "<color=#00FFFF>";
                richText += "\n該玩家為: 人類";
                richText += "</color>";
                chatRoom.text += richText;
                //播放人的圖案(gameManager下的動畫)
            }
        }));
    }
}
