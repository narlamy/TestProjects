'use strict'

// Protocol Buffer 형식의 패킷을 파싱합니다.
module.exports = function(req, res, next) {

    let protobufDecorder = require('./ProtobufDecorder');
            
    let conetntType = req.headers['content-type'];
    if(conetntType === 'n2pb') { // || conetntType === 'application/octet-stream') {

        //console.log('req.parse() => ' + new Date())

        // binary
        let getRawBody = require('raw-body');
        let option =  {
            length: req.headers['content-length']
        }
        
        getRawBody(req, option, function (err, buf) {
            
            if (err) return next(err)
                
            protobufDecorder.createMessage(req.path, buf, function(err, message) {

                if(!err)
                    protobufDecorder.attachMessage(req, res, message, true);

                if(!req.body)
                    req.body = {}

                return next();
            })
        })
    } else if(conetntType.contains('json') && req.headers['n2_pet']=='pb') {

        // text 기반인 경우        
        protobufDecorder.parseFrom(req, function(err, message){   

            if(!err)
                protobufDecorder.attachMessage(req, res, message, true);
            return next();
        });
    } else {
        return next();
    }
}
