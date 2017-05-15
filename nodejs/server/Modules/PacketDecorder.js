
module.exports = function(req, res, next) {

    if(req.headers.endcoded) {
        // 암호화 되어 있다면 암호화 해제
        var encoded = req.body['__sab'] || '';
        next();

    } else {

        // 암호화 되어 있지 않다면 그대로 진행
        next();   
    }
}