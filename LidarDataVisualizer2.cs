using System;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class LidarDataVisualizer2 : MonoBehaviour
{
    [SerializeField] private GameObject instancePrefab; // プレファブをシリアライズフィールドからアタッチ

    private SerialPort serialPort;
    private List<byte> buffer = new List<byte>();
    private List<LidarFrame> allFrames = new List<LidarFrame>();
    private bool isRunning = true;
    private Thread dataThread;

    void Start()
    {
        string portName = "COM5";

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
        // リストが空の場合はスキップ
        if (frames == null || frames.Count == 0)
        {
            return;
        }

        List<LidarFrame> framesCopy;
        try
        {
            framesCopy = new List<LidarFrame>(frames);
        }
        catch (ArgumentException ex)
        {
            Debug.LogError($"フレームのコピー中にエラーが発生しました: {ex.Message}");
            return;
        }

        Camera camera = Camera.main;

        foreach (LidarFrame frame in framesCopy)
        {
            List<Vector3> pointsPositions = new List<Vector3>();

            foreach (LidarPoint point in frame.Points)
            {
                if (frame.EndAngle > frame.StartAngle)
                {
                    float angleInDegrees = (frame.StartAngle + (frame.EndAngle - frame.StartAngle) / 12 * frame.Points.IndexOf(point)) / 100.0f;
                    float distanceInMeters = point.Distance / 1000.0f;

                    float x = distanceInMeters * Mathf.Cos(angleInDegrees * Mathf.Deg2Rad);
                    float y = distanceInMeters * Mathf.Sin(angleInDegrees * Mathf.Deg2Rad);

                    Vector3 pointPosition = new Vector3(x, y, 0);
                    pointsPositions.Add(pointPosition);

                    Debug.DrawLine(Vector3.zero, pointPosition, Color.green, 0.1f);
                }
            }

            PlacePrefabs(pointsPositions);
        }
    }

    private void PlacePrefabs(List<Vector3> pointsPositions)
    {
        HashSet<int> usedIndices = new HashSet<int>();

        for (int i = 0; i < pointsPositions.Count; i++)
        {
            if (usedIndices.Contains(i))
                continue;

            List<int> closePointsIndices = new List<int> { i };

            for (int j = i + 1; j < pointsPositions.Count; j++)
            {
                if (usedIndices.Contains(j))
                    continue;

                if (Vector3.Distance(pointsPositions[i], pointsPositions[j]) < 0.1f)
                {
                    closePointsIndices.Add(j);

                    if (closePointsIndices.Count == 3)
                        break;
                }
            }

            if (closePointsIndices.Count == 3)
            {
                Vector3 centroid = Vector3.zero;

                foreach (int index in closePointsIndices)
                {
                    centroid += pointsPositions[index];
                    usedIndices.Add(index);
                }

                centroid /= closePointsIndices.Count;

                Instantiate(instancePrefab, centroid, Quaternion.identity);
            }
        }
    }

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
