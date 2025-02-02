using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField IDInput;      //사용자 아이디 입력받는 input field
    [SerializeField] GameObject NormalComponent;  //평상시 보이는 요소들 (로그인 필드, 버튼 ..)
    [SerializeField] GameObject LoadingComponent; //로딩중에 보여줄 글씨

    public string PlayerID { get; set; }
    public JBUS_Player LoginPlayer { get; set; }

    const string ROOM_NAME = "JBNU";
    const string GAME_SENCE = "Game";
    const string JOIN_SCENE = "Join";
    const string LOGIN_URL = "https://jbus.herokuapp.com/user/login"; //로그인 요청을 보낼 url

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        PlayerID = "";
        LoginPlayer = null;
    }

    //Join 버튼을 누르면 해당 scene를 불러와줌
    public void OnJoin()
    {
        SceneManager.LoadScene(JOIN_SCENE);
    }

    //Login 버튼을 누르면 시도
    public void OnLogin()
    {
        PlayerID = IDInput.text;
        if (string.IsNullOrEmpty(PlayerID)) return; //유효성 검사

        DisplayLoading();
        StartCoroutine(Login_REST());
    }

    void DisplayLoading()
    {
        NormalComponent.SetActive(false);
        LoadingComponent.SetActive(true);
    }

    void DisplayNormal()
    {
        NormalComponent.SetActive(true);
        LoadingComponent.SetActive(false);
    }

    IEnumerator Login_REST()
    {
        WWWForm form = new WWWForm();
        form.AddField("playerID", PlayerID);

        using (UnityWebRequest www = UnityWebRequest.Post(LOGIN_URL, form))
        {
            yield return www.SendWebRequest();

            //로그인에 실패함
            if (www.result != UnityWebRequest.Result.Success)
            {
                DisplayNormal();
                //TODO: 로그인 실패 메세지 보여주기
            }
            else
            {
                if (www.isDone)
                {
                    string res = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
                    LoginPlayer = JBUS_Player.CreateFromJSON(res);
                    PhotonNetwork.JoinRoom(ROOM_NAME);
                }
            }
        }
    }

    //Room join에 실패할 때 -> 스스로 방을 만들음
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(ROOM_NAME);
    }

    //Room join에 성공하면 Game Scene을 불러옴
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel(GAME_SENCE);
    }
}
