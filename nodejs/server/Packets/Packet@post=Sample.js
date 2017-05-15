

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
    // 
    // var id = requestData.id || -1;
    // var name = requestData.name || '?'
    var id = request.body.id || -1;
    var name = request.body.name || '?'
    //console.log('ID = '+ id +  '\nName = ' + name);

    requestData.RET = {
        id : id,
        name : name
    }

    if(response.SendProtob)  {

        // 내부에서 protocol buffer 로 만들어서 보냄
        var Sample = require('../Protocols/Sample_pb').Sample;
        var sample = new Sample();
        sample.setId(9988)
        sample.setName('hohoho')
        response.SendProtob(sample);
    }
    else {
        
        // 모듈 호출해서 객체 생성 팔요

        response.status(200)
        response.json(requestData);
        //response.json('{ "Error": 0, "ReponseName" : "_unknown" }')
    }
}