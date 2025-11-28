Find all usages of: {ARG}

Search strategy:
1. Class/Interface usage: using statements, inheritance, type parameters
2. Method usage: method calls, delegates
3. Property/Field usage: reads and writes

Use Grep with appropriate patterns:
- "{ARG}" for exact matches
- "using.*{ARG}" for namespace imports  
- ": {ARG}" for inheritance
- "<{ARG}>" for generics

Return: file paths, line numbers, and usage context.