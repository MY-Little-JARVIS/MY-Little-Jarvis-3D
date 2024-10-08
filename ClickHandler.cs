using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityWeld.Binding;

[Binding] 
public class ClickHandler : MonoBehaviour, IPointerClickHandler 
{

    [SerializeField] public Animator _animator; 

    public void OnPointerClick(PointerEventData eventData)
    {
        if (StatusManager.Instance.IsDragging)    
            return;
        
        // 좌클릭
        if (eventData.button == PointerEventData.InputButton.Left) {

            // 서버가 구동중인지 확인
            if (ServerManager.IsJarvisServerRunning())
            {
                // TODO : ping 확인하는 함수(최대 20회 초당 1회)
                ChatBalloonManager.Instance.ShowChatBalloon();
            } else {
                // 서버 구동중이지 않음
                if (SettingManager.Instance.settings.isAskedTurnOnServer)  // 서버구동 물어볼지 여부
                {
                    // 에디터일 경우 바로 ChatBalloon 보여주기
                    #if UNITY_EDITOR
                        ChatBalloonManager.Instance.ShowChatBalloon();
                    #else
                        // 서버 설치되어있는지 확인
                        string streamingAssetsPath = Application.streamingAssetsPath;  // StreamingAssets 폴더 경로
                        string jarvisServerPath = Path.Combine(streamingAssetsPath, "jarvis_server_jp.exe");
                        if (File.Exists(jarvisServerPath))
                        {
                            // 서버 구동할지 물어보기
                            AskBalloonManager.Instance.SetCurrentQuestion("start_ai_server");  // InitializeQuestions에서 목록 확인(많아질 경우 Enum으로 관리)
                            AskBalloonManager.Instance.ShowAskBalloon();  // 들어가기
                        }
                        else
                        {
                            string installPath = Path.Combine(streamingAssetsPath, "Install_3D.exe");
                            if (File.Exists(installPath))
                            {
                                // 서버 설치할지 물어보기
                                AskBalloonManager.Instance.SetCurrentQuestion("install_ai_server");  // InitializeQuestions에서 목록 확인(많아질 경우 Enum으로 관리)
                                AskBalloonManager.Instance.ShowAskBalloon();  // 들어가기
                            }
                            else 
                            {
                                // TODL : 서버 다운로드할지 물어보기
                                Debug.Log("No Install File");
                            }
                        }
                    #endif
                }
                else 
                {
                    // 애니메이션 재생, special>select(대사있음)>random 순으로
                    if (isAnimatorTriggerExists(_animator, "doSpecial")) {  // special
                        _animator.SetTrigger("doSpecial");
                        StatusManager.Instance.SetStatusTrueForSecond(value => StatusManager.Instance.IsOptioning = value, 7.5f); // 15초간 isOptioning을 true로
                    } else if (isAnimatorTriggerExists(_animator, "doSelect")) {  // select
                        Dialogue select = DialogueManager.instance.GetRandomSelect();
                        DoDialogueBehaviour(select); // select 행동 
                    } else {  // random
                        PlayRandomAnimation(); // 랜덤 애니메이션 재생
                    }
                }
            }
        }
        // 중앙클릭
        if (eventData.button == PointerEventData.InputButton.Middle) {
            Dialogue idle = DialogueManager.instance.GetRandomIdle();
            DoDialogueBehaviour(idle);// idle 행동 
        }
        // 우클릭 - Menu Triggger로 이동
        // if (eventData.button == PointerEventData.InputButton.Right)
        // {
        // }
    }

    private void PlayRandomAnimation()
    {
        List<string> randomMotionTriggers = new List<string>();
        // doRandomMotion1, doRandomMotion2, doRandomMotion3의 존재 여부를 확인
        if (isAnimatorTriggerExists(_animator, "doRandomMotion1"))
        {
            randomMotionTriggers.Add("doRandomMotion1");
        }
        if (isAnimatorTriggerExists(_animator, "doRandomMotion2"))
        {
            randomMotionTriggers.Add("doRandomMotion2");
        }
        if (isAnimatorTriggerExists(_animator, "doRandomMotion3"))
        {
            randomMotionTriggers.Add("doRandomMotion3");
        }
        // 리스트에 존재하는 트리거 중 랜덤한 하나를 선택하여 반환
        if (randomMotionTriggers.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, randomMotionTriggers.Count);
            string motion = randomMotionTriggers[randomIndex];
            _animator.SetTrigger(motion);  
            StatusManager.Instance.SetStatusTrueForSecond(value => StatusManager.Instance.IsOptioning = value, 5f); // 15초간 isOptioning을 true로
        }
    }

    // 현재 재생중인 애니메이션 클립의 길이를 반환하는 함수 (타임rag 있어서 사용시 신중)
    public float GetAnimationClipLengthByStateName()
    {
        // 0번째 레이어의 상태 정보를 가져옴
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        AnimatorClipInfo[] clipInfos = _animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfos.Length > 0)
        {
            AnimationClip clip = clipInfos[0].clip;
            float clipLength = clip.length;  // 애니메이션 클립의 길이
            float speedMultiplier = stateInfo.speed;  // 재생 배율

            // 실제 재생 시간 = 클립 길이 / 재생 배율
            float actualPlayTime = clipLength / speedMultiplier;

            Debug.Log($"애니메이션 클립 길이: {clipLength}초, 재생 배율: {speedMultiplier}, 실제 재생 시간: {actualPlayTime}초");
            
            return actualPlayTime;
        }
        return 0f;
    }

    public bool isAnimatorTriggerExists(Animator animator, string triggerName)
    {
        // Animator의 모든 파라미터 확인
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            // 해당 파라미터가 Trigger이고 이름이 일치하는지 확인
            if (param.type == AnimatorControllerParameterType.Trigger && param.name == triggerName)
            {
                return true;  // Trigger가 존재함
            }
        }
        return false;  // Trigger가 존재하지 않음
    }

    public void DoDialogueBehaviour(Dialogue dialogue) {
            // 음성있을 경우 재생
            if (!string.IsNullOrEmpty(dialogue.filePath)) {
                VoiceManager.Instance.PlayAudioFromPath(dialogue.filePath);  // 음성 재생
            }

            // 대사 있을 경우 (각 국가별)
            string dialogueString = dialogue.englishDialogue;
            if (SettingManager.Instance.settings.ui_language == "ko" ) {
                dialogueString = dialogue.koreanDialogue;
            } else if (SettingManager.Instance.settings.ui_language == "jp" ) {
                dialogueString = dialogue.japaneseDialogue;
            }
            if (!string.IsNullOrEmpty(dialogueString)) {
                AnswerBalloonSimpleManager.Instance.ShowAnswerBalloonSimple();
                AnswerBalloonSimpleManager.Instance.ModifyAnswerBalloonSimpleText(dialogueString);
                AnswerBalloonSimpleManager.Instance.HideAnswerBalloonSimpleAfterAudio();
            }

            // 지정 모션 있을 경우 있는지 확인 후 실행 없으면 랜덤 모션
            if (!string.IsNullOrEmpty(dialogue.trigger)) {
                if (isAnimatorTriggerExists(_animator, dialogue.trigger)) {
                    _animator.SetTrigger(dialogue.trigger);
                    StatusManager.Instance.SetStatusTrueForSecond(value => StatusManager.Instance.IsOptioning = value, 15f); // 15초간 isOptioning을 true로
                } else {
                    PlayRandomAnimation(); // 랜덤 애니메이션 재생
                }
            } else {
                PlayRandomAnimation(); // 랜덤 애니메이션 재생
            }
    }
}
