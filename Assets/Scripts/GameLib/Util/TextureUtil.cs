using System.Threading;
using UnityEngine;

namespace GameLib
{
    public static class TextureUtils
    {
        public sealed class ThreadData
        {
            public int start;
            public int end;

            public ThreadData(int start, int end)
            {
                this.start = start;
                this.end = end;
            }
        }

        private static Color[] m_TextureColors;
        private static Color[] m_NewColors;
        private static int m_W;
        private static int m_W2;
        private static float m_RatioX;
        private static float m_RatioY;
        private static int m_FinishCount;
        private static Mutex m_Mutex;

        public static void Point(Texture2D texture, float scaleFactor)
        {
            ThreadedScale(texture, (int)(texture.width * scaleFactor), (int)(texture.height * scaleFactor), false);
        }

        public static void Bilinear(Texture2D texture, float scaleFactor)
        {
            ThreadedScale(texture, (int)(texture.width * scaleFactor), (int)(texture.height * scaleFactor), true);
        }

        public static void Point(Texture2D texture, int newWidth, int newHeight)
        {
            ThreadedScale(texture, newWidth, newHeight, false);
        }

        public static void Bilinear(Texture2D texture, int newWidth, int newHeight)
        {
            ThreadedScale(texture, newWidth, newHeight, true);
        }

        private static void ThreadedScale(Texture2D texture, int newWidth, int newHeight, bool useBilinear)
        {
            m_TextureColors = texture.GetPixels();
            m_NewColors = new Color[newWidth * newHeight];

            if (useBilinear)
            {
                m_RatioX = 1.0f / ((float)newWidth / (texture.width - 1));
                m_RatioY = 1.0f / ((float)newHeight / (texture.height - 1));
            }
            else
            {
                m_RatioX = ((float)texture.width) / newWidth;
                m_RatioY = ((float)texture.height) / newHeight;
            }

            m_W = texture.width;
            m_W2 = newWidth;

            var cores = Mathf.Min(SystemInfo.processorCount, newHeight);
            var slice = newHeight / cores;

            m_FinishCount = 0;

            if (m_Mutex == null)
            {
                m_Mutex = new Mutex(false);
            }

            if (cores > 1)
            {
                int i = 0;

                ThreadData threadData;

                for (i = 0; i < cores - 1; i++)
                {
                    threadData = new ThreadData(slice * i, slice * (i + 1));

                    var threadStart = useBilinear ?
                        new ParameterizedThreadStart(BilinearScale) :
                        new ParameterizedThreadStart(PointScale);

                    var thread = new Thread(threadStart);
                    thread.Start(threadData);
                }

                threadData = new ThreadData(slice * i, newHeight);

                if (useBilinear)
                {
                    BilinearScale(threadData);
                }
                else
                {
                    PointScale(threadData);
                }

                while (m_FinishCount < cores)
                {
                    Thread.Sleep(1);
                }
            }
            else
            {
                var threadData = new ThreadData(0, newHeight);

                if (useBilinear)
                {
                    BilinearScale(threadData);
                }
                else
                {
                    PointScale(threadData);
                }
            }

            texture.Resize(newWidth, newHeight);
            texture.SetPixels(m_NewColors);
            texture.Apply();

            m_TextureColors = null;
            m_NewColors = null;
        }

        public static void BilinearScale(object obj)
        {
            var threadData = obj as ThreadData;

            for (var y = threadData.start; y < threadData.end; y++)
            {
                int yFloor = (int)Mathf.Floor(y * m_RatioY);
                var y1 = yFloor * m_W;
                var y2 = (yFloor + 1) * m_W;
                var yw = y * m_W2;

                for (var x = 0; x < m_W2; x++)
                {
                    int xFloor = (int)Mathf.Floor(x * m_RatioX);
                    var xLerp = x * m_RatioX - xFloor;

                    m_NewColors[yw + x] = ColorLerpUnclamped(
                        ColorLerpUnclamped(m_TextureColors[y1 + xFloor], m_TextureColors[y1 + xFloor + 1], xLerp),
                        ColorLerpUnclamped(m_TextureColors[y2 + xFloor], m_TextureColors[y2 + xFloor + 1], xLerp),
                        y * m_RatioY - yFloor);
                }
            }

            m_Mutex.WaitOne();
            m_FinishCount++;
            m_Mutex.ReleaseMutex();
        }

        public static void PointScale(object obj)
        {
            var threadData = obj as ThreadData;

            for (var y = threadData.start; y < threadData.end; y++)
            {
                var thisY = (int)(m_RatioY * y) * m_W;
                var yw = y * m_W2;

                for (var x = 0; x < m_W2; x++)
                {
                    m_NewColors[yw + x] = m_TextureColors[(int)(thisY + m_RatioX * x)];
                }
            }

            m_Mutex.WaitOne();
            m_FinishCount++;
            m_Mutex.ReleaseMutex();
        }

        private static Color ColorLerpUnclamped(Color c1, Color c2, float value)
        {
            return new Color(
                c1.r + (c2.r - c1.r) * value,
                c1.g + (c2.g - c1.g) * value,
                c1.b + (c2.b - c1.b) * value,
                c1.a + (c2.a - c1.a) * value);
        }
    }
}