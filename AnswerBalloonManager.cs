using System.Collections;
using UnityEngine;
using TMPro;

public class AnswerBalloonManager : MonoBehaviour
{
    [SerializeField] private Canvas _canvas; // AnswerBalloon 이미지
    [SerializeField] private GameObject answerBalloon; // AnswerBalloon 이미지
    [SerializeField] private TextMeshProUGUI answerText; // AnswerBalloon 하위의 TMP 텍스트
    [SerializeField] private RectTransform characterTransform; // AnswerBalloon이 표시될 캐릭터의 Transform
    [SerializeField] private RectTransform answerBalloonTransform; // AnswerBalloon의 Transform
    public TextMeshProUGUI answerBalloonText; // AnswerBalloon Text의 Transform

    private float hideTimer = 0f; // 타이머 변수 추가

    // 싱글톤 인스턴스
    private static AnswerBalloonManager instance;

    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        HideAnswerBalloon(); // 시작 시 AnswerBalloon 숨기기
    }

    // 상태 갱신 로직
    private void Update()
    {
        // 타이머 갱신
        if (hideTimer > 0f)
        {
            hideTimer -= Time.deltaTime;
        }

        // 타이머가 완료되면 AnswerBalloon 숨기기
        if (hideTimer <= 0f && StatusManager.Instance.IsAnswering)
        {
            HideAnswerBalloon();
        }

        if (StatusManager.Instance.IsAnswering)
        {
            UpdateAnswerBalloonPosition();
        }

        if (StatusManager.Instance.IsPicking || StatusManager.Instance.IsListening)
        {
            HideAnswerBalloon();
        }
    }


    // 싱글톤 인스턴스에 접근하는 속성
    public static AnswerBalloonManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AnswerBalloonManager>();
            }
            return instance;
        }
    }

    // AnswerBalloon을 보이고 텍스트를 초기화하는 함수
    public void ShowAnswerBalloon()
    {
        answerBalloon.SetActive(true);
        answerText.text = string.Empty; // 텍스트 초기화
        StatusManager.Instance.IsAnswering = true; // StatusManager 상태 업데이트
        UpdateAnswerBalloonPosition();  // AnswerBalloon 위치 조정하
    }

    // AnswerBalloon의 텍스트를 수정하고 오디오를 재생하는 함수
    public void ModifyAnswerBalloonText(string text)
    {
        answerText.text = text; // 텍스트 변경

        // 높이 조정
        float textHeight = answerBalloonText.preferredHeight;
        answerBalloonTransform.sizeDelta = new Vector2(answerBalloonTransform.sizeDelta.x, textHeight + 60);

        
    }
    
    // 현재(마지막) 오디오 재생 후 AnswerBalloon을 숨기는 코루틴 호출
    public void HideAnswerBalloonAfterAudio()
    {
        AudioClip clip = VoiceManager.Instance.GetAudioClip();

        if (clip != null)
        {
            hideTimer = clip.length + 0.5f; // 타이머를 오디오 재생 시간 + 0.5초로 설정
        }
    }

    // AnswerBalloon을 숨기는 함수
    public void HideAnswerBalloon()
    {
        answerBalloon.SetActive(false);
        StatusManager.Instance.IsAnswering = false; 
    }

    // AnswerBalloon의 위치를 캐릭터 바로 위로 조정하는 함수
    private void UpdateAnswerBalloonPosition()
    {
        Vector2 charPosition = characterTransform.anchoredPosition;
        
        // 캐릭터의 X 위치와 동일하게 설정
        answerBalloonTransform.anchoredPosition = new Vector2(charPosition.x, charPosition.y + 270); // Y축 창크기 270만큼
    }
}
