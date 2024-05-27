using CSCore;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.Streams;
using CSCore.Streams.Effects;


WasapiLoopbackCapture soundIn = new WasapiLoopbackCapture();

soundIn.Initialize();
var soundInSource = new SoundInSource(soundIn);
ISampleSource source = soundInSource.ToSampleSource().AppendSource(x => new PitchShifter(x), out var _pitchShifter);

FftProvider fftProvider = new FftProvider(soundInSource.WaveFormat.Channels, FftSize.Fft4096);

var notificationSource = new SingleBlockNotificationStream(source);
notificationSource.SingleBlockRead += (s, a) =>
{
    fftProvider.Add(a.Left, a.Right);
};


float[] buffer = new float[source.WaveFormat.BytesPerSecond / 2];
soundInSource.DataAvailable += (s, aEvent) =>
{
    int read;
    while ((read = notificationSource.Read(buffer, 0, buffer.Length)) > 0) ;
};


soundIn.Start();


float[] fftData = new float[(int)fftProvider.FftSize];
ArraySegment<float> fftDataSegment = new ArraySegment<float>(fftData, 0, 64);
string outStr = "";
while (true)
{
    if (fftProvider.IsNewDataAvailable)
    {
        fftProvider.GetFftData(fftData);
        outStr = "";
        for (int i = 0; i < fftDataSegment.Count; i++)
        {
            outStr += Math.Round(fftDataSegment[i] * (int)fftProvider.FftSize);
            if (i < fftDataSegment.Count - 1) outStr += ", ";
        }

        Console.WriteLine(outStr);
    }

    Thread.Sleep(25);
};

