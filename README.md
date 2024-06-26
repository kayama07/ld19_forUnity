概要
LD-19　LidarセンサーをUNITYで使うためのスクリプトです
シリアル通信を使いますのでProjectSettingsより各種シリアル通信向けの設定を行ってください

利用方法1
LidarDataVisualizer.cs　シーン上の適当なGameObjectにアタッチしてください
unityシーンビューで検知位置状況を視認できます

利用方法2
LidarDataVisualizer2.cs　シーン上の適当なGameObjectにアタッチしてください
検知点が集まっている部分にプレファブを配置します
配置するプレファブには自壊用スクリプト(SelfDestruct.cs)をアタッチしておくと
自動で消えます

Overview
 This is a script for using the Lidar sensor LD-19 with UNITY
Since it uses serial communication, please set various serial communication settings in ProjectSettings

Usage 1
LidarDataVisualizer.cs Attach to a suitable GameObject in the scene
You can visually check the detection position status in the Unity scene view

Usage 2
LidarDataVisualizer2.cs Attach to a suitable GameObject in the scene
Place a prefab in the area where the detection points are concentrated
If you attach a self-destruct script (SelfDestruct.cs) to the prefab you place,
it will disappear automatically
![Screenshot 2024-06-11 011944](https://github.com/kayama07/ld19_forUnity/assets/63775012/b5ff4ae8-25f3-4fc8-acd0-1afb2329fbe0)
![Screenshot 2024-06-11 012042](https://github.com/kayama07/ld19_forUnity/assets/63775012/a0615d08-87fb-4f3b-88a0-d58197bb7356)
![Screenshot 2024-06-11 005412](https://github.com/kayama07/ld19_forUnity/assets/63775012/1a7fbeb4-927a-4f3a-bcfe-815b464e308f)
