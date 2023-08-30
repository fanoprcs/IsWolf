using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Voice.Unity;
using Photon.Voice.PUN;

public class MicrophoneManager : MonoBehaviour
{
    public UnityEngine.Sprite microphoneOn;
    public UnityEngine.Sprite microphoneOff;
    public Recorder recorder;
    public Speaker speaker;

    private bool isMicrophoneEnabled = false;
    //private PunVoiceClient voiceClient;
    void Start()
    {
        // 初始化 Photon 語音模塊
       
        //voiceClient = GetComponent<PunVoiceClient>();
     
        //voiceClient.ConnectAndJoinRoom();
        
        //print(voiceClient.ClientState);
    }
    
    public void ToggleMicrophone(GameObject micBtn){
        if (isMicrophoneEnabled){
            micBtn.GetComponent<UnityEngine.UI.Image>().sprite = microphoneOff;
            recorder.enabled = false;
            speaker.enabled = false;
        }
        else{
            micBtn.GetComponent<UnityEngine.UI.Image>().sprite = microphoneOn;
            recorder.enabled = true;
            speaker.enabled = true;
        }

        isMicrophoneEnabled = !isMicrophoneEnabled;
    }
}