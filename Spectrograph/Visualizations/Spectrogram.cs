using CSCore.DSP;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Spectrograph.Visualizations
{
    public class Spectrogram : SpectrumBase
    {
        private int _currentLine = 0;
        private double _barSpacing;
        private double _barWidth;
        private int _barCount;
        private Size _currentSize;
        private Bitmap _previousBitmap;

        public double BarWidth
        {
            get
            {
                return _barWidth;
            }
        }

        public double BarSpacing
        {
            get
            {
                return _barSpacing;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                _barSpacing = value;
                UpdateFrequencyMapping();
            }
        }

        public int BarCount
        {
            get
            {
                return _barCount;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                _barCount = value;
                SpectrumResolution = value;
                UpdateFrequencyMapping();
            }
        }

        public Size CurrentSize
        {
            get
            {
                return _currentSize;
            }
            protected set
            {
                _currentSize = value;
            }
        }

        public Spectrogram(FftSize fftSize)
        {
            FftSize = fftSize;
        }

        public Bitmap CreateSpectrogramLine(Size size, Color foregroundColor, Color backgroundColor, bool highQuality)
        {
            if (!UpdateFrequencyMappingIfNessesary(size))
            {
                return null;
            }

            var fftBuffer = new float[(int)FftSize];

            if (SpectrumProvider.GetFftData(fftBuffer, this))
            {
                using (var foregroundPen = new Pen(foregroundColor))
                using (var backgroundPen = new Pen(backgroundColor))
                {
                    var bitmap = _previousBitmap == null ? new Bitmap(size.Width, (int)FftSize) : new Bitmap(_previousBitmap, size.Width, (int)FftSize);

                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        PrepareGraphics(graphics, highQuality);

                        if (_previousBitmap == null)
                        {
                            graphics.Clear(backgroundColor);
                        }

                        DrawSpectrogramLine(graphics, foregroundPen, backgroundPen, fftBuffer, size);
                    }

                    _previousBitmap = bitmap;
                    return new Bitmap(bitmap, size);
                }
            }
            return null;
        }

        private void DrawSpectrogramLine(Graphics graphics, Pen foregroundPen, Pen backgroundPen, float[] fftBuffer, Size size)
        {
            SpectrumPointData[] spectrumPoints = CalculateSpectrumPoints(size.Height, fftBuffer);

            if (spectrumPoints.Length == (int)FftSize)
            {
                graphics.DrawLine(backgroundPen, _currentLine, 0, _currentLine, (int)FftSize);

                for (int i = 0; i < spectrumPoints.Length; i++)
                {
                    graphics.DrawLine(new Pen(Color.FromArgb((int)spectrumPoints[i].Value > 255 ? 255 : (int)spectrumPoints[i].Value, foregroundPen.Color)),
                                      _currentLine, (int)FftSize - i, _currentLine, (int)FftSize - i - 1);
                }
            }

            _currentLine++;
            _currentLine = _currentLine > size.Width - 1 ? 0 : _currentLine;
        }

        private bool UpdateFrequencyMappingIfNessesary(Size newSize)
        {
            if (newSize != CurrentSize)
            {
                CurrentSize = newSize;
                UpdateFrequencyMapping();
            }

            return newSize.Width > 0 && newSize.Height > 0;
        }

        protected override void UpdateFrequencyMapping()
        {
            _barWidth = 1;
            base.UpdateFrequencyMapping();
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
