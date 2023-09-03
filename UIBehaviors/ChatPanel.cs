using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class ChatPanel : MonoBehaviourPunCallbacks
{
    private string chatSb;
    [SerializeField] UnityEngine.UI.Text chatRoom;
    [SerializeField] UnityEngine.UI.InputField inputChatMessage; 
    private PlayerController _pc;
    void Start()
    {
        chatSb = "";
        _pc = GameObject.Find(PhotonNetwork.LocalPlayer.NickName + "(player)").GetComponent<PlayerController>();
        inputChatMessage.onValueChanged.AddListener(OnInputValueChanged);//當筆記本再輸入字的時候玩家不能移動
        inputChatMessage.onEndEdit.AddListener(OnInputEndEdit);//當筆記本再輸入字的時候玩家不能移動
    }

    public void OnClickChat(){
        string chatMessage = GetChatMessage();
        if(IsValidMessage(chatMessage)){
            ExitGames.Client.Photon.Hashtable table = new ExitGames.Client.Photon.Hashtable();
            table.Add("chatRoom", chatMessage);
            PhotonNetwork.LocalPlayer.SetCustomProperties(table);
        }
        inputChatMessage.text = "";
    }
    private string GetChatMessage(){
        return inputChatMessage.text.Trim();//Trim是過濾字元，預設會過濾空白
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps){
        string richText = "";
        if(targetPlayer.IsLocal){
            richText = "\n<color=#" + ColorUtility.ToHtmlStringRGB(Color.blue) + ">";
            richText += targetPlayer.NickName + "(" + targetPlayer.ActorNumber + ") : </color>";
        }
        else{
            richText += targetPlayer.NickName + "(" + targetPlayer.ActorNumber + ") : ";
        }
        chatSb += "\n" + richText + (string)changedProps["chatRoom"];
        chatRoom.text = chatSb;
    }
    public void CloseChatPanel(){
        this.gameObject.SetActive(false);
    }
    private bool IsValidMessage(string str){//言論審查
        return true;
    }
    private void OnInputValueChanged(string value)//用來限制當打字時角色不能移動
    {
        if(value!="")//因為當button發送時，value變成"" 
            _pc.allowMovement = false;
        else
            _pc.allowMovement = true;
    }

    private void OnInputEndEdit(string value)
    {
        _pc.allowMovement = true;
    }
}
