using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private Text text;
    [SerializeField] private GameObject Login;
    [SerializeField] private GameObject Acount;

    public PlayerLeaderboardEntry MyPlayFabInfo;
    public List<PlayerLeaderboardEntry> PlayFabUserList = new List<PlayerLeaderboardEntry>();
    public InputField EmailInput, PasswordInput, MEmailInput, MPasswordInput, MNickNameInput;

    public void Awake()
    {
        Screen.SetResolution(1280, 720, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
        Acount.gameObject.SetActive(false);
    }

    public void Btn()
    {
        var request = new LoginWithEmailAddressRequest { Email = EmailInput.text, Password = PasswordInput.text };
        PlayFabClientAPI.LoginWithEmailAddress(request, (result) =>
        { GetLeaderboard(result.PlayFabId); 
            PhotonNetwork.ConnectUsingSettings(); 
            text.text = "로그인 성공";
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Connect();
        }, (error) => text.text = "로그인 실패");
    }

    public void MakeAcountBtn() 
    {
        Acount.gameObject.SetActive(true);
    }

    public void ExitMA() 
    {
        Acount.gameObject.SetActive(false);
    }

    public void MABtn() 
    {
        var request = new RegisterPlayFabUserRequest { Email = MEmailInput.text, Password = MPasswordInput.text, Username =  MNickNameInput.text,  DisplayName = MNickNameInput.text };
        PlayFabClientAPI.RegisterPlayFabUser(request, (result) =>
        { text.text = "회원가입 성공";
            SetStat(); 
            SetData("default"); 
        }, (error) => text.text = "회원가입 실패");
    }

    void SetStat()
    {
        var request = new UpdatePlayerStatisticsRequest { Statistics = new List<StatisticUpdate> { new StatisticUpdate { StatisticName = "IDInfo", Value = 0 } } };
        PlayFabClientAPI.UpdatePlayerStatistics(request, (result) => { }, (error) => print("값 저장실패"));
    }

    void GetLeaderboard(string myID)
    {
        PlayFabUserList.Clear();

        for (int i = 0; i < 10; i++)
        {
            var request = new GetLeaderboardRequest
            {
                StartPosition = i * 100,
                StatisticName = "IDInfo",
                MaxResultsCount = 100,
                ProfileConstraints = new PlayerProfileViewConstraints() { ShowDisplayName = true }
            };
            PlayFabClientAPI.GetLeaderboard(request, (result) =>
            {
                if (result.Leaderboard.Count == 0) return;
                for (int j = 0; j < result.Leaderboard.Count; j++)
                {
                    PlayFabUserList.Add(result.Leaderboard[j]);
                    if (result.Leaderboard[j].PlayFabId == myID) MyPlayFabInfo = result.Leaderboard[j];
                }
            },
            (error) => { });
        }
    }

    void SetData(string curData)
    {
        var request = new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>() { { "Home", curData } },
            Permission = UserDataPermission.Public
        };
        PlayFabClientAPI.UpdateUserData(request, (result) => { }, (error) => print("데이터 저장 실패"));
    }

    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = MyPlayFabInfo.DisplayName;
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 2 }, null);
    }

    public override void OnJoinedRoom()
    {
        Login.gameObject.SetActive(false);
        Spawn();
    }

    public void Spawn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate("GameCharacter1", new Vector3(0.5471144f, 4.295326f, 117.64f), Quaternion.identity);
        }
        else
        {
            PhotonNetwork.Instantiate("GameCharacter2", new Vector3(42.7f, 4.295326f, 117.64f), Quaternion.identity);
        }
    }
}
