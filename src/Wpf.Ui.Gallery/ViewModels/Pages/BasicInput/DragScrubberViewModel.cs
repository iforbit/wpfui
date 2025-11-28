// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Gallery.ViewModels.Pages.BasicInput;

public partial class DragScrubberViewModel : ViewModel
{
    private double _sampleValue = 42.5;
    private double _width = 120.0;
    private double _height = 80.0;

    public double SampleValue
    {
        get => _sampleValue;
        set
        {
            if (Math.Abs(_sampleValue - value) > double.Epsilon)
            {
                _sampleValue = value;
                OnPropertyChanged();
            }
        }
    }

    public double Width
    {
        get => _width;
        set
        {
            if (Math.Abs(_width - value) > double.Epsilon)
            {
                _width = value;
                OnPropertyChanged();
            }
        }
    }

    public double Height
    {
        get => _height;
        set
        {
            if (Math.Abs(_height - value) > double.Epsilon)
            {
                _height = value;
                OnPropertyChanged();
            }
        }
    }
}