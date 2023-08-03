using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class VoteBehaviors : MonoBehaviourPunCallbacks
{
    [SerializeField]UnityEngine.UI.Text []PlayerName;
    [SerializeField]GameObject []PlayerImg;//此欄位底下有放置icon的地方，要利用關聯位置來找到物件
    [SerializeField]UnityEngine.Sprite []PlayerIcon;
    [SerializeField]GameObject VotePanel;
    [SerializeField]GameObject SkipBtn;
    [SerializeField]GameObject CheckVoteBtn;
    [SerializeField]GameObject ResetSelectedBtn;
    [SerializeField]GameObject []already;
    [SerializeField]GameObject []dead;
    GameManager _gm;
    [SerializeField] GameObject VoteBtn;
    [SerializeField] AudioClip audio_checkSelected;//選擇投票對象時的音效(只有本地會播放)
    int votePlayerkey;
    public bool alreadyVote;
    void Start()
    {
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        alreadyVote = false;
        votePlayerkey = -1;
        print("VoteBehaviors");
    }

    public void showVotePanel(){
        foreach(var kvp in PhotonNetwork.CurrentRoom.Players){
            PlayerName[kvp.Value.ActorNumber-1].text = kvp.Value.NickName;
            PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().sprite = GameObject.Find(kvp.Value.NickName + "(player)").GetComponent<SpriteRenderer>().sprite;
            Color color = PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().color;
            color.a = 1f;
            PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().color = color;
            PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().SetNativeSize();
        }
        SkipBtn.SetActive(true);
        CheckVoteBtn.SetActive(false);
        ResetSelectedBtn.SetActive(false);
        VoteBtn.SetActive(false);
        VotePanel.SetActive(true);
        votePlayerkey = -1;
    }
    public void closeVotePanel(){
        alreadyVote = false;//重設是否投票
        VoteBtn.SetActive(true);
        VotePanel.SetActive(false);
    }
    public void VotePlayer(){
        CheckVoteBtn.SetActive(false);
        ResetSelectedBtn.SetActive(false);
        SkipBtn.SetActive(false);
        alreadyVote = true;
        if(votePlayerkey!=-1){//vote
            UnityEngine.UI.Image playerImg =  VotePanel.transform.Find("Player" + votePlayerkey).gameObject.GetComponent<UnityEngine.UI.Image>();
            Color color = playerImg.color;
            color.a = 0f;
            playerImg.color = color;
            print(PhotonNetwork.LocalPlayer.NickName + " vote " + _gm.FindPlayerByKey(votePlayerkey).NickName + ".");
        }
        else{//skip
            print(PhotonNetwork.LocalPlayer.NickName + " skip the vote.");
        }
        _gm.CallRpcAlreadyVote(votePlayerkey, PhotonNetwork.LocalPlayer.ActorNumber);//投票的對象、做出投票動作的人
    }
    public void SelectedPlayer(int key){
        if(!alreadyVote){
            // play sound
            alreadyVote = true;
            votePlayerkey = key;
            UnityEngine.UI.Image playerImg =  VotePanel.transform.Find("Player" + votePlayerkey).gameObject.GetComponent<UnityEngine.UI.Image>();
            Color color = playerImg.color;
            color.a = 0.4f;
            playerImg.color = color;
            
            CheckVoteBtn.SetActive(true);
            ResetSelectedBtn.SetActive(true);
        }
    }
    public void SkipVote(){
        if(!alreadyVote){
            // play sound
            alreadyVote = true;
            votePlayerkey = -1;
            CheckVoteBtn.SetActive(true);
            ResetSelectedBtn.SetActive(true);
        }
    }
    public void ResetSelected(){
        CheckVoteBtn.SetActive(false);
        ResetSelectedBtn.SetActive(false);
        alreadyVote = false;
        GameObject selectedPlayer;
        if(votePlayerkey!=-1){
            selectedPlayer = VotePanel.transform.Find("Player" + votePlayerkey).gameObject;
            UnityEngine.UI.Image playerImg = selectedPlayer.GetComponent<UnityEngine.UI.Image>();
            Color color = playerImg.color;
            color.a = 0f;
            playerImg.color = color;
        }       
        votePlayerkey = -1;
    }
}