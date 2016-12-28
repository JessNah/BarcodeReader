
BarCodeReader that displays the value of the barcode when it is in view. Uses the zxing library


<img src="https://cloud.githubusercontent.com/assets/14356838/21531732/6106e314-cd18-11e6-9302-bee0736e5a6e.png">

<img src ="https://cloud.githubusercontent.com/assets/14356838/18962118/8349087a-863d-11e6-86b7-0a8cdd92941f.jpg">

On the bottom of the display window, it shows the code read by the program.
The second image, is the barcode that I had held up for testing. As you can see,
it reads the same value as the barcode.


This program makes use of the C#WebCam respository to make use of your device's webcam
and display a live video feed upon the click of the start button.
The stream image may be captured and saved at any time.
The program works live and dynamically to detect any barcodes that may be present in its view.
If a barcode is detected, it then presents its decoded barcode value.

