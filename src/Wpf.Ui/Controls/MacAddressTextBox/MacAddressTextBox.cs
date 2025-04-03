// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wpf.Ui.Controls;

/// <summary>
/// Provides a masked text box for formatted text input.
/// This partial class implements the UI logic for handling input masks, such as MAC addresses.
/// </summary>
public partial class MacAddressTextBox : Wpf.Ui.Controls.TextBox
{
    public static readonly DependencyProperty MaskProperty = DependencyProperty.Register(
          nameof(Mask),
          typeof(string),
          typeof(MacAddressTextBox),
          new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets 입력값을 제한할 정규 표현식 패턴을 지정합니다.
    /// 예: "[0-9A-Fa-f]{2}(:[0-9A-Fa-f]{2}){5}" (MAC 주소)
    /// </summary>
    public string Mask
    {
        get => (string)GetValue(MaskProperty);
        set => SetValue(MaskProperty, value);
    }

    public static readonly DependencyProperty MaskTypeProperty = DependencyProperty.Register(
        nameof(MaskType),
        typeof(MaskType),
        typeof(MacAddressTextBox),
        new PropertyMetadata(MaskType.None));

    /// <summary>
    /// Gets or sets 마스크의 타입을 지정합니다. 기본은 None이며, RegEx로 설정하면 Mask 속성이 정규식으로 해석됩니다.
    /// </summary>
    public MaskType MaskType
    {
        get => (MaskType)GetValue(MaskTypeProperty);
        set => SetValue(MaskTypeProperty, value);
    }

    public static readonly DependencyProperty PromptCharProperty = DependencyProperty.Register(
        nameof(PromptChar),
        typeof(char),
        typeof(MacAddressTextBox),
        new PropertyMetadata('_'));

    /// <summary>
    /// Gets or sets 마스크 입력 시 자리 채움 문자입니다.
    /// </summary>
    public char PromptChar
    {
        get => (char)GetValue(PromptCharProperty);
        set => SetValue(PromptCharProperty, value);
    }

    public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register(
        nameof(Watermark),
        typeof(string),
        typeof(MacAddressTextBox),
        new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets a value indicating whether 사용자가 텍스트를 입력했는지 여부를 나타냅니다.
    /// 초기 상태에서는 false이며, 입력이 시작되면 true로 전환됩니다.
    /// </summary>
    public bool HasUserInput
    {
        get => (bool)GetValue(HasUserInputProperty);
        set => SetValue(HasUserInputProperty, value);
    }

    public static readonly DependencyProperty HasUserInputProperty = DependencyProperty.Register(
    nameof(HasUserInput),
    typeof(bool),
    typeof(MacAddressTextBox),
    new PropertyMetadata(false));

    /// <summary>
    /// Gets or sets a value indicating whether 입력된 원시 텍스트(콜론 제외)가 12자리인지 여부를 나타냅니다.
    /// </summary>
    public bool IsComplete
    {
        get => (bool)GetValue(IsCompleteProperty);
        set => SetValue(IsCompleteProperty, value);
    }

    public static readonly DependencyProperty IsCompleteProperty = DependencyProperty.Register(
        nameof(IsComplete),
        typeof(bool),
        typeof(MacAddressTextBox),
        new PropertyMetadata(false));

    /// <summary>
    /// Gets or sets 입력값이 없을 때 표시할 힌트 텍스트입니다.
    /// </summary>
    public string Watermark
    {
        get => (string)GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    public MacAddressTextBox()
    {
        // 기본 스타일(템플릿) 지정 – Generic.xaml에서 정의
        // DefaultStyleKeyProperty.OverrideMetadata(typeof(MacAddressTextBox), new FrameworkPropertyMetadata(typeof(MacAddressTextBox)));
    }

    protected override void OnPreviewTextInput(TextCompositionEventArgs e)
    {
        if (MaskType == MaskType.RegEx && !string.IsNullOrWhiteSpace(Mask))
        {
            string proposedText = GetProposedText(e.Text);

            // 콜론 제거 후, OnTextChanged와 동일한 방식으로 포맷팅
            string textWithoutColons = proposedText.Replace(":", string.Empty);
            string formattedText = string.Empty;

            // 2글자마다 콜론 삽입 및 대문자 변환
            for (int i = 0; i < textWithoutColons.Length; i++)
            {
                if (i > 0 && i % 2 == 0)
                {
                    formattedText += ":";
                }

                formattedText += textWithoutColons[i];
            }

            // 입력된 텍스트를 대문자로 변환
            formattedText = formattedText.ToUpper();

            // 포맷팅된 문자열을 사용하여 검증
            if (formattedText.Length < 17)
            {
                Regex partialRegex = new Regex(@"^(?:[0-9A-Fa-f]{0,2}(?::[0-9A-Fa-f]{0,2}){0,5})$");
                if (!partialRegex.IsMatch(formattedText))
                {
                    e.Handled = true;
                    return;
                }
            }
            else
            {
                Regex fullRegex = new Regex("^" + Mask + "$");
                if (!fullRegex.IsMatch(formattedText))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        base.OnPreviewTextInput(e);
    }

    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);

        if (MaskType == MaskType.RegEx && !string.IsNullOrWhiteSpace(Mask))
        {
            // 기존 텍스트에서 콜론(:) 제거
            string textWithoutColons = this.Text.Replace(":", string.Empty);
            string formattedText = string.Empty;

            // 2글자마다 콜론 삽입
            for (int i = 0; i < textWithoutColons.Length; i++)
            {
                if (i > 0 && i % 2 == 0)
                {
                    formattedText += ":";
                }

                formattedText += textWithoutColons[i];
            }

            // 전체 텍스트를 대문자로 변환
            formattedText = formattedText.ToUpper();

            // 텍스트가 달라졌다면 갱신 (커서 위치는 단순하게 끝으로 이동)
            if (this.Text != formattedText)
            {
                this.SetCurrentValue(TextProperty, formattedText);
                this.SelectionStart = formattedText.Length;
            }

            // 사용자가 입력을 시작했음을 표시 (텍스트가 비어있지 않으면)
            SetCurrentValue(HasUserInputProperty, !string.IsNullOrEmpty(textWithoutColons));

            // IsComplete 값을 업데이트 (12자리 이상이면 true)
            SetCurrentValue(IsCompleteProperty, textWithoutColons.Length >= 12);
        }
    }

    private string GetProposedText(string input)
    {
        string text = this.Text ?? string.Empty;
        int selectionStart = this.SelectionStart;
        int selectionLength = this.SelectionLength;

        // 선택된 부분을 제거한 후 새 입력을 삽입
        string newText = text.Remove(selectionStart, selectionLength).Insert(selectionStart, input);
        return newText;
    }
}
