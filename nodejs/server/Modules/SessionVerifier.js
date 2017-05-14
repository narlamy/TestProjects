module.exports = function(req, res, next) {

    // header에 해당 정보를 넣지 않은 이유는 암호화가 필요한 경우
    // 정보를 암호화쪽에 넣기 위해서 입니다.

    var CHK_SESSION = 0;
    var CHK_CODE = 1;
    var ALWAYS_PASS = 2;
    var sessionCheckType = req.body['__chkType'] || ALWAYS_PASS;

    switch(sessionCheckType) {
        case CHK_SESSION: // check stored session
            // DB 혹은 Redis에 저장 된 세션과 보내온 세션 정보가 다르면
            // 에러 처리
            return next();

        case CHK_CODE: // check code
            // 클라이언트에서 보내온 키값과 인증값을 가지고 진행 여부 처리
            return next();

        case ALWAYS_PASS: // ignore check, always pass
            // 무조건 검사 안 함
            // 단, 클라이언트 패킷은 무조건 믿을 수 없기 때문에
            // 이 경우 서버에 패스 리스트에 포함되어 있는 경우에만 처리
            return next();
    }
}