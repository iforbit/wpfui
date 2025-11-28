// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Demo.Writer.SharedMemory;

public enum SignalType
{
    Sine,
    Cosine,
    Square,
    Triangle,
    Sawtooth,
    Noise
}

public sealed class SignalGenerator
{
    private readonly Random _random = new();

    public double Frequency { get; set; } = 1.0;
    public double Amplitude { get; set; } = 1.0;
    public double Phase { get; set; } = 0.0;
    public double Offset { get; set; } = 0.0;
    public SignalType Type { get; set; } = SignalType.Sine;

    public SignalGenerator() { }

    public SignalGenerator(SignalType type, double frequency, double amplitude, double phase = 0.0, double offset = 0.0)
    {
        Type = type;
        Frequency = frequency;
        Amplitude = amplitude;
        Phase = phase;
        Offset = offset;
    }

    public float Generate(double time)
    {
        double t = 2.0 * Math.PI * Frequency * time + Phase;
        double value = Type switch
        {
            SignalType.Sine => Math.Sin(t),
            SignalType.Cosine => Math.Cos(t),
            SignalType.Square => Math.Sign(Math.Sin(t)),
            SignalType.Triangle => (2.0 / Math.PI) * Math.Asin(Math.Sin(t)),
            SignalType.Sawtooth => 2.0 * ((Frequency * time + Phase / (2.0 * Math.PI)) % 1.0) - 1.0,
            SignalType.Noise => _random.NextDouble() * 2.0 - 1.0,
            _ => 0.0
        };

        return (float)(Amplitude * value + Offset);
    }

    public void GeneratePoints(double startTime, double duration, int pointCount, Span<(double X, float Y)> output)
    {
        if (pointCount <= 0)
            throw new ArgumentException("Point count must be positive", nameof(pointCount));

        if (output.Length < pointCount)
            throw new ArgumentException("Output buffer is too small", nameof(output));

        // Use millisecond scale for X-axis (more readable: 0.1, 0.2, 1, 2, 3...)
        double startTimeMs = startTime * 1000.0;
        double durationMs = duration * 1000.0;
        double dt = durationMs / pointCount;

        for (int i = 0; i < pointCount; i++)
        {
            double xValue = startTimeMs + i * dt;
            double time = startTime + (i * duration / pointCount);
            output[i] = (xValue, Generate(time));
        }
    }

    public (double X, float Y)[] GeneratePoints(double startTime, double duration, int pointCount)
    {
        var result = new (double X, float Y)[pointCount];
        GeneratePoints(startTime, duration, pointCount, result);
        return result;
    }

    /// <summary>
    /// Generate points with sample index as X-axis and elapsed time for Y calculation
    /// </summary>
    /// <param name="startIndex">Starting sample index</param>
    /// <param name="sampleRate">Samples per second (e.g., 300000 for 60Hz * 5000 points)</param>
    /// <param name="pointCount">Number of points to generate</param>
    public (double X, float Y)[] GeneratePointsWithIndex(long startIndex, double sampleRate, int pointCount)
    {
        var result = new (double X, float Y)[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            // X = sample index (0, 1, 2, 3, ...)
            double xValue = startIndex + i;

            // Y = signal value calculated from sample index / sample rate
            // This creates repeating waveform with consistent frequency
            double time = (startIndex + i) / sampleRate;
            float yValue = Generate(time);

            result[i] = (xValue, yValue);
        }

        return result;
    }
}
