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
    public GameObject wolfMicBtn;
    public GameObject voteMicBtn;

    public bool isMicrophoneEnabled = false;
    private PunVoiceClient voiceClient;
    void Start(){
        voiceClient = GetComponent<PunVoiceClient>();
    }
    void Update(){
        if(recorder.enabled)
            Debug.Log("Is Recording: " + recorder.IsCurrentlyTransmitting);
    }
    public void ToggleMicrophone(GameObject micBtn){
        if (isMicrophoneEnabled){
            micBtn.GetComponent<UnityEngine.UI.Image>().sprite = microphoneOff;
            recorder.TransmitEnabled = false;
        }
        else{
            micBtn.GetComponent<UnityEngine.UI.Image>().sprite = microphoneOn;
            recorder.TransmitEnabled = true;
        }

        isMicrophoneEnabled = !isMicrophoneEnabled;
    }
}