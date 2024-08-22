# 🤖 Sharp

**Sharp** is a powerful Discord bot designed to assist .NET developers by running code snippets, decompiling code to various languages, and providing JIT disassembly for specified architectures.

[**Invite it now!**](https://discord.com/oauth2/authorize?client_id=803324257194082314&permissions=274877908992&scope=bot)

## 🛠️ Getting Started

1. **Invite the Bot**: Use the link above to invite Sharp to your Discord server.
2. **Run a Command**: Try running `#run` with a simple C# code snippet to see it in action.
3. **Join the Community**: [Join our support Discord](https://discord.gg/meaSHTGyUH) to ask questions, report issues, or suggest new features.

## ✨ Features

- **Run Code**: Execute your code directly within Discord.
- **Decompile Code**: Convert your code to another supported language.
- **JIT Disassembly**: View JIT disassembly of your code for the specified architecture.

## 🔗 Links

- [Invitation Link](https://discord.com/oauth2/authorize?client_id=803324257194082314&permissions=274877908992&scope=bot)
- [Support Discord](https://discord.gg/meaSHTGyUH)
- [Terms of Service](TOS.md)
- [Privacy Policy](PRIVACY.md)

## 📝 Commands

- `#run <architecture?> <code>` — Runs the provided code, using ARM64 architecture by default.
  - **Example**:
    ````
    #run
    ```c#
    Console.Write("Hello, World!");
    ```
    ````
  - **Output**:
    ```
    Hello, World!
    ```

- `#<language> <code>` — Decompiles the provided code to the specified language.
  - **Example**:
    ````
    #c#
    ```f#
    printf "Hello, World!"
    ```
    ````

- `#<architecture> <code>` — Shows the architecture-specific JIT disassembly of the provided code.
  - **Example**:
    ````
    #arm64
    ```c#
    Console.Write("Hello, World!");
    ```
    ````

The code can be provided as is, as a code block or as an attachment.

## 🛎️ Support

- **Compilation**: **C#**, **VB**, **F#**, **IL**
- **Decompilation**: **C#**, **IL**
- **Architectures**: **x64**, **ARM64**
