

module.exports = function (request, response) {
    
    var packet = require('../Packets/PvpPacket');
    require('../Connection').BindSkipVersion(request, true);
    packet.CalcResult(request, response);
}