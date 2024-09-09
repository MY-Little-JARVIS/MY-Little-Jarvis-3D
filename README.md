# MY-Little-Jarvis-3D

- 개요
  - 본 프로젝트인 MY-Little-Jarvis는 Stand-Alone인 동시에 Server로 활용할 수 있음
  - 멀티 플랫폼으로서의 예시를 보여주기 위해, Unity를 이용한 3D 인터페이스를 구축
- 프로젝트 환경
  - Unity 2022.3.44f1
  - C#

## 주요 서비스 화면 예시

<차후 추가>

## Component

### UI

- Canvas
  - Render Mode : Screen Space - Camera
  - Render Camera : Main Camera
  - Image_input
    - 하위로 input Field(TMP)로 input을 받음
    - Character 좌클릭시 등장
    - 입력창 내에서 Enter로 입력받기, ESC로 취소
- Character
  - MeshCollider로 이벤트 감지
  - Script : Draggable Image
  - Animator
    - Entry, Idle, Walk, Pick을 boolean으로 Exit Time없이 transition
    - Any State, Motion, Exit을 Trigger로 transition
        ![alt text](Docs/animator.png)

### Camera

- 캐릭터와 UI를 제외한 것이 보이지 않게 설정
  - Clear Flags를 Solid Color로 설정
  - Background 색상을 투명(RGBA = (0, 0, 0, 0))으로 설정
  - Culling Mask에서 투명하지 않은 오브젝트 레이어(UI, Char) 설정
- Field of View를 10으로 설정하여, 3D Object인 Char이 정면을 보게 계속 설정

### Text

- TextMeshPro
- Font : 상업화이용까지 가능한 폰트
  - 일본어, 영어 : NotoSansJP
  - 한국어 : Fallback으로 SUIT

## Script

### Manager

- 개요 : 여러 객체나 시스템을 관리하고 조정. 싱글톤
  - GameManager : 전체 상태를 관리하고 시스템간 상호작용
  - AudioManager : 게임의 오디오 요소를 관리
  - UIManager : UI요소 관리 및 화면 전환/업데이트
  - APIManager : MY-Little-Jarvis 서버를 호출하고 반환값을 관리

## Handler

- 개요 : 특정/단일 이벤트나 작업을 처리하는 단위
  - ChatHandler : 채팅시 일어나는 이벤트 관련
