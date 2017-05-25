'use strict'

// 현재 디렉토리로 변경
process.chdir(__dirname);

// 디버그 모드 설정
var Config = require('./Data/ServerConfig.json');
var isDebug = false

//  프로토콜 버퍼 테스트 서버
class ServerApp
{
    constructor() {
        this.application = null
        this.InitApp();    
    }

    InitApp() {
    
        var express = require('express');
        var bodyParser = require('body-parser');
        var cookieParser = require('cookie-parser');
            
        // 예외를 가로채서 로그찍기 (서비스 모드에서만)
        // 예외를 가로채면 서버가 죽지는 않지만,
        // 문제가 발생했을 때 정확한 위치를 알기 힘듭니다.
        // 디버그 모드에서는 예외를 가로채지 않도록 합시다.
        process.on('uncaughtException', function (err) {
            
            //var logger = require('./Modules/Logger');
            //logger.error('[ServerApp] ' + err.stack);
            console.log(err.stack)
        });
         
        // HTTP 서버 띄우기
        var SECRET_KEY = '_sample_server_protocol_buffers';    
        var app = express();
        
        app.use(bodyParser.json());
        app.use(bodyParser.urlencoded({ extended: true }));
        app.use(cookieParser(SECRET_KEY));
        
        this.application = app;
        this._AttachPostParser(app);
        this._BindPackets(app);
    }

    // 표준 파서 진행후 추가로 처리되는 과정 처리
    _AttachPostParser(app) {

        // 필요하다면 서버가 닫혀있을때 사전 검사하는 루틴도 추가 가능
        let isCheckWhiteList = false;
        if(isCheckWhiteList) {
            // 보통 서버 점검 중일때 옵션을 켜서 서버를 시작하도록 합니다.
            // 
        }

        // 패킷 에러가 발생했을 경우 바로 에러 리스폰스 처리
        let packetErrHandler = require('./Modules/PacketErrorHandler');
        app.use(packetErrHandler);

        // Step1. 프로토콜 버퍼 사용중이라면 파싱
        let protobufParser = require('./Modules/ProtobufParser');
        app.use(protobufParser);
        
        // // Step2. 패킷 암호화 해제
        // let packetDecorder = require('./Modules/PacketDecorder');
        // app.use(packetDecorder);

        // Step3. 패킷 세션 확인 
        // 세션 토큰이 쿠키에 저장되어 전달 받는 다면 암호화 루틴 이전에 처리 가능 
        let sessionVerifier = require('./Modules/SessionVerifier');
        app.use(sessionVerifier);        
    }

    //==============================================================================
    // 패킷 연결 : 지정한 폴더 아래 있는 파일을 분석해서 자동으로 추가
    //==============================================================================
    _BindPackets(app) {
    
        var PACKET_CLASS_DIR = './Packets/';  // 패킷 클래스가 위치해 있는 폴더
        var PRE_WORD = 'Packet@'
        var EXT = '.js';

        var self = this;
        var fs = require('fs');
        var filenames = fs.readdirSync(PACKET_CLASS_DIR);   
        
        for (var k in filenames) {

            var filename = filenames[k];
            //var lowcasedName = filename.toLowerCase();

            if (filename.endsWith(EXT) && filename.startsWith(PRE_WORD)) {
                
                // 확장자 제거
                var packetName = filename.substring(0, filename.indexOf('.'));

                var parts = packetName.split('=');            
                var prefix = parts.shift();  // Packet_ 키워드는 잘라냅니다.
                var path = parts[0].replace('-', '/');

                try {

                    var reqFunc = require(PACKET_CLASS_DIR + filename);
                    if (reqFunc) {
                        
                        console.log('[REG] ' + prefix + ' : ' + path);
                        
                        if (prefix == PRE_WORD + 'post') app.post('/' + path, reqFunc);
                        else if (prefix == PRE_WORD + 'get') app.get('/' + path, reqFunc);
                        else {
                            console.error('[ServerApp] doest not support method : ' + filename);
                        }
                    }
                    else {
                        
                        // 등록된 API가 없습니다.
                        console.error('[ServerApp] Donnot regist API : ' + filename);
                    }
                }
                catch (e) {
                    console.error('[ServerApp] Exception : ' +  e +  '(' + filename + ')');
                    process.exit(0);
                }
            }
        }    

        // 미정의 패킷 
        app.post('/*', function (request, response) {
        
            var output = {
                Message : 'Unknown request type : "' + request.path + '"'
            }
            console.warn('[Game] unknown request : ' + request.path);
            response.status(403);
            response.json(output);
        });
    }

    ShowTitle(port, callback) {

        // 시스템 정보 요청
        let hostname = require('os').hostname()
        require('dns').lookup(hostname, function (err, ip, fam) {
            
            //var curVersion = require('./Modules/Version').CurrentVersion();
            let db_url = Config.DBUrl;
            
            // 타이틀 출력
            let url = 'http://' + ip;
            let title = 
            '\n===========================================================================' +
            '\n[ NARLAMY_SERVER ]' + (isDebug ? ' (DEBUG_MODE)' : '') + '\n' +
            '\n  URL : http://' + ip + ':' + port +
            '\n  HostName : ' + (hostname || '?') + 
            // '\n  ServerID : ' + serverId +
            // '\n  Service : ' + service +
            //'\n  Version : ' + curVersion.toText() + ' (' + curVersion.toNumber() + ')' +
            '\n  DB : http://' + db_url +
            '\n  Started : ' + new Date() +
            '\n===========================================================================\n';
            
            console.log(title);
            
            if(callback)
                callback(ip);
        });
    }

    //==============================================================================
    // 게임 서버 구동
    //==============================================================================
    _Run() {
        
        var DEFAULT_PORT = 19977;
        
        var self = this;
        var http = require('http');
        
        // 게임 포트 설정
        var port = Config.Port || DEFAULT_PORT;
        
        self.ShowTitle(port, function () {
            
            // HTTP 서버 구동
            http.createServer(self.application).listen(port, function () {
                
                console.log(">> Listen...");
            });
        });
        
        // 버전 처리
        // var Version = require('./Modules/Version');    
        // Version.LoadPatchVersion(function (curVersion) {
            
        //     // 암호화 하지 않은 패킷도 처리 가능하도록 설정
        //     //require('../Connection').isEncryptOnly = false;
            
        //     self.ShowTitle(port, function () {
            
        //         // HTTP 서버 구동
        //         http.createServer(self.application).listen(port, function () {
                    
        //             console.log(">> Listen...");
        //         });
        //     });
        // });
    }

    // 실행
    static Run() {
        var serverInst = new ServerApp();
        serverInst._Run();
        return serverInst;
    }
};

ServerApp.Run();