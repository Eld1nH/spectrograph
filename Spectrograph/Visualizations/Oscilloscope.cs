using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;

namespace Spectrograph.Visualizations
{
    public class Oscilloscope
    {
        private List<float> _left = new List<float>();
        private List<float> _right = new List<float>();

        private object _lockObject = new object();

        public void Add(float left, float right)
        {
            lock (_lockObject)
            {
                _left.Add(left);
                _right.Add(right);
            }
        }

        public Bitmap CreateOscilloscope(Size size, Color leftColor, Color rightColor, Color backgroundColor, bool highQuality)
        {
            using (var leftPen = new Pen(leftColor, 1))
            using (var rightPen = new Pen(rightColor, 1))
            {
                var bitmap = new Bitmap(size.Width, size.Height);

                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    PrepareGraphics(graphics, highQuality);
                    graphics.Clear(backgroundColor);

                    DrawOscilloscope(graphics, size.Width, size.Height, leftPen, rightPen);
                }

                return bitmap;
            }
        }

        private void DrawOscilloscope(Graphics graphics, int width, int height, Pen leftPen, Pen rightPen)
        {
            graphics.Clear(Color.Black);

            const int pixelsPerSample = 2;

            var samplesLeft = GetSamplesToDraw(_left, width / pixelsPerSample).ToArray();
            var samplesRight = GetSamplesToDraw(_right, width / pixelsPerSample).ToArray();

            graphics.DrawLines(leftPen, GetPoints(samplesLeft, pixelsPerSample, width, height, height / -4).ToArray());
            graphics.DrawLines(rightPen, GetPoints(samplesRight, pixelsPerSample, width, height, height / 4).ToArray());

            lock (_lockObject)
            {
                _left.Clear();
                _right.Clear();
            }
        }

        private IEnumerable<float> GetSamplesToDraw(List<float> inputSamples, int numberOfSamplesRequested)
        {
            float[] samples;
            lock (_lockObject)
            {
                samples = inputSamples.ToArray();
                inputSamples.Clear();
            }

            var resolution = samples.Length / numberOfSamplesRequested;
            int index = 0;
            float currentMax = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                if (i > index * resolution)
                {
                    yield return currentMax;
                    currentMax = 0;
                    index++;
                }

                if (Math.Abs(currentMax) < Math.Abs(samples[i]))
                {
                    currentMax = samples[i];
                }
            }
        }

        private IEnumerable<Point> GetPoints(float[] samples, int pixelsPerSample, int width, int height, int offset)
        {
            int halfY = height / pixelsPerSample;
            if (samples.Length >= 2)
            {
                for (int i = 0; i < samples.Length; i++)
                {
                    Point point = new Point
                    {
                        X = i * pixelsPerSample,
                        Y = halfY + (int)(samples[i] * halfY) + offset
                    };
                    yield return point;
                }
            }
            else
            {
                yield return new Point(0, halfY + offset);
                yield return new Point(width, halfY + offset);
            }
        }

        private void PrepareGraphics(Graphics graphics, bool highQuality)
        {
            if (highQuality)
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.CompositingQuality = CompositingQuality.AssumeLinear;
                graphics.PixelOffsetMode = PixelOffsetMode.Default;
                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            }
            else
            {
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.PixelOffsetMode = PixelOffsetMode.None;
                graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            }
        }
    }
}
