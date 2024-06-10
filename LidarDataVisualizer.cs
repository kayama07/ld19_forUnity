using System;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class LidarVisualizer : MonoBehaviour
{
    private SerialPort serialPort;
    private List<byte> buffer = new List<byte>();
    private List<LidarFrame> allFrames = new List<LidarFrame>();
    private bool isRunning = true;
    private Thread dataThread;

    // Unityの初期化
    void Start()
    {
        // 固定されたシリアルポート名
        string portName = "COM5";

        // シリアルポートの設定
        serialPort = new SerialPort(portName, 230400, Parity.None, 8, StopBits.One);
        serialPort.ReadTimeout = 500;

        try
        {
            serialPort.Open();
            dataThread = new Thread(ReadDataFromSerial);
            dataThread.Start();
        }
        catch (Exception ex)
        {
            Debug.LogError($"シリアルポートの接続中にエラーが発生しました: {ex.Message}");
            isRunning = false;
        }
    }

    // Unityの終了時に実行
    void OnDestroy()
    {
        isRunning = false;
        if (dataThread != null && dataThread.IsAlive)
        {
            dataThread.Join();
        }
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }
    }

    // UnityのUpdateで実行
    void Update()
    {
        if (allFrames.Count > 0)
        {
            VisualizeLidarFrames(allFrames);
            allFrames.Clear();
        }

        
    }

    private void ReadDataFromSerial()
    {
        while (isRunning)
        {
            try
            {
                while (serialPort.BytesToRead > 0)
                {
                    byte[] data = new byte[serialPort.BytesToRead];
                    serialPort.Read(data, 0, data.Length);
                    buffer.AddRange(data);
                }

                ProcessBuffer();
            }
            catch (Exception ex)
            {
                Debug.LogError($"シリアルポートからのデータ読み取り中にエラーが発生しました: {ex.Message}");
                isRunning = false;
            }
        }
    }

    private void ProcessBuffer()
    {
        while (buffer.Count >= 47)
        {
            // ヘッダーの確認
            if (buffer[0] != 0x54)
            {
                int index = buffer.IndexOf(0x54);
                if (index != -1)
                {
                    buffer.RemoveRange(0, index);
                }
                else
                {
                    buffer.Clear();
                    return;
                }
            }

            // 47バイトのデータがあるか確認
            if (buffer.Count < 47)
            {
                break;
            }

            byte[] frameData = buffer.GetRange(0, 47).ToArray();
            buffer.RemoveRange(0, 47);

            if (CheckLidarFrameData(frameData))
            {
                LidarFrame frame = GetLidarFrame(frameData);
                allFrames.Add(frame);
            }
        }
    }

    private bool CheckLidarFrameData(byte[] data)
    {
        return data[1] == 0x2C && data.Length == 47;
    }

    private LidarFrame GetLidarFrame(byte[] data)
    {

        LidarFrame frame = new LidarFrame();
        frame.Header = data[0];
        frame.VerLen = data[1];
        frame.Speed = BitConverter.ToUInt16(data, 2);
        frame.StartAngle = BitConverter.ToUInt16(data, 4);

        List<LidarPoint> points = new List<LidarPoint>();
        for (int i = 0; i < 12; i++)
        {
            int startIndex = 6 + i * 3;
            LidarPoint point = new LidarPoint
            {
                Distance = BitConverter.ToUInt16(data, startIndex),
                Intensity = data[startIndex + 2]
            };
            points.Add(point);
        }
        frame.Points = points;
        frame.EndAngle = BitConverter.ToUInt16(data, 42);
        frame.Timestamp = BitConverter.ToUInt16(data, 44);
        frame.Crc8 = data[46];

        return frame;
    }

    private void VisualizeLidarFrames(List<LidarFrame> frames)
    {
        // リストのコピーを作成
        List<LidarFrame> framesCopy = new List<LidarFrame>(frames);

        // カメラのスクリーン座標を計算するための変換
        Camera camera = Camera.main;

        // リストのコピーを使ってフレームをループ
        foreach (LidarFrame frame in framesCopy)
        {
            // フレーム内のポイントを処理
            foreach (LidarPoint point in frame.Points)
            {
                if (frame.EndAngle > frame.StartAngle) {
                // 距離と角度を使用して座標を計算
                float angleInDegrees = (frame.StartAngle + (frame.EndAngle - frame.StartAngle) / 12 * frame.Points.IndexOf(point)) / 100.0f;
                float distanceInMeters = point.Distance / 1000.0f;  // 距離はmmで与えられているため、mに変換

                // 座標を計算
                float x = distanceInMeters * Mathf.Cos(angleInDegrees * Mathf.Deg2Rad);
                float y = distanceInMeters * Mathf.Sin(angleInDegrees * Mathf.Deg2Rad);

                // スクリーン座標に変換
                Vector3 pointPosition = new Vector3(x, y, 0);

                // ポイントを描画
                Debug.DrawLine(Vector3.zero, pointPosition, Color.green, 0.1f);
                }
            }
        }
    }




    // LidarFrameとLidarPointのクラス定義
    private class LidarFrame
    {
        public byte Header { get; set; }
        public byte VerLen { get; set; }
        public ushort Speed { get; set; }
        public ushort StartAngle { get; set; }
        public List<LidarPoint> Points { get; set; }
        public ushort EndAngle { get; set; }
        public ushort Timestamp { get; set; }
        public byte Crc8 { get; set; }
    }

    private class LidarPoint
    {
        public ushort Distance { get; set; }
        public byte Intensity { get; set; }
    }
}