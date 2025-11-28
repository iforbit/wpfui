# WPF.UI Ribbon 고도화 계획서

> 작성일: 2025-01-23
> 최종 갱신: 2025-10-23 (Simplified Ribbon 완료 + 추가 컨트롤 계획)
> 기반: Fluent.Ribbon vs WPF.UI Ribbon 비교 분석 + Telerik RibbonView 분석

---

## 📋 목차

1. [현재 작업: Simplified Ribbon 아키텍처 재설계](#현재-작업-simplified-ribbon-아키텍처-재설계)
2. [현재 상태 분석](#현재-상태-분석)
3. [완료된 작업](#완료된-작업)
4. [Simplified Ribbon 구현 계획](#simplified-ribbon-구현-계획)

---

## 🚀 현재 작업: Simplified Ribbon 아키텍처 재설계

### 날짜: 2025-10-23

### 배경

MS Outlook의 Simplified Ribbon과 Telerik RibbonView 분석 결과, 현재 WPF.UI의 Transform 방식(단일 컨트롤을 Trigger로 변형)을 **Collection Swap 방식**(Classic/Simplified 컬렉션 분리)으로 변경하기로 결정

### 핵심 설계 원칙

#### 1. **인터페이스 통합**
```csharp
// ❌ 기존: 인터페이스 분리
public interface IRibbonControl { ... }
public interface ISimplifiedRibbonControl : ISimplifiedStateControl { ... }

// ✅ 새 설계: IRibbonControl 확장
public interface IRibbonControl : IHeaderedControl, ILogicalChildSupport
{
    RibbonControlSize Size { get; set; }
    RibbonControlSizeDefinition SizeDefinition { get; set; }
    RibbonControlSizeDefinition SimplifiedSizeDefinition { get; set; }  // ← 추가
    object? Icon { get; set; }

    bool ShowInSimplified { get; set; }  // ← 새 속성: 컬렉션 선별용
    bool IsSimplified { get; }           // ← 이동: 현재 모드 상태
}
```

**변경 이유:**
- `ISimplifiedRibbonControl` 제거 → 단일 인터페이스로 단순화
- `ShowInSimplified` 추가 → 사용자가 Simplified 모드 표시 여부 제어
- Telerik 방식과 유사하지만 더 직관적

#### 2. **Collection Swap 방식**

```xml
<ui:RibbonTabItem Header="Home">
    <!-- Classic 모드용 컬렉션: 모든 그룹 -->
    <ui:RibbonTabItem.Items>
        <ui:RibbonGroupBox Header="Clipboard" ShowInSimplified="True">...</ui:RibbonGroupBox>
        <ui:RibbonGroupBox Header="Font" ShowInSimplified="True">...</ui:RibbonGroupBox>
        <ui:RibbonGroupBox Header="Advanced" ShowInSimplified="False">...</ui:RibbonGroupBox>
    </ui:RibbonTabItem.Items>
</ui:RibbonTabItem>
```

**내부 동작:**
```csharp
// RibbonTabItem.cs
public ObservableCollection<object> Items { get; }             // Classic용 (기존)
public ObservableCollection<object> SimplifiedItems { get; }   // Simplified용 (자동 생성)

private void OnItemsChanged()
{
    SimplifiedItems.Clear();
    foreach (var item in Items)
    {
        if (item is IRibbonControl control && control.ShowInSimplified)
        {
            SimplifiedItems.Add(item);
        }
    }
}
```

**Template:**
```xml
<ControlTemplate TargetType="RibbonTabControl">
    <Grid>
        <!-- Classic 모드 -->
        <ItemsPresenter x:Name="PART_DefaultItemsPresenter"
                        ItemsSource="{TemplateBinding Items}" />

        <!-- Simplified 모드 -->
        <ItemsControl x:Name="PART_SimplifiedItemsControl"
                      ItemsSource="{TemplateBinding SimplifiedItems}"
                      Visibility="Collapsed" />
    </Grid>

    <ControlTemplate.Triggers>
        <DataTrigger Binding="{Binding LayoutMode}" Value="Simplified">
            <Setter TargetName="PART_SimplifiedItemsControl" Property="Visibility" Value="Visible"/>
            <Setter TargetName="PART_DefaultItemsPresenter" Property="Visibility" Value="Collapsed"/>
        </DataTrigger>
    </ControlTemplate.Triggers>
</ControlTemplate>
```

#### 3. **컨트롤 레벨 Template Swap (Outlook 방식)**

Telerik은 아이콘만 표시하지만, **Outlook은 아이콘+캡션 가로 배치**를 사용:

```xml
<!-- Classic 템플릿: Vertical (아이콘 위, 캡션 아래) -->
<ControlTemplate x:Key="RibbonButton.Classic">
    <StackPanel Orientation="Vertical">
        <IconPresenter />
        <ContentControl />  <!-- Header -->
    </StackPanel>
</ControlTemplate>

<!-- Simplified 템플릿: Horizontal (아이콘 왼쪽, 캡션 오른쪽) -->
<ControlTemplate x:Key="RibbonButton.Simplified">
    <StackPanel Orientation="Horizontal">
        <IconPresenter />  <!-- Medium Size -->
        <ContentControl />  <!-- Header -->
    </StackPanel>
</ControlTemplate>

<!-- Style Trigger -->
<Style TargetType="RibbonButton">
    <Setter Property="Template" Value="{StaticResource RibbonButton.Classic}" />
    <Style.Triggers>
        <Trigger Property="IsSimplified" Value="True">
            <Setter Property="Template" Value="{StaticResource RibbonButton.Simplified}" />
            <Setter Property="IconSize" Value="Medium" />  <!-- 높이에 맞춤 -->
        </Trigger>
    </Style.Triggers>
</Style>
```

**이미 구현됨!** (Line 70-153 in RibbonButton.xaml)

### 아키텍처 비교

| 방식 | Telerik | Outlook | WPF.UI 새 설계 |
|------|---------|---------|---------------|
| **Tab 레벨** | SimplifiedItems 컬렉션 | (추정) 컬렉션 선별 | SimplifiedItems 자동 생성 |
| **Group 레벨** | Transform (속성 변경) | (추정) Transform | Transform (속성 변경) |
| **Button 레벨** | Transform (아이콘만) | **Template Swap (가로 배치)** | **Template Swap (가로 배치)** ✅ |
| **Caption** | ❌ 숨김 | ✅ **표시 (가로)** | ✅ **표시 (가로)** |
| **선별 방식** | 수동 Add | (추정) 자동 | **자동 (ShowInSimplified)** ✅ |

### 구현 상태

#### ✅ 완료 (2025-10-23 세션 2) - Phase 2A/2B 핵심 완료

##### Phase 2A: 인터페이스 재설계 ✅

- [x] IRibbonControl 인터페이스 확장 (SimplifiedSizeDefinition, ShowInSimplified, IsSimplified 추가)
- [x] RibbonControl 기본 클래스에 속성 구현 ([RibbonControl.cs:185-207](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\Core\RibbonControl.cs#L185-L207))
- [x] RibbonButton ShowInSimplified 속성 추가 (기본값: true)
- [x] RibbonToggleButton ShowInSimplified 추가 ([RibbonToggleButton.cs:143-154](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\Buttons\RibbonToggleButton.cs#L143-L154))
- [x] RibbonCheckBox ShowInSimplified 추가 ([RibbonCheckBox.cs:117-128](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\Buttons\RibbonCheckBox.cs#L117-L128))
- [x] RibbonDropDownButton ShowInSimplified 추가 ([RibbonDropDownButton.cs:251-262](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\Buttons\RibbonDropDownButton.cs#L251-L262))
- [x] RibbonSplitButton은 상속으로 자동 지원 확인
- [x] InRibbonGallery ShowInSimplified 추가 ([InRibbonGallery.cs:815-826](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\Gallery\InRibbonGallery.cs#L815-L826))
- [x] RibbonMenuItem 전체 속성 추가 ([RibbonMenuItem.cs:52-74](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\Menu\RibbonMenuItem.cs#L52-L74))

##### Phase 2B: Collection Swap 구현 ✅

- [x] RibbonTabItem.SimplifiedItems 컬렉션 추가 ([RibbonTabItem.cs:43,231](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\Tabs\RibbonTabItem.cs#L43))
- [x] OnGroupsCollectionChanged에서 SimplifiedItems 자동 필터링 ([RibbonTabItem.cs:234-333](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\Tabs\RibbonTabItem.cs#L234-L333))
- [x] ShouldShowInSimplified() 헬퍼 메서드 구현 ([RibbonTabItem.cs:338-352](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\Tabs\RibbonTabItem.cs#L338-L352))
- [x] SwapGroupsCollection() 컬렉션 스왑 로직 ([RibbonTabItem.cs:460-481](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\Tabs\RibbonTabItem.cs#L460-L481))
- [x] OnIsSimplifiedChanged에서 컬렉션 스왑 자동 호출 ([RibbonTabItem.cs:440-455](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\Tabs\RibbonTabItem.cs#L440-L455))
- [x] 빌드 검증 완료 (에러 0개)

##### 구현 핵심 메커니즘

```csharp
// 1. 자동 필터링: Groups → SimplifiedItems
OnGroupsCollectionChanged() {
    if (ShouldShowInSimplified(element))
        simplifiedItems.Add(element);
}

// 2. 컬렉션 스왑: IsSimplified 변경 시
OnIsSimplifiedChanged() {
    SwapGroupsCollection(isSimplified);
    // true: SimplifiedItems 사용
    // false: Groups 사용
}
```

#### ✅ 추가 완료 (2025-10-23 세션 3 + 4 + 5)

- [x] **RibbonGroupBox에 ShowInSimplified 속성 추가** ([RibbonGroupBox.cs:506-514](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\Groups\RibbonGroupBox.cs#L506-L514))
- [x] **ISimplifiedRibbonControl 인터페이스 완전 제거**
  - C# 클래스 4개 수정 (InRibbonGallery, RibbonToggleButton, RibbonCheckBox, RibbonDropDownButton)
  - XAML 파일 3개 수정 (AncestorType 변경)
  - RibbonProperties.cs 수정
  - 파일 삭제 완료
- [x] **ShouldShowInSimplified() 메서드 수정** - Reflection만 사용하도록 단순화
- [x] **RibbonTabControl 템플릿 수정** - IsSimplified=True일 때 ContentHeight를 68px로 변경
- [x] **OnGroupsCollectionChanged 로직 수정** - IsSimplified 상태에 따라 조건부로 Children에 추가
- [x] **TransitionStoryboard 참조 수정** - `ui.Ribbon` → `controls.Ribbon` 네이밍 컨벤션 통일
- [x] **불필요한 Command 제거** - SwitchToClassic/SwitchToSimplified 제거, ToggleSimplified만 유지
- [x] **SwapGroupsCollection 방식 변경** - Clear+Add → Visibility 기반 필터링
- [x] **높이 제어 구조 단순화** - RibbonTabControl만 ContentHeight 제어, RibbonGroupBox는 부모에 맞춤
- [x] **아이콘 크기 최적화** - Classic: 32px, Simplified: 24px (MediumIcon 추가)
- [x] **CommonPanel 지원 추가** - Ribbon.CommonPanel의 컨트롤도 IsSimplified 전파
- [x] **PART_RibbonPanel 이름 지정** - 메인 컨텐츠 영역에 명확한 이름 부여
- [x] **Spacing 최적화** - RibbonGroupBox Simplified 모드에서 여백 최소화

**핵심 수정사항:**

```xml
<!-- RibbonTabControl.xaml - ContentHeight 68px -->
<Trigger Property="IsSimplified" Value="True">
    <Setter TargetName="PART_SimplifiedToggle" Property="ToolTip" Value="Switch to Classic Ribbon" />
    <Setter TargetName="PART_SimplifiedToggleIcon" Property="Symbol" Value="ChevronDown24" />
    <Setter Property="ContentHeight" Value="68" />
</Trigger>
```

```csharp
// RibbonTabItem.cs - Visibility 기반 필터링
private void SwapGroupsCollection(bool useSimplified)
{
    foreach (UIElement item in Groups)
    {
        if (useSimplified)
        {
            item.Visibility = ShouldShowInSimplified(item) ? Visibility.Visible : Visibility.Collapsed;
        }
        else
        {
            item.Visibility = Visibility.Visible;
        }
    }
}
```

```xml
<!-- RibbonGroupBox.xaml - 높이 제어 제거, 부모에 맞춤 -->
<Trigger Property="IsSimplified" Value="True">
    <Setter TargetName="PART_ParentPanel" Property="Margin" Value="0" />
    <Setter TargetName="PART_UpPanel" Property="Margin" Value="2,0" />
    <Setter TargetName="PART_UpPanel" Property="Orientation" Value="Horizontal" />
    <Setter TargetName="PART_UpPanel" Property="VerticalAlignment" Value="Center" />
    <!-- Height 설정 제거 - 부모(RibbonTabControl)가 제어 -->
</Trigger>
```

#### ✅ Simplified Ribbon 완료!

**완성도:**
- Classic ↔ Simplified 모드 전환 완벽 동작
- ContentHeight 자동 조정 (Classic: 100px, Simplified: 68px)
- ShowInSimplified 속성 기반 필터링 동작
- 아이콘 크기 자동 조정 (Large: 32px, Medium: 24px)
- CommonPanel 동기화
- 높이 제어 구조 최적화

#### ⏭️ 다음 단계: 추가 입력 컨트롤 구현

**참고:** [RIBBON_CONTROLS_PLAN.md](C:\Temp\github\WPFUI\src\Wpf.Ui\Controls\Ribbon\RIBBON_CONTROLS_PLAN.md)

**우선순위 🔥 높음:**
- [ ] RibbonComboBox (폰트, 스타일 선택) - 4시간
- [ ] RibbonTextBox (검색, 입력) - 3시간

**우선순위 🔶 중간:**
- [ ] RibbonRadioButton (정렬, 옵션 선택) - 2시간
- [ ] RibbonSlider (줌, 투명도 조정) - 3시간

**우선순위 🔵 낮음:**
- [ ] RibbonColorPicker (색상 선택) - 6시간
- [ ] RibbonNumberBox (숫자 입력) - 3시간

##### 중요 발견 및 설계 결정

- **RibbonMenuItem**: 드롭다운 메뉴/Backstage에만 사용되므로 ShowInSimplified 필터링 불필요
- **필터링 레벨**: RibbonTabItem.Groups → SimplifiedItems (그룹 단위 필터링만)
- **RibbonGroupBox.Items**: 추가 필터링 불필요! MS Office도 그룹 단위로만 표시/숨김
- **RibbonButton 등**: Simplified 템플릿으로 자동 전환 (숨기지 않음, 레이아웃만 변경)

### 기술적 결정

#### ShowInSimplified 기본값: `true`
```csharp
public static readonly DependencyProperty ShowInSimplifiedProperty =
    DependencyProperty.Register(
        nameof(ShowInSimplified),
        typeof(bool),
        typeof(RibbonButton),
        new PropertyMetadata(BooleanBoxes.TrueBox)  // ← 기본 true
    );
```

**이유:**
- 대부분의 컨트롤은 Simplified에도 표시되어야 함
- 고급 기능만 `ShowInSimplified="False"`로 명시
- Opt-out 방식이 Opt-in보다 사용자 친화적

#### 자동 필터링 시점
```csharp
// Option 1: Items CollectionChanged 이벤트
Items.CollectionChanged += (s, e) => UpdateSimplifiedItems();

// Option 2: Ribbon.LayoutMode 변경 시
PropertyChanged(LayoutMode) => UpdateSimplifiedItems();

// 선택: Option 1 (즉시 반영)
```

### 예상 효과

#### 사용자 경험
```xml
<!-- Before: 복잡한 Transform 로직 -->
<ui:RibbonGroupBox Header="Advanced" IsSimplified="{Binding ...}">
    <ui:RibbonGroupBox.Style>
        <Style TargetType="ui:RibbonGroupBox">
            <Style.Triggers>
                <Trigger Property="IsSimplified" Value="True">
                    <Setter Property="Visibility" Value="Collapsed" />
                </Trigger>
            </Style.Triggers>
        </Style>
    </ui:RibbonGroupBox.Style>
</ui:RibbonGroupBox>

<!-- After: 명시적 선별 -->
<ui:RibbonGroupBox Header="Advanced" ShowInSimplified="False">
    <!-- Simplified 모드에서 자동으로 안 보임! -->
</ui:RibbonGroupBox>
```

#### 코드 품질
- **복잡도 감소**: Transform 로직 제거
- **명확성 향상**: "이 컨트롤을 Simplified에 보여줄까?" 명확히 표현
- **유지보수성**: Classic/Simplified 로직 분리

---

## 현재 상태 분석

### ✅ 이미 구현된 핵심 기능 (WPF.UI)

| 분류 | 컨트롤/기능 | 상태 |
|------|------------|------|
| **핵심 구조** | Ribbon, RibbonTabControl, RibbonTabItem | ✅ |
| **그룹 시스템** | RibbonGroupBox (+ Dialog Launcher 포함!) | ✅ |
| **버튼 컨트롤** | Button, CheckBox, ToggleButton, DropDown, Split | ✅ |
| **갤러리** | InRibbonGallery, Gallery, GalleryItem | ✅ |
| **레이아웃** | RibbonGroupsContainer, RibbonGroupBoxWrapPanel | ✅ |
| **Backstage** | Backstage, BackstageTabControl, BackstageTabItem | ✅ |
| **기타 UI** | ScreenTip, RibbonTitleBar, TransitioningControl | ✅ |
| **상태 관리** | RibbonStateStorage | ✅ |
| **Simplified** | RibbonButton Simplified Template (Horizontal) | ✅ |

### 📊 코드 규모 (2025-10-23 기준)

**전체 Ribbon 코드:**
- **총 파일:** 134개 C# 파일
- **총 코드:** 26,512 라인
- **평균:** 파일당 ~198 라인

---

## 완료된 작업

### ✅ Phase 1: 코드 정리 및 구조 개선 (2025-10-23 완료)

(기존 내용 유지... 생략)

---

## Simplified Ribbon 구현 계획

### Phase 2A: 인터페이스 재설계 ✅ 완료

#### 작업 목록

1. ✅ IRibbonControl 확장 (SimplifiedSizeDefinition, ShowInSimplified, IsSimplified 추가)
2. ✅ RibbonControl 기본 클래스에 속성 구현
3. ✅ RibbonButton 수정 (ShowInSimplified 추가)
4. ✅ RibbonToggleButton, RibbonCheckBox 수정
5. ✅ RibbonDropDownButton, RibbonSplitButton 수정
6. ✅ InRibbonGallery, RibbonMenuItem 수정

#### 소요 시간: 약 2시간 (예상 4시간보다 빠름)

---

### Phase 2B: Collection Swap 구현 ✅ 완료

#### 작업 목록

1. ✅ RibbonTabItem.SimplifiedItems 컬렉션 추가
2. ✅ OnGroupsCollectionChanged 이벤트 핸들러 수정
3. ✅ ShouldShowInSimplified() 자동 필터링 로직
4. ✅ SwapGroupsCollection() 컬렉션 스왑 메서드
5. ✅ OnIsSimplifiedChanged에서 스왑 호출

#### 소요 시간: 약 1.5시간 (예상 6시간보다 훨씬 빠름)

**참고:** RibbonTabControl 템플릿 수정은 현재 구조상 불필요 (RibbonTabItem이 직접 처리)

---

### Phase 2C: Simplified 모드 완성

#### 작업 목록
1. OnToggleSimplifiedCommandExecuted 수정
2. SimplifiedSizeDefinition 활용 로직
3. ContentHeight 자동 조정 (Classic: 68px, Simplified: 42px)
4. Group Header 표시/숨김 로직
5. CommonPanel (Copilot 스타일) 통합

#### 예상 시간: 8시간

---

### 총 예상 시간 (Simplified Ribbon 완성)
- Phase 2A: **4시간**
- Phase 2B: **6시간**
- Phase 2C: **8시간**
- **전체: 약 18시간 (2~3일)**

---

## 기술 참고

### Telerik RibbonView 분석 결과
- **Path:** `D:\refProject\Telerik_UI_for_WPF_2025_3_813_Source\Controls\RibbonView`
- **방식:** Transform + Content Swap 하이브리드
  - Tab: SimplifiedItems 컬렉션 (Swap)
  - Group: DataTrigger (Transform)
  - Button: DataTrigger (Transform)
- **특징:** 아이콘만 표시 (캡션 숨김)

### MS Outlook 분석 결과
- **Classic:** 아이콘(Vertical) + 캡션(아래)
- **Simplified:** 아이콘(Horizontal) + 캡션(오른쪽), 크기 작아짐
- **선별:** 고급 그룹은 Simplified에서 숨김

### WPF.UI 설계 철학
- **Telerik + Outlook 장점 결합**
- **자동 필터링** (ShowInSimplified)
- **Template Swap** (Outlook 스타일 가로 배치)
- **사용자 친화적** (명시적 선언)

---

**문서 작성자:** Claude
**최초 작성일:** 2025-01-23
**최종 갱신일:** 2025-10-23 (Simplified Ribbon Architecture)
**버전:** 3.0 (Simplified Ribbon 설계 추가)
