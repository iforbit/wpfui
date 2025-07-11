# 📘 WPF.UI.DirectX - 고성능 그래프 시스템 개발 문서

## 🎯 프로젝트 개요

WPF.UI.DirectX는 WPF 환경에서 수백~수천 개의 시리즈와 수백만~수억 개의 데이터 포인트를 실시간으로 렌더링할 수 있도록 설계된 GPU 가속 그래프 프레임워크입니다.  
Direct3D11 기반으로 렌더링하며, MVVM 구조와 완전한 WPF 통합을 목표로 합니다.

---

## 🛠️ 기능 로드맵

| 기능 범주     | 기능                                             | 상태        |
| --------- | ---------------------------------------------- | --------- |
| 성능 최적화    | GPU 기반 Transform (ViewMatrix/ProjectionMatrix) | ✅ 완료      |
|           | Double Buffering / Vertex Buffer 재사용 구조        | ✅ 완료      |
|           | RingBuffer/ChunkedBuffer 구조                    | ✅ 완료      |
|           | 렌더 스레드 구조 개선 (큐 기반, 병렬 처리)                     | 🔄 진행 중     |
|           | DrawCall 병합: 시리즈 단위 렌더 수집/최적화                  | 🔄 진행 중 |
| 시리즈 구조    | `GraphSeries<T>` 및 `SeriesRenderer<T>` 분리      | ✅ 완료   |
| 셰이더 구성    | `ShaderManager` + `ShaderPipeline` 설계          | 🔄 진행 중   |
| 축 시스템     | `AxisManager` 중심의 Transform 적용 및 다중 축 지원                | 🕓 예정     |
| 어노테이션     | `AnnotationLayer` 기반 Line/Text 주석 시스템        | 🕓 예정     |
| 범례        | WPF Overlay 기반 Legend UI                       | 🕓 예정     |
| 상호작용      | ChartModifier 구조 (Cursor, Zoom, Pan 등)         | 🕓 예정     |
| 다중 컨트롤 지원 | `RenderThreadService` 기반 다중 `GraphControl` 렌더링 지원          | ✅ 완료      |
| 스타일링      | Series별 색상, 두께, 마커 스타일                         | 🕓 예정     |
| MVVM 대응   | SeriesBinding, AxisBinding, CommandTrigger 등   | ✅ 구조 확립 중 |

---

## 🧩 아키텍처 설계 (모듈 구조)

```plaintext
[GraphControl.xaml/.cs]                   ← WPF 컨트롤 진입점
├─ AxisManager                            ← 축 범위 및 Transform 계산 (공통)
├─ SeriesManager                          ← 시리즈 목록 및 상태 관리
│  └─ List<GraphSeries<T>>                ← 순수 데이터와 상태를 갖는 시리즈
├─ SeriesRendererManager                  ← 시리즈별 렌더러 구성/조회
│  └─ Dictionary<GraphSeries, Renderer>  ← SeriesRenderer<T>
├─ D3D11Renderer                          ← 셰이더, 버퍼, SwapChain 관리
│  └─ ShaderManager                       ← ShaderPipeline /셰이더/버텍스레이아웃 캐싱 및 재사용
│      └─ ShaderPipeline                  ← VS/PS/InputLayout 구성체
├─ DrawGroupBatcher                       ← 시리즈 수집 및 동일 셰이더 그룹 병합 및 DrawCall 최적화
├─ AnnotationLayer                        ← UIElement 기반 WPF Overlay 주석 시스템
├─ LegendPanel                            ← Series 정보 기반 WPF 오버레이 범례 시스템
└─ RenderThreadService                    ← 각 렌더러를 독립 스레드에서 실행/관리
```
---
### 📌 GraphSeries 구조 요약 (RealTimeSeries 기반 실시간 시리즈)

- `RealTimeSeries<T>`는 내부에 `RingBuffer<T>`를 사용하여 실시간 데이터 누적
- `Append(ReadOnlySpan<T>)`을 통해 데이터를 누적하며 `LastX`, `MinX`, `MaxX` 값을 추적
- `GetSpans(out span1, out span2)` 구조로 단일/분할 버퍼 접근 제공 (Wrap-around 대응)
- `Upload()` 단계에서는 RingBuffer가 wrap-around 된 경우에도 시계열 데이터가 시각적으로 끊기지 않도록 span1과 span2를 병합하여 연속된 메모리로 만든 후 GPU로 전송합니다.
이 과정은 LineStrip 등 연결성이 중요한 시리즈에서 단절 없는 그래프 표현을 보장하기 위한 핵심 처리입니다.
- `SeriesRenderer<T>`가 이를 기반으로 VertexBuffer를 생성 및 재사용
- 실시간 그래프는 `Lines` / `LineStrip` 등으로 구성 가능
- Transform은 ViewProjectionBuffer를 통해 적용되며, XOffset/XScale을 통해 이동/확대 가능
- `GraphControl.FollowLatestX()` 호출 시 마지막 X값 기준으로 자동 Transform 이동

```
var series = new RealTimeSeries<VertexPosition>(capacity: 100_000);
graphControl.AddSeries(series);
series.Append(span); // 실시간 데이터 누적
```
---
### 🔁 DrawCall 병합 구조 (DrawGroupBatcher)
DrawGroupBatcher 동작 계획
 - 시리즈 단위로 SeriesRenderer<T>에서 Upload 완료 후 DrawCallRequest 발행
 - 동일 셰이더, 동일 스타일을 가진 시리즈를 하나의 그룹으로 묶음
 - DrawGroupBatcher.Execute()가 한 번의 DrawCall로 처리

 효과:  DrawCall 횟수 최소화
 
 렌더 중 0개의 버텍스, 이미 disposed된 버퍼 호출 방지

안정성, 성능, 예외 관리 모두 개선
---
## ✅ MVVM 대응 사항

* `GraphSeries<T>`는 ViewModel에서 구성하고 `GraphControl.AddSeries()`로 전달
* `Legend`와 `Annotation`은 WPF UI에서 MVVM 기반으로 바인딩 지원
* `GraphControl`은 MVVM ViewModel에 Transform 정보 (XStart, XEnd 등)를 전달하거나 받아서 동기화 가능
* 렌더 요청은 MVVM → GraphControl `RequestRender()` 또는 자동 타이머로 연동 가능

- 추후 `ChartModifier`, `ZoomCommand`, `SelectionCommand`, `VisibleRange` 바인딩 등도 MVVM Command/Property 구조로 통합 예정
```
- Series 추가
var series = new RealTimeSeries<VertexPosition>(...) { Name = "MySeries" };
graphControl.AddSeries(series);

- 실시간 데이터 추가
series.Append(generatedSpan); // Span<VertexPosition>

- 렌더 요청 (ViewModel → View)
graphControl.RequestRender(); // 또는 DispatcherTimer 내부 호출

- Transform 바인딩 (MVVM)
graphControl.UpdateTransform(xOffset, xScale, yScale, yOffset);
- UI 확장:	Legend / Annotation은 MVVM 기반 WPF로 처리 예정
```
---

## 🧵 고성능 렌더링 스레드 구조 (예정)

| 항목                 | 내용                                                                                       |
| ------------------ | ---------------------------------------------------------------------------------------- |
| 다중 GraphControl 지원 | 각 Control은 독립적으로 렌더링 요청                                                                  |
| 시리즈 병렬 처리          | 시리즈별 Upload → 병렬 가능 (단, Context는 주의)                                                     |
| 스레드 안전성            | Lock-free 큐, Dispatcher 최소 사용                                                            |
| Frame Drop 대응      | Upload 시간 초과 시 Skip 허용                                                                   |
|예외 및 재초기화 대응 |	SEHException, 0x887A0005 등 발생 시 TryRecover() 또는 무시 처리|
| DrawCall 병합        | DrawGroupBatcher를 통해 시리즈를 그룹으로 수집하여 한 번에 렌더링 수행 → Draw 호출 최소화, 업데이트 오류 및 0버텍스 호출 방지에 효과적 |
| 성능 측정 도구           | FPS, Upload 시간, Frame Cost 측정기 내장 예정                                                     |

예정 기능:

* `GraphControl`당 `ConcurrentQueue<RenderTask>` 도입
* `SeriesRenderer` Upload에 `MapTimeout` 또는 `TryMap` 구조
* `[Render] CH1 took 1.4ms` 등의 로그 기록
* `RenderProfilerOverlay`를 통한 실시간 프로파일링 디버그 기능

---

## 📦 파일 구조 예시

```plaintext
Wpf.Ui.DirectX/
├─ Core/
│  └─ IGraphSeries.cs, IRenderable.cs, IRenderStyleKeyProvider.cs
├─ Controls/
│  └─ GraphControl.cs, LegendPanel.xaml
├─ Helpers/
│  └─ VertexBufferFactory.cs
├─ Models/
│  └─ GraphSeries.cs,RealTimeSeries.cs, AxisManager.cs, VertexTypes/, Buffers/
├─ Rendering/
│  └─ D3D11Renderer.cs, SeriesRendererBase.cs, RealTimeSeriesRenderer.cs, SeriesRendererManager.cs, ShaderManager.cs, DrawGroupBatcher.cs Interop/ 
│  └─ Transform/
│     └─ TransformManager.cs, ViewProjectionBuffer.cs
├─ Services/
│  └─ GraphicsService.cs, RenderThreadService.cs
├─ Extensions/
│  └─ ChartModifier, AnnotationLayer
├─ Shaders/
│  └─ LinePixelShader.hlsl, LineVertexShader.hlsl, ConstPixelShader.hlsl,ConstVertexShader.hlsl
└─ README.md
```

---

## 🧱 개발 우선순위 제안

1. `AxisManager` 중심 Transform 적용 구조 정착
2. `ShaderManager`, `ShaderPipeline` 설계 및 등록 방식 완성
3. Annotation/Legend를 MVVM + WPF 오버레이 방식으로 구현
4. ChartModifier 구조 도입 (Zoom, Cursor 등)
5. RenderThread 병렬 구조화 및 Queue 처리 최적화


## ✍️ 문서 목적

이 문서는 개발자가 전체 구조와 목표를 이해하고 모듈 간 연계를 정확히 파악할 수 있도록 작성되었으며, 지속적으로 업데이트됩니다.
