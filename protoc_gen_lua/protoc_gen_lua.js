'use strict'

const fs = require('fs')
const path = require('path');

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

var PB_TYPE = null;

function InitProtoTypeDefine() {
    let pb = require('google-protobuf/google/protobuf/descriptor_pb')
    PB_TYPE = pb.FieldDescriptorProto.Type;
}

// 루아 코드 생성기
class LuaGen {

    constructor() {

        InitProtoTypeDefine()

        this._definedMessages = {};
        this._outputText = "";
        this._SND_METHOD = 'CS.N2.Network.LuaSender.Send'
        this._PB_CALC_NAMESPACE = 'CS.N2.Network.LuaSender.'
        this._descriptors = []
        this._dependecies = []
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

        // 디스크립터 등록 (proto 파일 단위)
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
            self.WriteMessageDefine(reqMsg);
            self.WriteMessageDefine(resMsg);
            
            // methods
            self.WriteMethodGetIndex(reqMsg)
            self.WriteMethodGetIndex(resMsg)

            self.WriteMethodProperties(reqMsg, true)
            self.WriteMethodProperties(resMsg, false)
            
            self.WriteMethodCalcLength(reqMsg);
            self.WriteMethodCalcLength(resMsg);
            
            self.WriteMethodNew(reqMsg)
            self.WriteMethodNew(resMsg)

            // Request 객체에 맴버로 Response 객체를 연결
            self.WriteResponsAttaching(reqMsg, resMsg);
            
            // request에만 제공
            self.WriteMethodSend(reqMsg)
            
            // export
            self.WriteExport(reqMsg)
        } 
    
        // 문자열 반환
        return self._outputText;       
    }

    // 해당 이름의 message를 검색합니다.
    // 메세지 이름은 Package 이름이 포함 된 full name
    FindMessage(msgName) {
        return this._definedMessages[msgName];
    }

    // 등록 된 Descriptor를 전부 검색해서 message 이름에 preWord가 포함되는 message를 검색
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
            'end\n\n'
    }

    _MakeGetFunction(msgName, fieldName) {
        this._outputText += 
            'function ' + msgName + ':Get' + fieldName + '()\n' +
            '  return self.' + fieldName + ';\n' +
            'end\n\n'
    }

    WriteResponsAttaching(reqMsg, resMsg) {

        this._outputText += reqMsg.getName() + '.Res = ' + resMsg.getName() + ';\n\n'
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

                case PB_TYPE.TYPE_GROUP:
                    //self._outputText += self._WriteCalcLine('ComputeInt32Size', fieldName)
                    break;
            }
        } 

        self._outputText += '  return size;\n' + 'end\n\n'
    }

    _WriteCalcLine(method, fieldName) {
        return '  if self.' + fieldName + ' then size = size + ' + this._PB_CALC_NAMESPACE + method + '(self.' + fieldName + '); ' + ' end\n'
    }

    WriteMethodSend(msg) {
        this._outputText += 
            'function ' + msg.getName() + ':Send(onRes)\n' + 
            '  ' + this._SND_METHOD + '(self)\n' +
            'end\n\n'
    }

    WriteMethodGetIndex(msg) {

        let isFirst = true
        let self = this
        
        self._outputText += 'function ' + msg.getName() + ':GetIndex(fieldName)\n'        
        self._WriteGetIndexCondition(msg, isFirst)
        self._outputText += '  return -1;\n' + 'end\n\n';
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

    WriteMethodNew(msg) {
        let msgName = msg.getName()
        this._outputText += 
            'function ' + msgName + ':new()\n' + 
            '  return setmetatable(self or {}, {_index=' + msgName + '});\n' +
            'end\n\n';
    }

    WriteExport(msg) {
        if(!msg) return;
        this._outputText += '\n' + 'return ' + msg.getName() + '.new();\n'
    }

    Blink(step) {        
        let out = ''
        let n = step || 0
        while(n-- > 0) out += '  '
        //return '<' + out + (out.length).toString() + '>'
        return out;
    }

    // 프로토콜 루아 테이블 (맴버 정의 및 초기화)
    WriteMessageDefine(msg, step) {

        if(!msg || !msg.getFieldList) return;

        let self = this;
        let msgName = msg.getName()
        
        if(step==undefined) step = 0;

        // 맴버 정의 
        // 메세지 정의 일 경우에만 메세지 이름을 적용시키고 
        // 서브 메세지인 경우에는 블록만 정의합니다.
        // TYPE = { M1=0, M2=0 }
        self._outputText += (step==0 ? msgName : '') + ' = {\n';
        
        let isFirst = true;
        let fields = msg.getFieldList()

        for(let k in fields) {

            let field = fields[k] 
            
            // 이전 데이터 정의와 구분
            if(!isFirst) self._outputText += ',\n'
            else  isFirst = false

            // 깊이에 따른 공백 처리
            self._outputText += self.Blink(step+1)

            // 맴버 이름 출력
            self._outputText += field.getName()

            // 타입 이름 : Message인 경우에만 유효
            let typeName = field.getTypeName();

            // if(typeName)
            //     console.error('>> ' + field.getName() + ' = ' + field.getNumber() + ' (' + field.getType() + ') : '+ field.getTypeName())

            let sub = self.FindMessage(typeName)
            if(sub) {
                // 서브 메세지 
                self.WriteMessageDefine(sub, step+1)
            }
            else {
                // 값 출력
                self._outputText += ' = ' + self._getInitText(field)
            }
        }

        if(step > 0)
            self._outputText += '\n' + self.Blink(step) + '}';
        else
            self._outputText += '\n}\n\n';
    }

    _getInitText (field) {

        switch(field.getType()) {
            case PB_TYPE.TYPE_INT64:
            case PB_TYPE.TYPE_INT32:
            case PB_TYPE.TYPE_UINT64:
            case PB_TYPE.TYPE_UINT32:
            case PB_TYPE.TYPE_FIXED64:
            case PB_TYPE.TYPE_FIXED32:
            case PB_TYPE.TYPE_SFIXED64:
            case PB_TYPE.TYPE_FIXED32:
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
