var EventEmitter2 = require('eventemitter2').EventEmitter2;
var spawn = require('child_process').spawn;
var path = require("path");
var isWindows = process.platform === "win32";

var events = new EventEmitter2({wildcard: true});
var noop = function () { };
var hook = {
    windowsFocusManagementBinary: '',
    windowsFocusManagementBinaryDefault: path.join(__dirname, "WinFormsApp1", "WinFormsApp1", "bin", "Release", "net6.0-windows", "WinFormsApp1.exe"),
    ev: null,
    start() {
        if(!this.windowsFocusManagementBinary) this.windowsFocusManagementBinary = this.windowsFocusManagementBinaryDefault
        this.ev = spawn(this.windowsFocusManagementBinary, []);
        this.ev.stdout.on('data', function(data) {
            data = data.toString().split(/\r\n|\r|\n/).filter(function(item) { return item; });
            for (var i in data) {
                var parts = data[i].split(':');
                events.emit(parts[0], parts.slice(1));
            }
        });
        return events
    },
    stop() {
        events.removeAllListeners()
        this.ev.stdin.pause();
        this.ev.kill();
    }
}
// var event = hook.start()
// event.on('key.*', function(data) {
//     console.log(this['event'], data)
// })
// setTimeout(() => {
//     hook.stop()
// }, 5000)
module.exports = hook