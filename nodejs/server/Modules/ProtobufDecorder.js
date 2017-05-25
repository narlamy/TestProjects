var getProtoName = function(path) {

    var tok = path.split('/');
    return tok[tok.length-1]
}

var getProtoFileName = function(protoName) {

    return '../Protocols/' + protoName.substring(3) + '_pb.js';
}

var loadProtoMessage = function (protoName) {

    try {
        let filePath = getProtoFileName(protoName);
        let PROTOCOL = require(filePath)
        return PROTOCOL[protoName];
    }
    catch(e) {
        console.error(e)
        return null;
    }
}

var createMessage = function (path, buf, callback) {

    let protoName = getProtoName(path);
    let prototype = loadProtoMessage(protoName);

    if(prototype)
        callback(null, prototype.deserializeBinary(buf));
    else 
        callback(new Error('protocol file does not exists it : ' + path));
}

// base64로 인코딩 된 문자열 입력
// protocol buffer
var parseFromBase64 = function (protoName, base64String, callback) {

    //console.log('[ProtobufDecorder] parseFromBase64("' + base64String + '")')

    // (1) base64를 buffer로 변경
    // (2) buffer를 proto type을 참조해서  message 객체로 변경
    // (3) 메세지를 바로 전달하든지 변형해서 리턴

    try {
        //let Buffer = require('buffer');
        let buf = Buffer.from(base64String, 'base64');

        // Buffer에 있는 내용을 가지고 message 객체를 반환합니다.
        createMessage(protoName, buf, callback);
        
        //callback(null, { _buf : buf, _txt : base64String });
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

    // 프로토콜 버퍼 인스턴스를 입력받아서 응답처리합니다.
    // 
    res.SendProtob = function(protob_messag, option) {

        let isOption = typeof(option) == 'object';
        let errCode = isOption ? option.errCode : option
        
        if(errCode===undefined)
            errCode=0;

        if(isOption && option.bin) {

            if(protob_messag.ErrCode!=undefined)
                protob_messag.ErrCode = option.errCode;

            // 직접 바이너리로 전달합니다.
            let bin = protob_messag.serializeBinary();
            let buf = new Buffer(bin);
            //this.set('Content-Type', 'n2pb')
            //console.log('res.send() => ' + new Date())
            this.send(buf);
        }
        else {

            // 프로토콜 버퍼 메세지 객체를 Base64 문자열로 전달합니다.
            let retObj = protob_messag.toObject();
            let buf = protob_messag.serializeBinary();
            let base64Text = Buffer.from(buf, 'utf8').toString('base64')
            let name = protob_messag.protoName || "";
            let output = { ErrorCode: errCode, __pbDat : base64Text, __pbName : name, __debug: retObj }
            this.json(output);   
        }
    }

    res.SendProtobBin = function(protob_messag, errCode) {
        this._SendBinProtoMessage(protob_messag, { errCode:errCode, bin:true})
    }

    res._protoName = getProtoName(req.path).replace('Req','Res')
    res.CreateMessage = function() {
        let self_res = this
        let prototype = loadProtoMessage(self_res._protoName);
        let instance = new prototype();
        instance.Send = function(option) {
            self_res.SendProtob(this, option);
        }
        return instance;
    }

    // body에 protocol buffer message의 프로퍼티를 복사합니다.
    if(copyToBody && message) {
        
        if(!req.body)
            req.body = {}
        
        var obj = message.toObject();        
        for(let k in obj) {
            var field = obj[k];
            if(typeof(field) != 'function' && !k.startsWith('_')) {
                req.body[k] = field;
            }
        }
    }
}

var parseFrom = function(req, callback) {

    let BIN_KEY = '__pbDat'
    let NAME_KEY = '__pbName'

    let encodedText = req.body[BIN_KEY];
    let protoName = req.body[NAME_KEY] || getProtoName(req.path);

    if(encodedText) {

        req.body[BIN_KEY] = null;
        req.body[NAME_KEY] = null;

        parseFromBase64(protoName, encodedText, callback);
    } else {
        callback(new Error('not found key'), null);
    }
}

exports.createMessage = createMessage;
exports.attachMessage = attachMessage;
exports.parseFrom = parseFrom;
exports.parseFromBase64 = parseFromBase64;