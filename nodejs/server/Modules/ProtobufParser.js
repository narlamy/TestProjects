'use strict'

module.exports = function(req, res, next) {

    //console.log('[CheckProtob] : ' + req.path);

    var encode = req.headers ? req.headers.encode : null;
    if(encode == 'protobuf') {

        // protocol buffer 데이터이므로  
        
        let BIN_KEY = '__pbDat'
        let NAME_KEY = '__pbName'

        let encodedText = req.body[BIN_KEY];
        let protoName = req.body[NAME_KEY] || '';
            
        if(encodedText) {

            req.body[BIN_KEY] = null;
            req.body[NAME_KEY] = null;

            let protobufDecorder = require('./ProtobufDecorder');

            protobufDecorder.parseFromBase64(protoName, encodedText, function(err, message){
                protobufDecorder.attachMessage(req, res, message, true);
                return next();
            });

        } else {
            // 키가 없어서 프로토콜 버퍼 파싱 불가, 그대로 진행
            return next();
        }
    }
    else {
        // protocol buffer 데이터가 아니므로 그대로 진행
        return next();
    }
}
