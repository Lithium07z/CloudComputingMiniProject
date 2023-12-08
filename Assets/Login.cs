using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Login : MonoBehaviour
{
    public static string IP;                        // 사용자에게 입력받은 IP + 포트번호, Ex) 13.125.237.96:8080​ 
    public static string url = @"/refresh";         // refresh 함수 
    public static string send_url = @"/send/";      // send 함수
    public static string reset_url = @"/reset/";    // reset 함수
    public TMP_InputField input;                    // IP 입력받는 필드

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject); // 씬 이동에도 삭제되지 않게 설정
    }
    public void SetIP()
    {
        IP = input.text;    // 입력받은 IP + 포트번호를 IP로 설정

        SceneManager.LoadScene("SampleScene");  // 버튼 눌리면 SampleScene으로 이동
    }
}
