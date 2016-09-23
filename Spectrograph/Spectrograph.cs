using CSCore;
using CSCore.Codecs.WAV;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.Streams;
using Spectrograph.Visualizations;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Spectrograph
{
    public partial class Spectrograph : Form
    {
        private WasapiLoopbackCapture _soundIn;
        private IWaveSource _source;
        private IWriteable _writer;
        private BasicSpectrumProvider _lineSpectrumProvider;
        private BasicSpectrumProvider _spectrogramProvider;

        private const FftSize fftSize = FftSize.Fft4096;

        private LineSpectrum _lineSpectrum;
        private Oscilloscope _oscilloscope;
        private Spectrogram _spectrogram;
        private KeyboardVisualizer _keyboardVisualizer;

        private string _loopbackDir = Environment.SpecialFolder.ApplicationData + "/Spectrograph";
        
        public Spectrograph()
        {
            InitializeComponent();

            _soundIn = new WasapiLoopbackCapture();
            _soundIn.Initialize();

            var soundInSource = new SoundInSource(_soundIn);
            var singleBlockNotificationStream = new SingleBlockNotificationStream(soundInSource);
            _source = singleBlockNotificationStream.ToWaveSource();

            if (!Directory.Exists(_loopbackDir))
            {
                Directory.CreateDirectory(_loopbackDir);
            }

            _writer = new WaveWriter(_loopbackDir + "/loopback.wav", _source.WaveFormat);

            byte[] buffer = new byte[_source.WaveFormat.BytesPerSecond / 2];
            soundInSource.DataAvailable += (s, e) =>
            {
                int read;
                while ((read = _source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    _writer.Write(buffer, 0, read);
                }
            };

            _lineSpectrumProvider = new BasicSpectrumProvider(_source.WaveFormat.Channels, _source.WaveFormat.SampleRate, fftSize);
            _spectrogramProvider = new BasicSpectrumProvider(_source.WaveFormat.Channels, _source.WaveFormat.SampleRate, fftSize);

            singleBlockNotificationStream.SingleBlockRead += SingleBlockNotificationStream_SingleBlockRead;
            _soundIn.Start();

            _lineSpectrum = new LineSpectrum(fftSize)
            {
                SpectrumProvider = _lineSpectrumProvider,
                UseAverage = true,
                BarCount = 22,
                BarSpacing = 1,
                IsXLogScale = true,
                ScalingStrategy = ScalingStrategy.Sqrt
            };
            _oscilloscope = new Oscilloscope();
            _spectrogram = new Spectrogram(fftSize)
            {
                SpectrumProvider = _spectrogramProvider,
                UseAverage = true,
                BarCount = (int)fftSize,
                BarSpacing = 0,
                IsXLogScale = true,
                ScalingStrategy = ScalingStrategy.Sqrt
            };
            _keyboardVisualizer = new KeyboardVisualizer();

            UpdateTimer.Start();
        }

        private void SingleBlockNotificationStream_SingleBlockRead(object sender, SingleBlockReadEventArgs e)
        {
            _lineSpectrumProvider.Add(e.Left, e.Right);
            _spectrogramProvider.Add(e.Left, e.Right);
            _oscilloscope.Add(e.Left, e.Right);
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            UpdateTimer.Stop();

            if (_soundIn != null)
            {
                _soundIn.Stop();
                _soundIn.Dispose();
                _soundIn = null;
            }
            if (_source != null)
            {
                _source.Dispose();
                _source = null;
            }
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            GenerateLineSpectrum();
            GenerateOscilloscope();
            GenerateSpectrogram();
            GenerateKeyboardSpectrum();
        }

        private void GenerateLineSpectrum()
        {
            Image image = LineSpectrumPictureBox.Image;
            var newImage = _lineSpectrum.CreateSpectrumLine(LineSpectrumPictureBox.Size, Color.Green, Color.Red, Color.Black, false);
            if (newImage != null)
            {
                LineSpectrumPictureBox.Image = newImage;
                if (image != null)
                {
                    image.Dispose();
                }
            }
        }

        private void GenerateOscilloscope()
        {
            Image image = OscilloscopePictureBox.Image;
            var newImage = _oscilloscope.CreateOscilloscope(OscilloscopePictureBox.Size, Color.Red, Color.DeepSkyBlue, Color.Black, true);
            if (newImage != null)
            {
                OscilloscopePictureBox.Image = newImage;
                if (image != null)
                {
                    image.Dispose();
                }
            }
        }

        private void GenerateSpectrogram()
        {
            Image image = SpectrogramPictureBox.Image;
            var newImage = _spectrogram.CreateSpectrogramLine(SpectrogramPictureBox.Size, Color.White, Color.Black, true);
            if (newImage != null)
            {
                SpectrogramPictureBox.Image = newImage;
                if (image != null)
                {
                    image.Dispose();
                }
            }
        }

        private void GenerateKeyboardSpectrum()
        {
            _keyboardVisualizer.SetKeyboardLighting((Bitmap)LineSpectrumPictureBox.Image);
        }
    }
}
