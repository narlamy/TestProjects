

module.exports = function (request, response) {
    
    var requestData = null;
    var responseData = {};

    // [2017-05-15] narlamy
    // protocol buffer 형식의 데이터를 해석하면서 기존 처럼 접근하기 쉽게 
    // body에 파라메터를 붙였습니다.

    if(request._getProtoMessage)
        requestData = request._getProtoMessage()
    else 
        requestData = {
            id: request.body['id'] || -1,
            name: request.body['name'] || 'unknown'
        }
        
    var id = request.body.id || -1;
    var name = request.body.name || '?'
    //console.log('ID = '+ id +  '\nName = ' + name);

    if(response.SendProtob)  {

        // ProtocolBuffer message 인스턴스를 이용해서
        // response data 전송
        
        var Sample = require('../Protocols/Sample_pb').Sample;
        var sample = new Sample();
        sample.setId(id)
        sample.setName(name)
        response.SendProtob(sample);
    }
    else {
        
        // 정말 테스트 코드
        // 직접 JSON 데이터 만들어서 반환
        requestData.RET = {
            id : id,
            name : name
        }

        response.status(200)
        response.json(requestData);
    }
}