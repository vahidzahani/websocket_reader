<?php
$host = "http://127.0.0.1:1988";
$data = '{"print_setting": {...}, "visibleContent": "...", "serverAddress": "...", "rootPath": "..."}';

$options = [
    'http' => [
        'method' => 'POST',
        'header' => 'Content-type: application/json',
        'content' => $data
    ]
];

$context = stream_context_create($options);
$response = file_get_contents($host, false, $context);

echo "Reply From Server: " . $response;
//{"print_setting":"{\"0\":\"5\",\"id\":\"5\",\"1\":\"paziresh\",\"type\":\"paziresh\",\"2\":\"paziresh_form\",\"page_name\":\"paziresh_form\",\"3\":\"answer\",\"printer_name\":\"Microsoft Print to PDF\",\"4\":\"0.2\",\"margin_top\":\"0.2\",\"5\":\"0\",\"margin_right\":\"0\",\"6\":\"0\",\"margin_bottom\":\"0\",\"7\":\"0\",\"margin_left\":\"0\",\"8\":\"\",\"header\":\"\",\"9\":\"\",\"footer\":\"\",\"10\":\"\",\"page_address\":\"\",\"11\":\"0\",\"is_direct\":\"0\",\"12\":\"0\",\"closure\":\"0\",\"13\":\"چاپ فرم پذیرش بستری\",\"page_title\":\"چاپ فرم پذیرش بستری\"}","visibleContent":"<head></head><body><div style=\"width: 15cm;\">\n        <span>this is a test for client .</span>\n\t\t<strong>این متن آزمایشی است</strong>\n\t\t<button id=\"btnClick\">Microsoft Print to PDF</button>\n\t\t<button id=\"btnClick2\">label</button>\n\t\t<p style=\"direction: ltr;\">#showprintersocket</p>\n\t</div>\n\t\n\n</body>","serverAddress":"http://192.168.1.200/care2","rootPath":"../../"}
/*
$host = "127.0.0.1";
$port = 1988;
// No Timeout 
set_time_limit(0);
// Note: Here the port and host should be same as defined in server.

$socket = socket_create(AF_INET, SOCK_STREAM, 0) or die("Could not create socket\n");

$result = socket_connect($socket, $host, $port) or die("Could not connect toserver\n");

// $data=json_decode($_REQUEST['MyData'],1);
$data='{"print_setting":"{\"0\":\"5\",\"id\":\"5\",\"1\":\"paziresh\",\"type\":\"paziresh\",\"2\":\"paziresh_form\",\"page_name\":\"paziresh_form\",\"3\":\"answer\",\"printer_name\":\"Microsoft Print to PDF\",\"4\":\"0.2\",\"margin_top\":\"0.2\",\"5\":\"0\",\"margin_right\":\"0\",\"6\":\"0\",\"margin_bottom\":\"0\",\"7\":\"0\",\"margin_left\":\"0\",\"8\":\"\",\"header\":\"\",\"9\":\"\",\"footer\":\"\",\"10\":\"\",\"page_address\":\"\",\"11\":\"0\",\"is_direct\":\"0\",\"12\":\"0\",\"closure\":\"0\",\"13\":\"چاپ فرم پذیرش بستری\",\"page_title\":\"چاپ فرم پذیرش بستری\"}","visibleContent":"<head></head><body><div style=\"width: 15cm;\">\n        <span>this is a test for client .</span>\n\t\t<strong>این متن آزمایشی است</strong>\n\t\t<button id=\"btnClick\">Microsoft Print to PDF</button>\n\t\t<button id=\"btnClick2\">label</button>\n\t\t<p style=\"direction: ltr;\">#showprintersocket</p>\n\t</div>\n\t\n\n</body>","serverAddress":"http://192.168.1.200/care2","rootPath":"../../"}';

socket_write($socket, json_encode($data), strlen(json_encode($data))) or die("Could not send data to server\n");

$result = socket_read ($socket, 1024) or die("Could not read server response\n");
echo "Reply From Server :".$result;

socket_close($socket);
// print_r($_REQUEST['MyData']);
// Complete Code

*/