module.exports = function (request, response) {
    
    var Version = require('../Version.js');
    var Config = require('../Common/ServerConfig.js')

    var GameInfo = {
        Version: Version.CurrentVersion().toNumber(),
        ServerID: Config.ServerID,
        Service: Config.Service,
    }
    response.status(200).send(GameInfo);
}