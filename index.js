var EventEmitter2 = require('eventemitter2').EventEmitter2;
var spawn = require('child_process').spawn;
var path = require("path");


var events = new EventEmitter2({wildcard: true});
var hook = {
    windowsFocusManagementBinary: '',
    windowsFocusManagementBinaryDefault: path.join(__dirname, "WinFormsApp1", "WinFormsApp1", "bin", "Release", "net6.0-windows", "WinFormsApp1.exe"),
    ev: null,
    sendText(hwnd, text) {
        if(!this.windowsFocusManagementBinary) this.windowsFocusManagementBinary = this.windowsFocusManagementBinaryDefault
        this.ev = spawn(this.windowsFocusManagementBinary, ['--sendText', text, hwnd]);
        this.ev.stdout.on('data', (data) => {
            console.log('data',data.toString())
            // if(data.toString().includes("end")) {
            //     this.ev.stdin.pause();
            //     this.ev.kill();
            // }
        })
    },
    sendChar(hwnd, text, timeout = 0) {
        if(!this.windowsFocusManagementBinary) this.windowsFocusManagementBinary = this.windowsFocusManagementBinaryDefault
        this.ev = spawn(this.windowsFocusManagementBinary, ['--sendChar', text, hwnd, timeout]);
        this.ev.stdout.on('data', (data) => {
            console.log('data',data.toString())
            if(data.toString().includes("end")) {
                this.ev.stdin.pause();
                this.ev.kill();
            }
        })
    },
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
        if(this.ev) {
            this.ev.stdin.pause();
            this.ev.kill();
        }
    }
}
// hook.sendText(790060, 'bà ơi bà cháu yêu bà lắm')
// hook.sendChar(790060, '417;224;417;224;417;224;417;224;417;224;417;224', 100)
// var event = hook.start()
// event.on('key.*', function(data) {
//     console.log(this['event'], data)
// })

module.exports = hook