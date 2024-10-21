# The Way Home
[![license](https://img.shields.io/badge/License-MIT-red)](https://github.com/CHOULOKY/Isaac-Imitation?tab=MIT-1-ov-file)
[![code](https://img.shields.io/badge/Code-C%23-purple)](https://dotnet.microsoft.com/ko-kr/platform/free)
[![IDE](https://img.shields.io/badge/IDE-VS-blueviolet)](https://visualstudio.microsoft.com/ko/vs/)
[![Engine](https://img.shields.io/badge/Engine-Unity(22.3.23f1)-white?logo=unity&logoColor=white)](https://unitysquare.co.kr/etc/eula)
[![Server](https://img.shields.io/badge/Server-Photon-004480?logo=Photon&logoColor=white)](https://doc.photonengine.com/ko-kr/server/current/operations/licenses)
[![Collabo](https://img.shields.io/badge/Collabo-GithubDesktop-purple)](https://docs.github.com/ko/desktop)
[![Collabo](https://img.shields.io/badge/Collabo-Notion-000000?logo=notion&logoColor=white)](https://www.notion.so/ko/pricing)
[![Art](https://img.shields.io/badge/Art-Aseprite-7D929E?logo=aseprite&logoColor=white)](https://store.steampowered.com/app/431730/Aseprite/?l=koreana)

<br><br><br>

## 1. Overview 
- Project Name: The Way Home
- Description: Platformer game using Photon Server (Up to Chapter 1)
- Genre: Action, Platformer, Adventure, Puzzle, Co-op

<br><br>

## 2. Project Members
|Team Leader|Developer|Developer|
|:--:|:--:|:--:|
|김나영<sub>Kim Nayoung</sub>|박인희<sub>Park Inhee</sub>|김용휘<sub>Kim Yonghwi</sub>|
|![다운로드](https://github.com/user-attachments/assets/f8d4e10d-f847-4170-a6fc-af61cf8fbe99)|![pngtree-recycle-garbage-bag-png-image_6491460](https://github.com/user-attachments/assets/ad653ad3-e628-42f2-92b1-85f7daaff750)|![다운로드](https://github.com/user-attachments/assets/f8d4e10d-f847-4170-a6fc-af61cf8fbe99)|
|[![Github](https://img.shields.io/badge/Github-181717?style=for-the-badge&logo=Github&logoColor=white)](https://github.com/NaYoung1017)|[![Github](https://img.shields.io/badge/Github-181717?style=for-the-badge&logo=Github&logoColor=white)](https://github.com/CHOULOKY)|[![Github](https://img.shields.io/badge/Github-181717?style=for-the-badge&logo=Github&logoColor=white)](https://github.com/HOKAGO-MEMORIES)|

<br><br>

## 3. Role
||김나영<sub>Kim Nayoung</sub>|박인희<sub>Park Inhee</sub>|김용휘<sub>Kim Yonghwi</sub>|
|--|--|--|--|
|Project Planning & Management|O|O|O|
|Team Reading & Communication|O|X|X|
|Lobby Development|O|X|X|
|InGame Development|X|O|X|
|Map Design|X|X|O|
|And So On|O|O|O|
|Problem Solving|O|O|O|

<br><br>

## 4. Key Features
- **Lobby Management**
  - 호스트가 공개 로비를 생성, 게스트는 호스트가 생성한 로비에 참가, 랜덤 매칭, 로비 내에서 채팅을 통해 플레이어 간 의사소통
- **Player Control**
  - 플레이어가 플레이어 캐릭터를 조작할 수 있는 기능(이동, 공격, 능력 등)
- **Monster AI**
  - 몬스터를 FSM(유한 상태 기계) 디자인 패턴과 AStar 알고리즘을 사용하여, 플레이어를 자동으로 감지, 추격, 공격하는 기능
- **Check Point**
  - 플레이어가 사망 시 체크포인트로 돌아가는 기능
- **Multi-Play**
  - Photon 서버를 사용하여 두 플레이어가 같은 게임 공간에서 플레이할 수 있는 기능

<br><br>

## 5. Player Types
||Girl|Robot|
|:--:|:--:|:--:|
|Preview|![스크린샷 2024-09-27 005501](https://github.com/user-attachments/assets/cde00f23-0254-44ae-9c85-845404c6529a)|![스크린샷 2024-09-27 005513](https://github.com/user-attachments/assets/c4493ef2-2825-4e4b-8917-884c343869fb)|
|Attack|Mouse L|Mouse L|
|Move|WASD or Arrow|WASD or Arrow|
|Jump|Space bar|Space bar|
|Ability|Jump + Hold Space bar|Jump + Space bar|

<br><br>

## 6. Monster Types
||Dobermann|Bird|
|:--:|:--:|:--:|
|Preview|![스크린샷 2024-09-27 003607](https://github.com/user-attachments/assets/11449cd4-fed6-4c67-b033-55ee4f0de603)|![스크린샷 2024-09-27 003625](https://github.com/user-attachments/assets/58503f83-9769-47b1-a974-729f6b0a305b)|
|Attack|Melee|Melee|
|Move|Walk|Fly|
|Tech|FSM|FSM, AStar|

<br><br><br><br>

## Reference
<img align="right" src="https://github.com/user-attachments/assets/141c54f0-2640-4423-b313-8dde2cfa098c" width="75" height="75" />
