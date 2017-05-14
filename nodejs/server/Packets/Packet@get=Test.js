module.exports = function (request, response) {
    
    console.log(request.path);
    response.status(200)
    response.send('ok')
}