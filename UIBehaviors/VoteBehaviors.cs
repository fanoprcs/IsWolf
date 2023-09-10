using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Voice.PUN;
public class VoteBehaviors : MonoBehaviourPunCallbacks
{
    [SerializeField]UnityEngine.UI.Text []PlayerName;
    [SerializeField]GameObject []PlayerImg;//此欄位底下有放置icon的地方，要利用關聯位置來找到物件
    [SerializeField]GameObject []micIcon;
    public UnityEngine.Sprite micIconOn;
    public UnityEngine.Sprite micIconOff;
    [SerializeField]UnityEngine.Sprite []PlayerIcon;
    public GameObject VotePanel;
    public GameObject ChatPanel;
    public GameObject CloseVotePanelBtn;
    public GameObject SkipPanel;
    public GameObject VoteBtn;
    [SerializeField]GameObject SkipBtn;
    [SerializeField]GameObject CheckVoteBtn;
    [SerializeField]GameObject ResetSelectedBtn;
    [SerializeField]GameObject []already;
    [SerializeField]GameObject []dead;
    MicrophoneManager _mc;
    GameManager _gm;
    int votePlayerKey;//1~9
    public bool alreadySelect;
    void Start()
    {
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        _mc = FindObjectOfType<PunVoiceClient>().GetComponent<MicrophoneManager>();
        alreadySelect = false;
        votePlayerKey = -1;
        foreach(var kvp in PhotonNetwork.CurrentRoom.Players){
            PlayerName[kvp.Value.ActorNumber-1].text = kvp.Value.NickName;
            PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().sprite = GameObject.Find(kvp.Value.NickName + "(player)").GetComponent<SpriteRenderer>().sprite;
            Color color = PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().color;
            color.a = 1f;
            PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().color = color;
            PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().SetNativeSize();
            
        }
        print("VoteBehaviors");
    }

    public void ShowVotePanel(){
        foreach(var kvp in PhotonNetwork.CurrentRoom.Players){
            if(_gm.playerMap[kvp.Value][0] == 0){//已經死了，則顯示X，並且更新為倒下來的照片
                PlayerImg[kvp.Value.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().sprite = GameObject.Find(kvp.Value.NickName + "(player)").GetComponent<SpriteRenderer>().sprite;
                dead[kvp.Value.ActorNumber-1].SetActive(true);
            }
        }
        SkipBtn.SetActive(true);
        CheckVoteBtn.SetActive(false);
        ResetSelectedBtn.SetActive(false);
        VoteBtn.SetActive(false);
        VotePanel.SetActive(true);
    }
    public void CloseVotePanel(){
        if(votePlayerKey!=-1){//已經選中，但是未確認投票時間就到了，則要把已經選中的效果恢復原狀
            UnityEngine.UI.Image playerImg = VotePanel.transform.Find("Player" + votePlayerKey).gameObject
                                            .GetComponent<UnityEngine.UI.Image>();
            Color color = playerImg.color;
            color.a = 0f;
            playerImg.color = color;
        }
        votePlayerKey = -1;
        alreadySelect = false;//重設是否選擇
        ChatPanel.SetActive(false);
        VoteBtn.SetActive(true);
        VotePanel.SetActive(false);
    }
    public void VotePlayer(){
        CheckVoteBtn.SetActive(false);
        ResetSelectedBtn.SetActive(false);
        SkipBtn.SetActive(false);
        alreadySelect = true;
        if(votePlayerKey!=-1){//vote
            UnityEngine.UI.Image playerImg =  VotePanel.transform.Find("Player" + votePlayerKey).gameObject.GetComponent<UnityEngine.UI.Image>();
            Color color = playerImg.color;
            color.a = 0f;
            playerImg.color = color;
            print(PhotonNetwork.LocalPlayer.NickName + " vote " + PlayerName[votePlayerKey-1].text + ".");
        }
        else{//skip
            print(PhotonNetwork.LocalPlayer.NickName + " skip the vote.");
        }
        _gm.CallRpcAlreadyVote(votePlayerKey, PhotonNetwork.LocalPlayer.ActorNumber);//投票的對象、做出投票動作的人
    }
    public void SelectedPlayer(int key){//1~9
        if(_gm.FindPlayerByKey(key) != null){//只是因為測試的時候沒有其他玩家會有bug暫時設置的
            if(_gm.playerMap[_gm.FindPlayerByKey(key)][0] == 1){//選擇的對象還活著
                if(!alreadySelect && _gm.playerMap[PhotonNetwork.LocalPlayer][0] == 1){//自己本人還沒選擇且還活著
                    //should play sound?
                    alreadySelect = true;
                    votePlayerKey = key;
                    UnityEngine.UI.Image playerImg =  VotePanel.transform.Find("Player" + votePlayerKey).gameObject.GetComponent<UnityEngine.UI.Image>();
                    Color color = playerImg.color;
                    color.a = 0.4f;
                    playerImg.color = color;
                    
                    CheckVoteBtn.SetActive(true);
                    ResetSelectedBtn.SetActive(true);
                }
            }
        }
    }
    public void SkipVote(){
        if(!alreadySelect && _gm.playerMap[PhotonNetwork.LocalPlayer][0] == 1){
            // play sound
            alreadySelect = true;
            votePlayerKey = -1;
            CheckVoteBtn.SetActive(true);
            ResetSelectedBtn.SetActive(true);
        }
    }
    public void ResetSelected(){
        CheckVoteBtn.SetActive(false);
        ResetSelectedBtn.SetActive(false);
        alreadySelect = false;
        if(votePlayerKey!=-1){
            GameObject selectedPlayer = VotePanel.transform.Find("Player" + votePlayerKey).gameObject;
            UnityEngine.UI.Image playerImg = selectedPlayer.GetComponent<UnityEngine.UI.Image>();
            Color color = playerImg.color;
            color.a = 0f;
            playerImg.color = color;
        }
        votePlayerKey = -1;
    }
    public void ShowChatPanel(){
        if(ChatPanel.activeInHierarchy)
            ChatPanel.SetActive(false);
        else
            ChatPanel.SetActive(true);
    }
    public void SwitchMicrophoneChatMode(GameObject micBtn){
        if(_mc.isMicrophoneEnabled){
            micIcon[PhotonNetwork.LocalPlayer.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().sprite = micIconOff;
        }
        else{
            micIcon[PhotonNetwork.LocalPlayer.ActorNumber-1].GetComponent<UnityEngine.UI.Image>().sprite = micIconOn;
        }
        _mc.ToggleMicrophone(micBtn);
    }
}
