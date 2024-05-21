# VNZ-HOOK
An event based, Global Keyboard and Mouse listener.


## Why?
Node didn't have any global keyboard and mouse listener available at the time.


## Getting started
Install vnz-gkm via node.js package manager:

    npm install vnz-hook --save

Then require the package in your code:

```javascript
var vnz_hook = require('vnz-hook');
vnz_hook.windowsFocusManagementBinary = ... //replace exe path (optional)
var hook = vnz_hook.start()
// Listen to all key events (down, up)
hook.on('key.*', function(data) {
    console.log(this.event + ' ' + data);
});

// Listen to all mouse events (click, down, up, moved, dragged)
hook.on('mouse.*', function(data) {
    console.log(this.event + ' ' + data);
});
//stop listener
vnz_hook.stop()
```

## License
The code is licensed under the MIT license (http://opensource.org/licenses/MIT). See license.txt.