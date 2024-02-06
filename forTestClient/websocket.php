<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Document</title>
</head>
<body>
   
<?php
$url = 'http://127.0.0.1:1988'; // آدرس برنامه C#
$data = array(
    'print_setting' => array(
        'id' => '1',
        'printer_name' => 'MyPrinter',
        // دیگر فیلدها
    ),
    'visibleContent' => '<html><body><h1>Hello from PHP!</h1></body></html>',
    // سایر اطلاعات
);

$options = array(
    'http' => array(
        'method'  => 'POST',
        'header'  => 'Content-type: application/json',
        'content' => json_encode($data)
    )
);
$context  = stream_context_create($options);
$result = file_get_contents($url, false, $context);
if ($result === FALSE) {
    echo "Error sending data to C# program.";
} else {
    echo $result;
}
?> 


<script src="vahidnew.js"></script>
</body>
</html>
