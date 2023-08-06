using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer instance;
    /*這邊存的音效是所有人可以聽到的*/
    //player
    public AudioClip audio_splatter;//玩家被狼殺死噴血音效
    //door
    public AudioClip audio_doorBell;
    //bgm
    public AudioClip dayBackgroundMusic;
    public AudioClip nightBackgroundMusic;
    //vote
    public AudioClip audio_checkVote;//進行投票動作時的音效
    public AudioClip audio_settle;//依序顯示投票結果時的音效
    public AudioClip audio_resultAnimate;//結算畫面音效
    //hunter
    public AudioClip audio_shoot;//獵人射擊音效
    void Awake()
    {
        if(instance == null){
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else{
            Destroy(this.gameObject);
        }
    }
    public AudioClip getAudioSound(int audioCode){
        if(audioCode == 0){
            return audio_shoot;
        }
    


        return null;
    }
}
