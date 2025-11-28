Use the Glob and Grep tools to find class definitions matching: {ARG}

1. Search for class files: **/*{ARG}*.cs
2. Search for class definitions: pattern "class\s+.*{ARG}" in *.cs files  
3. Include interface matches: pattern "interface\s+I?{ARG}" in *.cs files

Return format:
- File path with line number
- Class/interface signature
- Namespace

Keep output focused and concise.