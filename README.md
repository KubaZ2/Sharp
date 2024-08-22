# 🤖 Sharp

A Discord bot designed to help you with .NET development.

## 🔗 Links

- [Invitation Link](https://discord.com/oauth2/authorize?client_id=803324257194082314&permissions=274877908992&scope=bot)
- [Support Discord](https://discord.gg/meaSHTGyUH)
- [Terms of Service](TOS.md)
- [Privacy Policy](PRIVACY.md)

## ✨ Features

- Running your code
- Decompiling your code to the specified language
- Showing JIT disassembly of your code for the specified architecture

## 📝 Commands

- `#run <architecture?> <code>` — runs the provided code, uses ARM64 architecture by default
- `#<language> <code>` — decompiles the provided code to the specified language
- `#<architecture> <code>` — shows the architecture-specific JIT disassembly of the provided code

The code can be provided as is, as a code block or as an attachment.

## 🛎️ Support

### Compilation

- C#
- VB
- F#
- IL

### Decompilation
- C#
- IL

### Architectures
- x64
- ARM64

## 📘 Examples

### Running C# code:
#run  
\```c#  
Console.Write("Hello, World!");  
\```

### Decompiling F# code to C#:
#c#  
\```f#  
printf "Hello, World!"  
\```

### Decompiling C# code to IL:
#il  
\```c#  
Console.Write("Hello, World!");  
\```

### Showing JIT disassembly of C# code for ARM64:
#arm64  
\```c#  
Console.Write("Hello, World!");  
\```
