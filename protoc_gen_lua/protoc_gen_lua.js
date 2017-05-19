'use strict'

// var goog = require('google-protobuf');
// var descriptor = require('./node_modules/google-protobuf/google/protobuf/descriptor_pb.js');

//descriptor.DescriptorProto();
//descriptor.FieldDescriptorProto

// var buf;

// var serialized = process.stdin.read()
// if(serialized == null) {
//     let fs = require('fs')
//     buf = fs.readFileSync('d:/proto/Sample.proto');
    
//     console.log('read from file')
// }
// else {
//     console.log('buf lenght = ' + buf.length)

//     buf = serialized.buffer;
//     console.log('stdin')
// }

// if(buf) {
// }

let fs = require('fs')

class Gen {

    constructor() {
        this._definedMessages = {};
        this._outputText = "";
        this._SND_METHOD = 'CS.N2.Network.LuaSender.Send'
    }

    Start() {
        let self = this;
        let pbjs = require('protobufjs');
        let path = 'd:/Projects/TestPB/proto/Person.proto'
        //let path = 'd:/proto/Sample.proto'
        pbjs.load(path).
        then((root) => {

            self._Look(root);
            self.GenearteLuaCode(root);
            self.SaveToFile();

            console.log('complete')
            process.exit(0)
        }).
        catch((err)=> {
            console.log('ERROR : ' + err)
            process.exit(0)
        })
    }

    // 루아 코드 생성
    GenearteLuaCode(root) {

        let self = this;

        // 메세지 수집
        self.CollectMessage(root);

        // message defines
        for(let mk in self._definedMessages) {
            self.WriteMessage(self._definedMessages[mk]);
        }

        // 
        let reqMsg = self.FindMessageWith('Req');
        let resMsg = self.FindMessageWith('Res');

        // methods
        self.WritePropertyMethods(reqMsg)
        self.WriteMethodSend(reqMsg)
        self.WriteMethodGetIndex(reqMsg)
        self.WriteAttachRes(reqMsg, resMsg);        
        self.WriteNew(reqMsg)

        // export
        self.WriteExport(reqMsg)            
    }

    SaveToFile() {

        let err = fs.writeFileSync('./Person.lua', this._outputText)
        if(err) {
            console.log('Error => ' + err)
        }   
    }

    FindMessage(msgName) {
        return this._definedMessages[msgName];
    }

    CollectMessage(node) {        

        if(node==null) return;
        let self = this;
        
        if(node.fields) {
            self._definedMessages[node.name] = node;
        }

        // 자식 노드로 탐색 
        // fields 정보가 없어도 탐색에 들어가야합니다.
        for(let n in node.nested) {

            let child = node.nested[n];
            self.CollectMessage(child);      
        }
    }

    FindMessageWith (preWord) {        
        for(let k in this._definedMessages) {
            if(k.startsWith(preWord))
                return this._definedMessages[k]
        }
        return null;
    }

    WritePropertyMethods(msg) {
        
        var self = this

        for(let f in msg.fields) {
            let field = msg.fields[f]
            switch(field.type) {
                case 'int32':
                case 'int64':
                case 'uint32':
                case 'uint64':
                case 'fixed64':
                case 'fixed32':
                    self._MakeSetFunction(msg.name, field.name);
                    //self._MakeGetFunction(msg.name, field.name);
                    break;

                case 'float':
                case 'double':
                    self._MakeSetFunction(msg.name, field.name);
                    //self._MakeGetFunction(msg.name, field.name);
                    break;

                case 'string':
                    self._MakeSetFunction(msg.name, field.name);
                    break;

                case 'Struct':
                    self._MakeSetFunction(msg.name, field.name);
                    break;
            }
        }        
    }

    _MakeSetFunction(msgName, fieldName) {
        var varName = fieldName.toLowerCase()
        this._outputText += 
            'function ' + msgName + ':Set' + fieldName + '(' + varName + ')\n' +
            '  self.' + fieldName + '=' + varName + ';\n  return self\n' +
            'end\n'
    }

    _MakeGetFunction(msgName, fieldName) {
        this._outputText += 
            'function ' + msgName + ':Get' + fieldName + '(' + varName + ')\n' +
            //'  self.' + fieldName + '=' + varName + ';\n   return self\n' + 
            '  return self.' + fieldName + '\n' +
            'end\n'
    }

    WriteAttachRes(reqMsg, resMsg) {

        this._outputText += reqMsg.name + '.Res=' + resMsg.name + '\n'
    }

    WriteMethodSend(msg) {
        this._outputText += 
            'function ' + msg.name + ':Send(onRes)\n' + 
            '  ' + this._SND_METHOD + '(self)\n' +
            'end\n'
    }

    WriteMethodGetIndex(msg) {

        let isFirst = true
        let self = this
        
        self._outputText += 'function ' + msg.name + ':GetIndex(fieldName)\n'        
        self._WriteGetIndexCondition(msg, isFirst)
        self._outputText += '}\nend\n';
    }

    _WriteGetIndexCondition(msg, isFirst) {

        let self = this
        ;
        for(let k in msg.fields) {

            let field = msg.fields[k]
            var op = isFirst ? 'if' : 'elseif'
            self._outputText += '  ' + op + ' fieldName == "' + field.name + '" then ' + 'return '+ field.id + ';\n'

            let sub = self.FindMessage(field.name)
            if(sub) {
                self._WriteGetIndexCondition(sub)            
            }
        }
    }

    WriteNew(msg) {
        this._outputText += 
            'function ' + msg.name + ':new()\n' + 
            '  return setmetatable(self or {}, {_index=' + msg.name + '})\n' +
            'end\n';
    }

    WriteExport(msg) {
        if(!msg) return;
        this._outputText += '\n' + 'return ' + msg.name + '.new()'
    }

    Blik(space) {        
        let o = ''
        let r = space;
        while(r-- > 0) o += '  '
        return o
    }

    WriteMessage(node, step) {

        if(!node || !node.fields) return;
        let self = this;
        let isSubMessage = step ? true : false;

        // 맴버 정의
        // TYPE = { M1=0, M2=0 }
        self._outputText += (isSubMessage ? '' : node.name) + ' = {\n  ';
        let isFirst = true;
        
        for(let k in node.fields) {

            self._outputText += self.Blik(step)

            if(!isFirst) self._outputText += ', '
            else  isFirst = false

            let field = node.fields[k] 
            self._outputText += field.name

            let msg = self.FindMessage(field.type)
            if(msg) {
                // 
                //self._outputText += 'nil'
                self.WriteMessage(msg, step ? step+1 : 1)
            }
            else {
                // general type
                 self._outputText += '=' + (field.type=='string' ? '""': 0)
            }
        }

        if(isSubMessage)
            self._outputText += self.Blik(step) + '\n}';
        else 
            self._outputText += '\n}\n\n';
    }

    _Look(node) {

        if(node==null) return;
        let self = this;
        
        if(node.fields) {
            
            // 필드가 있는 경우에만 처리
            if(node.parent)
                console.log(node.parent.name + '.' + node.name);
            else 
                console.log(node.name);

            for(let f in node.fields) {

                let field = node.fields[f];
                console.log('   >>> ' + node.name + '.' + field.name + ' : ' + field.type);
        
            }
        }

        // 자식 노드로 탐색 
        // fields 정보가 없어도 탐색에 들어가야합니다.
        for(let n in node.nested) {

            let child = node.nested[n];
            self._Look(child);
        }
    }
}

new Gen().Start();
