# 고성능 그래프 렌더링 엔진 로드맵 (WPF.UI.DirectX 기반)

본 문서는 고성능 실시간 그래프 렌더링 구조를 구현하는 프로젝트의 진행 상황과 아키텍처를 설명한다.  
이 문서는 사용자에게 보여주기 위한 것이 아니라, **AI가 이 프로젝트의 구조와 현재 상태를 항상 인식하고 대응할 수 있도록** 작성되었다.

---

## ✅ 프로젝트 개요

- 플랫폼: WPF (.NET 8) + Direct3D11 (Vortice.Windows)
- 목표: 수백~수천 개의 선형 그래프를 실시간으로 동시에 렌더링
- 핵심 기술: ChunkedVertexBuffer, GraphLineItem, RenderThread, TransformMatrix

---

## 🚧 현재 구현 상태 (2025-06-21 기준)

| 구성 요소 | 상태 | 비고 |
|-----------|------|------|
| `ChunkedVertexBuffer` | ✅ 구현 완료 | 메모리 누적 + 범위 복사 (Span 기반) |
| `GraphLineItem` | ✅ 초기 구조 구현됨 | 내부 버퍼 소유 구조로 전환 테스트 완료 |
| `DashboardViewModel` | ✅ `_buffers` 방식 안정화됨 | `AppendPoint → Enqueue → UpdateVertices` 방식 |
| `GraphControl` + `RenderThreadService` | ✅ 완성 | 렌더링 스레드 분리, Transform 전파 처리 |
| `Update()` → `UpdateVertices()` | ⚠ 예외 발생 중 | `Map()` 충돌 (렌더 중 동시 접근) |
| `SEHException 대응` | ⛔ 근본 해결 안 됨 | Double Buffering 필요 |
| `GPU View/Projection 행렬 처리` | ❌ 미적용 | CPU에서 Transform 처리 중 |
| `FastGraphItem` 구조 | ❌ 미도입 | 도입 예정 |
| `버퍼 Resize 제거` | ❌ 미적용 | 현재 동적 Resize 발생 중 |

---

## 🧭 구현 로드맵

### 1️⃣ GPU 변환 행렬 적용 (ViewMatrix + ProjectionMatrix)

- 목표: Transform (`XOffset`, `XScale`, `YScale`) 계산을 GPU에서 처리
- 방식: ConstantBuffer에 행렬 전달 → VertexShader에서 적용
- 결과: `UpdateVertices()` 호출 없이 화면 이동 가능

### 2️⃣ FastGraphItem 구조 도입

- `AppendPoint()` → 내부 메모리에 누적만
- `Update()`에서 필요한 범위만 GPU로 업로드
- ViewModel은 더 이상 Enqueue/Copy 등 렌더링 관여 안 함

### 3️⃣ Double Buffering (버텍스 버퍼 교차 사용)

- `Map()` 예외 제거 핵심
- `_vertexBufferA`와 `_vertexBufferB`를 프레임마다 번갈아 사용
- GPU와 CPU가 동시에 하나의 버퍼를 참조하지 않도록 보장

### 4️⃣ 버퍼 Resize 제거 및 RingBuffer 적용

- 초기 고정 크기 버퍼를 생성
- 일정 이상 누적되면 오래된 포인트부터 제거
- Chunk 기반 순환 처리로 재할당 방지

---

## 🧩 시스템 구성 요약

```plaintext
[DashboardViewModel]              [RenderThreadService]
       │                                    │
 AppendPoint(x, y)                         ├─▶ GraphLineItem.Update()
       │                                    │     ├─ CopyVerticesInRange()
    (FastGraphItem)                         │     └─ UploadVertices() → Map()
       ▼                                    ▼
[GraphLineItem] → ChunkedVertexBuffer   GraphItem.Render() → GPU DrawCall
                          ▲
                          └─ AppendPoint() 누적만, 직접 Upload 없음
