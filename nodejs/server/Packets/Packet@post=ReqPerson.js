

module.exports = function (request, response) {
    
    var requestData = null;
    var responseData = {};

    // [2017-05-15] narlamy
    // protocol buffer 형식의 데이터를 해석하면서 기존 처럼 접근하기 쉽게 
    // body에 파라메터를 붙였습니다.

    var id = request.body.id || -1;

    if(response.CreateMessage)  {

        // ProtocolBuffer message 인스턴스를 이용해서
        // response data 전송
        
        var resPerson = response.CreateMessage();
        resPerson.setName('DEMO SERVER : ' + id)
        resPerson.setAge(100 + (isNaN(id) ? 0 :Number(id)))
        resPerson.Send({bin:true, errCode:0});
    }
    else {
        
        // 정말 테스트 코드
        // 직접 JSON 데이터 만들어서 반환
        response.status(200)
        response.json({ErrCode:-1});
    }
}