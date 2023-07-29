using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class NoteBook : MonoBehaviour
{
    
    [SerializeField] UnityEngine.UI.Text chatRoom;
    [SerializeField] UnityEngine.UI.InputField inputChatMessage;
    [SerializeField] UnityEngine.UI.Button minimize;
    [SerializeField] UnityEngine.UI.Button maximize;
    [SerializeField] GameObject notebookOpen;
    [SerializeField] GameObject notebookClose;
    // Start is called before the first frame update
    void Start()
    {
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
}
