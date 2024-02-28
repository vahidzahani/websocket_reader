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
	var scriptRegex = /<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi;
	var htmlDoc = data.replace(scriptRegex, "");
    var htmlDoc = parser.parseFromString(data, 'text/html');
    // حذف تگ‌های غیرنمایشی مانند <script>
	
    var scriptTags = htmlDoc.querySelectorAll('script');
    scriptTags.forEach(function(scriptTag) {
        try	{scriptTag.remove();}catch{}
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
    
	var Action ='test';
	rootpath='../../../';
	// alert(rootpath);
	const url = 'http://127.0.0.1:1988'; // آدرس برنامه C#
	var data=JSON.stringify(Data_For_Print);
  $.ajax({
    url: url,
    type: 'POST',
    contentType: 'application/json',
    data: data,
    success: function(result) {
      
        if (closewindow == 1)
            setTimeout(function() {
                window.close();
            }, 300);
        //console.log(result);
    },
    error: function(error) {
        //console.log(error);
        window.print();
        if (closewindow == 1)
            window.close();
    }
});
}
