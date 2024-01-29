function convertDigitsToPersian(text) {
    var digitsMap = {
      '0': '۰',
      '1': '۱',
      '2': '۲',
      '3': '۳',
      '4': '۴',
      '5': '۵',
      '6': '۶',
      '7': '۷',
      '8': '۸',
      '9': '۹'
    };
  
    return text.replace(/\d/g, function(match) {
      return digitsMap[match];
    });
  }
 
function printer_socket(print_setting,data,serverAddress,rootPath,closewindow) {
    var parser = new DOMParser();
    var htmlDoc = parser.parseFromString(data, 'text/html');
    // حذف تگ‌های غیرنمایشی مانند <script>
    var scriptTags = htmlDoc.querySelectorAll('script');
    scriptTags.forEach(function(scriptTag) {
        scriptTag.remove();
    });
    
	if(print_setting=='')
	{
		alert('پرینتر جهت چاپ مستقیم تعریف نشده است');
        window.print();
		return;
	}
	
    var visibleContent = htmlDoc.documentElement.innerHTML;
 
    var Data_For_Print={};
    Data_For_Print.print_setting=print_setting;
    Data_For_Print.visibleContent=visibleContent;
    Data_For_Print.serverAddress=serverAddress;
    Data_For_Print.rootPath=rootPath;
    
	var arrbuf=new ArrayBuffer(10485760,{maxByteLength:104857600});
	arrbuf=JSON.stringify(Data_For_Print);
    var ws = new WebSocket("ws://localhost:1988");
	ws.binaryType="arraybuffer";
    ws.onopen = function () {
        ws.send(arrbuf); // I WANT TO SEND THIS MESSAGE TO THE SERVER!!!!!!!!
		if(closewindow==1)
			setTimeout(function (){window.close();},300);
    };

    ws.onmessage = function (evt) {
        var received_msg = evt.data;
    };
    ws.onclose = function () {
        // websocket is closed.
        alert("Connection is closed...");
        window.print();
		if(closewindow==1)
			window.close();
    };
};