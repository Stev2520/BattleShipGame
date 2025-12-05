using System;
using NAudio.Wave;
using System.Threading.Tasks;

namespace BattleShipGame2.Models;

public static class SoundManager
{
    /// <summary>
    /// Воспроизводит звук попадания по кораблю.
    /// </summary>
    public static void PlayHit()
    {
        PlayTone(800, 100);
    }

    /// <summary>
    /// Воспроизводит звук промаха.
    /// </summary>
    public static void PlayMiss()
    {
        PlayTone(300, 150);
    }

    /// <summary>
    /// Воспроизводит звук потопления корабля.
    /// </summary>
    public static void PlaySunk()
    {
        Task.Run(() =>
        {
            PlayTone(600, 100);
            Task.Delay(50).Wait();
            PlayTone(500, 100);
            Task.Delay(50).Wait();
            PlayTone(400, 200);
        });
    }

    /// <summary>
    /// Воспроизводит звук победы в игре.
    /// </summary>
    public static void PlayWin()
    {
        Task.Run(() =>
        {
            PlayTone(523, 150);
            Task.Delay(50).Wait();
            PlayTone(659, 150);
            Task.Delay(50).Wait();
            PlayTone(784, 300);
        });
    }

    /// <summary>
    /// Воспроизводит звук проигрыша в игре.
    /// </summary>
    public static void PlayLose()
    {
        Task.Run(() =>
        {
            PlayTone(400, 200);
            Task.Delay(50).Wait();
            PlayTone(300, 300);
        });
    }

    private static void PlayTone(double frequency, int durationMs)
    {
        Task.Run(() =>
        {
            try
            {
                int sampleRate = 44100;
                double amplitude = 0.5;

                // Создаем WAV файл в памяти
                using (var memoryStream = new System.IO.MemoryStream())
                {
                    // Заголовок WAV файла
                    WriteWavHeader(memoryStream, durationMs, sampleRate);

                    // Аудиоданные (синусоида)
                    short[] data = new short[(int)(sampleRate * durationMs / 1000.0)];
                    double freq = frequency * 2.0 * Math.PI / sampleRate;

                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = (short)(amplitude * Math.Sin(freq * i) * short.MaxValue);
                    }

                    // Конвертируем short[] в byte[]
                    byte[] byteData = new byte[data.Length * 2];
                    Buffer.BlockCopy(data, 0, byteData, 0, byteData.Length);

                    memoryStream.Write(byteData, 0, byteData.Length);
                    memoryStream.Position = 0;

                    // Воспроизводим
                    using (var audioFile = new WaveFileReader(memoryStream))
                    using (var outputDevice = new WaveOutEvent())
                    {
                        outputDevice.Init(audioFile);
                        outputDevice.Play();

                        // Ждем окончания звука
                        while (outputDevice.PlaybackState == PlaybackState.Playing)
                        {
                            Task.Delay(50).Wait();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Sound Error] {ex.Message}");
            }
        });
    }

    private static void WriteWavHeader(System.IO.Stream stream, int durationMs, int sampleRate)
    {
        int numChannels = 1;
        int bitsPerSample = 16;
        int numSamples = sampleRate * durationMs / 1000;
        int subChunk2Size = numSamples * numChannels * bitsPerSample / 8;

        // RIFF заголовок
        WriteString(stream, "RIFF");
        WriteInt(stream, 36 + subChunk2Size);
        WriteString(stream, "WAVE");

        // fmt подзаголовок
        WriteString(stream, "fmt ");
        WriteInt(stream, 16);
        WriteShort(stream, 1); // AudioFormat = PCM
        WriteShort(stream, (short)numChannels);
        WriteInt(stream, sampleRate);
        WriteInt(stream, sampleRate * numChannels * bitsPerSample / 8); // ByteRate
        WriteShort(stream, (short)(numChannels * bitsPerSample / 8)); // BlockAlign
        WriteShort(stream, (short)bitsPerSample);

        // data подзаголовок
        WriteString(stream, "data");
        WriteInt(stream, subChunk2Size);
    }

    private static void WriteString(System.IO.Stream stream, string s)
    {
        byte[] bytes = System.Text.Encoding.ASCII.GetBytes(s);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static void WriteInt(System.IO.Stream stream, int value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static void WriteShort(System.IO.Stream stream, short value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }
}