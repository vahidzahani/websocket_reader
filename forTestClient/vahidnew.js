var url = 'http://127.0.0.1:1988'; // آدرس برنامه C#
var data = {
    print_setting: {
        id: '1',
        printer_name: 'MyPrinter',
        // دیگر فیلدها
    },
    visibleContent: '<html><body><h1>Hello from JavaScript!</h1></body></html>',
    // سایر اطلاعات
};

var xhr = new XMLHttpRequest();
xhr.open('POST', url, true);
xhr.setRequestHeader('Content-Type', 'application/json');

xhr.onload = function() {
    if (xhr.status >= 200 && xhr.status < 300) {
        console.log('Data sent successfully to C# program.');
        console.log(xhr.responseText);
    } else {
        console.error('Error sending data to C# program.');
    }
};

xhr.onerror = function() {
    console.error('Error sending data to C# program.');
};

xhr.send(JSON.stringify(data));
