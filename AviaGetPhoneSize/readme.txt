iphone 6 size:
iphone text at 
x = width * 30%
y = height * 70%
width = width * 40%
height = height * 7% 
(0.3,0.7,0.4,0.07)

camera
mode =1
1920x1080
rectangle:
Rectangle r = new Rectangle(196, 665, 269, 628);
rectangle in mirror:
new Rectangle(517, 823, 152, 298);

mode=0
1944x2592
Rectangle r = new Rectangle(334, 774, 452, 1016);
sample color:
Rectangle r = new Rectangle(112, 797, 65, 33);

green backgroud:
mask = img1.InRange(new Bgr(38, 58, 39), new Bgr(90, 120, 70));

iphone icon:
RectangleF rf = new RectangleF(0.35f * m.Width, 0.18f * m.Height, 0.30f * m.Width, 0.20f * m.Height);



AviaGetPhoneSize.exe

command line:
-wait -timeout=60
-wait, wait for device inplace and detect the device size and color.
-timeout=60, optional, default is 60 seconds, 
return:
0, device in place
1, device not present
2, other error
stdout:
device=ready
size=1, size id, 1, 2, 3...
color=1, color id, 1, 2, 3...

color id:
1: Blue (iPhone XR)
2: Red (iPhone 8 Plus)
3: Gray (iPhone 8 Plus/iPhone 8)
4: Silver (iPhone 8 Plus)
5: Gold (iPhone 8/iPhone 8 Plus)
6: Sliver (iPhone 6/iPhone 6S/iPhone 7)
7: Gray (iPhone 6/iPhone 6S)
8: Rosegold (iPhone 6/iPhone 6S/iPhone 7)
9: Gold (iPhone 7/iPhone 6 Plus)
10: Silver (iPhone 7)
11: Black/Matte Black (iPhone 7/iPhone 7 Plus)



size id:
1: 75.7x150.9 (iPhone XR)
2: 78.1x158.4 (iPhone 8 Plus)
3: 67.1x138.3 (iPhone 6 7 8)

command line:
-detect -image=<full path of image> -color=id -size=id
-detect, to do model recognize,
-image=<full path of image>, the device back image file fullpath in BMP format.
-color=id, return by fist command
-size=id, return by fist command
return:
0, model detect success
1, model detect failed
stdout
model=iPhone 8 Plus
or
model=iPhone XR