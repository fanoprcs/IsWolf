using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

public class NoteBook : MonoBehaviour
{
    
    [SerializeField] UnityEngine.UI.Text chatRoom;
    [SerializeField] UnityEngine.UI.InputField inputChatMessage;
    [SerializeField] UnityEngine.UI.Button minimize;
    [SerializeField] UnityEngine.UI.Button maximize;
    [SerializeField] GameObject notebookOpen;
    [SerializeField] GameObject notebookClose;
    private PlayerController _pc;
    // Start is called before the first frame update
    void Start()
    {
        _pc = GameObject.Find(PhotonNetwork.LocalPlayer.NickName + "(player)").GetComponent<PlayerController>();
        inputChatMessage.onValueChanged.AddListener(OnInputValueChanged);//當筆記本再輸入字的時候玩家不能移動
        inputChatMessage.onEndEdit.AddListener(OnInputEndEdit);//當筆記本再輸入字的時候玩家不能移動
    }
    public void OnClickChat(){
        string chatMessage = GetChatMessage();
        if(IsValidMessage(chatMessage)){
            string richText = "\n" + "<size=15>" + "<color=#"  + ColorUtility.ToHtmlStringRGB(Color.gray) + ">" + 
            "[" + DateTime.Now.Hour.ToString() + ": " + DateTime.Now.Minute.ToString() + ": " + DateTime.Now.Second.ToString() + "]: "
            + "</color>" + "</size>" + chatMessage;
            chatRoom.text += richText;
        }
        inputChatMessage.text = "";
        inputChatMessage.DeactivateInputField();//取消自動選中
    }
    public void OnClickMinimize(){
        notebookOpen.SetActive(false);
        notebookClose.SetActive(true);
    }
    public void OnClickMaximize(){
        notebookOpen.SetActive(true);
        notebookClose.SetActive(false);
    }
    private string GetChatMessage(){
        return inputChatMessage.text.Trim();//Trim是過濾字元，預設會過濾空白
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
