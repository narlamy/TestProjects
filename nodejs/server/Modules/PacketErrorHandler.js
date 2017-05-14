module.exports = function(err, req, res, next) {

    console.log('Error Handler ===')
    
    // [2016-09-22] narlamy
    // 요청이 취소되었으므로 응답도 하지 않고 에러 메세지도 출력하지 않고 무시합니다.
    // request aborted 에러 무시
    if (err.code == 'ECONNABORTED') {
        
        console.warn('[PacketError] Req="%s"', req.url);
        return;
    }

    if (err.status && (err.status < 200 || err.status >= 300)) {
        
        var util = require('util');
        var t = new Date();
        var m = util.format('[PacketError] Req="%s" (Status="%d", IP="%s", ConentType="%s")\nReq.Body="%s"\nReq.Header="%s"\nRes.Body="%s"\nErr="%s"\nTime="%s"', 
            req.url, 
            err.status,
            'ip',
            req.headers.ContentType,
            util.inspect(req.body),
            util.inspect(req.headers),
            util.inspect(res.body),
            util.inspect(err),
            t.toISOString()
        );

        res.send(m);
        
        // 에러이므로 더 이상 진행하지 않고 바로 응답 패킷 전송
        //var errCode = require('../PacketErrors/CommonError').Errors.BokenPacket;
        //var output = { Status : err.status };
        //require('../Packet').SendPacket(res, output, errCode);
    }
    else {
        // 에러 상태 코드가 200번대라면 다음으로 진행
        // 
        return next();
    }
}
