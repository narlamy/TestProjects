'use strict'

const fs = require('fs')
const path = require('path');
let TYPE = {
    Fixed32 : 0,    
    Fixed64 : 0,
    Sfixed32 : 0,    
    Sfixed64 : 0,
    Int32 : 5,
    Int64 : 6,
    Sint32 : 0,
    Sint64 : 0,
    Float : 0,
    Double : 0,
    Bool : 0,
    String : 9,
    Bytes : 0,
    Struct : 11
}

/*
proto.google.protobuf.FieldDescriptorProto.Type = {
  TYPE_DOUBLE: 1,
  TYPE_FLOAT: 2,
  TYPE_INT64: 3,
  TYPE_UINT64: 4,
  TYPE_INT32: 5,
  TYPE_FIXED64: 6,
  TYPE_FIXED32: 7,
  TYPE_BOOL: 8,
  TYPE_STRING: 9,
  TYPE_GROUP: 10,
  TYPE_MESSAGE: 11,
  TYPE_BYTES: 12,
  TYPE_UINT32: 13,
  TYPE_ENUM: 14,
  TYPE_SFIXED32: 15,
  TYPE_SFIXED64: 16,
  TYPE_SINT32: 17,
  TYPE_SINT64: 18
};
 */

var PB_TYPE ;

function GetProtoTypeDefine() {
    let pb = require('google-protobuf/google/protobuf/descriptor_pb')
    PB_TYPE = pb.FieldDescriptorProto.Type;
}

// 루아 코드 생성기
class LuaGen {

    constructor() {
        this._definedMessages = {};
        this._outputText = "";
        this._SND_METHOD = 'CS.N2.Network.LuaSender.Send'
        this._PB_CALC_NAMESPACE = 'CS.N2.Network.LuaSender.'
        this._descriptors = []
        this._dependecies = []
        GetProtoTypeDefine()
    }

    // protoc plugin으로 입력받은 binary data를 parsing해서 
    // 파일을 생성합니다. (stdin stream으로 전달 받음)
    Execute() {    

        let self = this;
        const pluginPb = require('google-protobuf/google/protobuf/compiler/plugin_pb')

        process.stdin.on('readable', (a,b,c)=> {

            if(process.stdin.readable) {

                //process.stdin.setEncoding('utf8')
                let buf = process.stdin.read();
                if(buf) {

                    let genReq = pluginPb.CodeGeneratorRequest;
                    let msg = genReq.deserializeBinary(buf);

                    let protoFiles = msg.getProtoFileList();
                    for(let k in protoFiles) {

                        let desc = protoFiles[k];
                        let protoName = desc.getName();
                        let p = desc.getPackage();

                        self.AddDescriptor(desc)
                    }
                }
            }
        })

        process.stdin.on('end', (x)=> {

            let self = this;
            let genRes = new pluginPb.CodeGeneratorResponse();
            
            // 코드 생성
            for(let i in self._descriptors) {
                
                let desc = self._descriptors[i];
                let filename = path.basename(desc.getName(), '.proto');
                let outputName = filename + '.lua'
                
                    
                var content = self.GenearteLuaCode(filename, desc);

                if(content && content.length > 0) {

                    const file = new pluginPb.CodeGeneratorResponse.File();
                    outputName && file.setName(outputName)
                    content && file.setContent(content);
                    
                    genRes.addFile(file);
                }
            }

            process.stdout.write( new Buffer( genRes.serializeBinary() ) )
            process.exit(0)
        });
        
    }

    AddDescriptor(desc) {

        // 모든 디스크립터 수집
        this._descriptors.push(desc)

        // 메세지 이름으로 빠르게 검색할 수 있는 커랙터 처리
        this.CollectionMessage(desc);
    }

    Init() {
        this._outputText = "";
        this._dependecies = []
    }

    // 루아 코드 생성
    GenearteLuaCode(descName, desc) {

        let self = this;

        // intialize
        self.Init()
        
        // message defines
        let reqMsg = self.FindMessage(self._MakeMsgKey(desc, 'Req' + descName));
        let resMsg = self.FindMessage(self._MakeMsgKey(desc, 'Res' + descName));

        if(reqMsg && resMsg) {

            // defines
            //self.WriteExternMessage(desc)
            self.WriteMessage(reqMsg);
            self.WriteMessage(resMsg);
            
            // methods
            self.WriteMethodProperties(reqMsg)
            self.WriteMethodSend(reqMsg)
            self.WriteMethodGetIndex(reqMsg)
            self.WriteMethodCalcLength(reqMsg);
            self.WriteMethodCalcLength(resMsg);
            self.WriteAttachRes(reqMsg, resMsg);        
            self.WriteNew(reqMsg)

            // export
            self.WriteExport(reqMsg)
        } 
    
        // 문자열 반환
        return self._outputText;       
    }

    FindMessage(msgName) {
        return this._definedMessages[msgName];
    }

    ScanMessage(preWord, dependencyName) {

        for(let k in this._descriptors) {
            let desc = this._descriptors[k];
            if(desc.getName() == dependencyName) {
                let msgList = desc.getMessageTypeList()
                for(let f in msgList){
                    let msg = msgList[f]                    
                    if(msg.getName().includes(preWord)) {
                        return msg;
                    }
                }
                return null;
            }
        }
    }

    _MakeMsgKey (desc, msgName) {
        return '.' + desc.getPackage() + '.' + (msgName || '');
    }

    CollectionMessage(desc) {

        if(desc==null) return;

        let self = this;
        let messageTypes = desc.getMessageTypeList();
        const keyPreWord = self._MakeMsgKey(desc);
        
        for(let k in messageTypes) {
            
            let msg = messageTypes[k];
            let key = keyPreWord + msg.getName()
            self._definedMessages[key || '?'] = msg;

            self.CollectionNestedMessage(msg, key, desc)
        }
    }

    CollectionNestedMessage(msg, key, desc) {
        
        let self = this        
        //console.error('>> ' + key + '\t\tin "' + desc.getName() + '"');

        if(msg==null || !msg.getNestedTypeList) return

        const keyPreWord = key + '.';

        let nestedList = msg.getNestedTypeList()

        for(let k in nestedList) {

            let nested = nestedList[k]          
            let key = keyPreWord + nested.getName()  
            self._definedMessages[key || '?'] = nested;

            self.CollectionNestedMessage(nested, key, desc)
        }
    }

    WriteMethodProperties(msg, isSetter) {

        let self = this
        let fields = msg.getFieldList()

        for(let f in fields) {

            let field = fields[f]
            let msgName = msg.getName();
            let fieldName = field.getName()

            switch(field.getType()) {               
                case PB_TYPE.TYPE_ENUM:
                case PB_TYPE.TYPE_MESSAGE:
                case PB_TYPE.TYPE_GROUP:
                    // if(isSetter) self._MakeSetFunction(msgName, fieldName);
                    // else self._MakeGetFunction(msgName, fieldName);
                    // ToDo: 어떤식으로 표현해줘야 할까?
                    break;

                default:
                    if(isSetter) self._MakeSetFunction(msgName, fieldName);
                    else self._MakeGetFunction(msgName, fieldName);
                    break;
            }
        }        
    }

    _MakeSetFunction(msgName, fieldName) {
        var varName = fieldName.toLowerCase()
        this._outputText += 
            'function ' + msgName + ':Set' + fieldName + '(' + varName + ')\n' +
            '  self.' + fieldName + '=' + varName + ';\n  return self;\n' +
            'end\n'
    }

    _MakeGetFunction(msgName, fieldName) {
        this._outputText += 
            'function ' + msgName + ':Get' + fieldName + '()\n' +
            '  return self.' + fieldName + ';\n' +
            'end\n'
    }

    WriteAttachRes(reqMsg, resMsg) {

        this._outputText += reqMsg.getName() + '.Res=' + resMsg.getName() + ';\n'
    }

    // 데이터가 저장 될 크기를 계산합니다.
    // 일부 데이터는 가변 길이를 갖습니다.
    WriteMethodCalcLength(msg) {

        let self = this

        self._outputText += 'function ' + msg.getName() + ':CalcSize()\n' + 
            '  local size = 0;\n';
        
        let fields = msg.getFieldList()        
        for(let f in fields) {

            let field = fields[f]
            let msgName = msg.getName();
            let fieldName = field.getName()

            switch(field.getType()) {
                case PB_TYPE.TYPE_INT64:
                case PB_TYPE.TYPE_UINT64:
                    self._outputText += self._WriteCalcLine('ComputeInt64Size', fieldName)
                    break;

                case PB_TYPE.TYPE_FIXED64:
                case PB_TYPE.TYPE_SFIXED64:
                    self._outputText += self._WriteCalcLine('ComputeFixed64Size', fieldName)
                    break;

                case PB_TYPE.TYPE_INT32:
                case PB_TYPE.TYPE_UINT32:
                    self._outputText += self._WriteCalcLine('ComputeInt32Size', fieldName)
                    break;

                case PB_TYPE.TYPE_FIXED32:
                case PB_TYPE.TYPE_SFIXED32:
                    self._outputText += self._WriteCalcLine('ComputeFixed32Size', fieldName)
                    break;

                case PB_TYPE.TYPE_FLOAT:
                    self._outputText += self._WriteCalcLine('ComputeFloatSize', fieldName)
                    break;

                case PB_TYPE.TYPE_DOUBLE:
                    self._outputText += self._WriteCalcLine('ComputeDoubleSize', fieldName)
                    break;

                case PB_TYPE.TYPE_STRING:
                    self._outputText += self._WriteCalcLine('ComputeStringSize', fieldName)
                    break;

                case PB_TYPE.TYPE_BOOL:
                    self._outputText += self._WriteCalcLine('ComputeBoolSize', fieldName)
                    break;

                case PB_TYPE.TYPE_MESSAGE:
                    //self._outputText += self._WriteCalcLine('ComputeBoolSize', fieldName)
                    break;

                case TYPE.GROUP:
                    //self._outputText += self._WriteCalcLine('ComputeInt32Size', fieldName)
                    break;
            }
        } 

        self._outputText += '  return size;\n' + 'end\n'
    }

    _WriteCalcLine(method, fieldName) {
        return '  if self.' + fieldName + ' then size = size + ' + this._PB_CALC_NAMESPACE + method + '(self.' + fieldName + '); ' + ' end\n'
    }

    WriteMethodSend(msg) {
        this._outputText += 
            'function ' + msg.getName() + ':Send(onRes)\n' + 
            '  ' + this._SND_METHOD + '(self)\n' +
            'end\n'
    }

    WriteMethodGetIndex(msg) {

        let isFirst = true
        let self = this
        
        self._outputText += 'function ' + msg.getName() + ':GetIndex(fieldName)\n'        
        self._WriteGetIndexCondition(msg, isFirst)
        self._outputText += '\nend\n';
    }

    _WriteGetIndexCondition(msg, isFirst) {

        let self = this
        let fields = msg.getFieldList()

        for(let k in fields) {

            let field = fields[k]

            var op = isFirst ? 'if' : 'elseif'
            self._outputText += '  ' + op + ' fieldName == "' + field.getName() + '" then ' + 'return '+ field.getNumber() + '; end\n'

            let sub = self.FindMessage(field.getName())
            if(sub) {
                self._WriteGetIndexCondition(sub)            
            }
        }
    }

    WriteNew(msg) {
        this._outputText += 
            'function ' + msg.name + ':new()\n' + 
            '  return setmetatable(self or {}, {_index=' + msg.getName() + '});\n' +
            'end\n';
    }

    WriteExport(msg) {
        if(!msg) return;
        this._outputText += '\n' + 'return ' + msg.getName() + '.new();'
    }

    Blik(space) {        
        let o = ''
        let r = space;
        while(r-- > 0) o += '  '
        return o
    }

    // 프로토콜 루아 테이블 (맴버 정의 및 초기화)
    WriteMessage(msg, step) {

        if(!msg || !msg.getFieldList) return;

        let self = this;
        let isSubMessage = step ? true : false;
        let msgName = msg.getName()

        // 맴버 정의
        // TYPE = { M1=0, M2=0 }
        self._outputText += (isSubMessage ? '' : msgName) + ' = {\n  ';
        let isFirst = true;
        let fields = msg.getFieldList()

        for(let k in fields) {

            self._outputText += self.Blik(step)

            if(!isFirst) self._outputText += ', '
            else  isFirst = false

            let field = fields[k] 
            self._outputText += field.getName()

            let typeName = field.getTypeName();

            // if(typeName)
            //     console.error('>> ' + field.getName() + ' = ' + field.getNumber() + ' (' + field.getType() + ') : '+ field.getTypeName())

            let sub = self.FindMessage(typeName)
            if(sub) {
                self.WriteMessage(sub, step ? step+1 : 1)
            }
            else {
                // general type
                self._outputText += '=' + self._getInitText(field)
            }
        }

        if(isSubMessage)
            self._outputText += self.Blik(step) + '\n}';
        else 
            self._outputText += '\n}\n\n';
    }

    _getInitText (field) {

        switch(field.getType()) {
            case PB_TYPE.INT64:
            case PB_TYPE.INT32:
            case PB_TYPE.UINT64:
            case PB_TYPE.UINT32:
            case PB_TYPE.FIXED64:
            case PB_TYPE.FIXED32:
            case PB_TYPE.SFIXED64:
            case PB_TYPE.SFIXED32:
                return '0';

            case PB_TYPE.TYPE_FLOAT:
            case PB_TYPE.TYPE_DOUBLE:
                return '0.0';

            case PB_TYPE.TYPE_STRING:
                return '""';

            case PB_TYPE.TYPE_BOOL:
                return 'false'

            case PB_TYPE.TYPE_MESSAGE:
                return "{}"
        }
    }
}

new LuaGen().Execute();
