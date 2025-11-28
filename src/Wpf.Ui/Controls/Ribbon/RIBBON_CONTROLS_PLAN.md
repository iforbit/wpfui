# Ribbon Controls Implementation Plan

> 작성일: 2025-10-23
> 상태: Simplified Ribbon 구현 완료 후 추가 컨트롤 계획

---

## 📋 목차

1. [현재 구현 상태](#현재-구현-상태)
2. [추가 구현 계획](#추가-구현-계획)
3. [WPF.UI 기존 컨트롤 매핑](#wpfui-기존-컨트롤-매핑)
4. [구현 우선순위](#구현-우선순위)

---

## 현재 구현 상태

### ✅ 이미 구현된 Ribbon 컨트롤

| 컨트롤 | 파일 위치 | Simplified 지원 | 비고 |
|--------|----------|----------------|------|
| **RibbonButton** | [Buttons/RibbonButton.cs](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\Buttons\RibbonButton.cs) | ✅ | Large/Medium/Small 템플릿 |
| **RibbonToggleButton** | [Buttons/RibbonToggleButton.cs](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\Buttons\RibbonToggleButton.cs) | ✅ | IsChecked 상태 |
| **RibbonCheckBox** | [Buttons/RibbonCheckBox.cs](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\Buttons\RibbonCheckBox.cs) | ✅ | 체크박스 |
| **RibbonDropDownButton** | [Buttons/RibbonDropDownButton.cs](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\Buttons\RibbonDropDownButton.cs) | ✅ | Popup 메뉴 |
| **RibbonSplitButton** | [Buttons/RibbonSplitButton.cs](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\Buttons\RibbonSplitButton.cs) | ✅ | 버튼 + 드롭다운 |
| **InRibbonGallery** | [Gallery/InRibbonGallery.cs](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\Gallery\InRibbonGallery.cs) | ✅ | 아이템 갤러리 |
| **RibbonMenuItem** | [Menu/RibbonMenuItem.cs](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\Menu\RibbonMenuItem.cs) | ✅ | 메뉴 항목 |
| **RibbonGroupBox** | [Groups/RibbonGroupBox.cs](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\Groups\RibbonGroupBox.cs) | ✅ | 그룹 컨테이너 |
| **ScreenTip** | [Primitives/ScreenTip.cs](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\Primitives\ScreenTip.cs) | N/A | 툴팁 |

---

## 추가 구현 계획

### ⚠️ 구현 필요한 Ribbon 컨트롤

| 컨트롤 | WPF.UI 베이스 | 우선순위 | 예상 시간 | 비고 |
|--------|--------------|---------|----------|------|
| **RibbonComboBox** | `ComboBox` | 🔥 높음 | 4h | 드롭다운 선택 |
| **RibbonTextBox** | `TextBox` | 🔥 높음 | 3h | 텍스트 입력 |
| **RibbonRadioButton** | `RadioButton` | 🔶 중간 | 2h | 라디오 버튼 그룹 |
| **RibbonSlider** | `Slider` | 🔶 중간 | 3h | 값 조정 (줌 등) |
| **RibbonColorPicker** | `ColorPicker` (사용자 정의) | 🔶 중간 | 6h | 색상 선택 |
| **RibbonNumberBox** | `NumberBox` | 🔵 낮음 | 3h | 숫자 입력 |
| **RibbonDatePicker** | `DatePicker` | 🔵 낮음 | 3h | 날짜 선택 |
| **RibbonToggleSwitch** | `ToggleSwitch` | 🔵 낮음 | 2h | ON/OFF 스위치 |

---

## WPF.UI 기존 컨트롤 매핑

### 입력 컨트롤

| WPF.UI 컨트롤 | 위치 | Ribbon 래퍼 | 용도 |
|--------------|------|------------|------|
| `TextBox` | [Controls/TextBox/TextBox.cs](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\TextBox\TextBox.cs) | **RibbonTextBox** | 텍스트 입력 |
| `NumberBox` | [Controls/NumberBox/NumberBox.cs](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\NumberBox\NumberBox.cs) | **RibbonNumberBox** | 숫자 입력, 스피너 |
| `PasswordBox` | [Controls/PasswordBox/PasswordBox.cs](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\PasswordBox\PasswordBox.cs) | ❌ 불필요 | Ribbon에서 사용 안 함 |
| `RichTextBox` | [Controls/RichTextBox/RichTextBox.cs](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\RichTextBox\RichTextBox.cs) | ❌ 불필요 | Ribbon에서 사용 안 함 |

### 선택 컨트롤

| WPF.UI 컨트롤 | 위치 | Ribbon 래퍼 | 용도 |
|--------------|------|------------|------|
| `ComboBox` | (WPF 기본) | **RibbonComboBox** | 드롭다운 선택 (폰트, 크기 등) |
| `RadioButton` | (WPF 기본) | **RibbonRadioButton** | 라디오 그룹 (정렬 등) |
| `ToggleSwitch` | [Controls/ToggleSwitch/ToggleSwitch.cs](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\ToggleSwitch\ToggleSwitch.cs) | **RibbonToggleSwitch** | ON/OFF 스위치 |

### 날짜/시간 컨트롤

| WPF.UI 컨트롤 | 위치 | Ribbon 래퍼 | 용도 |
|--------------|------|------------|------|
| `DatePicker` | (WPF 기본) | **RibbonDatePicker** | 날짜 선택 |
| `TimePicker` | [Controls/TimePicker/TimePicker.cs](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\TimePicker\TimePicker.cs) | **RibbonTimePicker** | 시간 선택 |
| `CalendarDatePicker` | (WPF 기본) | ❌ 불필요 | DatePicker로 충분 |

### 범위/값 컨트롤

| WPF.UI 컨트롤 | 위치 | Ribbon 래퍼 | 용도 |
|--------------|------|------------|------|
| `Slider` | (WPF 기본) | **RibbonSlider** | 값 조정 (줌, 투명도 등) |
| `ProgressBar` | (WPF 기본) | ❌ 불필요 | Ribbon에서 사용 안 함 |
| `ProgressRing` | [Controls/ProgressRing/ProgressRing.cs](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\ProgressRing\ProgressRing.cs) | ❌ 불필요 | Ribbon에서 사용 안 함 |

### 색상 컨트롤

| WPF.UI 컨트롤 | 위치 | Ribbon 래퍼 | 용도 |
|--------------|------|------------|------|
| `ColorPicker` | ❌ 없음 | **RibbonColorPicker** | 색상 선택 (직접 구현 필요) |
| `ThumbRate` | [Controls/ThumbRate/ThumbRate.cs](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\ThumbRate\ThumbRate.cs) | ❌ 불필요 | Ribbon에서 사용 안 함 |

---

## 구현 우선순위

### Phase 1: 필수 입력 컨트롤 (우선순위 🔥 높음)

#### 1.1 RibbonComboBox
**예상 시간: 4시간**

```xml
<!-- 사용 예시: 폰트 선택 -->
<ui:RibbonComboBox Header="Font"
                   Width="120"
                   Icon="{ui:SymbolIcon TextFont24}"
                   ItemsSource="{Binding Fonts}"
                   SelectedItem="{Binding SelectedFont}"
                   ShowInSimplified="True" />
```

**구현 포인트:**
- WPF 기본 `ComboBox` 상속
- `IRibbonControl` 인터페이스 구현
- Simplified 모드: Width 축소 (예: 120 → 100)
- Classic 모드: Label 위, ComboBox 아래
- Simplified 모드: Label 왼쪽, ComboBox 오른쪽 (또는 Label 숨김)

**템플릿 구조:**
```xml
<!-- Classic: Vertical -->
<StackPanel Orientation="Vertical">
    <TextBlock Text="{TemplateBinding Header}" />
    <ComboBox ... />
</StackPanel>

<!-- Simplified: Horizontal or Label Hidden -->
<ComboBox ... />  <!-- Header는 ToolTip으로 -->
```

#### 1.2 RibbonTextBox
**예상 시간: 3시간**

```xml
<!-- 사용 예시: 검색 -->
<ui:RibbonTextBox Header="Search"
                  Width="150"
                  Icon="{ui:SymbolIcon Search24}"
                  Text="{Binding SearchText}"
                  ShowInSimplified="True" />
```

**구현 포인트:**
- WPF.UI `TextBox` 상속 (아이콘 지원)
- Simplified 모드: Width 유지 또는 축소
- PlaceholderText 지원

---

### Phase 2: 선택 컨트롤 (우선순위 🔶 중간)

#### 2.1 RibbonRadioButton
**예상 시간: 2시간**

```xml
<!-- 사용 예시: 정렬 옵션 -->
<ui:RibbonGroupBox Header="Alignment">
    <ui:RibbonRadioButton Header="Left"
                          Icon="{ui:SymbolIcon AlignLeft24}"
                          GroupName="Alignment"
                          IsChecked="True" />
    <ui:RibbonRadioButton Header="Center"
                          Icon="{ui:SymbolIcon AlignCenter24}"
                          GroupName="Alignment" />
    <ui:RibbonRadioButton Header="Right"
                          Icon="{ui:SymbolIcon AlignRight24}"
                          GroupName="Alignment" />
</ui:RibbonGroupBox>
```

**구현 포인트:**
- WPF 기본 `RadioButton` 상속
- RibbonToggleButton과 유사한 스타일
- GroupName 지원

#### 2.2 RibbonSlider
**예상 시간: 3시간**

```xml
<!-- 사용 예시: 줌 레벨 -->
<ui:RibbonSlider Header="Zoom"
                 Width="100"
                 Minimum="50"
                 Maximum="200"
                 Value="{Binding ZoomLevel}"
                 ShowInSimplified="True" />
```

**구현 포인트:**
- WPF 기본 `Slider` 상속
- Simplified 모드: 컴팩트 버전
- 값 표시 (예: "100%")

---

### Phase 3: 고급 컨트롤 (우선순위 🔵 낮음)

#### 3.1 RibbonColorPicker
**예상 시간: 6시간**

```xml
<!-- 사용 예시: 텍스트 색상 -->
<ui:RibbonColorPicker Header="Font Color"
                      Icon="{ui:SymbolIcon Color24}"
                      SelectedColor="{Binding FontColor}"
                      ShowInSimplified="True" />
```

**구현 포인트:**
- 별도 ColorPicker 컨트롤 구현 필요
- Popup 기반 색상 팔레트
- 최근 사용 색상 표시
- Office 스타일 색상 선택기

#### 3.2 RibbonNumberBox
**예상 시간: 3시간**

```xml
<!-- 사용 예시: 폰트 크기 -->
<ui:RibbonNumberBox Header="Font Size"
                    Width="60"
                    Minimum="8"
                    Maximum="72"
                    Value="{Binding FontSize}"
                    ShowInSimplified="True" />
```

**구현 포인트:**
- WPF.UI `NumberBox` 상속
- SpinButton 지원
- Simplified 모드: Width 축소

---

## 구현 템플릿 가이드

### 공통 구현 패턴

모든 Ribbon 컨트롤은 다음 패턴을 따릅니다:

#### 1. C# 클래스 구조

```csharp
namespace Wpf.Ui.Controls;

public class RibbonXXX : XXX, IRibbonControl
{
    // IRibbonControl 구현
    public RibbonControlSize Size { get; set; }
    public RibbonControlSizeDefinition SizeDefinition { get; set; }
    public RibbonControlSizeDefinition SimplifiedSizeDefinition { get; set; }

    public bool ShowInSimplified { get; set; } = true;
    public bool IsSimplified { get; private set; }

    public object? Icon { get; set; }
    public object? Header { get; set; }

    // ISimplifiedStateControl 구현
    public void UpdateSimplifiedState(bool isSimplified)
    {
        IsSimplified = isSimplified;
        // 추가 로직...
    }
}
```

#### 2. XAML 템플릿 구조

```xml
<ResourceDictionary>
    <!-- Classic Template -->
    <ControlTemplate x:Key="RibbonXXX.Classic">
        <StackPanel Orientation="Vertical">
            <controls:IconPresenter Icon="{TemplateBinding Icon}" IconSize="Large" />
            <ContentControl Content="{TemplateBinding Header}" />
            <!-- 실제 컨트롤 -->
        </StackPanel>
    </ControlTemplate>

    <!-- Simplified Template -->
    <ControlTemplate x:Key="RibbonXXX.Simplified">
        <StackPanel Orientation="Horizontal">
            <controls:IconPresenter Icon="{TemplateBinding Icon}" IconSize="Medium" />
            <!-- 실제 컨트롤 -->
        </StackPanel>
    </ControlTemplate>

    <!-- Default Style -->
    <Style TargetType="{x:Type controls:RibbonXXX}">
        <Setter Property="Template" Value="{StaticResource RibbonXXX.Classic}" />
        <Style.Triggers>
            <Trigger Property="IsSimplified" Value="True">
                <Setter Property="Template" Value="{StaticResource RibbonXXX.Simplified}" />
            </Trigger>
        </Style.Triggers>
    </Style>
</ResourceDictionary>
```

#### 3. 파일 구조

```
src/Wpf.Ui/Controls/Ribbon/Input/
├── RibbonComboBox.cs
├── RibbonComboBox.xaml
├── RibbonTextBox.cs
├── RibbonTextBox.xaml
├── RibbonNumberBox.cs
├── RibbonNumberBox.xaml
└── ...
```

---

## 총 예상 시간

| Phase | 컨트롤 수 | 예상 시간 |
|-------|----------|----------|
| **Phase 1** | 2개 (ComboBox, TextBox) | 7시간 |
| **Phase 2** | 2개 (RadioButton, Slider) | 5시간 |
| **Phase 3** | 2개 (ColorPicker, NumberBox) | 9시간 |
| **총계** | **6개** | **약 21시간 (3일)** |

---

## 참고 사항

### MS Office Ribbon 분석 결과

**자주 사용되는 입력 컨트롤:**
1. **ComboBox** (폰트, 크기, 스타일 선택) - 🔥 최우선
2. **TextBox** (검색, URL 입력) - 🔥 최우선
3. **ColorPicker** (색상 선택) - 🔶 중간
4. **Slider** (줌 레벨) - 🔶 중간
5. **NumberBox** (간격, 크기 조정) - 🔵 낮음

**사용 빈도가 낮은 컨트롤:**
- DatePicker: 특정 앱(Outlook)에서만
- TimePicker: 거의 사용 안 됨
- ToggleSwitch: Ribbon에서 거의 사용 안 됨 (CheckBox/ToggleButton으로 대체)

### 구현 우선순위 결정 기준

1. **사용 빈도** (Office 앱 분석)
2. **범용성** (여러 시나리오에서 사용 가능)
3. **구현 복잡도** (낮을수록 우선)

---

**문서 작성자:** Claude
**작성일:** 2025-10-23
**버전:** 1.0
