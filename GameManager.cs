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
    [SerializeField] GameObject Vote;
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
    
    private NightMask _nm;
    public bool gameStart;
    public bool playVoteAnimate;
    private int voteCount = 0;
    private int[] voteSituation = new int[TotalPlayer];//9個玩家
    
    [SerializeField] GameObject InsideArea;//要設active，不然會名字還沒改動就偵測到，導致出錯
    [SerializeField] GameObject waitingUI;
    [SerializeField] GameObject progressBarBack;
    [SerializeField] GameObject progressBar;
    [SerializeField] GameObject dayTimeBarBack;
    [SerializeField] GameObject dayTimeBar;
    //Vote
    [SerializeField] GameObject VotePanel;
    [SerializeField] GameObject VoteBtn;
    [SerializeField] GameObject CloseVotePanelBtn;
    [SerializeField] GameObject SkipPanel;
    [SerializeField] GameObject []alreadyVoteIcon;
    //
    [SerializeField] UnityEngine.UI.Text Date;
    [SerializeField] UnityEngine.UI.Text chatRoom;
    private MusicPlayer musicManager;
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
        int[] allocateCareerTest = {(int)Careers.hunter, (int)Careers.wolf, (int)Careers.doctor, (int)Careers.hunter, (int)Careers.human
                                , (int)Careers.human, (int)Careers.wolf, (int)Careers.engineer, (int)Careers.wolf};
        GetComponent<PhotonView>().RPC("RpcInitCharacters", RpcTarget.All, allocateCareerTest, allocateSkin);
    }
    [PunRPC]
    void RpcInitCharacters(int[] allocateCareer, int[] allocateSkin){
        print("init");
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
        this.skin = 1;
        GameObject player;
        if(this.skin == 1){
            player = PhotonNetwork.Instantiate("Players/Player_1", new Vector3(spawnX, spawnY, 0), Quaternion.identity);
            
        }
        else if(this.skin == 2){
            player = PhotonNetwork.Instantiate("Players/Player_2", new Vector3(spawnX, spawnY, 0), Quaternion.identity);
            
        }
        else if(this.skin == 3){
            player = PhotonNetwork.Instantiate("Players/Player_3", new Vector3(spawnX, spawnY, 0), Quaternion.identity);
            
        }
        else if(this.skin == 4){
            player = PhotonNetwork.Instantiate("Players/Player_4", new Vector3(spawnX, spawnY, 0), Quaternion.identity);
            
        }
        else if(this.skin == 5){
            player = PhotonNetwork.Instantiate("Players/Player_5", new Vector3(spawnX, spawnY, 0), Quaternion.identity);

        }
        else if(this.skin == 6){
            player = PhotonNetwork.Instantiate("Players/Player_6", new Vector3(spawnX, spawnY, 0), Quaternion.identity);
            
        }
        else if(this.skin == 7){
            player = PhotonNetwork.Instantiate("Players/Player_7", new Vector3(spawnX, spawnY, 0), Quaternion.identity);
            
        }
        else if(this.skin == 8){
            player = PhotonNetwork.Instantiate("Players/Player_8", new Vector3(spawnX, spawnY, 0), Quaternion.identity);
            
        }
        else{
            player = PhotonNetwork.Instantiate("Players/Player_9", new Vector3(spawnX, spawnY, 0), Quaternion.identity);
        }
        int index = 0;
        foreach(GameObject pc in Computer){//每台電腦都要加到gamemanager的宣告中
            pc.GetComponent<ComputerBehavior>().enabled = true;
            pc.GetComponent<ComputerBehavior>().computerKey = index;
            if(playerMap[FindPlayerByKey(index+1)][1] == (int)Careers.engineer){
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
    
    void Update(){
        
        
    }
    [PunRPC]
    void RpcGameMode(int gameMode){
        mode = gameMode;
        if(mode == 0){//除了第一天白天接晚上之外，白天後面接討論環節
            float DayTime = 1f;
            StartCoroutine(DayNightTimeBar((int)GamePhase.day , DayTime, () =>{
                int nextMode;
                if(dayNightCount == 1)
                    nextMode = (int)GamePhase.night;
                else
                    nextMode = (int)GamePhase.vote;
                SwitchToNextMode(nextMode);
            }));
        }
        else if(mode == 1){//晚上後面接白天
            float NightTime = 1f;
            StartCoroutine(DayNightTimeBar((int)GamePhase.night , NightTime, () =>{  
                SwitchToNextMode((int)GamePhase.day);
            }));
        }
        else if(mode == 2){//投票後面接續晚上
            float VoteTime = 20f;
            StartCoroutine(DayNightTimeBar((int)GamePhase.vote , VoteTime, () =>{
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
            Date.text = "第一日";
        }
        else if(dayNightCount/3 == 1){
            Date.text = "第二日";
        }
        else if(dayNightCount/3 == 2){
            Date.text = "第三日";
        }
        else if(dayNightCount/3 == 3){
            Date.text = "第四日";
        }
        else{
            Date.text = "第五日";
        }
        if(modeStatus == 0){   
            Date.text += " 白天";        
            SetDayBehaviors();
        }
        else if(modeStatus == 1){
            Date.text += " 晚上";
            SetNightBehaviors();
        }
        else{
            Date.text += " 討論";
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
        print(this.playerCareer);
        if(this.playerCareer == Careers.engineer){//工程師白天能連線
            Engineer.GetComponent<EngineerBehaviors>().alreadyUsed = true;
        }
        else if(this.playerCareer == Careers.wolf){//狼人白天不能殺人
            Wolf.GetComponent<WolfBehaviors>().canKilled = false;
            Wolf.GetComponent<WolfBehaviors>().alreadyBreakDoor = true;
        }
    }
    private void SetNightBehaviors(){//晚上
        IsDayTime = false;
        
        if(this.playerCareer == Careers.engineer){//工程師重製晚上才能連線
            Engineer.GetComponent<EngineerBehaviors>().alreadyUsed = false;
        }
        else if(this.playerCareer == Careers.wolf){//狼人晚上才能殺人
            Wolf.GetComponent<WolfBehaviors>().canKilled = true;
            Wolf.GetComponent<WolfBehaviors>().alreadyBreakDoor = false;
        }
        /*播放晚上的配樂
        musicManager.GetComponent<AudioSource>().clip = nightBackgroundMusic;
        musicManager.GetComponent<AudioSource>().Play();
        */
        //為了晚上增添加一層幕，室外變黑，並且在房間外看不到室內s
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
        for(int i = 0; i < TotalPlayer; i++){//將是否以投票icon初始化
            alreadyVoteIcon[i].SetActive(false);
        }
        
        Vote.SetActive(true);
        Vote.GetComponent<VoteBehaviors>().alreadyVote = false;
        VoteBtn.SetActive(true);
        CloseVotePanelBtn.SetActive(true);
        SkipPanel.SetActive(false);
    }
    private IEnumerator PlayVote(){
        //撥放投票動畫時強制顯示Panel
        
        Vote.GetComponent<VoteBehaviors>().alreadyVote = true;//讓skip以及選擇玩家等功能失效
        if(!VotePanel.activeInHierarchy){
            Vote.GetComponent<VoteBehaviors>().showVotePanel();
        }
        CloseVotePanelBtn.SetActive(false);
        SkipPanel.SetActive(true);
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
                    GameObject showIcon = VotePanel.transform.Find("Player" + voteSituation[i]).
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
                    GameObject showIcon = VotePanel.transform.Find("SkipPanel").
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
        yield return new WaitForSeconds(5f);
        VotePanel.SetActive(false);
        Vote.SetActive(false);//投票完接晚上
        //結算
        int maxVotes = 0;
        List<int> maxIndices = new List<int>();
        for(int i = 0; i < TotalPlayer; i++){
            if(voteIconIndex[i] > maxVotes){
                maxVotes = voteIconIndex[i];
            }
        }
        for(int i = 0; i < TotalPlayer; i++){
            if(voteIconIndex[i] == maxVotes){
                maxIndices.Add(i);
            }
        }
        if(maxIndices.Count != 1){//表示本輪投票平局或是大家都放棄投票
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
        if(voteCount == alivePeopleCount()){//投票人數跟存活的人一樣的時候
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
    public void ccDead(int deadMode){
        RpcIsDead(FindPlayerByKey(PhotonNetwork.LocalPlayer.ActorNumber).NickName, deadMode);
    }
    [PunRPC]
    void RpcIsDead(string name, int causeOfDeath){
        GameObject playerObject = GameObject.Find(name + "(player)");
        playerMap[playerObject.GetComponent<PhotonView>().Owner][0] = 0;
        playerObject.GetComponent<Animator>().SetBool("dying", true);
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
        if(playerObject.GetComponent<PhotonView>().IsMine){
            SwitchBehaviors();
            SwitchToDeadMode();
            //PhotonNetwork.Destroy(playerObject);
        }
        if(PhotonNetwork.IsMasterClient && CheckGameOver()){//遊戲結束
            //CallRpcReloadGame();
        }
    }
    void SwitchToDeadMode(){
        //do something
    }
    bool CheckGameOver(){
        // 判斷遊戲是否結束
        return false;
    }
    int alivePeopleCount(){
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
    public void CallRpcDoorSwitchStatus(int doorKey, bool whetherOpen, bool isWolfBreak){
        GetComponent<PhotonView>().RPC("RpcDoorSwitchStatus", RpcTarget.All, doorKey, whetherOpen, isWolfBreak);
    }
    [PunRPC]public void RpcDoorSwitchStatus(int doorKey, bool whetherOpen, bool isWolfBreak){
        DoorBehaviors db = Door[doorKey].GetComponent<DoorBehaviors>();
        if(isWolfBreak){
            db.whetherOpen = whetherOpen;
            Door[doorKey].GetComponent<Animator>().SetBool("open", whetherOpen);
            if(whetherOpen == true){//如果開門的話，則不能夠查看了
                db.canCheck = false; 
            }
        }
        else if(!db.whetherLock){
            db.whetherOpen = whetherOpen;
            Door[doorKey].GetComponent<Animator>().SetBool("open", whetherOpen);
            if(whetherOpen == true){//如果開門的話，則不能夠查看了
                db.canCheck = false;
            }
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
        GetComponent<PhotonView>().RPC("RpcSwitchAnimatorNightmaskBool", RpcTarget.Others, playerName, status);
    }
    [PunRPC]
    public void RpcSwitchAnimatorNightmaskBool(string playerName, bool status){
        if(!IsDayTime)
            if(GameObject.Find(PhotonNetwork.LocalPlayer.NickName + "(player)").GetComponent<PlayerController>().canPeek){
                //print(PhotonNetwork.LocalPlayer.NickName + " can peek");
                //print(playerName + " GetComponent<Animator>" + status);
                GameObject.Find(playerName+ "(player)").GetComponent<Animator>().SetBool("night_mask", status);
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
    
    public IEnumerator GenerateProgressBar(float x, float y, float loadTime, bool isBreakDoor,Action callback){
        PlayerController pc = GameObject.Find(PhotonNetwork.LocalPlayer.NickName + "(player)").GetComponent<PlayerController>();
        pc.allowMovement = false;
        progressBarBack.transform.position = new Vector2(x-progressBarBack.GetComponent<RectTransform>().sizeDelta.x/2, y);
        progressBar.transform.position = new Vector2(x-progressBar.GetComponent<RectTransform>().sizeDelta.x/2, y);
        progressBarBack.SetActive(true);
        progressBar.SetActive(true);
        //初始化為0
        progressBar.transform.localScale = new Vector3(0f, progressBar.transform.localScale.y, progressBar.transform.localScale.z);;
        // 模擬加載資源的過程，5秒鐘
        if(isBreakDoor){
            //do some things
        }
        float elapsedTime = 0f;
        while (elapsedTime < loadTime) {
            // 計算進度條的值，範圍從 0 到 1
            float progress = elapsedTime / loadTime;

            // 更新進度條的值
            progressBar.transform.localScale = new Vector3(progress, progressBar.transform.localScale.y, progressBar.transform.localScale.z);;

            // 等待一幀
            yield return null;

            // 更新已經經過的時間
            elapsedTime += Time.deltaTime;
        }
        progressBarBack.SetActive(false);
        progressBar.SetActive(false);
        pc.allowMovement = true;
        callback.Invoke();
    }
    
    
    IEnumerator DayNightTimeBar(int mode, float time, Action callback){
        dayTimeBar.transform.localScale = new Vector3(0f, dayTimeBar.transform.localScale.y, dayTimeBar.transform.localScale.z);;
        float elapsedTime = time;
        while (elapsedTime > 0) {
            if(mode == (int)GamePhase.vote){
                if(playVoteAnimate){
                    break;
                }
            }
            float progress = elapsedTime / time;
            dayTimeBar.transform.localScale = new Vector3(progress, dayTimeBar.transform.localScale.y, dayTimeBar.transform.localScale.z);;
            yield return null;
            elapsedTime -= Time.deltaTime;
        }
        callback.Invoke();
    }
    [PunRPC]
    void RpcGameStart(){//確認所有人都進入再switch
        waitingUI.SetActive(false);
        SwitchBehaviors();
        musicManager = FindObjectOfType<MusicPlayer>();
        musicManager.GetComponent<AudioSource>().clip = musicManager.dayBackgroundMusic;
        musicManager.GetComponent<AudioSource>().Play();
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
