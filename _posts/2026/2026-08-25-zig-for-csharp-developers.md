---
layout: post
title: "Zig for C# Developers"
date: 2025-08-25 08:00:00 +0200
tags:
 - zig
---

As someone who has spent the better part of 20 years in the managed world of C# and .NET—and nearly 30 years in programming overall—I've seen languages come and go. We've moved from the manual memory management of C++ to the safety of the CLR, and now we're seeing a resurgence of systems programming languages that promise both safety and control without the "black box" overhead of a Garbage Collector.

Zig (specifically version 0.15.1, released mid-August 2025) offers a different path. It isn't just a "C replacement"; it's a rethink of how we build robust software. For a C# developer, Zig feels like looking under the hood of the car you've been driving for two decades. It's raw, it's powerful, and it lacks the hidden magic we often take for granted in .NET.

## The Zen of Zig: A Different Mental Model

In .NET, we are taught to embrace abstractions. In Zig, the philosophy is captured in the **Zen of Zig** (accessible via `zig zen`). Key pillars that will resonate with C# veterans include:

* **No hidden control flow.** (No exceptions, no properties with hidden getters/setters).
* **No hidden memory allocations.** (If a function allocates, it must take an `Allocator` as an argument).
* **Only one obvious way to do things.** (Contrast this with C#, where you can perform the same task via LINQ, `foreach`, or a `Span<T>`).
* **Communicate intent precisely.** (No implicit casting or hidden behavior).

For a C# dev, Zig feels like working with `unsafe` code and `Span<T>` constantly, but with a compiler that actually helps you stay safe.

## Primitive Types

Zig's type system is explicit about bit-width, which C# developers will appreciate, though the naming is more granular.

| C# | Zig | Notes |
| :--- | :--- | :--- |
| `byte` | `u8` | Unsigned 8-bit |
| `int` | `i32` | Signed 32-bit |
| `long` | `i64` | Signed 64-bit |
| `float` | `f32` | 32-bit float |
| `double` | `f64` | 64-bit float |
| `bool` | `bool` | `true` or `false` |
| `string` | `[]const u8` | A slice of constant bytes |

Zig also allows for arbitrary bit-width integers like `u24` or `i128`, which can be incredibly useful for protocol implementation.

## Memory: Classes vs. Structs

In C#, the distinction between `class` and `struct` is fundamental: **Reference Types vs. Value Types.**

### C# Mental Model

We use **classes** for almost everything (identity, long-lived objects) and **structs** for small, immutable data (performance, stack allocation).

```csharp
// Reference type: Allocated on the Heap, tracked by GC
public class Player {
    public string Name { get; set; }
    public int Level { get; set; }
    
    public void PrintInfo() => Console.WriteLine($"{Name}: {Level}");
}

// Value type: Allocated on the Stack (usually), copied by value
public struct Vector3 {
    public float X, Y, Z;
}
```

### The Zig Way: Everything is Data

Zig has no "classes." It only has `structs`. Whether something is on the heap or stack depends entirely on **how you declare it**, not **what it is**.

```zig
const std = @import("std");

pub const Player = struct {
    name: []const u8,
    level: i32,

    pub fn printInfo(self: Player) void {
        std.debug.print("{s}: {d}\n", .{self.name, self.level});
    }
};

// Stack allocation (like a C# struct)
var p1 = Player{ .name = "Christian", .level = 99 };

// Heap allocation (like a C# class)
// We must explicitly pass an allocator
const p2 = try allocator.create(Player);
defer allocator.destroy(p2); // Manual cleanup!
p2.* = .{ .name = "Helle", .level = 100 };
```

In Zig, the "Class vs. Struct" debate is replaced by **"Pointer vs. Value."** This gives you the performance of structs with the flexibility of classes, without the GC overhead.

## Code Structure: Namespaces vs. Files

### C# Project Structure

In .NET, we use `namespace` declarations and often have one class per file. The folder structure usually matches the namespace, but the compiler doesn't strictly enforce this.

```csharp
// File: /Models/Player.cs
namespace MyGame.Models {
    public class Player { ... }
}
```

### Zig Project Structure: The File is the Namespace

In Zig, there is no `namespace` keyword. **Each file is a struct.** You import a file, and its public members are accessed via the variable name you give the import.

**Standard Directory Layout:**

* `src/main.zig`: The entry point.
* `src/models.zig`: A module containing structs.
* `build.zig`: The build script.

```zig
// In main.zig
const models = @import("models.zig"); // 'models' acts as the namespace

pub fn main() !void {
    const p = models.Player{ .name = "Chris", .level = 1 };
}
```

This leads to a much flatter and more transparent project structure than the deeply nested namespaces common in enterprise C# projects.

## Conditionals and Looping

Zig’s `if` and `switch` are expressions, much like modern C# switch expressions, but more powerful.

### Pattern Matching and Switches

C# has made great strides with pattern matching, but Zig's `switch` is fundamentally exhaustive and acts as an expression.

```zig
const Category = enum { beginner, intermediate, expert };

const cat = switch (level) {
    0...10 => Category.beginner,
    11...20 => Category.intermediate,
    else => Category.expert,
};
```

### Looping

Zig replaces `for` and `foreach` with a single `for` that operates on "multi-sequences" (arrays/slices), and uses `while` for everything else.

```zig
// Zig: Iterating with an index (0..)
for (items, 0..) |item, i| {
    std.debug.print("{d}: {d}\n", .{i, item});
}
```

## Error Handling: No More Exceptions

This is where the paths diverge significantly. C# uses `try-catch` blocks which can be expensive and hide control flow. Zig uses **Error Union Types**.

### C #

```csharp
try {
    var data = File.ReadAllText("config.json");
} catch (IOException ex) {
    // Handle error
}
```

### Zig (0.15.1)

```zig
const data = std.fs.cwd().readFileAlloc(allocator, "config.json", 1024) catch |err| {
    // Zig's 'catch' is an operator, not a block.
    std.log.err("Failed to load: {}", .{err});
    return err; 
};
```

In Zig, a function that can fail returns `Error!Type`. You use `try` to bubble up the error or `catch` to handle it. It's essentially a compiler-enforced version of the `Result<T>` pattern.

## Comptime: Generics and Reflection on Steroids

While C# uses Reflection (Runtime) or Source Generators (Compile-time), Zig uses `comptime`. This allows you to write code that runs *during* compilation using the same syntax as your runtime code.

Instead of `List<T>`, you write a function that takes a `type` and returns a `type`:

```zig
fn List(comptime T: type) type {
    return struct {
        items: []T,
        count: usize,
        
        pub fn add(self: *@This(), item: T) void { ... }
    };
}

// Usage:
var my_list = List(i32){ ... };
```

The compiler generates a specific version of this struct for `i32` during compilation. There is no "vtable" lookup, no boxing, and no runtime overhead.

## Unit Testing: The Proximity Pattern

In C#, we typically create a separate project (`MyProject.Tests`) and use a runner like xUnit. In Zig, tests are **first-class citizens** and are usually placed at the bottom of the source file they test.

```zig
// src/math_util.zig
fn add(a: i32, b: i32) i32 {
    return a + b;
}

test "internal addition test" {
    try std.testing.expect(add(10, 5) == 15);
}
```

**Why this pattern?**

1. **Documentation**: Tests serve as live documentation of how to use the code above.
2. **Access**: Tests can access private (non-`pub`) members without `InternalsVisibleTo`.
3. **Refactoring**: If you change a private helper, you don't have to jump between projects to update tests.

## Build Systems: MSBuild vs. Zig Build

The most significant change for a .NET dev is moving from `csproj` (XML) to `build.zig` (Zig code).

* **MSBuild**: Declarative (mostly) XML. You describe *what* you want.
* **Zig Build**: Imperative Zig code. You write a script that describes *how* to build it. It’s essentially a small Zig program that orchestrates the compiler.

This makes cross-compilation incredibly easy. To build a Windows executable from macOS in Zig:
`zig build -Dtarget=x86_64-windows`

## CLI: dotnet vs. zig

The `dotnet` CLI is fantastic, but the `zig` CLI is even more compact.

| Task | Dotnet CLI | Zig CLI (0.15.1) |
| :--- | :--- | :--- |
| Create Project | `dotnet new console` | `zig init` |
| Run App | `dotnet run` | `zig build run` |
| Run Tests | `dotnet test` | `zig build test` |
| Compile | `dotnet build` | `zig build` |

## Final Thoughts: 30 Years Later

After three decades of programming, what I appreciate about Zig 0.15.1 is its **honesty**. There are no "background threads" you didn't start, no "stop-the-world" GC pauses, and no hidden vtables.

If you are a C# developer looking to build high-performance systems, CLI tools, or WebAssembly, Zig is the most rewarding language you can learn today. It won't replace C# for enterprise web APIs, but it will become your favorite tool for everything else.
