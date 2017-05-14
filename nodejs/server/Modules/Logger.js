var util = require('util');

var _logInstance = new Logger();

//-----------------------------------------------------------------------------
// Logger 객체 생성자
// require('logger')를 호출하면 Logger 인스턴스가 반환됩니다.
//-----------------------------------------------------------------------------
function Logger ()
{
    var MAX_FILE_SIZE = 200 * 1024 * 1024;  // 200MB 단위로 (NotePad++에서 너무 크면 못 불러옴)
    var MAX_FILES = 10000;
    
    //var CONFIG = require('./Common/ServerConfig');
    var isHistory = false //CONFIG.DevSetting.IsHistory || false;
    var isDebug = true //CONFIG.IsDebug();
    var isLive = false //CONFIG.IsLive();
    var isQA = false //CONFIG.IsQA();
    
    // 현재 버전 문자열
    //var version = require('./Version.js').CurrentVersion().toString();        
    
    // application name
    var path = process.argv[1];
    var separator = path.indexOf("\\") > 0 ? '\\' : '/';
    var tok = path.split(separator);
    var appName = tok[tok.length - 1];
    
    var dotIndex = appName.indexOf('.');
    if (dotIndex >= 0)
        appName = appName.slice(0, dotIndex);
    
    // start winston logger
    var Winston = require('winston');
    var Slack = require('slack-winston').Slack;
	var WinstonMongo = require('winston-mongodb').MongoDB; // 자동으로 winston안에 MongoDB 추가됨
    
    // 로그 레벨 참조
    //levels = {
    //	silly : 0,
    //	debug : 1,
    //	verbose : 2,
    //    info : 3,
    //	warn : 4,		
    //    error : 5
    //}

    // 클리어
    Winston.clear();
    
    var LOG_DIR_PATH = '../../Logs';
    var fs = require('fs');
    if (!fs.existsSync(LOG_DIR_PATH))
        fs.mkdirSync(LOG_DIR_PATH);    
    
    // 일반 로그 : 무조건 파일에는 남깁니다.
    if (isHistory) {
        Winston.add(Winston.transports.File, {
            level : isDebug ? 'debug' : 'info',
            filename: LOG_DIR_PATH + '/[sc] ' + appName + '_history.log',
            maxsize : MAX_FILE_SIZE,
            maxFiles : MAX_FILES,
            prettyPrint : false
        });
    }

    // 일반 로그 : 라이브의 경우 슬랙으로 에러 리포팅
    if (isLive || isQA) {
        
        //var userName = CONFIG.Service.toLowerCase() + '_err_report : ' + CONFIG.ServerID + ', ("' + version + '")';
        var userName = 'TEST'
        
        // [2016-08-01] narlamy
        // slack에서 설정하려면 아래 페이지에 연결해서 AddConfiguration 버튼을 눌러 새로운 채널 연결을 설정
        // https://cocoonbeat.slack.com/apps/A0F7XDUAZ-incoming-webhooks
        
        //Live 체널 : https://hooks.slack.com/services/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  
        //QA   체널 : https://hooks.slack.com/services/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  
        
        // 새로운 슬랙 체널
        // https://hooks.slack.com/services/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

        // sleck Live 체널에 로그 남김
        Winston.add(Slack, {
            domain: isLive ? "cocoonbeat-dev" : "cocoonbeat",
            token: isLive ? "XFvECwyDYqbRec47UN8T0qin" : "8R7t3Jp7EnNdmLqQVc3RJkKP",
            channel: isLive ? "#error_logs" : "#qa_server_errors",
            username: userName,
            level: 'error',
            icon_emoji: ':warning:'
        });       
    }
    else {
        // 일반 로그 : 콘솔 옵션
        Winston.add(Winston.transports.Console, {
            level : 'warn',		// 보통은 warn, 테스트때는 info, verbose, debug, silly 등...
            silent : false,
            timestamp : false
        });
    }
    
    // 통계 등 기존 로그와는 다른 용도의 로그를 사용하는 목적으로 별도의
    // winston 인스턴스를 만들어 사용합니다.
    this.Loggers = Winston.loggers;

    // [ToDo] 폴더 검사 등을 해야합니다.
    var logFileName = '../../Logs/' + appName + '_packet.log'
    
    // 패킷 로그 (파일로 저장)
	this.Loggers.add('packet', {
		transports : [
          new (Winston.transports.File)( { 
        	    level:'verbose', 
        	    filename: logFileName,
                prettyPrint : true,
                mxsize : 200*1024*1024,	// 200MB
                maxFiles : 1000
          })
        ]
    });
    	
    // DB 로그
    // var dbUrl = require('../Data/ServerConfig.json').DBUrl;
    var writeLogTransport = [
        new (Winston.transports.MongoDB)({
            level : 'debug',
            db : dbUrl,
            collection : 'logs',
            prettyPrint : true,
        }),
    ]
    
    if (isLive || isQA) {        
        // 슬랙 메세지 전송
        writeLogTransport.push(
            new (Slack)({
                domain: "cocoonbeat",
                token: isLive ? "qXJkxJPm35fTBd0xFij5U74r" : "8R7t3Jp7EnNdmLqQVc3RJkKP",
                channel: isLive ? "#live_server_errors" : "#qa_server_errors",
                username: userName,
                level: 'error',
                icon_emoji: ':warning:'
            })
        );
    }
    else {
        // 콘솔 출력 (개발 모드)
        writeLogTransport.push(
            new (Winston.transports.Console)({
                level: 'error',
                slient: false,
                timestamp : true
            })
        );
    }
    
    this.Loggers.add('write', {
        transports : writeLogTransport
    });        
    
    // 기본 로그 기능 상속
    var logger = Winston;
	logger.extend(this);
}

Logger.prototype.TimeText = function()
{
	var today = new Date();
	
	return util.format("%d/%d/%d %d:%d:%d", 
			today.getFullYear(), today.getMonth()+1, today.getDate(), 
			today.getHours(), today.getMinutes(), today.getSeconds());
}

Logger.prototype.packet = function(userID, packetName, data, length)
{
	var meta;
	
	if(typeof data == 'string')
	{
		meta = { 
			UserID:userID, 
			//Time:this.TimeText(), 
			Message:data 
		};	
	}
	else
	{
		meta = data;
		meta.UserID = userID;
		//meta.Time = this.TimeText();
	}
		
	this.Loggers.get('packet').verbose("[%s] length=%d", packetName, length, meta);
}

// User 정보를 DB에서 꺼내올때 로그 남기는데 필요한 정보만 복사해오도록 합니다.
Logger.prototype.RecordUserFilter = {
	_id : false, 
	UserID : true, 
	Level : true, 
	Gem : true, 
	Gold : true, 
    PurchasePerMonth : true,
    'Runtime.IP' : true	
};

// // GM 명령 사용시 언제나 로그를 남깁니다.
// Logger.prototype.logCmd = function (userData, cmd, param) 
// {
//     try {
//         var logData = {
//             Command : cmd,
//             IP : userData.Runtime.IP,
//             UserID : userData.UserID,
//             DeviceID : userData.Runtime.DeviceID,   // 키로 사용되는 DeviceID가 아니라 접속한 디바이스의 UUID
//             Param : param
//         };

//         // DB에 로그 남기기 (Winston 로그 사용)
//         this.Loggers.get('gmCmd').verbose('[%s]', cmd, logData);
//     }
//     catch (e) {

//         console.log('[logger] logCmd() => ' + e);
//     }
// }

// // 운영자 권한으로 전체 매일 발송시 남기는 로그
// // 사용하지 않을 가능성이 높습니다. GM 툴에서 바로 DB에 남깁니다.
// // 현재 매일 발송은 패킷으로 처리하는데 DB에 바로 쓰는 방식이 된다면, 
// // 로그는 어떻게 남길까요? GM 툴에서 직접 로그 DB에 저장해야 겠군요
// Logger.prototype.logGmMail = function (req, sendingUserID, receiverID, mailID, subject, itemID, itemParam) 
// {
//     var Statistic = require('./Statistic');
//     Statistic.WriteLog(
//         sendingUserID,
//         Statistic.Type.Mail,
//         Statistic.Command.Send,
//         Statistic.Situation.GmMail,
//         Statistic.ParamMail(mailID, sendingUserID, receiverID),
//         Statistic.ParamItem(itemID, itemParam)
//     )
// }


//==============================================================================
// 일반적인 로그 외에 각종 통계를 위해 로그를 남길 수 있습니다.
//==============================================================================
module.exports = _logInstance;