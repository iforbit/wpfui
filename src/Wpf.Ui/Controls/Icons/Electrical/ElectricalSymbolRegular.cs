// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace Wpf.Ui.Controls;

/// <summary>
/// Represents a list of electrical symbols from diagrams.net converted via IcoMoon.
/// <para>May be converted to <see langword="char"/> using <c>GetGlyph()</c> or to <see langword="string"/> using <c>GetString()</c></para>
/// </summary>
#pragma warning disable CS1591
public enum ElectricalSymbolRegular
{
    /// <summary>
    /// Empty icon for navigation purposes.
    /// </summary>
    Empty = 0x0,

    // Unicode mappings from IcoMoon electrical-icons font (from selection.json)
    Magnetoresistor = 0xE900, // 59648
    Memristor1 = 0xE901, // 59649
    Memristor2 = 0xE902, // 59650
    NonlinearResistor = 0xE903, // 59651
    Potentiometer1 = 0xE904, // 59652
    Potentiometer2 = 0xE905, // 59653
    ResistorAdjustableContact = 0xE906, // 59654
    ResistorShunt = 0xE907, // 59655
    Resistor1 = 0xE908, // 59656
    Resistor2 = 0xE909, // 59657
    Resistor3 = 0xE90A, // 59658
    Resistor4 = 0xE90B, // 59659
    ResistorWithInstrumentOrRelayShunt = 0xE90C, // 59660
    SymmetricalPhotoconductiveTransducer = 0xE90D, // 59661
    SymmetricalVaristor = 0xE90E, // 59662
    TappedResistor = 0xE90F, // 59663
    TrimmerPot1 = 0xE910, // 59664
    TrimmerPot2 = 0xE911, // 59665
    TrimmerResistor1 = 0xE912, // 59666
    TrimmerResistor2 = 0xE913, // 59667
    VariableResistor1 = 0xE914, // 59668
    VariableResistor2 = 0xE915, // 59669
}
#pragma warning restore CS1591
