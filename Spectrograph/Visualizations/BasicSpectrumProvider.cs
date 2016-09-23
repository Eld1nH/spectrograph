﻿using CSCore.DSP;
using System;
using System.Collections.Generic;

namespace Spectrograph.Visualizations
{
    public class BasicSpectrumProvider : FftProvider, ISpectrumProvider
    {
        private readonly int _sampleRate;
        private readonly List<object> _contexts = new List<object>();

        public BasicSpectrumProvider(int channels, int sampleRate, FftSize fftSize)
            : base(channels, fftSize)
        {
            if (sampleRate <= 0)
            {
                throw new ArgumentOutOfRangeException("sampleRate");
            }
            _sampleRate = sampleRate;
        }

        public int GetFftBandIndex(float frequency)
        {
            int fftSize = (int)FftSize;
            double f = _sampleRate / 2.0;
            return (int)((frequency / f) * (fftSize / 2));
        }

        public bool GetFftData(float[] fftResultBuffer, object context)
        {
            if (_contexts.Contains(context))
            {
                return false;
            }

            _contexts.Add(context);
            GetFftData(fftResultBuffer);
            return true;
        }

        public new void Add(float[] samples, int count)
        {
            base.Add(samples, count);
            if (count > 0)
                _contexts.Clear();
        }

        public new void Add(float left, float right)
        {
            base.Add(left, right);
            _contexts.Clear();
        }
    }
}
