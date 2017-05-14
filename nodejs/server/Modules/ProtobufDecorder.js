// base64로 인코딩 된 문자열 입력
// protocol buffer
var parseFromBase64 = function (protoName, base64String, callback) {

    console.log('REQUEST : "' + base64String + '"')

    // (1) base64를 buffer로 변경
    // (2) buffer를 proto type을 참조해서  message 객체로 변경
    // (3) 메세지를 바로 전달하든지 변형해서 리턴

    try {
        //let Buffer = require('buffer');
        let buf = Buffer.from(base64String, 'base64');

        // Buffer에 있는 내용을 가지고 message 객체를 반환합니다.
        callback(null, { _buf : buf, _txt : base64String });
    }
    catch (e) {
        console.error('[Decorde] ' + e);
        var err = new Error(e.message);
        callback(err, null);
    }
}

var toBase64 = function(protob_message) {

    let Buffer = require('buffer');
    let buf = {}
    return Buffer.toBase64(buf);
}

var attachMessage = function(req, res, message, copyToBody) {

    // 입력 받은 메세지 인스턴스
    req._protob_message = message;
    req._getProtoMessage = function() {
        return this._protob_message;
    }

    // 응답으로 보낼 메세지 인스턴스
    res.SendMessage = function(protob_message) {
        var output = { __pbDat : toBase64(protob_message) }
        this.json(output);   
    }

    // body에 protocol buffer message의 프로퍼티를 복사합니다.
    if(copyToBody && message) {
        for(let k in message) {
            var field = message[k];
            if(typeof(field) != 'function' && !k.startsWith('_')) {
                req.body[k] = field;
            }
        }
    }
}

exports.attachMessage = attachMessage;
exports.parseFromBase64 = parseFromBase64;