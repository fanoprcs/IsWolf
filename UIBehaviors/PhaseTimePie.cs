using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseTimePie : MonoBehaviour
{
    [SerializeField] UnityEngine.Sprite DayImg;
    [SerializeField] UnityEngine.Sprite NightImg;
    [SerializeField] UnityEngine.Sprite VoteImg;
    public UnityEngine.UI.Text Date;
    public UnityEngine.UI.Text Phase;
    [SerializeField] GameObject timeBar;
    [SerializeField] GameObject phaseTime;
    [SerializeField] GameObject[] phaseTimeLattice;
    
    // Start is called before the first frame update
    private GameManager _gm;
    void Start()
    {
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    public IEnumerator DayNightTimeBar(int mode, float time, System.Action callback){
        foreach(GameObject o in phaseTimeLattice){
            o.SetActive(true);
        }
        if(mode == (int)GamePhase.day){
            phaseTime.GetComponent<UnityEngine.UI.Image>().sprite = DayImg;
        }
        else if(mode == (int)GamePhase.night){
            phaseTime.GetComponent<UnityEngine.UI.Image>().sprite = NightImg;
        }
        else{
            phaseTime.GetComponent<UnityEngine.UI.Image>().sprite = VoteImg;
        }
        float elapsedTime = time;
        int count = 5;
        while (elapsedTime > 0) {
            if(mode == (int)GamePhase.vote){
                if(_gm.playVoteAnimate){//當正在撥放投票動畫時，停止倒數
                    break;
                }
            }
            
            float progress = elapsedTime / time;
            if(progress < (count/6.0f)){
                phaseTimeLattice[5 - count].SetActive(false);
                count -= 1;
            }
            timeBar.transform.localScale = new Vector3(progress, timeBar.transform.localScale.y, timeBar.transform.localScale.z);;
            yield return null;
            elapsedTime -= Time.deltaTime;
        }
        phaseTimeLattice[5].SetActive(false);
        callback.Invoke();
    }
}
