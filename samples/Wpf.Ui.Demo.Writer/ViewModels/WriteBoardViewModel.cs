// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;

using Wpf.Ui.Demo.Writer.SharedMemory;

namespace Wpf.Ui.Demo.Writer.ViewModels;

public partial class WriteBoardViewModel : ViewModel, IDisposable
{
    private readonly SharedMemoryWriter _writer;
    private readonly DispatcherTimer _timer;
    private readonly DateTime _startTime;
    private long _sampleIndex;
    private bool _isRunning;
    private int _updateRate = 60; // Hz (60 FPS)
    private int _channelCount = 1;
    private int _pointsPerUpdate = 1000; // Reduced for more visible waveform cycles

    public ObservableCollection<ChannelConfig> Channels { get; }

    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (_isRunning != value)
            {
                _isRunning = value;
                OnPropertyChanged();
            }
        }
    }

    public int UpdateRate
    {
        get => _updateRate;
        set
        {
            if (_updateRate != value && value > 0 && value <= 1000)
            {
                _updateRate = value;
                OnPropertyChanged();
                UpdateTimerInterval();
            }
        }
    }

    public int ChannelCount
    {
        get => _channelCount;
        set
        {
            if (_channelCount != value && value > 0 && value <= 10)
            {
                _channelCount = value;
                OnPropertyChanged();
                UpdateChannels();
            }
        }
    }

    public int PointsPerUpdate
    {
        get => _pointsPerUpdate;
        set
        {
            if (_pointsPerUpdate != value && value > 0 && value <= 50000)
            {
                _pointsPerUpdate = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ResetCommand { get; }

    public WriteBoardViewModel()
    {
        _writer = new SharedMemoryWriter(maxChannels: 10, maxPointsPerChannel: 50000);
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000.0 / _updateRate) };
        _timer.Tick += OnTimerTick;
        _startTime = DateTime.Now;
        _sampleIndex = 0;

        Channels = new ObservableCollection<ChannelConfig>();
        InitializeChannels();

        StartCommand = new RelayCommand(Start, () => !IsRunning);
        StopCommand = new RelayCommand(Stop, () => IsRunning);
        ResetCommand = new RelayCommand(Reset);
    }

    private void InitializeChannels()
    {
        Channels.Clear();

        // Default channel configurations for typical waveform display
        // Higher frequencies for more visible cycles on screen
        var defaultConfigs = new[]
        {
            new { Name = "CH1", Type = SignalType.Sine, Freq = 10.0, Amp = 5.0, Phase = 0.0, Offset = 0.0 },
            new { Name = "CH2", Type = SignalType.Cosine, Freq = 15.0, Amp = 4.0, Phase = 0.0, Offset = 2.0 },
            new { Name = "CH3", Type = SignalType.Square, Freq = 8.0, Amp = 3.0, Phase = 0.0, Offset = -2.0 },
            new { Name = "CH4", Type = SignalType.Triangle, Freq = 12.0, Amp = 4.5, Phase = 0.0, Offset = 5.0 }
        };

        for (int i = 0; i < _channelCount && i < defaultConfigs.Length; i++)
        {
            var config = defaultConfigs[i];
            Channels.Add(new ChannelConfig
            {
                Name = config.Name,
                Type = config.Type,
                Frequency = config.Freq,
                Amplitude = config.Amp,
                Phase = config.Phase,
                Offset = config.Offset
            });
        }

        // Add additional channels if needed
        for (int i = defaultConfigs.Length; i < _channelCount; i++)
        {
            Channels.Add(new ChannelConfig
            {
                Name = $"CH{i + 1}",
                Type = (SignalType)(i % 6),
                Frequency = 1.0 + i * 0.5,
                Amplitude = 1.0,
                Phase = 0.0,
                Offset = 0.0
            });
        }
    }

    private void UpdateChannels()
    {
        // Default configs for new channels
        var defaultConfigs = new[]
        {
            new { Name = "CH1", Type = SignalType.Sine, Freq = 2.0, Amp = 5.0, Phase = 0.0, Offset = 0.0 },
            new { Name = "CH2", Type = SignalType.Cosine, Freq = 3.0, Amp = 4.0, Phase = 0.0, Offset = 2.0 },
            new { Name = "CH3", Type = SignalType.Square, Freq = 1.5, Amp = 3.0, Phase = 0.0, Offset = -2.0 },
            new { Name = "CH4", Type = SignalType.Triangle, Freq = 2.5, Amp = 4.5, Phase = 0.0, Offset = 5.0 },
            new { Name = "CH5", Type = SignalType.Sawtooth, Freq = 1.8, Amp = 3.5, Phase = 0.0, Offset = 0.0 },
            new { Name = "CH6", Type = SignalType.Noise, Freq = 1.0, Amp = 2.0, Phase = 0.0, Offset = 0.0 }
        };

        while (Channels.Count < _channelCount)
        {
            int i = Channels.Count;
            if (i < defaultConfigs.Length)
            {
                var config = defaultConfigs[i];
                Channels.Add(new ChannelConfig
                {
                    Name = config.Name,
                    Type = config.Type,
                    Frequency = config.Freq,
                    Amplitude = config.Amp,
                    Phase = config.Phase,
                    Offset = config.Offset
                });
            }
            else
            {
                Channels.Add(new ChannelConfig
                {
                    Name = $"CH{i + 1}",
                    Type = (SignalType)(i % 6),
                    Frequency = 1.0 + i * 0.5,
                    Amplitude = 1.0,
                    Phase = 0.0,
                    Offset = 0.0
                });
            }
        }

        while (Channels.Count > _channelCount)
        {
            Channels.RemoveAt(Channels.Count - 1);
        }
    }

    private void UpdateTimerInterval()
    {
        if (_updateRate > 0)
        {
            _timer.Interval = TimeSpan.FromMilliseconds(1000.0 / _updateRate);
        }
    }

    private void Start()
    {
        IsRunning = true;
        _timer.Start();
    }

    private void Stop()
    {
        IsRunning = false;
        _timer.Stop();
    }

    private void Reset()
    {
        Stop();
        _sampleIndex = 0;
        _writer.Clear();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (!IsRunning)
            return;

        try
        {
            var channelData = new List<(double X, float Y)[]>();

            foreach (ChannelConfig config in Channels)
            {
                var generator = new SignalGenerator(
                    config.Type,
                    config.Frequency,
                    config.Amplitude,
                    config.Phase,
                    config.Offset
                );

                // Calculate sample rate: PointsPerUpdate points for 1 complete cycle at 1Hz
                // Example: Frequency=1Hz, PointsPerUpdate=5000 → sampleRate=5000 samples/sec
                //          → 5000 points will show exactly 1 complete cycle
                // Example: Frequency=2Hz, PointsPerUpdate=5000 → sampleRate=10000 samples/sec
                //          → 5000 points will show exactly 2 complete cycles
                double sampleRate = config.Frequency * _pointsPerUpdate;

                (double X, float Y)[] points = generator.GeneratePointsWithIndex(_sampleIndex, sampleRate, _pointsPerUpdate);
                channelData.Add(points);
            }

            // Convert to ReadOnlyMemory
            ReadOnlyMemory<(double X, float Y)>[] channelMemories = channelData.Select(arr => new ReadOnlyMemory<(double X, float Y)>(arr)).ToArray();
            _writer.WriteChannels(channelMemories);

            _sampleIndex += _pointsPerUpdate;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error writing to shared memory: {ex.Message}");
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer?.Stop();
            _writer?.Dispose();
        }
    }
}

public class ChannelConfig : INotifyPropertyChanged
{
    private string _name = "CH1";
    private SignalType _type = SignalType.Sine;
    private double _frequency = 1.0;
    private double _amplitude = 1.0;
    private double _phase = 0.0;
    private double _offset = 0.0;

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    public SignalType Type
    {
        get => _type;
        set
        {
            if (_type != value)
            {
                _type = value;
                OnPropertyChanged();
            }
        }
    }

    public double Frequency
    {
        get => _frequency;
        set
        {
            if (_frequency != value)
            {
                _frequency = value;
                OnPropertyChanged();
            }
        }
    }

    public double Amplitude
    {
        get => _amplitude;
        set
        {
            if (_amplitude != value)
            {
                _amplitude = value;
                OnPropertyChanged();
            }
        }
    }

    public double Phase
    {
        get => _phase;
        set
        {
            if (_phase != value)
            {
                _phase = value;
                OnPropertyChanged();
            }
        }
    }

    public double Offset
    {
        get => _offset;
        set
        {
            if (_offset != value)
            {
                _offset = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();
}
