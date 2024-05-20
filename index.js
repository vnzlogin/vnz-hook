var EventEmitter2 = require('eventemitter2').EventEmitter2;
var exec = require("child_process").exec;
var path = require("path");
var isWindows = process.platform === "win32";

var events = new EventEmitter2({wildcard: true});
var noop = function () { };
var vnz = {
    windowsFocusManagementBinary: '',
    windowsFocusManagementBinaryDefault: path.join(__dirname, "windows-console-app", "windows-console-app", "bin", "Release", "windows-console-app.exe"),
    /**
     * Get list of processes that are currently running
     *
     * @param {function} callback
     */
    getProcesses(callback) {
        callback = callback || noop;

        if (!isWindows) {
            callback("Non-Windows platforms are currently not supported");
        }

        var mappingFunction = (processes) => {
            return processes.map(p => {
                return {
                    pid: p.ProcessId,
                    mainWindowTitle: p.MainWindowTitle || "",
                    processName: p.ProcessName || ""
                };
            });
        };

        this.executeProcess("--processinfo", callback, mappingFunction);
    },
    /**
     * Focus a windows
     * Process can be a number (PID), name (process name or window title),
     * or a process object returning from getProcesses
     *
     * @param {number|string|ProcessInfo} process
     */
    focusWindow(process) {
        if (!isWindows) {
            throw "Non-windows platforms are currently not supported"
        }

        if (process === null)
            return;

        if (typeof process === "number") {
            this.executeProcess("--focus " + process.toString());
        } else if (typeof process === "string") {
            this.focusWindowByName(process);
        } else if (process.pid) {
            this.executeProcess("--focus " + process.pid.toString());
        }
    },

    /**
     * Get information about the currently active window
     *
     * @param {function} callback
     */
    getActiveWindow(callback) {
        callback = callback || noop;

        if (!isWindows) {
            callback("Non-windows platforms are currently not supported");
        }
    },

    /**
     * Helper method to focus a window by name
     */
    focusWindowByName(processName) {
        processName = processName.toLowerCase();

        this.getProcesses((err, result) => {
            var potentialResults = result.filter((p) => {
                var normalizedProcessName = p.processName.toLowerCase();
                var normalizedWindowName = p.mainWindowTitle.toLowerCase();

                return normalizedProcessName.indexOf(processName) >= 0
                    || normalizedWindowName.indexOf(processName) >= 0;
            });

            if (potentialResults.length > 0) {
                this.executeProcess("--focus " + potentialResults[0].pid.toString());
            }
        });
    },

    GetModuleHandle(callback) {
        callback = callback || noop;

        if (!isWindows) {
            callback("Non-Windows platforms are currently not supported");
        }

        this.executeProcess("--getmodule", callback);
    },
    MouseHookStart() {
        if(!this.windowsFocusManagementBinary) this.windowsFocusManagementBinary = this.windowsFocusManagementBinaryDefault
        var gkm = spawn(this.windowsFocusManagementBinary, ['--event']);
        gkm.stdout.on('data', function(data) {
            data = data.toString().split(/\r\n|\r|\n/).filter(function(item) { return item; });
            for (var i in data) {
                var parts = data[i].split(':');
                events.emit(parts[0], parts.slice(1));
            }
        });
        return events
    },
    MouseHookStop() {
        events.removeAllListeners()
    },

    /**
     * Helper method to execute the C# process that wraps the native focus / window APIs
     */
    executeProcess(arg, callback, mapper) {
        callback = callback || noop;
        if(!this.windowsFocusManagementBinary) this.windowsFocusManagementBinary = this.windowsFocusManagementBinaryDefault
        exec("\"" + this.windowsFocusManagementBinary + "\" " + arg, (error, stdout, stderr) => {
            if (error) {
                callback(error, null);
                return;
            }

            if (stderr) {
                callback(stderr, null);
                return;
            }

            var returnObject = JSON.parse(stdout);

            if (returnObject.Error) {
                callback(returnObject.Error, null);
                return;
            }

            var ret = returnObject.Result;

            ret = mapper ? mapper(ret) : ret;
            callback(null, ret);
        });
    }
}

module.exports = vnz