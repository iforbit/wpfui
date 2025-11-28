# diagrams.net Electrical Symbols to WPF Conversion

## Overview

diagrams.net (draw.io)의 electrical stencil 심볼들을 WPF에서 사용할 수 있는 형식으로 변환하는 프로젝트

## Source

- **Repository**: https://github.com/vmassol/draw.io (jgraph/drawio의 fork)
- **Location**: `war/stencils/electrical/`
- **License**: Apache 2.0
- **Total Files**: 23 XML stencil files

## Available Electrical Stencil Files

1. abstract.xml
2. capacitors.xml
3. diodes.xml
4. electro-mechanical.xml
5. iec417.xml
6. iec_logic_gates.xml
7. inductors.xml
8. instruments.xml
9. logic_gates.xml
10. miscellaneous.xml
11. mosfets1.xml
12. mosfets2.xml
13. op_amps.xml
14. opto-electronics.xml
15. opto_electronics.xml
16. plc_ladder.xml
17. power_semiconductors.xml
18. radio.xml
19. resistors.xml ✅ (완료)
20. signal_sources.xml
21. thermionic_devices.xml
22. transistors.xml
23. waveforms.xml

## Conversion Process

### 1. XML Structure Analysis

diagrams.net stencil XML 구조:

```xml
<shapes name="mxGraph.electrical.resistors">
  <shape name="Resistor 1" h="20" w="100" aspect="variable" strokewidth="inherit">
    <connections>
      <constraint x="0" y="0.5" perimeter="0" name="in"/>
      <constraint x="1" y="0.5" perimeter="0" name="out"/>
    </connections>
    <background>
      <rect x="18" y="0" w="64" h="20"/>
    </background>
    <foreground>
      <path>
        <move x="0" y="10"/>
        <line x="18" y="10"/>
      </path>
      <stroke/>
    </foreground>
  </shape>
</shapes>
```

### 2. Path Command Mapping

| mxGraph Command | WPF Path Data |
|----------------|---------------|
| `<move x="10" y="20"/>` | `M 10,20` |
| `<line x="30" y="40"/>` | `L 30,40` |
| `<close/>` | `Z` |
| `<arc rx="5" ry="5" x="20" y="10"/>` | `A 5,5 0 0 1 20,10` |
| `<rect x="10" y="5" w="50" h="20"/>` | `M 10,5 L 60,5 L 60,25 L 10,25 Z` |

### 3. WPF ResourceDictionary Output

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <GeometryGroup x:Key="Resistor_Resistor1">
        <PathGeometry Figures="M 18,0 L 82,0 L 82,20 L 18,20 Z" />
        <PathGeometry Figures="M 0,10 L 18,10" />
        <PathGeometry Figures="M 82,10 L 100,10" />
    </GeometryGroup>

</ResourceDictionary>
```

## Completed: resistors.xml

### Conversion Result

- **Source File**: https://raw.githubusercontent.com/vmassol/draw.io/master/war/stencils/electrical/resistors.xml
- **Output File**: `ResistorsResourceDictionary.xaml`
- **Total Shapes**: 22 resistor symbols

### Converted Symbols

1. Magnetoresistor (100x60)
2. Memristor 1 (100x20)
3. Memristor 2 (100x24)
4. Nonlinear Resistor (100x60)
5. Potentiometer 1 (100x40)
6. Potentiometer 2 (100x40)
7. Resistor, Adjustable Contact (100x40)
8. Resistor, Shunt (100x45)
9. Resistor 1 (100x20) - Standard rectangular resistor
10. Resistor 2 (100x20) - Zigzag resistor
11. Resistor 3 (100x20) - Rectangular resistor
12. Resistor 4 (100x20) - Zigzag resistor
13. Resistor With Instrument or Relay Shunt (100x45)
14. Symmetrical Photoconductive Transducer (100x60)
15. Symmetrical Varistor (100x60)
16. Tapped Resistor (100x40)
17. Trimmer Pot 1 (100x40)
18. Trimmer Pot 2 (100x40)
19. Trimmer Resistor 1 (100x40)
20. Trimmer Resistor 2 (100x40)
21. Variable Resistor 1 (100x40)
22. Variable Resistor 2 (100x40)

## Usage in WPF

### Example 1: Using Path with Geometry

```xml
<Path Data="{StaticResource Resistor_Resistor1}"
      Stroke="Black"
      StrokeThickness="1"
      Fill="Transparent"
      Width="100"
      Height="20"/>
```

### Example 2: Using in ItemsControl

```xml
<ItemsControl ItemsSource="{Binding ResistorSymbols}">
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <StackPanel Margin="10">
                <TextBlock Text="{Binding Name}"/>
                <Path Data="{Binding Geometry}"
                      Stroke="Black"
                      StrokeThickness="1"
                      Stretch="Uniform"
                      Width="100"/>
            </StackPanel>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

## Conversion Scripts

### PowerShell Script: convert-all-resistors.ps1

위치: `C:\Temp\github\WPFUI\convert-all-resistors.ps1`

주요 기능:
- XML 파일 파싱
- mxGraph path 명령을 WPF Path Data로 변환
- GeometryGroup 기반 ResourceDictionary 생성

## License Attribution

```
Electrical Symbols Source: draw.io (diagrams.net)
Copyright (c) JGraph Ltd
License: Apache 2.0
https://github.com/jgraph/drawio
```

## Next Steps

### Option 1: WPF Gallery Integration
- ResistorsResourceDictionary.xaml을 Gallery 프로젝트에 추가
- 새로운 페이지 생성하여 모든 저항 심볼 표시
- 각 심볼의 이름과 크기 정보 표시

### Option 2: Batch Conversion
나머지 22개 electrical stencil 파일 변환:
- capacitors.xml
- inductors.xml
- logic_gates.xml
- transistors.xml
- diodes.xml
- op_amps.xml
- ... (17개 추가)

### Option 3: Full Automation
- 23개 파일 전체 자동 변환 스크립트
- 통합 ResourceDictionary 생성
- 카테고리별 분류 및 인덱싱

## Files Generated

1. `resistors.xml` - 원본 stencil 파일
2. `convert-resistor-to-wpf.ps1` - 단일 심볼 변환 테스트 스크립트
3. `convert-all-resistors.ps1` - 전체 변환 스크립트
4. `ResistorsResourceDictionary.xaml` - WPF ResourceDictionary 출력

## Date

2025-10-22
