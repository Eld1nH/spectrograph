using CSCore;
using CSCore.Codecs.WAV;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.Streams;
using CSCore.Utils;
using Spectrograph.Visualizations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Spectrograph
{
    public partial class Visualizer : Form
    {
        private IWaveSource _source;
        private IWriteable _writer;

        private WasapiLoopbackCapture _soundIn;
        private FftProvider _fftProvider;

        private Oscilloscope _oscilloscope;

        private List<float> _left = new List<float>();
        private List<float> _right = new List<float>();
        private Graphics _graphics;

        private Pen[] _pens = new Pen[256];

        object _lockObject = new object();

        public Visualizer()
        {
            InitializeComponent();

            _graphics = DrawPanel.CreateGraphics();
            _graphics.SmoothingMode = SmoothingMode.AntiAlias;
            _graphics.CompositingQuality = CompositingQuality.AssumeLinear;
            _graphics.PixelOffsetMode = PixelOffsetMode.Default;
            _graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            _graphics.Clear(Color.Black);

            _oscilloscope = new Oscilloscope();

            for (int i = 0; i < _pens.Length; i++)
            {
                _pens[i] = new Pen(Color.FromArgb(i, i, i));
            }

            _fftProvider = new FftProvider(1, FftSize.Fft4096);

            _soundIn = new WasapiLoopbackCapture();
            _soundIn.Initialize();

            var soundInSource = new SoundInSource(_soundIn);
            var singleBlockNotificationStream = new SingleBlockNotificationStream(soundInSource);
            _source = singleBlockNotificationStream.ToWaveSource();

            if (!Directory.Exists("%AppData%/Spectrograph"))
            {
                Directory.CreateDirectory("%AppData%/Spectrograph");
            }

            _writer = new WaveWriter("%AppData%/Spectrograph/loopback.wav", _source.WaveFormat);

            byte[] buffer = new byte[_source.WaveFormat.BytesPerSecond / 2];
            soundInSource.DataAvailable += (s, e) =>
            {
                int read;
                while ((read = _source.Read(buffer, 0, buffer.Length)) > 0)
                    _writer.Write(buffer, 0, read);
            };

            singleBlockNotificationStream.SingleBlockRead += SingleBlockNotificationStreamOnSingleBlockRead;

            _soundIn.Start();
        }
        
        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            _soundIn.Stop();
            _soundIn.Dispose();
        }

        private void SingleBlockNotificationStreamOnSingleBlockRead(object sender, SingleBlockReadEventArgs e)
        {
            lock (_lockObject)
            {
                _left.Add(e.Left);
                _right.Add(e.Right);
                _fftProvider.Add(e.Left, e.Right);
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            DrawOscilloscope(_graphics, DrawPanel.Width, DrawPanel.Height);
            //DrawSpectrum(_graphics, Pens.Red, DrawPanel.Width, DrawPanel.Height);
            //DrawSpectrogram(_graphics, DrawPanel.Width, DrawPanel.Height);

            lock (_lockObject)
            {
                _left.Clear();
                _right.Clear();
            }
        }


        public void DrawOscilloscope(Graphics graphics, int width, int height)
        {
            graphics.Clear(Color.Black);

            const int pixelsPerSample = 2;
            var samplesLeft = GetSamplesToDraw(_left, width / pixelsPerSample).ToArray();
            var samplesRight = GetSamplesToDraw(_right, width / pixelsPerSample).ToArray();

            graphics.DrawLines(new Pen(Color.DeepSkyBlue, 1), GetPoints(samplesLeft, pixelsPerSample, width, height, height / 4).ToArray());
            graphics.DrawLines(new Pen(Color.FromArgb(150, Color.Red), 0.5f), GetPoints(samplesRight, pixelsPerSample, width, height, height / -4).ToArray());
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
                    currentMax = samples[i];
            }
        }


        public void DrawSpectrum(Graphics graphics, Pen pen, int width, int height)
        {
            _graphics.Clear(Color.Black);

            var fftBuffer = new float[(int)_fftProvider.FftSize];
            var fftData = new List<double>();

            int fftBand = 32;

            if (_fftProvider.GetFftData(fftBuffer))
            {
                for (int i = 0; i < fftBand; i++)
                {
                    double value = 0;
                    for (int j = i * fftBand; j < (i * fftBand) + fftBand; j++)
                    {
                        value += (((20 * Math.Log10(fftBuffer[i])) - -60) / -60) * height;
                    }
                    fftData.Add(value / fftBand);
                }

                int barWidth = width / fftData.Count;

                for (int i = 0; i < fftData.Count; i++)
                {
                    _graphics.DrawLine(Pens.Red, i * barWidth + (barWidth / 2), height, i * barWidth + (barWidth / 2), (float)(height - (fftData[i] < 0 ? 0 : fftData[i])));
                }
            }
        }


        private int _currentColumn = 0;

        public void DrawSpectrogram(Graphics graphics, int width, int height)
        {
            double minValue = 0.0;
            double maxValue = 0.0;

            var fftBuffer = new float[(int)_fftProvider.FftSize];
            var fftData = new List<double>();

            int fftBand = 256;

            if (_fftProvider.GetFftData(fftBuffer))
            {
                for (int i = 0; i < fftBand; i++)
                {
                    double value = 0;
                    for (int j = i * fftBand; j < (i * fftBand) + fftBand; j++)
                    {
                        value += (((20 * Math.Log10(fftBuffer[i])) - -90) / -90) * height;
                    }
                    fftData.Add(value / fftBand);
                }

                minValue = fftData.Min() < minValue ? fftData.Min() : minValue;
                maxValue = fftData.Max() > maxValue || maxValue == 0.0 ? fftData.Max() : maxValue;
                double delta = maxValue - minValue;

                for (int i = 0; i < fftData.Count; i++)
                {
                    fftData[i] = 255.0 - ((fftData[i] - minValue) / delta * 255.0);
                }

                int lineHeight = height / fftData.Count;

                int currentRow = 0;
                for (int i = fftData.Count - 1; i >= 0; i--)
                {
                    _graphics.DrawLine(_pens[(int)fftData[i]], _currentColumn, currentRow, _currentColumn, currentRow + lineHeight);
                    currentRow += lineHeight;
                }
            }
            
            _currentColumn++;
            if (_currentColumn > width - 1)
            {
                _currentColumn = 0;
            }
        }
    }
}
