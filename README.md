# SoundCheckEditor
- **Unity에서 직접 검색하고, Freesound를 통해 무료 사운드를 탐색할 수 있는 커스텀 에디터**
- Freesound : 저작권 무료 음원사이트. (https://freesound.org/)
- API 문서는 http://www.freesound.org/docs/api/ 에서 찾을 수 있음.
- API 키는 https://www.freesound.org/apiv2/apply/ 에서 신청 가능 .

# 패키지 다운로드 링크
2025.03 AssetStore 출시 예정

# 사용자 지원문서 (User Support Document)
- [KOR](/SoundCheckEditor/README_ENG.pdf)
- [ENG](/SoundCheckEditor/README_ENG.pdf)

# 개발 환경 
- Unity Engine 2023.2.16f1
- C#
- HTTP

# 기술 스택
- Unity Editor의 커스텀 에디터 기능
- Freesound APIv2 : Freesound의 API Key를 발급받아 사용
- NewtoneSoft Json : JSON 데이터 처리를 위해 사용

# 개요
1. 필요한 사운드 탐색을 위해 사이트를 이리저리 돌아다니지 않고, 에디터 내에서 바로 접근할 수 있는 기능을 만들고자 하였다.
2. 외부 API 및 HTTP통신을 Unity에서 사용하는 좋은 경험이 될 것이라 판단했고, 실제로 좋은 공부가 되었다.

# 사용 시 유의
- JSON 데이터 처리를 위해 Newtonsoft json 3.2.1 라이브러리가 사용되었음. 사용자는 별도의 다운로드가 필요 없도록 DLL과 Assembly Definition을 패키지에 포함시켜 놓았지만, 혹시 오류가 발생할 경우 아래와 같이 조치 바람.
  
  1. Json 데이터 처리를 위해 Newtonsoft json 3.2.1버전을 임포트(패키지 매니저 이름으로 찾기에서 com.unity.nuget.newtonsoft-json 입력 후 3.2.1버전 다운로드) 
  2. Assets폴더 내 Plugin 폴더에 직접 Newtonsoft.json dll파일을 넣어주면 해결

# 기능
1. 원하는 사운드를 검색 후 **Open in Browser**버튼을 클릭하면 Freesound의 해당 사운드 페이지로 연결된다.
2. 사용자 본인의 Freesound API키를 에디터 상에 입력하여 사용할 수 있다.
3. **Play Preview** 버튼을 클릭하여 사운드 미리듣기가 가능하며, 사운드 재생 중일 경우 자동으로 **Stop Preview** 버튼이 아래에 나타난다.

# 사용 예시
 ## Tool -> SoundCheckEditor 클릭
![toolbar](https://github.com/user-attachments/assets/a34742fa-0c4d-4cd1-9a42-17437d878ab5)

 ## 본인의 Freesound API KEY를 에디터 상에 입력(무료. 회원가입 시 발급 가능하며, 분당 60개 요청, 하루 2000개의 사용 제한 존재)
  Save API Key를 클릭하면 에디터 창 종료 후 재시작 시에도 값이 그대로 저장됨.
 ![Image](https://github.com/user-attachments/assets/d1c521f3-7f25-450c-88d1-df60ec7a635c)

 ## API 키 정상 입력 시
 ![Image](https://github.com/user-attachments/assets/2097126c-8ad4-4844-999b-ee8920cd3295)

 ## Search Sound Effect에 원하는 사운드 명 입력 후 Search 클릭
 ![Image](https://github.com/user-attachments/assets/5d6642a0-461c-4ec1-9ff1-ec91f1f1e8dd)

 ## 검색된 사운드 목록이 출력됨. 한 페이지 당 10개의 목록이 출력되며, 전-후 페이지 이동 가능
 ![Image](https://github.com/user-attachments/assets/848435d7-bffe-448a-b5a5-21e82fefbfe8)

 ## Play Preview 클릭 시 에디터 내에서 바로 사운드가 재생됨.
 ![Image](https://github.com/user-attachments/assets/d733a2d3-ba8a-4b54-8c56-74d68400f211)

 ## Open in Browser 클릭 시 Freesound의 해당 음향효과 검색 결과창으로 이동
 ![사이트연결2](https://github.com/user-attachments/assets/e027c82e-bb06-4384-a036-1ac89f44fdc1)

 ## API Key 오류 시(오류 조건 : 인증 오류 or 잘못된 접근)
 ![api키 에러시](https://github.com/user-attachments/assets/8f88e2ea-099e-4ca1-8351-3e988a6eec2c)
 ![에러 시 콘솔](https://github.com/user-attachments/assets/f55042a9-1939-42d7-97c1-dd15e9d134cd)
 
# 패키지 구성
![Image](https://github.com/user-attachments/assets/c126a09c-b072-41d3-898f-b2a3155a2884)
 1. FreesoundSearchResult : Freesound API의 검색 결과를 표현할 클래스
 2. FreesoundSound : Freesound API의 사운드 항목을 표현할 클래스
 3. SoundResult : 검색 결과를 표현할 클래스
 4. SoundCheckEditor : 에디터 구현 및 JSON 파싱
 5. Previews : 미리듣기 사운드의 품질 필드 클래스. 

# 버전목록
- 2024.09.04 Ver.1 
- 2024.09.05 Ver.2 
- 2025.03.18 Ver.3 : 2025.03 AssetStore 출시 예정

# 업데이트 노트
- Ver.1
1. Freesound API를 사용해 에디터 상에서 음향효과 목록을 검색하고 접근할 수 있는 기능 설계

- Ver.2
1. 매 프레임 호출되는 OnGUI의 비용 절감을 위해 EditorApplication.update를 이용해 상태기반 이벤트 처리 수행
2. 페이지 넘기기 기능 추가 : Freesound의 next, previous응답을 사용하고, API 요청 시 Authorization 헤더를 포함하여 요청을 보냄. 
3. 페이지 넘기기 UI 조절 : next page, previous page 버튼이 SoundResults에 가려 안보이게 됨을 방지하기 위해 빈 공간을 나누고 DisabledGroup으로 묶음. 
4. next, prev URL의 불필요한 부분 제거 
5. API Key 은닉 및 사용자친화적 입력 : 패키지 배포를 위해 APIKey를 숨기고, 사용자가 에디터 설정창을 통해 직접 본인의 APIKey를 입력받을 수 있도록 함

- Ver.3
1. Query에 라이선스 필터링 추가
2. URL의 공백 및 특수문자 입력 시 오류 방지를 위한 URL인코딩 추가
3. JSON 파싱 예외처리 추가
4. 검색 시 진행 표시기 구현
5. 사운드 미리듣기 기능 구현
6. 에디터 내 한글 설명 제거 및 API KEY 발급 필요 문구 추가
7. API KEY, URL 유효성 체크 추가
