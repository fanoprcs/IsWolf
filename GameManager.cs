using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Linq;
public enum Careers
{
    wolf, human, hunter, doctor, engineer
}
public enum GamePhase
{
    day, night, vote
}
public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject Human;
    [SerializeField] GameObject Wolf;
    [SerializeField] GameObject Doctor;
    [SerializeField] GameObject Engineer;
    [SerializeField] GameObject Hunter;
    
    [SerializeField] GameObject []Computer;
    [SerializeField] GameObject []Door;
    [SerializeField] UnityEngine.Sprite[] SkinIcon;//可以根據playerMap來對應
    static int TotalPlayer = 9;
    public Dictionary<Player, List<int>> playerMap = new Dictionary<Player, List<int>>();//first: alive, second: career, third: skin
    private int playerKey;
    public Careers playerCareer;
    private int skin;//1~9
    public bool IsDayTime;
    private int mode;//白天，晚上，投票
    private int dayNightCount;
    public float DayTime = 5f;
    public float NightTime = 20f;
    public float VoteTime = 5f;
    public bool gameStart;
    public bool playVoteAnimate;
    private int voteCount = 0;
    private int[] voteSituation = new int[TotalPlayer];//9個玩家
    
    [SerializeField] GameObject microphone;
    [SerializeField] GameObject InsideArea;//要設active，不然會名字還沒改動就偵測到，導致出錯
    [SerializeField] GameObject waitingUI;
    [SerializeField] GameObject progressBarBack;
    [SerializeField] GameObject progressBar;
    
    //Vote
    [SerializeField] GameObject Vote;
    [SerializeField] GameObject VoteUI;
    private VoteBehaviors _vb;
    [SerializeField] GameObject []alreadyVoteIcon;
    //
    
    private MusicPlayer musicManager;
    private NightMask _nm;
    private PhaseTimePie _ptp;
    private IEnumerator WaitForAllPlayersLoadScene()
    {
        bool allPlayersLoadedScene = false;
        while (!allPlayersLoadedScene)
        {
            allPlayersLoadedScene = true;
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if ((string)player.CustomProperties["scene"] != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
                {
                    allPlayersLoadedScene = false;
                    break;
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
        StartGame();
    }
    private IEnumerator WaitForAllPlayersInstantiate()//確認是否所有玩家都已經出現了
    {
        bool allPlayersInstantiate = false;

        while (!allPlayersInstantiate)
        {
            GameObject[] _players;
            _players = GameObject.FindGameObjectsWithTag("Player")
                    .Where(go => go.name.Contains("(player)"))
                    .ToArray();
            if(_players != null){
                if(_players.Length == PhotonNetwork.CurrentRoom.PlayerCount){
                    allPlayersInstantiate = true;
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
        photonView.RPC("RpcGameStart", RpcTarget.All);
        photonView.RPC("RpcGameMode", RpcTarget.All, 0);
    }
    void Start(){
        if(PhotonNetwork.CurrentRoom == null){
            UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
        }
        else{
            waitingUI.SetActive(true);
            ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
            customProperties.Add("scene", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
            _nm = GameObject.Find("Mask").GetComponent<NightMask>();
            _ptp = GameObject.Find("DayNightPie").GetComponent<PhaseTimePie>();
            mode = -1;
            dayNightCount = 0;
            SwitchDayNight(0);
            gameStart = false;
            if(PhotonNetwork.IsMasterClient)
            {   
                StartCoroutine(WaitForAllPlayersLoadScene());
            }
        }
    }
    void StartGame(){
        Init();
        StartCoroutine(WaitForAllPlayersInstantiate());
    }
    private void Init(){
        CallRpcInitCharacters();
    }
    void CallRpcInitCharacters(){
        int[] allocateCareer = {(int)Careers.wolf, (int)Careers.wolf, (int)Careers.wolf, (int)Careers.human, (int)Careers.human
                                , (int)Careers.human, (int)Careers.hunter, (int)Careers.doctor, (int)Careers.engineer};
        int[] allocateSkin = {1, 2, 3, 4, 5, 6, 7, 8, 9};
        System.Random random = new System.Random();
        allocateCareer = allocateCareer.OrderBy(x => random.Next()).ToArray();
        allocateSkin = allocateSkin.OrderBy(x => random.Next()).ToArray();
        //test
        int[] allocateCareerTest = {(int)Careers.wolf, (int)Careers.wolf, (int)Careers.doctor, (int)Careers.hunter, (int)Careers.human
                                , (int)Careers.engineer, (int)Careers.hunter, (int)Careers.engineer, (int)Careers.wolf};
        GetComponent<PhotonView>().RPC("RpcInitCharacters", RpcTarget.All, allocateCareerTest, allocateSkin);
    }
    [PunRPC]
    void RpcInitCharacters(int[] allocateCareer, int[] allocateSkin){
        print("init character");
        foreach(var kvp in PhotonNetwork.CurrentRoom.Players){//每個玩家的kvp 順序不同，因此要利用kvp.Value.ActorNumber分配
            print(kvp.Value.NickName);
            playerMap[kvp.Value] = new List<int>();
            playerMap[kvp.Value].Add(1);//alive
            if(PhotonNetwork.LocalPlayer == kvp.Value){
                this.playerKey = kvp.Value.ActorNumber;
                this.playerCareer = (Careers)allocateCareer[kvp.Value.ActorNumber-1];
                this.skin = allocateSkin[kvp.Value.ActorNumber-1];
            }
            playerMap[kvp.Value].Add(allocateCareer[kvp.Value.ActorNumber-1]);//career
            playerMap[kvp.Value].Add(allocateSkin[kvp.Value.ActorNumber-1]);//skin
            print(kvp.Value.NickName + " is a " + (Careers)playerMap[kvp.Value][1] + 
            " with skin " + playerMap[kvp.Value][2] + ", the key is" + kvp.Value.ActorNumber);
        }
        Generate();

    }
    void Generate(){
        float spawnX = this.playerKey;
        float spawnY = -3;/*UnityEngine.Random.Range(-3, 3);*/
        PhotonNetwork.Instantiate("Players/Player_" + skin.ToString(), new Vector3(spawnX, spawnY, 0), Quaternion.identity);  
        int index = 0;
        foreach(GameObject pc in Computer){//每台電腦都要加到gamemanager的宣告中
            pc.GetComponent<ComputerBehavior>().enabled = true;
            pc.GetComponent<ComputerBehavior>().computerKey = index;
            if(playerMap[FindPlayerByKey(index+1)][1] == (int)Careers.engineer){//所有玩家都需要知道工程師的電腦是哪台
                pc.GetComponent<ComputerBehavior>().belongsToEngineer = true;
            }
            index++;
        }
        index = 0;
        foreach(GameObject door in Door){//每個門都要加到gameManager的宣告中
            door.GetComponent<DoorBehaviors>().doorKey = index;
            index++;
        }      
    }
    
    [PunRPC]
    void RpcGameMode(int gameMode){
        mode = gameMode;
        if(mode == 0){//除了第一天白天接晚上之外，白天後面接討論環節
            print("----進入白天----");
            StartCoroutine(_ptp.DayNightTimeBar((int)GamePhase.day , DayTime, () =>{
                int nextMode;
                if(dayNightCount == 1)
                    nextMode = (int)GamePhase.night;
                else
                    nextMode = (int)GamePhase.vote;
                SwitchToNextMode(nextMode);
            }));
        }
        else if(mode == 1){//晚上後面接白天
            print("----進入晚上----");
            StartCoroutine(_ptp.DayNightTimeBar((int)GamePhase.night , NightTime, () =>{  
                SwitchToNextMode((int)GamePhase.day);
            }));
        }
        else if(mode == 2){//投票後面接續晚上
            print("----投票階段----");
            StartCoroutine(_ptp.DayNightTimeBar((int)GamePhase.vote , VoteTime, () =>{
                //先撥放動畫等，再switch
                if(!playVoteAnimate){//如果有玩家還沒有投票且時間到的話進入這裡
                    playVoteAnimate = true;
                    StartCoroutine(PlayVote());
                }
            }));
        }
    }
    void SwitchToNextMode(int nextMode){
        SwitchDayNight(nextMode);
        if(PhotonNetwork.IsMasterClient)
            photonView.RPC("RpcGameMode", RpcTarget.All, nextMode);
    }
    void SwitchDayNight(int modeStatus){//記得換一天時，電腦要重製mode
        dayNightCount++;
        if(dayNightCount/3 == 0){
            _ptp.Date.text = "第一日";
        }
        else if(dayNightCount/3 == 1){
            _ptp.Date.text = "第二日";
        }
        else if(dayNightCount/3 == 2){
            _ptp.Date.text = "第三日";
        }
        else if(dayNightCount/3 == 3){
            _ptp.Date.text = "第四日";
        }
        else{
            _ptp.Date.text = "第五日";
        }
        if(modeStatus == 0){   
            _ptp.Phase.text = "白天"; 
            SetDayBehaviors();
        }
        else if(modeStatus == 1){
            _ptp.Phase.text = "晚上";
            SetNightBehaviors();
        }
        else{
            _ptp.Phase.text = "討論";
            SetVoteBehaviors();
        }  
    }
    private void SetDayBehaviors(){//白天
        IsDayTime = true;
        if(gameStart)
            _nm.SwitchToDay();//夜晚遮罩模式的切換
        foreach(GameObject pc in Computer){//電腦要重製mode
            pc.GetComponent<ComputerBehavior>().mode = 0;
            pc.GetComponent<ComputerBehavior>().skinMode = 1;
        }
        if(this.playerCareer == Careers.engineer){//工程師白天不能連線
            Engineer.GetComponent<EngineerBehaviors>().alreadyUsed = true;
        }
        else if(this.playerCareer == Careers.wolf){//狼人白天不能殺人
            Wolf.GetComponent<WolfBehaviors>().canKilled = false;
            Wolf.GetComponent<WolfBehaviors>().alreadyBreak = true;
        }
        if(this.playerCareer == Careers.wolf){//狼人白天不能講話
            MicrophoneManager _mc = microphone.GetComponent<MicrophoneManager>();
            _mc.wolfMicBtn.GetComponent<UnityEngine.UI.Button>().interactable = false;
            
            if(_mc.isMicrophoneEnabled)//如果正在啟用就關掉
                _mc.ToggleMicrophone(_mc.wolfMicBtn);
        }
        if(dayNightCount != 1){//第一天以外才要重設(因為第一天還在等待中不應該播放音樂，之後晚上會換音樂因此要重設)
            if(musicManager.GetComponent<AudioSource>().clip != musicManager.dayBackgroundMusic){//此處之後如果晚上有音樂要卡掉，目前這樣寫是為了不要重新播放音樂
                musicManager.GetComponent<AudioSource>().clip = musicManager.dayBackgroundMusic;
                musicManager.GetComponent<AudioSource>().Play();
            }
        }
    
    }
    private void SetNightBehaviors(){//晚上
        IsDayTime = false;
        
        if(this.playerCareer == Careers.engineer){//工程師晚上才能連線
            Engineer.GetComponent<EngineerBehaviors>().alreadyUsed = false;
        }
        else if(this.playerCareer == Careers.wolf){//晚上狼人
            Wolf.GetComponent<WolfBehaviors>().canKilled = true;//晚上才能殺人
            Wolf.GetComponent<WolfBehaviors>().alreadyBreak = false;//重置是否可以破門
        }
        
        if(this.playerCareer == Careers.wolf){//晚上狼人可以使用麥克風
            microphone.GetComponent<MicrophoneManager>().wolfMicBtn.GetComponent<UnityEngine.UI.Button>().interactable = true;
        }
        else{//晚上，投票階段後，狼人以外的都不能聽到聲音
            microphone.GetComponent<MicrophoneManager>().speaker.gameObject.SetActive(false);
        }
        foreach(var kvp in PhotonNetwork.CurrentRoom.Players){//重設一次裡面或是外面
            if(PhotonNetwork.LocalPlayer != kvp.Value && playerMap[kvp.Value][0] == 1){//如果還活著且不是自己
                GameObject player = GameObject.Find(kvp.Value.NickName + "(player)");
                if(player.GetComponent<PlayerController>().canRing){//如果我在裡面且其他玩家在外面
                    player.GetComponent<Animator>().SetBool("night_mask", true);
                    GameObject.Find(kvp.Value.NickName + "(player)").transform.Find("Canvas")
                    .transform.Find("Name").gameObject.GetComponent<UnityEngine.UI.Text>().text = "";
                }
            }
        }
        /*播放晚上的配樂
        musicManager.GetComponent<AudioSource>().clip = musicManager.nightBackgroundMusic;
        musicManager.GetComponent<AudioSource>().Play();
        */
        //為了晚上增添加一層幕，室外變黑，並且在房間外看不到室內
        //如果角色在房間裡，則把所有不在室內的玩家動畫變成夜晚型態，如果角色離開房間，則把所有的玩家的動畫的bool night 調整回false
    }
    private void SetVoteBehaviors(){
        voteCount = 0;
        playVoteAnimate = false;
        
        foreach(var kvp in PhotonNetwork.CurrentRoom.Players){//傳送玩家
            if(PhotonNetwork.LocalPlayer == kvp.Value && playerMap[kvp.Value][0] == 1){//如果還活著就傳送位置
                //GameObject.Find(kvp.Value.NickName + "(player)").GetComponent<Transform>().position = new Vector2(1f, 1f);
            }
        }
        for(int i = 0; i < TotalPlayer; i++){//將投票情形初始化為-1
            voteSituation[i] = -1;//玩家對應到1~9
        }
        for(int i = 0; i < TotalPlayer; i++){//將是否已經投票的icon初始化
            alreadyVoteIcon[i].SetActive(false);
        }
        foreach(GameObject door in Door){//每個門都要加到gameManager的宣告中
            door.GetComponent<DoorBehaviors>().whetherLock = false;
        } 
        /*
            musicManager.GetComponent<AudioSource>().clip = musicManager.dayBackgroundMusic;
            musicManager.GetComponent<AudioSource>().Play();
        */
        microphone.GetComponent<MicrophoneManager>().speaker.gameObject.SetActive(true);//投票階段所有人偷要聽到聲音
        VoteUI.SetActive(true);
        _vb.alreadySelect = false;
        _vb.VoteBtn.SetActive(true);
        _vb.CloseVotePanelBtn.SetActive(true);
        _vb.SkipPanel.SetActive(false);
    }
    private IEnumerator PlayVote(){
        //撥放投票動畫時強制顯示Panel
        
        _vb.alreadySelect = true;//讓skip以及選擇玩家等功能失效
        if(!_vb.VotePanel.activeInHierarchy){
            _vb.ShowVotePanel();
        }
        _vb.CloseVotePanelBtn.SetActive(false);
        _vb.CheckVoteBtn.SetActive(false);
        _vb.ResetSelectedBtn.SetActive(false);
        _vb.ChatPanel.SetActive(false);
        _vb.SkipPanel.SetActive(true);
        int []voteIconIndex = new int [TotalPlayer];
        int skipIconIndex = 0;
        List<GameObject> storeShowIconList = new List<GameObject>();//為了將Icon重置
        yield return new WaitForSeconds(0.5f);
        for(int i = 0; i < TotalPlayer; i++){
            Player tmpPlayer = FindPlayerByKey(i+1);
            if(tmpPlayer != null){
                if(voteSituation[i] != -1){//表示該名玩家有投票
                    voteIconIndex[voteSituation[i]-1]++;
                    //將對應的vote面板更改
                    GameObject showIcon = _vb.VotePanel.transform.Find("Player" + voteSituation[i]).
                                                        transform.Find("P" + voteSituation[i] + "_IMG").
                                                        transform.Find("Icon").
                                                        transform.Find("Icon_" + voteIconIndex[voteSituation[i]-1]).gameObject;
                    storeShowIconList.Add(showIcon);//將有變化的icon物件儲存以便重置
                    UnityEngine.UI.Image showImg = showIcon.GetComponent<UnityEngine.UI.Image>();
                    showImg.sprite = SkinIcon[playerMap[tmpPlayer][2]-1];
                    Color color = showImg.color;
                    color.a = 1f;
                    showImg.color = color;
                    Vote.GetComponent<AudioSource>().clip = musicManager.audio_settle;
                    Vote.GetComponent<AudioSource>().Play();
                    yield return new WaitForSeconds(0.8f);
                }
                else{//表示沒投票或著是skip
                    skipIconIndex++;
                    GameObject showIcon = _vb.VotePanel.transform.Find("SkipPanel").
                                                        transform.Find("Icon").
                                                        transform.Find("Icon_" + skipIconIndex).gameObject;
                    storeShowIconList.Add(showIcon);//將有變化的icon物件儲存以便重置
                    UnityEngine.UI.Image showImg = showIcon.GetComponent<UnityEngine.UI.Image>();
                    if(playerMap[tmpPlayer][0] == 1){//還活著的話才有投票權，要顯示在skip裡面
                        showImg.sprite = SkinIcon[playerMap[tmpPlayer][2]-1];
                        Color color = showImg.color;
                        color.a = 1f;
                        showImg.color = color;
                        Vote.GetComponent<AudioSource>().clip = musicManager.audio_settle;
                        Vote.GetComponent<AudioSource>().Play();
                        yield return new WaitForSeconds(0.8f);
                    }
                }
                
            }
            //撥放投票音效
            
        }
        yield return new WaitForSeconds(5f);//給大家五秒鐘的時間看投票結果
        _vb.CloseVotePanel();
        VoteUI.SetActive(false);//投票完接晚上
        //結算
        List<int> maxIndices = new List<int>();
        int maxVoteCounts = voteIconIndex.Max();//得到該輪投票的最大值
        for(int i = 0; i < TotalPlayer; i++){
            if(voteIconIndex[i] == maxVoteCounts){
                maxIndices.Add(i);
            }
        }
        if(maxIndices.Count != 1){//表示本輪投票有平局或是大家都放棄投票
            print("no player leave");
        }
        else{
            CallRpcIsDead(FindPlayerByKey(maxIndices[0]+1).NickName, 2);
            
            //do somethings
        }
        //把人踢走的畫面
        //動畫撥放完重置
        for (int i = 0; i < storeShowIconList.Count; i++)
        {
            print("reset");
            UnityEngine.UI.Image showImg = storeShowIconList[i].GetComponent<UnityEngine.UI.Image>();
            Color color = showImg.color;
            color.a = 0f;
            showImg.color = color;
        }
        MicrophoneManager _mc = microphone.GetComponent<MicrophoneManager>();
        if(_mc.isMicrophoneEnabled)//如果正在啟用麥克風就關掉
            Vote.GetComponent<VoteBehaviors>().SwitchMicrophoneChatMode(microphone.GetComponent<MicrophoneManager>().voteMicBtn);
        //切換到晚上
        SwitchToNextMode((int)GamePhase.night);
    }
    public void CallRpcAlreadyVote(int key, int senderKey){
        GetComponent<PhotonView>().RPC("RpcAlreadyVote", RpcTarget.All, key, senderKey);
    }
    [PunRPC]
    public void RpcAlreadyVote(int key, int senderKey){//key = 1~9
        voteCount++;
        voteSituation[senderKey-1] = key;
        //所有人要顯示sender已打勾，以及投票聲音等
        print("vote");
        alreadyVoteIcon[senderKey-1].SetActive(true);
        Vote.GetComponent<AudioSource>().clip = musicManager.audio_checkVote;
        Vote.GetComponent<AudioSource>().Play();
        if(voteCount == AlivePeopleCount()){//投票人數跟存活的人一樣的時候
            playVoteAnimate = true;
            StartCoroutine(PlayVote());
        }

    }
    void SwitchBehaviors(){
        if(this.playerCareer == Careers.human){
            if(Human.activeInHierarchy)
                Human.SetActive(false);
            else
                Human.SetActive(true);
        }
        else if(this.playerCareer == Careers.wolf){
            if(Wolf.activeInHierarchy)
                Wolf.SetActive(false);
            else
                Wolf.SetActive(true);
        }
        else if(this.playerCareer == Careers.doctor){
            if(Doctor.activeInHierarchy)
                Doctor.SetActive(false);
            else
                Doctor.SetActive(true);
        }
        else if(this.playerCareer == Careers.engineer){
            if(Engineer.activeInHierarchy)
                Engineer.SetActive(false);
            else
                Engineer.SetActive(true);
        }
        else{
            if(Hunter.activeInHierarchy)
                Hunter.SetActive(false);
            else
                Hunter.SetActive(true);
        }
    }
    public override void OnPlayerEnteredRoom(Player player){
        playerMap[player][0] = 1;
    }
    public override void OnPlayerLeftRoom(Player player){
        if(playerMap.ContainsKey(player)){
            playerMap[player][0] = 0;
        }
        if(PhotonNetwork.CurrentRoom.PlayerCount<=1){
            PhotonNetwork.LeaveRoom();
        }
    }
    public override void OnLeftRoom(){
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }
    public void CallRpcIsDead(string name, int causeOfDeath){
        GetComponent<PhotonView>().RPC("RpcIsDead", RpcTarget.All, name, causeOfDeath);
    }
   
    [PunRPC]
    void RpcIsDead(string name, int causeOfDeath){
        GameObject playerObject = GameObject.Find(name + "(player)");
        if(playerMap[playerObject.GetComponent<PhotonView>().Owner][0] == 1){
            playerMap[playerObject.GetComponent<PhotonView>().Owner][0] = 0;
            
            if(causeOfDeath == 0){//被狼殺死
                playerObject.GetComponent<AudioSource>().clip = musicManager.audio_splatter;
                playerObject.GetComponent<AudioSource>().Play();
            }
            else if(causeOfDeath == 1){//被獵人殺死
                //playerObject.GetComponent<AudioSource>().clip = musicManager.audio_splatter;
                //playerObject.GetComponent<AudioSource>().Play();
            }
            else if(causeOfDeath == 2){//被投票殺死

            }
            playerObject.GetComponent<Animator>().SetBool("dying", true);
            if(playerObject.GetComponent<PhotonView>().IsMine){
                SwitchBehaviors();
                SwitchToDeadMode();
                //PhotonNetwork.Destroy(playerObject);
            }
            if(PhotonNetwork.IsMasterClient && CheckGameOver()){//遊戲結束
                //CallRpcReloadGame();
            }
        }
    }
    void SwitchToDeadMode(){
        //do something
    }
    bool CheckGameOver(){
        // 判斷遊戲是否結束
        return false;
    }
    int AlivePeopleCount(){
        int aliveCount = 0;
        foreach(var kvp in playerMap){
            if(kvp.Value[0] == 1) aliveCount++;
        }
        return aliveCount;
    }
    void CallRpcReloadGame(){
        GetComponent<PhotonView>().RPC("RpcReloadGame", RpcTarget.All);
    }
    [PunRPC]
    void RpcReloadGame(){
        UnityEngine.SceneManagement.SceneManager.LoadScene("MapScene");
        PhotonNetwork.LoadLevel("MapScene");
    }
    public void CallRpcEngineerConnected(int key, int engineerKey){//讓工程師和玩家的電腦連上線
        GetComponent<PhotonView>().RPC("RpcEngineerConnected", RpcTarget.All, key, engineerKey);
    }

    [PunRPC]
    void RpcEngineerConnected(int key, int engineerKey){
        ComputerBehavior clientComputer = Computer[key-1].GetComponent<ComputerBehavior>();
        ComputerBehavior engineerComputer = Computer[engineerKey-1].GetComponent<ComputerBehavior>();
        clientComputer.mode = 1;
        engineerComputer.mode = 1;
        clientComputer.connectedKey = engineerKey-1;
        engineerComputer.connectedKey = key-1;
    }
    
    public void CallRpcSendMsg(string msg, int computerKey, int senderComputerKey){
        GetComponent<PhotonView>().RPC("RpcSendMsg", RpcTarget.All, msg, computerKey, senderComputerKey);
    }
    [PunRPC]public void RpcSendMsg(string msg, int computerKey, int senderComputerKey){
        ComputerBehavior Pcb = Computer[computerKey].GetComponent<ComputerBehavior>();
        ComputerBehavior PcbSender = Computer[senderComputerKey].GetComponent<ComputerBehavior>();
        Pcb.skinMode = 3;
        PcbSender.chatRoom.text += "\n " + "<color=#" + ColorUtility.ToHtmlStringRGB(Color.blue) + ">" + "Me: "+ msg + "</color>";
        if(Pcb.belongsToEngineer)//如果收到訊息的電腦是工程師的電腦則會顯示名字
            Pcb.chatRoom.text += "\n " + FindPlayerByKey(senderComputerKey+1).NickName + ": " + msg;
        else
            Pcb.chatRoom.text += "\n engineer: " + msg;
    }
    public void CallRpcPcSwitchStatus(int computerKey, int computerSkin){
        GetComponent<PhotonView>().RPC("RpcPcSwitchStatus", RpcTarget.All, computerKey, computerSkin);
    }
    [PunRPC]public void RpcPcSwitchStatus(int computerKey, int computerSkin){
        Computer[computerKey].GetComponent<ComputerBehavior>().skinMode = computerSkin;
    }
    public void CallRpcDoorSwitchStatus(int doorKey, bool whetherOpen){
        GetComponent<PhotonView>().RPC("RpcDoorSwitchStatus", RpcTarget.All, doorKey, whetherOpen);
    }
    [PunRPC]public void RpcDoorSwitchStatus(int doorKey, bool whetherOpen){
        DoorBehaviors db = Door[doorKey].GetComponent<DoorBehaviors>();
        
        db.whetherOpen = whetherOpen;
        Door[doorKey].GetComponent<Animator>().SetBool("open", whetherOpen);
        if(whetherOpen == true){//如果開門的話，則不能夠查看了
            db.canCheck = false; 
        }
        
        
    }
    public void CallRpcDoorSwitchLock(int doorKey, bool whetherLock){
        GetComponent<PhotonView>().RPC("RpcDoorSwitchLock", RpcTarget.All, doorKey, whetherLock);
    }
    [PunRPC]public void RpcDoorSwitchLock(int doorKey, bool whetherLock){
        if(!Door[doorKey].GetComponent<DoorBehaviors>().whetherOpen)
            Door[doorKey].GetComponent<DoorBehaviors>().whetherLock = whetherLock;
    }
    
    public void CallRpcRingTheBell(int doorKey, int playerKey){
        GetComponent<PhotonView>().RPC("RpcRingTheBell", RpcTarget.All, doorKey, playerKey);
    }
    [PunRPC]
    public void RpcRingTheBell(int doorKey, int playerKey){
        DoorBehaviors db = Door[doorKey].GetComponent<DoorBehaviors>();
        Door[doorKey].GetComponent<AudioSource>().clip = musicManager.audio_doorBell;
        Door[doorKey].GetComponent<AudioSource>().Play();
        db.canCheck = true;
        db.CheckPlayerId = playerKey;
    }
    public void CallRpcSwitchAnimatorNightmaskBool(string playerName, bool status){
        GetComponent<PhotonView>().RPC("RpcSwitchAnimatorNightmaskBool", RpcTarget.Others, playerName, status);//自己以外的呼叫
    }
    [PunRPC]
    public void RpcSwitchAnimatorNightmaskBool(string playerName, bool status){//status表示其他玩家移動到外面還裡面
        if(!IsDayTime)
            if(GameObject.Find(PhotonNetwork.LocalPlayer.NickName + "(player)").GetComponent<PlayerController>().canPeek){//本地玩家在室內的話
                GameObject.Find(playerName+ "(player)").GetComponent<Animator>().SetBool("night_mask", status);
                GameObject.Find(playerName + "(player)").transform.Find("Canvas")
                    .transform.Find("Name").GetComponent<UnityEngine.UI.Text>().text = (status == true) ? "" : playerName;
            }
    }
    public void CallRpcSwitchArea(string playerName, bool status){
        GetComponent<PhotonView>().RPC("RpcSwitchArea", RpcTarget.Others, playerName, status);
    }
    [PunRPC]
    public void RpcSwitchArea(string playerName, bool status){
        //這個參數被用來判斷玩家在裡面還是外面
        print(playerName + " canring: " + status);
        GameObject.Find(playerName + "(player)").GetComponent<PlayerController>().canRing = status;
    }
    
    public IEnumerator GenerateProgressBar(float x, float y, float loadTime, bool isBreakDoor, int doorKey,Action callback){
        PlayerController pc = GameObject.Find(PhotonNetwork.LocalPlayer.NickName + "(player)").GetComponent<PlayerController>();
        pc.allowMovement = false;//例如鎖門解鎖、狼人破門、醫生檢查，都無法移動
        progressBarBack.transform.position = new Vector2(x-progressBarBack.GetComponent<RectTransform>().sizeDelta.x/2, y);
        progressBar.transform.position = new Vector2(x-progressBar.GetComponent<RectTransform>().sizeDelta.x/2, y);
        progressBarBack.SetActive(true);
        progressBar.SetActive(true);
        //初始化為0
        progressBar.transform.localScale = new Vector3(0f, progressBar.transform.localScale.y, progressBar.transform.localScale.z);;
        // 模擬加載資源的過程，5秒鐘
        
        float elapsedTime = 0f;
        while (elapsedTime < loadTime) {
            // 計算進度條的值，範圍從 0 到 1
            float progress = elapsedTime / loadTime;

            // 更新進度條的值
            progressBar.transform.localScale = new Vector3(progress, progressBar.transform.localScale.y, progressBar.transform.localScale.z);;

            // 等待一幀
            yield return null;
            if(isBreakDoor){
                //do some things
                DoorBehaviors _db = Door[doorKey].GetComponent<DoorBehaviors>();
                if(IsDayTime || _db.whetherOpen){//如果破門到一半變成白天或是門被打開
                    _db.successBreakDoor = false;
                    break;
                }
            }
            // 更新已經經過的時間
            elapsedTime += Time.deltaTime;
        }
        progressBarBack.SetActive(false);
        progressBar.SetActive(false);
        pc.allowMovement = true;
        callback.Invoke();
    }
    
    
    
    [PunRPC]
    void RpcGameStart(){//確認所有人都進入再switch
        waitingUI.SetActive(false);
        SwitchBehaviors();
        musicManager = FindObjectOfType<MusicPlayer>();
        musicManager.GetComponent<AudioSource>().clip = musicManager.dayBackgroundMusic;
        musicManager.GetComponent<AudioSource>().Play();
        Vote.SetActive(true);//這邊就要設定true是要進入script觸發start，為了讓所有人在投票panel中都是以正面顯示
        _vb = Vote.GetComponent<VoteBehaviors>();
        GameObject.Find("NotebookUI").GetComponent<NoteBook>().enabled = true;
        microphone.SetActive(true);
        InsideArea.SetActive(true);//開始需要辨別玩家位置
        gameStart = true;
    }
    public void CallRpcPlayAudioOnPlayer(string playerName, int audioCode){
         GetComponent<PhotonView>().RPC("RpcPlayAudioOnPlayer", RpcTarget.All, playerName, audioCode);
    }
    [PunRPC]
    private void RpcPlayAudioOnPlayer(string playerName, int audioCode)
    {
        print("play");
        AudioSource ap = GameObject.Find(playerName + "(player)").GetComponent<AudioSource>();
        ap.clip = musicManager.getAudioSound(audioCode);
        ap.Play();
    }
    
    public Player FindPlayerByNickname(string nickname)
    {
        return PhotonNetwork.PlayerList.FirstOrDefault(player => player.NickName == nickname);
    }
    public Photon.Realtime.Player FindPlayerByKey(int key)//1~9
    {
        Player player = null;
        foreach(var kvp in PhotonNetwork.CurrentRoom.Players){
            if(kvp.Value.ActorNumber == key){
                player = kvp.Value;
                break;
            }
        }
        return player;
    }
    public int FindKeyByPlayer(Player player)
    {
        int key = 0;
        foreach(var kvp in PhotonNetwork.CurrentRoom.Players){
            if(kvp.Value == player){
                key = kvp.Value.ActorNumber;
                break;
            }
        }
        return key;
    }
    public int FindKeyByNickName(string nickname)
    {
       return PhotonNetwork.PlayerList.FirstOrDefault(player => player.NickName == nickname).ActorNumber;
    }
 
}
