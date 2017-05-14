

module.exports = function (request, response) {
    
    console.log(request.path);

    var requestData = null;
    var responseData = {};

    if(request._getProtoMessage) {

        // request에 함수 붙여서 사용
        // ProtoBuffer를 사용하는 것을 고려합니다.
        // express에서 사전에 함수를 붙여보아요

        requestData = request._getProtoMessage()
        
    }
    else {

        requestData = {
            id: request.body['id'] || -1,
            name: request.body['name'] || 'unknown'
        }
    }

    // 
    var id = requestData.id || -1;
    var name = requestData.name || '?'
    console.log('ID = '+ id +  '\nName = ' + name);

    if(response.SendResponse)  {

        // 내부에서 protocol buffer 로 만들어서 보냄
        response.SendResponse(responseData);
    }
    else {
        
        // 모듈 호출해서 객체 생성 팔요

        response.status(200)
        response.json(requestData);
        //response.json('{ "Error": 0, "ReponseName" : "_unknown" }')
    }
}