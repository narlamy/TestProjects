'use strict'

// Protocol Buffer 형식의 패킷을 파싱합니다.
module.exports = function(req, res, next) {

    var encode = req.headers ? req.headers.encode : null;
    if(encode == 'protobuf') {

        let protobufDecorder = require('./ProtobufDecorder');
                
        let conetntType = req.headers['conent-type'];
        if(conetntType === 'meta' || conetntType === 'application/octet-stream') {

            // binary
            var getRawBody = require('raw-body');

            getRawBody(req, {
                length: req.headers['content-length']
            }, function (err, buf) {
                
                if (err)
                    return next(err)
                protobufDecorder.createMessage(req.path, buf, function(err, message) {

                    if(!err)
                        protobufDecorder.attachMessage(req, res, message, true);

                    if(!req.body)
                        req.body = {}

                    return next();
                })
            })
        } else {

            // text 기반인 경우        
            protobufDecorder.parseFrom(req, function(err, message){   

                if(!err)
                    protobufDecorder.attachMessage(req, res, message, true);
                return next();
            });
        }
    }
    else {
        // protocol buffer 데이터가 아니므로 그대로 진행
        return next();
    }
}
