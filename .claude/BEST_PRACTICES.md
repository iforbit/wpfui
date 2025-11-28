# Claude Code 베스트 프랙티스 (eLink 프로젝트)

## 웹에서 찾은 핵심 인사이트

### 🎯 훅(Hooks)보다 효과적인 방법들

**1. CLAUDE.md 파일 (자동 컨텍스트)**
- ✅ **효과**: 프로젝트 컨텍스트 자동 로드, 훅보다 훨씬 안정적
- ✅ **위치**: 프로젝트 루트 (`CLAUDE.md`)
- ✅ **장점**: 매 대화 시작 시 자동 로드, 시스템 규칙처럼 작동

**2. 서브에이전트 (Sub-Agents)**
- ✅ **효과**: 작업별 전문화, 컨텍스트 절약, 병렬 처리
- ✅ **위치**: `.claude/agents/`
- ✅ **장점**: 독립적 컨텍스트, 모델 선택 가능 (Haiku 4.5)

**3. MCP 서버 (Model Context Protocol)**
- ✅ **효과**: 외부 툴/데이터 연동 (GitHub, Sentry, PostgreSQL 등)
- ✅ **설정**: `claude mcp add {server-name}`
- ✅ **장점**: OAuth 지원, 표준화된 통합

## 서브에이전트 베스트 프랙티스

### 1. 전문화 (Specialization) 🎯

**❌ 나쁜 예 (Generalist)**
```markdown
---
name: general-helper
description: Do everything
model: haiku
tools: Read, Write, Edit, Grep, Glob, Bash
---
```

**✅ 좋은 예 (Specialist)**
```markdown
---
name: haiku-searcher
description: Fast file and code search ONLY
model: haiku
tools: Glob, Grep, Read, Bash
---
```

### 2. 명확한 역할 분담

**Planner-Worker-Evaluator 패턴**
```text
1. Sonnet (Planner): 복잡한 문제를 단계별 계획으로 분해
2. Haiku (Worker): 각 단계를 병렬 실행
3. Sonnet (Evaluator): 결과 검증 및 통합
```

**예시**:
```text
User: "전체 Repository 패턴을 수정해줘"

Sonnet:
1. haiku-searcher로 모든 Repository 찾기
2. 각 Repository에 대해 haiku-modifier 병렬 실행
3. build-analyzer로 빌드 검증
4. 결과 통합 및 보고
```

### 3. 최소 권한 원칙 (Least Privilege)

```markdown
# 검색 전용 → Read/Grep만
tools: Glob, Grep, Read

# 수정 전용 → Edit 추가
tools: Read, Edit, Grep

# 빌드 분석 → Bash 추가 (빌드 명령)
tools: Read, Grep, Bash
```

### 4. 프롬프트 품질

**❌ 나쁜 예**:
```markdown
Find bugs.
```

**✅ 좋은 예**:
```markdown
# eStudio 키워드 검색 전담 에이전트

## 검색 전략
- PropertyControl 패턴 인식
- XAML-C# 바인딩 추적

## 출력 형식
간결하게:
1. {FILE}:{LINE}
   {CODE_SNIPPET}

검색 결과만 빠르게 보고하세요.
```

## CLAUDE.md 베스트 프랙티스

### 포함할 내용

1. **기술 스택** - 버전 명시
2. **개발 명령어** - bash 명령어
3. **코딩 규칙** - 네이밍, 패턴
4. **서브에이전트 사용법** - 언제 어떻게
5. **프로젝트 특화 패턴** - 자주 쓰는 코드

### 포함하지 말 것

- ❌ 너무 장황한 설명
- ❌ 자주 변하는 정보
- ❌ 작업과 무관한 컨텍스트

### 계층 구조

```text
/CLAUDE.md              - 전체 프로젝트 컨텍스트
/tests/CLAUDE.md        - 테스트 특화 컨텍스트 (상속)
/src/eLink.UI/CLAUDE.md - UI 특화 컨텍스트 (상속)
```

## 워크플로우 패턴

### 1. 검색 → 수정 → 검증

```text
Task: Use haiku-searcher to find all ViewModels without INotifyPropertyChanged,
      then use haiku-modifier to add it,
      then use build-analyzer to verify build success
```

### 2. 병렬 처리

```text
Task: Run haiku-searcher for Repositories,
      and haiku-searcher for ViewModels,
      and haiku-searcher for Services in parallel
```

### 3. Extended Thinking (복잡한 문제)

```text
"Think hard about the best architecture for this feature"
- think < think hard < think harder < ultrathink
```

## MCP 서버 추천 (eLink 프로젝트)

### 현재 워크플로우 분석

**현재 환경:**
- Git: 자동 커밋 (dev 브랜치), 수동 커밋 (main)
- 빌드: Claude가 Bash로 직접 실행
- 로그: 로컬 파일 (`Logs/log-*.txt`)
- 파일 작업: Read/Write/Edit/Glob/Grep 도구 충분

**결론: MCP 전혀 필요 없음** ⭐⭐⭐

### MCP가 불필요한 이유

1. **Filesystem MCP** ❌
   - Claude Code의 Read/Write/Edit/Glob/Grep으로 충분
   - `Logs/log-*.txt`: Serilog만 기록 (Debug.WriteLine은 파일에 안 남음)
   - 추가 복잡도만 증가

2. **GitHub MCP** ❌
   - Git 작업 이미 자동화 (dev 자동 커밋)
   - Bash로 git 명령 직접 실행 가능

3. **PostgreSQL MCP** ❌
   - Dapper로 SQL 직접 작성 중
   - 자연어 쿼리 불필요

### 프로덕션 배포 후 재고려할 MCP

**외부 서비스 연동이 필요할 때만:**
- **Sentry MCP**: 실사용자 에러 추적 (외부 서비스)
- **Slack MCP**: 팀 알림 자동화 (외부 서비스)
- **Notion MCP**: 문서 동기화 (외부 서비스)

**현재는:** CLAUDE.md + 서브에이전트만으로 완벽!

## 실전 팁

### 1. 서브에이전트 리마인더

CLAUDE.md에 추가:
```markdown
## 서브에이전트 사용 지침

**중요**: 반복 작업은 Haiku에게, 복잡한 설계는 Sonnet에게!
```

### 2. 검증 패턴

```text
Task: Use haiku-modifier to make changes,
      then use independent build-analyzer to verify
      (독립적 검증으로 오버피팅 방지)
```

### 3. 컨텍스트 관리

```text
# 초반부터 서브에이전트 사용
User: "복잡한 문제..."
Sonnet: 즉시 haiku-searcher로 조사 시작 (컨텍스트 절약)
```

### 4. 병렬 처리 최대화

```text
# 최대 10개 병렬 실행
Task: Run 10 haiku-modifier agents in parallel to refactor all files
```

## 측정 가능한 효과

### 비용 절감
- **Haiku 4.5**: $1/$5 per M tokens
- **Sonnet 4.5**: $3/$15 per M tokens
- **절약**: 검색/수정 작업 시 3배 절감

### 속도 향상
- **Haiku 4.5**: Sonnet 대비 2배 빠름
- **병렬 처리**: 10개 작업 동시 실행

### 품질 향상
- **전문화**: 각 에이전트가 한 가지만 잘함
- **검증**: 독립적 에이전트로 교차 검증

## 참고 링크

- [Claude Code Best Practices (Anthropic)](https://www.anthropic.com/engineering/claude-code-best-practices)
- [7 Powerful Claude Code Subagents (eesel AI)](https://www.eesel.ai/blog/claude-code-subagents)
- [Best MCP Servers for Claude Code](https://mcpcat.io/guides/best-mcp-servers-for-claude-code/)
- [Agentic Coding with Haiku 4.5 (Skywork)](https://skywork.ai/blog/agentic-coding-claude-haiku-4-5-beginners-guide-sub-agent-orchestration/)

---

**결론**: 훅보다 CLAUDE.md + 서브에이전트 + MCP 조합이 훨씬 효과적!
