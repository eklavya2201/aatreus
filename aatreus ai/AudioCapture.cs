using NAudio.Wave;
using System.Collections.Generic;
using System.IO;

namespace SalesCopilot
{
    public class AudioCapture
    {
        private WaveInEvent _waveIn;
        private List<byte> _buffer = new();
        private WaveFormat _format;

        public void Start()
        {
            _buffer.Clear();
            _format = new WaveFormat(16000, 1);

            // ✅ ALWAYS USE DEFAULT MIC
            _waveIn = new WaveInEvent
            {
                DeviceNumber = 0,
                WaveFormat = _format,
                BufferMilliseconds = 100
            };

            _waveIn.DataAvailable += (s, e) =>
            {
                if (e.BytesRecorded > 0)
                    _buffer.AddRange(e.Buffer[..e.BytesRecorded]);
            };

            _waveIn.StartRecording();
        }

        public byte[] StopAndGetWav()
        {
            _waveIn?.StopRecording();
            _waveIn?.Dispose();

            if (_buffer.Count < _format.AverageBytesPerSecond * 1.2)
                return null;

            using var ms = new MemoryStream();
            using var writer = new WaveFileWriter(ms, _format);
            writer.Write(_buffer.ToArray(), 0, _buffer.Count);
            writer.Flush();

            return ms.ToArray();
        }
    }
}
