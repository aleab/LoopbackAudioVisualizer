using CSCore;
using System;

namespace Aleab.LoopbackAudioVisualizer
{
    public class SilenceSource : IWaveSource
    {
        public WaveFormat WaveFormat { get; } = new WaveFormat(44100, 16, 2);

        public long Position
        {
            get { return -1; }
            set { throw new InvalidOperationException(); }
        }

        public long Length { get { return -1; } }

        public bool CanSeek { get { return false; } }

        public int Read(byte[] buffer, int offset, int count)
        {
            Array.Clear(buffer, offset, count);
            return count;
        }

        public void Dispose()
        {
        }
    }
}