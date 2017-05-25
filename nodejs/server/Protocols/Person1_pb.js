/**
 * @fileoverview
 * @enhanceable
 * @public
 */
// GENERATED CODE -- DO NOT EDIT!

var jspb = require('google-protobuf');
var goog = jspb;
var global = Function('return this')();

var _Header_pb = require('./_Header_pb.js');
goog.exportSymbol('proto.Lua.User.ReqPerson1', null, global);
goog.exportSymbol('proto.Lua.User.ResPerson1', null, global);

/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.Lua.User.ReqPerson1 = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.Lua.User.ReqPerson1, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  proto.Lua.User.ReqPerson1.displayName = 'proto.Lua.User.ReqPerson1';
}


if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto suitable for use in Soy templates.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     com.google.apps.jspb.JsClassTemplate.JS_RESERVED_WORDS.
 * @param {boolean=} opt_includeInstance Whether to include the JSPB instance
 *     for transitional soy proto support: http://goto/soy-param-migration
 * @return {!Object}
 */
proto.Lua.User.ReqPerson1.prototype.toObject = function(opt_includeInstance) {
  return proto.Lua.User.ReqPerson1.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Whether to include the JSPB
 *     instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.Lua.User.ReqPerson1} msg The msg instance to transform.
 * @return {!Object}
 */
proto.Lua.User.ReqPerson1.toObject = function(includeInstance, msg) {
  var f, obj = {
    header: (f = msg.getHeader()) && _Header_pb._ReqHeader.toObject(includeInstance, f),
    id: msg.getId()
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.Lua.User.ReqPerson1}
 */
proto.Lua.User.ReqPerson1.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.Lua.User.ReqPerson1;
  return proto.Lua.User.ReqPerson1.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.Lua.User.ReqPerson1} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.Lua.User.ReqPerson1}
 */
proto.Lua.User.ReqPerson1.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = new _Header_pb._ReqHeader;
      reader.readMessage(value,_Header_pb._ReqHeader.deserializeBinaryFromReader);
      msg.setHeader(value);
      break;
    case 10:
      var value = /** @type {string} */ (reader.readString());
      msg.setId(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Class method variant: serializes the given message to binary data
 * (in protobuf wire format), writing to the given BinaryWriter.
 * @param {!proto.Lua.User.ReqPerson1} message
 * @param {!jspb.BinaryWriter} writer
 */
proto.Lua.User.ReqPerson1.serializeBinaryToWriter = function(message, writer) {
  message.serializeBinaryToWriter(writer);
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.Lua.User.ReqPerson1.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  this.serializeBinaryToWriter(writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the message to binary data (in protobuf wire format),
 * writing to the given BinaryWriter.
 * @param {!jspb.BinaryWriter} writer
 */
proto.Lua.User.ReqPerson1.prototype.serializeBinaryToWriter = function (writer) {
  var f = undefined;
  f = this.getHeader();
  if (f != null) {
    writer.writeMessage(
      1,
      f,
      _Header_pb._ReqHeader.serializeBinaryToWriter
    );
  }
  f = this.getId();
  if (f.length > 0) {
    writer.writeString(
      10,
      f
    );
  }
};


/**
 * Creates a deep clone of this proto. No data is shared with the original.
 * @return {!proto.Lua.User.ReqPerson1} The clone.
 */
proto.Lua.User.ReqPerson1.prototype.cloneMessage = function() {
  return /** @type {!proto.Lua.User.ReqPerson1} */ (jspb.Message.cloneMessage(this));
};


/**
 * optional Lua._ReqHeader Header = 1;
 * @return {proto.Lua._ReqHeader}
 */
proto.Lua.User.ReqPerson1.prototype.getHeader = function() {
  return /** @type{proto.Lua._ReqHeader} */ (
    jspb.Message.getWrapperField(this, _Header_pb._ReqHeader, 1));
};


/** @param {proto.Lua._ReqHeader|undefined} value  */
proto.Lua.User.ReqPerson1.prototype.setHeader = function(value) {
  jspb.Message.setWrapperField(this, 1, value);
};


proto.Lua.User.ReqPerson1.prototype.clearHeader = function() {
  this.setHeader(undefined);
};


/**
 * Returns whether this field is set.
 * @return{!boolean}
 */
proto.Lua.User.ReqPerson1.prototype.hasHeader = function() {
  return jspb.Message.getField(this, 1) != null;
};


/**
 * optional string ID = 10;
 * @return {string}
 */
proto.Lua.User.ReqPerson1.prototype.getId = function() {
  return /** @type {string} */ (jspb.Message.getFieldProto3(this, 10, ""));
};


/** @param {string} value  */
proto.Lua.User.ReqPerson1.prototype.setId = function(value) {
  jspb.Message.setField(this, 10, value);
};



/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.Lua.User.ResPerson1 = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.Lua.User.ResPerson1, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  proto.Lua.User.ResPerson1.displayName = 'proto.Lua.User.ResPerson1';
}


if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto suitable for use in Soy templates.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     com.google.apps.jspb.JsClassTemplate.JS_RESERVED_WORDS.
 * @param {boolean=} opt_includeInstance Whether to include the JSPB instance
 *     for transitional soy proto support: http://goto/soy-param-migration
 * @return {!Object}
 */
proto.Lua.User.ResPerson1.prototype.toObject = function(opt_includeInstance) {
  return proto.Lua.User.ResPerson1.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Whether to include the JSPB
 *     instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.Lua.User.ResPerson1} msg The msg instance to transform.
 * @return {!Object}
 */
proto.Lua.User.ResPerson1.toObject = function(includeInstance, msg) {
  var f, obj = {
    errcode: msg.getErrcode(),
    age: msg.getAge(),
    name: msg.getName()
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.Lua.User.ResPerson1}
 */
proto.Lua.User.ResPerson1.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.Lua.User.ResPerson1;
  return proto.Lua.User.ResPerson1.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.Lua.User.ResPerson1} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.Lua.User.ResPerson1}
 */
proto.Lua.User.ResPerson1.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {number} */ (reader.readInt32());
      msg.setErrcode(value);
      break;
    case 11:
      var value = /** @type {number} */ (reader.readInt32());
      msg.setAge(value);
      break;
    case 12:
      var value = /** @type {string} */ (reader.readString());
      msg.setName(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Class method variant: serializes the given message to binary data
 * (in protobuf wire format), writing to the given BinaryWriter.
 * @param {!proto.Lua.User.ResPerson1} message
 * @param {!jspb.BinaryWriter} writer
 */
proto.Lua.User.ResPerson1.serializeBinaryToWriter = function(message, writer) {
  message.serializeBinaryToWriter(writer);
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.Lua.User.ResPerson1.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  this.serializeBinaryToWriter(writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the message to binary data (in protobuf wire format),
 * writing to the given BinaryWriter.
 * @param {!jspb.BinaryWriter} writer
 */
proto.Lua.User.ResPerson1.prototype.serializeBinaryToWriter = function (writer) {
  var f = undefined;
  f = this.getErrcode();
  if (f !== 0) {
    writer.writeInt32(
      1,
      f
    );
  }
  f = this.getAge();
  if (f !== 0) {
    writer.writeInt32(
      11,
      f
    );
  }
  f = this.getName();
  if (f.length > 0) {
    writer.writeString(
      12,
      f
    );
  }
};


/**
 * Creates a deep clone of this proto. No data is shared with the original.
 * @return {!proto.Lua.User.ResPerson1} The clone.
 */
proto.Lua.User.ResPerson1.prototype.cloneMessage = function() {
  return /** @type {!proto.Lua.User.ResPerson1} */ (jspb.Message.cloneMessage(this));
};


/**
 * optional int32 ErrCode = 1;
 * @return {number}
 */
proto.Lua.User.ResPerson1.prototype.getErrcode = function() {
  return /** @type {number} */ (jspb.Message.getFieldProto3(this, 1, 0));
};


/** @param {number} value  */
proto.Lua.User.ResPerson1.prototype.setErrcode = function(value) {
  jspb.Message.setField(this, 1, value);
};


/**
 * optional int32 Age = 11;
 * @return {number}
 */
proto.Lua.User.ResPerson1.prototype.getAge = function() {
  return /** @type {number} */ (jspb.Message.getFieldProto3(this, 11, 0));
};


/** @param {number} value  */
proto.Lua.User.ResPerson1.prototype.setAge = function(value) {
  jspb.Message.setField(this, 11, value);
};


/**
 * optional string Name = 12;
 * @return {string}
 */
proto.Lua.User.ResPerson1.prototype.getName = function() {
  return /** @type {string} */ (jspb.Message.getFieldProto3(this, 12, ""));
};


/** @param {string} value  */
proto.Lua.User.ResPerson1.prototype.setName = function(value) {
  jspb.Message.setField(this, 12, value);
};


goog.object.extend(exports, proto.Lua.User);