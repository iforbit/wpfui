# eLink Haiku 4.5 검색/수정/삭제 에이전트

## 개요

Claude Haiku 4.5 기반 빠른 코드 검색, 수정, 삭제 전담 서브에이전트 시스템입니다.

**Haiku 4.5 출시 (2025-10-15):**

- Sonnet 4 수준의 코딩 성능
- 1/3 가격 ($1/$5 per M tokens)
- 2배 이상 빠른 속도

## 서브에이전트 (Subagents)

### haiku-searcher (검색)

빠른 파일 및 코드 검색 전담

**기능:**

- 파일명 검색
- 클래스/인터페이스 검색
- 메서드 검색
- 전체 텍스트 검색

**사용 예시:**

```text
Task: Use haiku-searcher to find all PostgreSqlConnectionService usages
```

### haiku-modifier (수정)

코드 수정 및 리팩토링 전담

**기능:**

- Rename (이름 변경)
- Refactor (리팩토링)
- Format (포맷팅)
- Update (API 업데이트)

**사용 예시:**

```text
Task: Use haiku-modifier to rename oldMethodName to newMethodName in all files
```

### haiku-deleter (삭제)

안전한 코드/파일 삭제 전담

**기능:**

- Dead code 탐지 및 제거
- Unused files 식별
- Deprecated code 정리

**사용 예시:**

```text
Task: Use haiku-deleter to find and list all unused private methods in the project
```

### build-analyzer (빌드 분석)

빌드 로그 분석 및 에러 진단

**기능:**

- 컴파일 에러 식별
- 경고 분석 및 분류
- 빌드 시간 성능 지표

**사용 예시:**

```text
Task: Use build-analyzer to analyze the last build output and identify top 5 issues
```

## 사용 방법

### 1. 직접 Task 도구 사용 (권장)

메인 에이전트(Sonnet 4.5)가 Task 도구로 Haiku 서브에이전트를 호출:

```text
User: PostgreSqlConnectionService를 찾아줘

Sonnet: Task 도구로 haiku-searcher 호출
Haiku: [빠른 검색 수행]
Sonnet: [결과 받아서 사용자에게 전달]
```

### 2. 슬래시 커맨드 사용 (편의성)

`.claude/commands/` 디렉토리의 커맨드들:

- `/search <keyword>` - 범용 검색
- `/find-class <classname>` - 클래스 찾기
- `/find-usage <identifier>` - 사용처 찾기

**주의:** 슬래시 커맨드는 Sonnet이 처리하며, Haiku로 위임하지 않습니다.

## 비용 절감 효과

10,000 토큰 작업 기준:

- **Sonnet 4.5**: $0.03 (입력) + $0.15 (출력) = $0.18
- **Haiku 4.5**: $0.01 (입력) + $0.05 (출력) = $0.06
- **절약: 3배**

속도까지 고려하면 훨씬 효율적입니다!

## Anthropic의 권장 패턴

> "Sonnet 4.5 can break down a complex problem into multi-step plans, then orchestrate a team of multiple Haiku 4.5s to complete subtasks in parallel."

이 시스템은 Anthropic의 권장 패턴을 따릅니다:

- Sonnet: 복잡한 문제 분석, 계획 수립
- Haiku: 반복적인 서브태스크 병렬 처리

## eLink 프로젝트 특화 검색

### SCL 관련

```text
Task: Use haiku-searcher to find IedRepository implementations
```

### Modbus 관련

```text
Task: Use haiku-searcher to find ModbusDevice classes
```

### PostgreSQL 관련

```text
Task: Use haiku-searcher to find all Repository classes
```

### ViewModel/View

```text
Task: Use haiku-searcher to find all ViewModels using RelayCommand
```

## 디렉토리 구조

```text
.claude/
├── agents/                  # Haiku 4.5 서브에이전트 (모델 지정 가능)
│   ├── haiku-searcher.md    # model: haiku - 검색
│   ├── haiku-modifier.md    # model: haiku - 수정
│   ├── haiku-deleter.md     # model: haiku - 삭제
│   └── build-analyzer.md    # model: haiku - 빌드 분석
├── commands/                # 슬래시 커맨드 (Sonnet이 처리)
│   ├── search.md
│   ├── find-class.md
│   └── find-usage.md
├── prompts/                 # 참고용 상세 프롬프트
│   ├── haiku-searcher-prompt.md
│   ├── haiku-modifier-prompt.md
│   └── haiku-deleter-prompt.md
├── settings.local.json      # 권한 설정
└── README.md
```

## 성능 최적화 팁

1. **반복 작업은 Haiku에게**: 검색, 간단한 수정, 삭제 탐지
2. **복잡한 분석은 Sonnet에게**: 아키텍처 설계, 비즈니스 로직
3. **병렬 처리 활용**: 여러 Haiku를 동시에 실행 가능
4. **프롬프트 캐싱**: 90% 비용 절감 가능

## 주의사항

- 서브에이전트는 Task 도구로만 호출 가능
- 슬래시 커맨드는 항상 Sonnet이 처리
- Haiku 4.5는 코딩 성능이 Sonnet 4와 동일하지만, 매우 복잡한 추론은 Sonnet 4.5 권장

## 참고 링크

- [Claude Haiku 4.5 출시 공지](https://www.anthropic.com/news/claude-haiku-4-5)
- [Claude Code 서브에이전트 문서](https://docs.claude.com/en/docs/claude-code/sub-agents)
- [Claude Code 설정 가이드](https://docs.claude.com/en/docs/claude-code/settings)
