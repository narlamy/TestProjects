module.exports = function (request, response) {
    
    console.log(request.path + ' : ' + JSON.stringify(request.body));
    response.status(200)
    response.send('ok')
}