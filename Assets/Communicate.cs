using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Communicate : MonoBehaviour
{
    public TMP_InputField Name;     // 사용자 이름을 입력받는 필드
    public TMP_InputField Message;  // 사용자 메세지를 입력받는 필드

    public TMP_Text chatLog;        // 채팅 로그를 출력하는 텍스트 필드

    public GameObject textbox;      // 채팅 로그를 표시할 텍스트 박스의 프리팹
    public RectTransform content;   // ScrollView의 Content 객체
    List<GameObject> list = new List<GameObject>(); // 채팅 로그의 각 항목을 저장하는 리스트

    private string id;      // 사용자 이름을 저장하는 문자열
    private string message; // 사용자 메세지를 저장하는 문자열

    private string url;         // 채팅 로그를 가져오는 URL
    private string send_url;    // 메세지를 보내는 URL
    private string reset_url;   // 채팅 로그를 초기화하는 URL

    void Start()
    {
        // 애플리케이션 시작 시 실행되는 메서드
        url = @"http://" + Login.IP + Login.url;            // 채팅 로그 URL 설정
        send_url = @"http://" + Login.IP + Login.send_url;  // 메세지 전송 URL 설정
        reset_url = @"http://" + Login.IP + Login.reset_url;// 채팅 로그 초기화 URL 설정
        StartCoroutine(UnityWebRequestGetChatLog());        // 채팅 로그를 주기적으로 가져오는 코루틴 시작
    }

    void Update()
    {
        
    }

    /// <summary>
    /// 채팅 로그를 초기화하는 메서드, 리셋버튼이 눌리면 호출
    /// </summary>
    public void Reset()
    {
        StartCoroutine(ResetAll()); // 채팅 내역 초기화 코루틴 실행
    }

    /// <summary>
    /// 메세지 입력 버튼이 눌리면 이름, 메세지 변수를 저장하고 Request메세지를 보냄 
    /// </summary>
    public void InputNameAndData()  // 메세지 입력 버튼이 눌린 경우 호출됨
    {
        id = Name.text;             // 이름 입력 필드에서 id 변수로 값 저장
        message = Message.text;     // 메세지 입력 필드에서 message 변수로 값 저장
        Debug.Log(id + " " + message);
        StartCoroutine(UnityWebRequestSendMessage());   // 메세지가 입력된 경우 서버로 전송하는 코루틴 실행
    }

    /// <summary>
    /// 채팅 로그를 초기화하는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator ResetAll()
    {
        UnityWebRequest unityWebRequest = UnityWebRequest.Get(reset_url);   // reset_url로 Get요청을 보내는 객체 생성

        yield return unityWebRequest.SendWebRequest();  // 요청을 보내고 응답을 기다림
    }

    /// <summary>
    /// 메세지를 서버로 전송하는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator UnityWebRequestSendMessage()
    {
        UnityWebRequest unityWebRequest = UnityWebRequest.Get(send_url+ $"?name={Name.text}&message={Message.text}");   // 메세지 전송 URL에 쿼리 파라미터 추가

        yield return unityWebRequest.SendWebRequest();  // 요청을 보내고 응답을 기다림

        if (unityWebRequest.result == UnityWebRequest.Result.Success)
        {
            // 성공적으로 데이터를 서버에 전송한 경우
            Debug.Log("Data sent successfully");

            // 서버에서 받은 응답 해석
            string responseData = unityWebRequest.downloadHandler.text;
            JsonResponse jsonResponse = JsonUtility.FromJson<JsonResponse>(responseData);
            DeleteItem();   // 기존 채팅 로그 삭제

            // 화면에 채팅 로그 출력
            foreach (var a in jsonResponse.data)
            {
                GameObject new_item = Instantiate(textbox, content);
                list.Add(new_item);
                var b = new_item.GetComponent<TMP_Text>();
                b.text = $"{a.id} {a.username} : {a.data}";
            }

            Message.text = "";  // 메세지 필드 초기화
        }
        else
        {
            // 요청 실패 시 오류 로그 출력
            Debug.LogError("Error: " + unityWebRequest.error);
        }
    }


    IEnumerator UnityWebRequestGetChatLog()
    {
        // 채팅 로그를 주기적으로 가져오는 코루틴
        while (true)    // 씬 로드 후 계속 수행
        {
            UnityWebRequest unityWebRequest = UnityWebRequest.Get(url); // 채팅 로그 URL로 Get요청

            yield return unityWebRequest.SendWebRequest();  // 요청을 보내고 응답을 기다림

            if (unityWebRequest.result == UnityWebRequest.Result.Success)   // 성공적으로 답장을 수신했다면
            {
                // 성공적으로 데이터를 서버에 전송한 경우
                Debug.Log("Data sent successfully");

                // 서버에서 받은 응답 해석
                string responseData = unityWebRequest.downloadHandler.text;
                
                Debug.Log(responseData);
                JsonResponse jsonResponse = JsonUtility.FromJson<JsonResponse>(responseData);
                DeleteItem();   // 기존에 있던 채팅 로그 삭제

                // 화면에 채팅 로그 출력
                foreach (UserData a in jsonResponse.data)
                {
                    GameObject new_item = Instantiate(textbox, content);
                    list.Add(new_item);
                    var b = new_item.GetComponent<TMP_Text>();
                    b.text = $"{a.id} {a.username} : {a.data}";
                }
            }
            else
            {
                // 요청 실패 시 오류 로그 출력
                Debug.LogError("Error: " + unityWebRequest.error);
            }
        
            yield return new WaitForSeconds(0.5f);  // 0.5초 간격으로 코루틴 재실행
        }
    }

    /// <summary>
    /// 채팅 갱신시 기존의 채팅 내역을 삭제하는 함수
    /// </summary>
    public void DeleteItem()
    {
        foreach (GameObject item in list)   // 리스트의 채팅 내역을
        {
            Destroy(item.gameObject);       // 삭제
        }
    }

    /// <summary>
    /// 로그인 화면으로 복귀하는 함수
    /// </summary>
    public void BackButton()    // 처음 Login 화면으로 복귀
    {
        SceneManager.LoadScene("Login");    // Login 씬 로드
    }
}

[System.Serializable]
public class UserData       // 답변으로 받아온 채팅 로그 정보
{
    public int id;          // 사용자 ID
    public string username; // 사용자 이름
    public string data;     // 채팅 내용
}

[System.Serializable]
public class JsonResponse   // 서버에서 받은 응답을 위한 클래스
{
    public string msg;          // 메시지
    public List<UserData> data; // 채팅 데이터
}