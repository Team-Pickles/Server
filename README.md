# JSON Save And Load(Prototype 적용)
MapData폴더에 json파일이 저장되어있음

- 파일명이 겹치는 경우
    - 파일명 뒤에 '_숫자'를 붙여서 저장  
    - 서버 코드와 호환되도록 변경하였음
    - 서버의 예시 맵들을 저장하고 불러 올 수 있음
    - 차후에 타일 배열에 사용하고픈 타일 추가 필요 
<br/>

# Server Code 
- 이동 투사체 발사 기능을 구현한 서버
    - prototype repository에 새로운 클라이언트를 push 하였음
    - 서버를 빌드 후, 클라이언트에서 접속을 해야함
    - 패킷의 경로
        + 클라이언트 입력 및 처리 -> client Send -> Server Handle -> 서버에서 처리 -> Server Send-> Client Handle
    - 투사체는 bullt과 Granade가 있음
        - grenade의 경우 예시 코드를 사용하다보니 현재 이름이 projectile로 대신 사용되고 있음 (projectile 코드 == grenamde 코드)
        - 현재 투사체는 충돌 감지 시 사라짐 그 이후의 로직은 구현 필요
<br/>
 
    