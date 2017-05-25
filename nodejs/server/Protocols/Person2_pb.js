/**
 * @fileoverview
 * @enhanceable
 * @public
 */
// GENERATED CODE -- DO NOT EDIT!

var jspb = require('google-protobuf');
var goog = jspb;
var global = Function('return this')();

goog.exportSymbol('proto.Lua.User.ReqPerson2', null, global);
goog.exportSymbol('proto.Lua.User.ResPerson2', null, global);

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
proto.Lua.User.ReqPerson2 = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.Lua.User.ReqPerson2, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  proto.Lua.User.ReqPerson2.displayName = 'proto.Lua.User.ReqPerson2';
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
proto.Lua.User.ReqPerson2.prototype.toObject = function(opt_includeInstance) {
  return proto.Lua.User.ReqPerson2.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Whether to include the JSPB
 *     instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.Lua.User.ReqPerson2} msg The msg instance to transform.
 * @return {!Object}
 */
proto.Lua.User.ReqPerson2.toObject = function(includeInstance, msg) {
  var f, obj = {
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
 * @return {!proto.Lua.User.ReqPerson2}
 */
proto.Lua.User.ReqPerson2.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.Lua.User.ReqPerson2;
  return proto.Lua.User.ReqPerson2.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.Lua.User.ReqPerson2} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.Lua.User.ReqPerson2}
 */
proto.Lua.User.ReqPerson2.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
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
 * @param {!proto.Lua.User.ReqPerson2} message
 * @param {!jspb.BinaryWriter} writer
 */
proto.Lua.User.ReqPerson2.serializeBinaryToWriter = function(message, writer) {
  message.serializeBinaryToWriter(writer);
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.Lua.User.ReqPerson2.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  this.serializeBinaryToWriter(writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the message to binary data (in protobuf wire format),
 * writing to the given BinaryWriter.
 * @param {!jspb.BinaryWriter} writer
 */
proto.Lua.User.ReqPerson2.prototype.serializeBinaryToWriter = function (writer) {
  var f = undefined;
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
 * @return {!proto.Lua.User.ReqPerson2} The clone.
 */
proto.Lua.User.ReqPerson2.prototype.cloneMessage = function() {
  return /** @type {!proto.Lua.User.ReqPerson2} */ (jspb.Message.cloneMessage(this));
};


/**
 * optional string ID = 10;
 * @return {string}
 */
proto.Lua.User.ReqPerson2.prototype.getId = function() {
  return /** @type {string} */ (jspb.Message.getFieldProto3(this, 10, ""));
};


/** @param {string} value  */
proto.Lua.User.ReqPerson2.prototype.setId = function(value) {
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
proto.Lua.User.ResPerson2 = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.Lua.User.ResPerson2, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  proto.Lua.User.ResPerson2.displayName = 'proto.Lua.User.ResPerson2';
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
proto.Lua.User.ResPerson2.prototype.toObject = function(opt_includeInstance) {
  return proto.Lua.User.ResPerson2.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Whether to include the JSPB
 *     instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.Lua.User.ResPerson2} msg The msg instance to transform.
 * @return {!Object}
 */
proto.Lua.User.ResPerson2.toObject = function(includeInstance, msg) {
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
 * @return {!proto.Lua.User.ResPerson2}
 */
proto.Lua.User.ResPerson2.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.Lua.User.ResPerson2;
  return proto.Lua.User.ResPerson2.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.Lua.User.ResPerson2} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.Lua.User.ResPerson2}
 */
proto.Lua.User.ResPerson2.deserializeBinaryFromReader = function(msg, reader) {
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
 * @param {!proto.Lua.User.ResPerson2} message
 * @param {!jspb.BinaryWriter} writer
 */
proto.Lua.User.ResPerson2.serializeBinaryToWriter = function(message, writer) {
  message.serializeBinaryToWriter(writer);
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.Lua.User.ResPerson2.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  this.serializeBinaryToWriter(writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the message to binary data (in protobuf wire format),
 * writing to the given BinaryWriter.
 * @param {!jspb.BinaryWriter} writer
 */
proto.Lua.User.ResPerson2.prototype.serializeBinaryToWriter = function (writer) {
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
 * @return {!proto.Lua.User.ResPerson2} The clone.
 */
proto.Lua.User.ResPerson2.prototype.cloneMessage = function() {
  return /** @type {!proto.Lua.User.ResPerson2} */ (jspb.Message.cloneMessage(this));
};


/**
 * optional int32 ErrCode = 1;
 * @return {number}
 */
proto.Lua.User.ResPerson2.prototype.getErrcode = function() {
  return /** @type {number} */ (jspb.Message.getFieldProto3(this, 1, 0));
};


/** @param {number} value  */
proto.Lua.User.ResPerson2.prototype.setErrcode = function(value) {
  jspb.Message.setField(this, 1, value);
};


/**
 * optional int32 Age = 11;
 * @return {number}
 */
proto.Lua.User.ResPerson2.prototype.getAge = function() {
  return /** @type {number} */ (jspb.Message.getFieldProto3(this, 11, 0));
};


/** @param {number} value  */
proto.Lua.User.ResPerson2.prototype.setAge = function(value) {
  jspb.Message.setField(this, 11, value);
};


/**
 * optional string Name = 12;
 * @return {string}
 */
proto.Lua.User.ResPerson2.prototype.getName = function() {
  return /** @type {string} */ (jspb.Message.getFieldProto3(this, 12, ""));
};


/** @param {string} value  */
proto.Lua.User.ResPerson2.prototype.setName = function(value) {
  jspb.Message.setField(this, 12, value);
};


goog.object.extend(exports, proto.Lua.User);