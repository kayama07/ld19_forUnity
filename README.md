概要
LD-19　LidarセンサーをUNITYで使うためのスクリプトです
シリアル通信を使いますのでProjectSettingsより各種シリアル通信向けの設定を行ってください

利用方法1
LidarDataVisualizer.cs　シーン上の適当なGAめObjectにアタッチしてください
unityシーンビューで検知位置状況を視認できます

利用方法2
LidarDataVisualizer2.cs　シーン上の適当なGameObjectにアタッチしてください
検知点が集まっている部分にプレファブを配置します
配置するプレファブには自壊用スクリプト(SelfDestruct.cs)をアタッチしておくと
自動で消えます

Overview
LD-19 This is a script for using the Lidar sensor with UNITY
Since it uses serial communication, please set various serial communication settings in ProjectSettings

Usage 1
LidarDataVisualizer.cs Attach to a suitable GameObject in the scene
You can visually check the detection position status in the Unity scene view

Usage 2
LidarDataVisualizer2.cs Attach to a suitable GameObject in the scene
Place a prefab in the area where the detection points are concentrated
If you attach a self-destruct script (SelfDestruct.cs) to the prefab you place,
it will disappear automatically

　
