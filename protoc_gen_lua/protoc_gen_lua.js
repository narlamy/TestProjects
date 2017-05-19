'use strict'

var goog = require('google-protobuf');
var pb = require('./node_modules/google-protobuf/google/protobuf/compiler/plugin_pb.js')
var desc_pd = require('./node_modules/google-protobuf/google/protobuf/descriptor_pb.js');

desc_pd.

var serialized = process.stdin.read()

var buf;
var fs = require('fs')
if(serialized == null) {
    buf = fs.readFileSync('d:/proto/Sample.proto');
    
    console.log('read from file')
}
else {
    console.log('buf lenght = ' + buf.length)

    buf = serialized.buffer;
    console.log('stdin')
}

//pb.CodeGeneratorRequest()
//console.log(pb.CodeGeneratorRequest.displayName);
var p = pb.CodeGeneratorRequest.deserializeBinary(buf);

var m = goog.Message;
var out = m.deserializeBinary(buf);

console.log(serialized.toString());

var response = pb.CodeGeneratorResponse();

for(file in request.proto_file) {

}

console.log('h')

