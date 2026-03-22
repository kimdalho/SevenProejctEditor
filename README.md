# Seven Project — Node Graph Editor 사용 설명서

---

## 1. 최초 설정

### 1-1. TSV 데이터 불러오기

1. **씬 하이어라키**에서 `StoryDatabase` 오브젝트 선택
2. Inspector → `Tsv Asset` 필드에 **TSV 파일**(TextAsset) 드래그
3. 게임 실행 시 자동으로 파싱됨

> TSV 파일 위치: `Assets/Resources/Story/`
> 포맷: `Id` `state_str` `state_int` `[추가 필드...]` (탭 구분)

### 1-2. 캐릭터 스프라이트 설정

- 경로: `Assets/Resources/Characters/`
- 파일명 규칙:
  - 기본 이미지: `{캐릭터이름}.png`
  - 표정 이미지: `{캐릭터이름}_{표정}.png` (예: `아리_Happy.png`)
- 게임 실행 시 `CharacterManager`가 자동으로 전부 로드함 (별도 설정 불필요)

### 1-3. UI 초기 생성

메뉴: **Tools → Node Graph → 에디터 인터페이스 생성**

> 씬에 NodeEditorPanel이 없을 때 최초 1회 실행. 이미 있으면 무시됨.
> **코드를 수정한 경우** 기존 오브젝트를 삭제하고 다시 생성해야 반영됨.

### 1-4. 노드 프리팹 자동 설정

메뉴: **Tools → Node Graph → Run AutoSetup on All Node Prefabs**

---

## 2. 에디터 단축키 (EditorScene 실행 중)

| 키 | 동작 |
|---|---|
| **F5** | TSV 데이터 → 노드 그래프 생성 (기존 노드 전부 삭제 후 재생성) |
| **F6** | 현재 노드 상태 → TSV 파일 저장 |
| **F7** | CutScene 씬으로 넘어가서 전체 시뮬레이션 실행 |
| **Space** | 에디터 내 런타임 재생 (시작 노드부터 순서대로 실행) |
| **ESC** | 런타임 재생 중단 |
| **W / A / S / D** | 카메라 이동 (위/왼/아래/오른) |
| **마우스 휠** | 카메라 줌 인/아웃 (범위: 0.5 ~ 20) |
| **X** | 선택된 노드 삭제 |

> **주의:** InputField에 타이핑 중일 때는 WASD/X 단축키 자동 차단됨

---

## 3. 노드 조작

### 노드 이동
- 노드 바탕(BG)을 **마우스 클릭 드래그** → 자유 이동
- 드롭 시 Y축 자동 고정

### 노드 선택 & 편집
- 노드 클릭 → 우측 **NodeEditorPanel** 패널이 열림
- 패널에서 해당 노드의 데이터를 직접 편집 가능

### 노드 연결 (Edge)
- 노드 **우측 버튼**(Next) 드래그 → 다른 노드 **좌측 버튼**(Prev)에 놓기
- 연결된 엣지는 베지어 곡선으로 표시됨
- 연결 후 드래그해도 엣지가 자동으로 따라옴

### 노드 생성
- UI 패널의 **노드 타입 버튼** 클릭 → (0, 0, 0) 위치에 생성
- 이후 드래그로 배치

---

## 4. 노드 타입별 설명

### Say (대사)
| 필드 | 설명 |
|---|---|
| 캐릭터 | 이름표에 표시될 캐릭터명 |
| 대사 | 출력할 텍스트 (여러 줄 입력 가능) |

실행: 대사 출력 → Space/Enter/클릭으로 다음 진행

---

### ShowCharacter (캐릭터 표시)
| 필드 | 설명 |
|---|---|
| 캐릭터 | 불러올 캐릭터 이름 (Resources/Characters/ 기준) |
| 표정 | None / Happy / Sad / Angry / Surprise |
| 위치 | Left / Mid / Right |
| EaseType | 이동 애니메이션 종류 (OutCubic 등) |
| 시간(초) | 등장 연출 시간 |

실행: 화면 밖에서 목표 위치로 이동 + 페이드인

> **⚠️ 미구현 / 주의사항**
> - 캐릭터 이름 입력 후 Enter 없이 바로 F7 실행 시 `characterData`가 null일 수 있음
>   → 캐릭터가 화면에 표시되지 않음 (오류는 없음)
> - Resources/Characters/ 폴더에 해당 이름의 스프라이트가 없으면 동일하게 빈 표시
> - **캐릭터 숨기기(HideCharacter) 노드 없음** — 현재 등장 전용만 구현됨
> - 같은 캐릭터를 다시 ShowCharacter 하면 위치/표정 갱신이 아닌 중복 생성될 수 있음

---

### Fade (페이드)
| 필드 | 설명 |
|---|---|
| 방향 | FadeOut (화면 어두워짐) / FadeIn (화면 밝아짐) |
| EaseType | 페이드 애니메이션 종류 |
| 시간(초) | 페이드 지속 시간 |

---

### Wait (대기)
| 필드 | 설명 |
|---|---|
| 대기(초) | 지정 시간 동안 정지 후 다음 노드 진행 |

---

### Choice (선택지)
| 필드 | 설명 |
|---|---|
| 선택지 수 | 2 / 3 / 4 개 |
| 선택지 1~4 | 버튼에 표시될 텍스트 |
| 선택지 1~4 → | 해당 선택 시 이동할 노드 ID (엣지 연결로 설정) |

연결 방법: Choice 노드의 **분기 버튼**을 드래그 → 이동할 노드에 연결

---

## 5. F5 — TSV → 노드 그래프 생성

- `StoryDatabase`에 로드된 TSV 데이터를 기반으로 노드 자동 생성
- 기존 노드 전부 삭제 후 재생성 (편집 내용 사라짐)
- 노드 배치: 왼→오, 위→아래 격자 형태 (Inspector에서 간격 조정 가능)
- 자동 체인 연결: TSV 순서대로 Next 연결
- Choice 분기: TSV의 `targets` 필드를 읽어 자동 복원

**NodeGraphManager Inspector 설정값:**
| 항목 | 설명 |
|---|---|
| Nodes Per Row | 한 행에 배치할 노드 수 (기본 6) |
| Spacing X / Z | 노드 간 가로/세로 간격 |
| Start Pos | 첫 번째 노드 시작 위치 |

---

## 6. F6 — TSV 저장

- 현재 노드 체인을 DFS 순서로 수집하여 TSV 파일로 저장
- 저장 위치: `Assets/Resources/Story/{원본파일명}_{날짜시간}.tsv`
- 원본 파일을 덮어쓰지 않고 **타임스탬프 새 파일** 생성
- Choice 분기도 포함하여 전체 저장

> 저장 후 Unity가 `Assets/Resources/Story/`를 감지하면 자동 임포트됨
> 새 파일을 사용하려면 `StoryDatabase`의 `Tsv Asset`을 교체할 것

---

## 7. F7 — CutScene 시뮬레이션

- 현재 노드 그래프 데이터를 **CutScene 씬**으로 넘겨 전체 재생
- 노드가 없으면 `StoryDatabase` 원본 데이터로 실행
- CutScene 씬이 Build Settings에 없으면 에러 출력

**CutScene 씬 내 조작:**
| 키 | 동작 |
|---|---|
| Space / Enter / 클릭 | 대사 넘기기 |
| ESC | 에디터 씬으로 복귀 |

복귀 시 노드 그래프 자동 복원됨

> **사전 조건:** Build Settings에 `CutScene` 씬이 추가되어 있어야 함
> 메뉴: File → Build Settings → Scenes In Build 에 추가

---

## 8. Space — 에디터 내 런타임 재생

- CutScene 씬으로 이동 없이 **현재 씬에서 바로 재생**
- PrevNode가 없는 노드(체인의 시작)부터 자동 탐색
- ESC로 언제든지 중단

---

## 9. 미구현 / 향후 작업 목록

| 항목 | 상태 | 비고 |
|---|---|---|
| ShowCharacter — 캐릭터 숨기기 | ❌ 미구현 | HideCharacter 노드 없음 |
| ShowCharacter — 중복 등장 처리 | ❌ 미구현 | 같은 캐릭터 재등장 시 갱신 아닌 중복 |
| ShowCharacter — 실행 중 characterData null 체크 | ⚠️ 불완전 | 스프라이트 없으면 조용히 실패 |
| 노드 실행 취소 (Undo) | ❌ 미구현 | — |
| 노드 복사/붙여넣기 | ❌ 미구현 | — |
| TSV 저장 후 StoryDatabase 자동 교체 | ❌ 미구현 | 수동으로 Tsv Asset 교체 필요 |
| CutScene 씬 자동 생성 도구 | ⚠️ 에러 메시지만 | "Tools > CutScene > Create" 언급하나 해당 메뉴 없음 |
| BGManager | ⚠️ 미연결 | 코드는 있으나 노드에서 사용 안 함 |
| Panel_FindGroup | ❌ 미구현 | 껍데기만 존재 |
