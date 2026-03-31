---
layout: post
title: A comprehensive Rust learning guide for C# developers
date: 2026-03-31
author: Christian Helle
tags:
  - Rust
  - .NET
  - C#
---

After two decades of writing C#, I recently started learning Rust. The experience has been simultaneously frustrating and fascinating. Rust forces you to think about problems that C# developers never have to consider, while also offering guarantees that C# simply cannot provide. This guide is what I wish I had when I started—a comprehensive comparison between Rust and C# for experienced C# developers making the leap.

This isn't a beginner's guide to programming. It assumes you're comfortable with C#, understand concepts like generics and interfaces, and have written production code. What it provides is a systematic comparison of how Rust approaches the same problems you've been solving in C#, with explanations of why the differences exist and what they mean for how you write code.

## Setting Up and Getting Started

Before writing any code, you need the tooling installed and working.

### Installation

**C#:**

```bash
# Download and install .NET SDK from Microsoft
# Windows: Use installer from dotnet.microsoft.com
# Linux/macOS: Use package manager or installer script
dotnet --version
```

**Rust:**

```bash
# Install rustup (Rust toolchain installer)
# Linux/macOS:
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh

# Windows: Download rustup-init.exe from rustup.rs

rustc --version
cargo --version
```

Rustup is similar to the .NET SDK installer but more powerful. It manages multiple Rust versions, targets, and components. The closest .NET equivalent would be if the .NET SDK installer could also manage multiple SDK versions and cross-compilation targets.

### Hello World

**C#:**

```csharp
// Program.cs
using System;

Console.WriteLine("Hello, World!");
```

```bash
dotnet run
```

**Rust:**

```rust
// main.rs
fn main() {
    println!("Hello, World!");
}
```

```bash
cargo run
```

Both are straightforward, but notice that Rust requires an explicit `main` function as the entry point, while modern C# allows top-level statements.

### Project Structure

**C# (.csproj):**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>
</Project>
```

**Rust (Cargo.toml):**

```toml
[package]
name = "myapp"
version = "0.1.0"
edition = "2021"

[dependencies]
serde_json = "1.0"
```

Cargo.toml is more concise than .csproj files. It uses TOML instead of XML and handles dependencies with semantic versioning by default. The Cargo ecosystem strongly encourages semantic versioning, while NuGet is more flexible about versioning schemes.

### Build Commands

| Task          | C#                        | Rust                  |
| ------------- | ------------------------- | --------------------- |
| Debug build   | `dotnet build`            | `cargo build`         |
| Release build | `dotnet build -c Release` | `cargo build --release` |
| Run           | `dotnet run`              | `cargo run`           |
| Test          | `dotnet test`             | `cargo test`          |
| Clean         | `dotnet clean`            | `cargo clean`         |
| Format        | `dotnet format`           | `cargo fmt`           |

The commands are remarkably similar. Both ecosystems converged on a unified CLI tool that handles building, testing, and package management.

### Package Management

**C# (NuGet):**

```bash
dotnet add package Newtonsoft.Json
dotnet restore
```

**Rust (Cargo):**

```bash
cargo add serde_json
cargo build  # Automatically fetches dependencies
```

In C#, you explicitly restore dependencies. In Rust, Cargo automatically fetches dependencies when you build. Both use centralized package repositories (NuGet.org and crates.io), though Cargo's dependency resolution is deterministic by default through the `Cargo.lock` file.

## Variables and Mutability

This is where Rust starts diverging from C# in fundamental ways.

### Immutability by Default

**C#:**

```csharp
// Variables are mutable by default
var x = 5;
x = 6;  // Allowed

// Immutability requires explicit keywords
const int MAX = 100;
readonly int _value = 42;
```

**Rust:**

```rust
// Variables are immutable by default
let x = 5;
x = 6;  // ERROR: cannot assign twice to immutable variable

// Mutability requires explicit keyword
let mut y = 5;
y = 6;  // OK

// Constants use const
const MAX: i32 = 100;
```

In C#, everything is mutable unless you say otherwise. In Rust, everything is immutable unless you say otherwise. This isn't just philosophy—it enables the borrow checker to make stronger guarantees about your code.

### Type Inference

Both languages have excellent type inference, but with different syntax.

**C#:**

```csharp
var name = "Christian";  // Type inferred as string
var count = 42;          // Type inferred as int
```

**Rust:**

```rust
let name = "Christian";  // Type inferred as &str
let count = 42;          // Type inferred as i32
```

In Rust, you can also be explicit about types:

```rust
let count: i32 = 42;
let name: &str = "Christian";
```

### Shadowing

Rust has a feature called shadowing that C# doesn't have.

**Rust:**

```rust
let x = 5;
let x = x + 1;  // Shadows the previous x
let x = x * 2;  // Shadows again
println!("{}", x);  // 12
```

This looks like mutation but isn't. Each `let` creates a new variable that shadows the previous one. This is useful for transforming values while keeping them immutable at each step.

**C# equivalent would be:**

```csharp
var x1 = 5;
var x2 = x1 + 1;
var x3 = x2 * 2;
Console.WriteLine(x3);  // 12
```

Shadowing also lets you change types:

```rust
let spaces = "   ";
let spaces = spaces.len();  // Now an integer
```

In C#, you'd need different variable names:

```csharp
var spacesStr = "   ";
var spacesCount = spacesStr.Length;
```

## Data Types

### Scalar Types

Both languages have integers, floats, booleans, and characters, but with different defaults and semantics.

**Integers:**

**C#:**

```csharp
int x = 42;           // 32-bit signed
uint y = 42;          // 32-bit unsigned
long z = 42;          // 64-bit signed
byte b = 255;         // 8-bit unsigned
```

**Rust:**

```rust
let x: i32 = 42;      // 32-bit signed
let y: u32 = 42;      // 32-bit unsigned
let z: i64 = 42;      // 64-bit signed
let b: u8 = 255;      // 8-bit unsigned
```

Rust is more explicit about signedness and size. It has `i8`, `i16`, `i32`, `i64`, `i128` for signed integers and `u8`, `u16`, `u32`, `u64`, `u128` for unsigned. It also has `isize` and `usize` for pointer-sized integers, similar to C#'s `nint` and `nuint`.

**Floats:**

**C#:**

```csharp
float x = 3.14f;      // 32-bit
double y = 3.14;      // 64-bit (default)
decimal z = 3.14m;    // 128-bit fixed-point
```

**Rust:**

```rust
let x: f32 = 3.14;    // 32-bit
let y: f64 = 3.14;    // 64-bit (default)
// No decimal type in core Rust
```

C# has `decimal` for financial calculations. Rust doesn't have this in the standard library, but the `rust_decimal` crate provides similar functionality.

**Booleans and Characters:**

**C#:**

```csharp
bool isReady = true;
char letter = 'A';    // UTF-16 code unit
```

**Rust:**

```rust
let is_ready: bool = true;
let letter: char = 'A';    // Unicode scalar value
```

Rust's `char` is a full Unicode scalar value (4 bytes), not a UTF-16 code unit like C#. This means Rust's `char` can represent any Unicode character directly.

### Compound Types

**Tuples:**

**C#:**

```csharp
var tuple = (42, "hello");
Console.WriteLine(tuple.Item1);  // 42
Console.WriteLine(tuple.Item2);  // "hello"

// Named tuples
var named = (Count: 42, Message: "hello");
Console.WriteLine(named.Count);
```

**Rust:**

```rust
let tuple = (42, "hello");
println!("{}", tuple.0);  // 42
println!("{}", tuple.1);  // "hello"

// Pattern matching to destructure
let (count, message) = tuple;
```

Rust doesn't have named tuples in the same way, but you can use structs for that purpose.

**Arrays:**

**C#:**

```csharp
int[] numbers = new int[5];
int[] values = { 1, 2, 3, 4, 5 };
```

**Rust:**

```rust
let numbers: [i32; 5] = [0; 5];  // [0, 0, 0, 0, 0]
let values = [1, 2, 3, 4, 5];    // Type: [i32; 5]
```

Rust arrays have their size as part of the type signature. `[i32; 5]` is a different type from `[i32; 10]`. This is a major difference from C#. For dynamic-sized collections, both languages provide growable types (`List<T>` in C#, `Vec<T>` in Rust).

### String Types

This is one of the most confusing areas for C# developers learning Rust.

**C#:**

```csharp
string s1 = "hello";              // Heap-allocated, immutable reference type
ReadOnlySpan<char> s2 = "hello";  // Stack-allocated view
```

**Rust:**

```rust
let s1: String = String::from("hello");  // Heap-allocated, growable
let s2: &str = "hello";                  // String slice, usually points to static memory
```

In Rust:
- `String` is like C#'s `StringBuilder`—an owned, heap-allocated, growable string.
- `&str` is like C#'s `ReadOnlySpan<char>`—a view into string data owned by someone else.

String literals in Rust are `&str` types, and they're often stored in the binary's data segment. Converting between them:

```rust
let s1: String = String::from("hello");
let s2: &str = &s1;  // Borrow s1 as a string slice

let s3: &str = "world";
let s4: String = s3.to_string();  // Convert to owned String
```

This distinction matters because of Rust's ownership system, which we'll cover in detail later.

## Functions

Function syntax is similar but with key differences.

**C#:**

```csharp
int Add(int x, int y)
{
    return x + y;
}

// Expression-bodied method
int Add(int x, int y) => x + y;
```

**Rust:**

```rust
fn add(x: i32, y: i32) -> i32 {
    return x + y;
}

// Expression-based return (no semicolon)
fn add(x: i32, y: i32) -> i32 {
    x + y
}
```

In Rust, if you omit the semicolon on the last line, that value is returned. This is because Rust distinguishes between statements and expressions. An expression evaluates to a value; a statement does not.

**Statements vs Expressions:**

```rust
fn example() -> i32 {
    let x = 5;  // Statement
    x + 1       // Expression, returned
}
```

If you add a semicolon, it becomes a statement and doesn't return a value:

```rust
fn example() -> i32 {
    let x = 5;
    x + 1;  // Statement, returns nothing
}  // ERROR: mismatched types
```

**C# doesn't make this distinction:**

```csharp
int Example()
{
    var x = 5;
    return x + 1;  // Must explicitly return
}
```

## Control Flow

### If Expressions

**C#:**

```csharp
// if is a statement
int number;
if (condition)
{
    number = 5;
}
else
{
    number = 10;
}

// Or with ternary operator
int number = condition ? 5 : 10;
```

**Rust:**

```rust
// if is an expression
let number = if condition {
    5
} else {
    10
};
```

In Rust, `if` is an expression that returns a value. Both branches must return the same type.

### Loops

**C# loops:**

```csharp
// while loop
while (condition)
{
    // code
}

// for loop
for (int i = 0; i < 10; i++)
{
    // code
}

// foreach loop
foreach (var item in collection)
{
    // code
}
```

**Rust loops:**

```rust
// while loop
while condition {
    // code
}

// loop (infinite loop)
loop {
    // code
    if done { break; }
}

// for loop (iterator-based)
for i in 0..10 {
    // code
}

// for loop over collection
for item in collection {
    // code
}
```

Rust's `loop` keyword is specifically for infinite loops. The compiler understands this and can make different optimizations. The `for` loop in Rust is always iterator-based—there's no C-style `for (;;)` loop.

### Match vs Switch

**C# switch:**

```csharp
int number = 3;
string result = number switch
{
    1 => "one",
    2 => "two",
    3 => "three",
    _ => "other"
};
```

**Rust match:**

```rust
let number = 3;
let result = match number {
    1 => "one",
    2 => "two",
    3 => "three",
    _ => "other"
};
```

They look similar, but Rust's `match` has exhaustiveness checking. If you forget the `_` case, the code won't compile. C# switch expressions require exhaustiveness for the expression form but not for the statement form.

**Matching ranges:**

```rust
let number = 5;
match number {
    1..=5 => println!("small"),
    6..=10 => println!("medium"),
    _ => println!("large")
}
```

**C# equivalent:**

```csharp
var number = 5;
var result = number switch
{
    >= 1 and <= 5 => "small",
    >= 6 and <= 10 => "medium",
    _ => "large"
};
```

## Ownership and Memory Management

This is the biggest conceptual difference between Rust and C#. If you understand only one thing from this guide, understand this section.

### The Problem C# Solves with GC

In C#, you allocate objects and the garbage collector automatically cleans them up when they're no longer referenced. You never think about manually freeing memory:

```csharp
void ProcessData()
{
    var data = new byte[1024 * 1024];  // Allocate 1MB
    // Use data
}  // GC will eventually clean up data
```

The garbage collector runs in the background, tracks references, and deallocates objects when nothing points to them anymore. This is convenient but comes with costs:
- Non-deterministic cleanup timing
- GC pause times
- Memory overhead
- Runtime performance cost

### The Problem Rust Solves with Ownership

Rust provides memory safety without garbage collection through its ownership system. The compiler enforces rules at compile time that prevent memory errors.

**Ownership Rules:**

1. Each value in Rust has an owner
2. There can only be one owner at a time
3. When the owner goes out of scope, the value is dropped

**C# equivalent concept (but not enforced):**

```csharp
void Example()
{
    var data = new byte[1024];  // data "owns" the array
    UseData(data);              // Pass reference, but ownership isn't tracked
}  // data goes out of scope, but GC handles cleanup later
```

**Rust:**

```rust
fn example() {
    let data = vec![0u8; 1024];  // data owns the Vec
    use_data(data);              // Ownership moves to use_data
}  // data is already gone, nothing to clean up here

fn use_data(data: Vec<u8>) {
    // data is owned here
}  // data is dropped here
```

When you pass `data` to `use_data`, ownership moves. The original `data` variable can't be used anymore:

```rust
fn example() {
    let data = vec![0u8; 1024];
    use_data(data);
    println!("{}", data.len());  // ERROR: value borrowed after move
}
```

### Move Semantics

**C# reference semantics:**

```csharp
var s1 = new List<int> { 1, 2, 3 };
var s2 = s1;  // s2 now references the same list as s1
s1.Add(4);    // Modifies the list
s2.Add(5);    // Modifies the same list
Console.WriteLine(s1.Count);  // 5
```

**Rust move semantics:**

```rust
let s1 = vec![1, 2, 3];
let s2 = s1;  // s1's ownership moves to s2
// s1 can no longer be used
println!("{:?}", s2);  // OK
println!("{:?}", s1);  // ERROR: value borrowed after move
```

For simple types that implement `Copy` (like integers), Rust copies instead of moves:

```rust
let x = 5;
let y = x;  // x is copied to y
println!("{}", x);  // OK, x is still valid
```

### References and Borrowing

To use a value without taking ownership, you borrow it with a reference.

**C# references (always reference types):**

```csharp
void UseList(List<int> list)
{
    list.Add(42);  // Modifies the original list
}

var myList = new List<int>();
UseList(myList);  // Pass reference
Console.WriteLine(myList.Count);  // 1
```

**Rust borrowing:**

```rust
fn use_vec(vec: &Vec<i32>) {
    println!("{:?}", vec);
    // Can't modify - this is an immutable borrow
}

let my_vec = vec![1, 2, 3];
use_vec(&my_vec);  // Borrow
println!("{:?}", my_vec);  // Still valid
```

For mutable access, use `&mut`:

```rust
fn add_item(vec: &mut Vec<i32>) {
    vec.push(42);  // Can modify
}

let mut my_vec = vec![1, 2, 3];
add_item(&mut my_vec);  // Mutable borrow
println!("{:?}", my_vec);  // [1, 2, 3, 42]
```

**Borrowing rules enforced at compile time:**

1. You can have either one mutable reference OR any number of immutable references
2. References must always be valid

This prevents data races at compile time:

```rust
let mut s = String::from("hello");
let r1 = &s;     // OK
let r2 = &s;     // OK
let r3 = &mut s; // ERROR: cannot borrow as mutable because it's already borrowed as immutable
```

In C#, this would compile but could cause race conditions in multithreaded code:

```csharp
var s = new StringBuilder("hello");
var r1 = s;  // Reference 1
var r2 = s;  // Reference 2
r1.Append(" world");  // Both r1 and r2 can modify
r2.Append("!");       // Potential race condition in multithreaded code
```

### Slices

Slices let you reference a contiguous sequence of elements without ownership.

**C#:**

```csharp
var numbers = new int[] { 1, 2, 3, 4, 5 };
var slice = new Span<int>(numbers, 1, 3);  // Elements 2, 3, 4
```

**Rust:**

```rust
let numbers = vec![1, 2, 3, 4, 5];
let slice = &numbers[1..4];  // Elements 2, 3, 4
```

String slices work similarly:

```rust
let s = String::from("hello world");
let hello = &s[0..5];   // "hello"
let world = &s[6..11];  // "world"
```

### RAII and Drop

Rust uses RAII (Resource Acquisition Is Initialization) for deterministic cleanup.

**C# IDisposable:**

```csharp
using (var file = File.OpenWrite("data.txt"))
{
    file.Write(data);
}  // Dispose called here
```

**Rust Drop trait:**

```rust
{
    let file = File::create("data.txt")?;
    file.write_all(&data)?;
}  // Drop called here, file closed
```

The difference is that in Rust, `Drop` is called automatically and predictably when a value goes out of scope. In C#, you need to explicitly use `using` statements or call `Dispose()`, and the garbage collector handles the rest eventually.

## Structs and Classes

### Defining Types

**C# class:**

```csharp
public class User
{
    public string Name { get; set; }
    public int Age { get; set; }
    
    public User(string name, int age)
    {
        Name = name;
        Age = age;
    }
}
```

**Rust struct:**

```rust
pub struct User {
    pub name: String,
    pub age: i32,
}

impl User {
    pub fn new(name: String, age: i32) -> Self {
        User { name, age }
    }
}
```

Rust separates data (struct) from behavior (impl blocks). Methods are defined in separate `impl` blocks, not inside the struct definition.

### Methods

**C# methods:**

```csharp
public class Rectangle
{
    public int Width { get; set; }
    public int Height { get; set; }
    
    public int Area()
    {
        return Width * Height;
    }
    
    public static Rectangle Square(int size)
    {
        return new Rectangle { Width = size, Height = size };
    }
}
```

**Rust methods:**

```rust
pub struct Rectangle {
    pub width: i32,
    pub height: i32,
}

impl Rectangle {
    // Instance method with immutable self
    pub fn area(&self) -> i32 {
        self.width * self.height
    }
    
    // Instance method with mutable self
    pub fn set_width(&mut self, width: i32) {
        self.width = width;
    }
    
    // Associated function (like C# static method)
    pub fn square(size: i32) -> Self {
        Rectangle { width: size, height: size }
    }
}
```

Rust has three forms of `self`:
- `&self` - Immutable borrow (read-only access)
- `&mut self` - Mutable borrow (can modify)
- `self` - Takes ownership (consumes the value)

In C#, instance methods always operate on `this` by reference and can always modify the object unless you use readonly structs.

### No Inheritance

Rust doesn't have class inheritance. There's no `abstract class` or virtual methods.

**C# inheritance:**

```csharp
public abstract class Animal
{
    public abstract void MakeSound();
}

public class Dog : Animal
{
    public override void MakeSound()
    {
        Console.WriteLine("Woof!");
    }
}
```

Rust uses traits (interfaces) and composition instead:

```rust
pub trait Animal {
    fn make_sound(&self);
}

pub struct Dog;

impl Animal for Dog {
    fn make_sound(&self) {
        println!("Woof!");
    }
}
```

### Records vs Derive Macros

**C# records:**

```csharp
public record User(string Name, int Age);

var user1 = new User("Alice", 30);
var user2 = user1 with { Age = 31 };
```

**Rust structs with derive:**

```rust
#[derive(Debug, Clone, PartialEq)]
pub struct User {
    pub name: String,
    pub age: i32,
}

let user1 = User { name: String::from("Alice"), age: 30 };
let user2 = User { age: 31, ..user1.clone() };
```

The `#[derive]` attribute auto-generates implementations. `Debug` gives you debug printing, `Clone` allows cloning, `PartialEq` enables equality comparison.

### Tuple Structs

Rust has tuple structs—structs without named fields:

```rust
struct Color(i32, i32, i32);
let black = Color(0, 0, 0);
```

C# doesn't have a direct equivalent, though you could use regular tuples:

```csharp
var black = (0, 0, 0);
```

## Enums and Pattern Matching

This is where Rust truly shines compared to C#.

### C# Enums vs Rust Enums

**C# enum (simple):**

```csharp
public enum Status
{
    Pending,
    Active,
    Inactive
}

Status status = Status.Active;
```

**Rust enum (with data):**

```rust
pub enum Status {
    Pending,
    Active,
    Inactive,
}

let status = Status::Active;
```

So far, they're similar. But Rust enums can hold data:

```rust
pub enum Message {
    Quit,
    Move { x: i32, y: i32 },
    Write(String),
    ChangeColor(i32, i32, i32),
}
```

This has no direct C# equivalent. You'd need abstract classes or interfaces with multiple implementing types:

```csharp
public abstract class Message { }
public class Quit : Message { }
public class Move : Message 
{ 
    public int X { get; init; }
    public int Y { get; init; }
}
public class Write : Message 
{ 
    public string Text { get; init; }
}
```

### Option vs Nullable

**C# nullable types:**

```csharp
int? maybeNumber = null;
if (maybeNumber.HasValue)
{
    Console.WriteLine(maybeNumber.Value);
}

// Or with null-conditional
Console.WriteLine(maybeNumber?.ToString() ?? "None");
```

**Rust Option:**

```rust
let maybe_number: Option<i32> = None;
match maybe_number {
    Some(n) => println!("{}", n),
    None => println!("None"),
}

// Or with if let
if let Some(n) = maybe_number {
    println!("{}", n);
}
```

Rust doesn't have null. `Option<T>` is an enum:

```rust
pub enum Option<T> {
    Some(T),
    None,
}
```

This forces you to explicitly handle the "nothing" case. You can't accidentally use a `None` value:

```rust
let x: Option<i32> = Some(5);
let y = x + 1;  // ERROR: cannot add Option<i32> and i32
```

You must unwrap or handle it:

```rust
let y = x.unwrap() + 1;  // Panics if x is None
// Or safer:
let y = x.unwrap_or(0) + 1;
// Or with pattern matching:
let y = match x {
    Some(n) => n + 1,
    None => 0,
};
```

### Result vs Exceptions

This is a fundamental difference in error handling philosophy.

**C# exceptions:**

```csharp
public int Divide(int a, int b)
{
    if (b == 0)
        throw new DivideByZeroException();
    return a / b;
}

try
{
    var result = Divide(10, 0);
}
catch (DivideByZeroException ex)
{
    Console.WriteLine("Error: " + ex.Message);
}
```

**Rust Result:**

```rust
pub fn divide(a: i32, b: i32) -> Result<i32, String> {
    if b == 0 {
        Err(String::from("division by zero"))
    } else {
        Ok(a / b)
    }
}

match divide(10, 0) {
    Ok(result) => println!("Result: {}", result),
    Err(e) => println!("Error: {}", e),
}
```

`Result<T, E>` is an enum:

```rust
pub enum Result<T, E> {
    Ok(T),
    Err(E),
}
```

### Match Exhaustiveness

Rust's compiler ensures you handle all possible cases:

```rust
let status = Status::Active;
match status {
    Status::Pending => println!("pending"),
    Status::Active => println!("active"),
    // ERROR: missing match arm: Inactive
}
```

C# switch expressions have exhaustiveness checking but it's not as strict:

```csharp
Status status = Status.Active;
var message = status switch
{
    Status.Pending => "pending",
    Status.Active => "active",
    // Warning or error depending on context
};
```

### If Let and While Let

Rust provides convenient syntax for matching a single pattern:

```rust
let some_value = Some(3);

// Instead of full match
if let Some(x) = some_value {
    println!("{}", x);
}

// While let for loops
let mut stack = vec![1, 2, 3];
while let Some(top) = stack.pop() {
    println!("{}", top);
}
```

C# has similar patterns with `is`:

```csharp
object obj = "hello";
if (obj is string s)
{
    Console.WriteLine(s.ToUpper());
}
```

## Error Handling

Error handling is one of the most visible differences between the languages.

### Result and the ? Operator

**C# try/catch:**

```csharp
public string ReadConfig()
{
    try
    {
        var content = File.ReadAllText("config.txt");
        var config = JsonSerializer.Deserialize<Config>(content);
        return config.Value;
    }
    catch (FileNotFoundException ex)
    {
        throw new ConfigException("Config file not found", ex);
    }
    catch (JsonException ex)
    {
        throw new ConfigException("Invalid config format", ex);
    }
}
```

**Rust Result with ?:**

```rust
use std::fs;
use std::io;

pub fn read_config() -> Result<String, io::Error> {
    let content = fs::read_to_string("config.txt")?;
    let config: Config = serde_json::from_str(&content)?;
    Ok(config.value)
}
```

The `?` operator automatically propagates errors. If the operation returns `Err`, the function returns early with that error. If it returns `Ok`, it unwraps the value.

It's roughly equivalent to:

```rust
let content = match fs::read_to_string("config.txt") {
    Ok(c) => c,
    Err(e) => return Err(e),
};
```

### Panic vs Exceptions

**C# exceptions can be caught:**

```csharp
try
{
    throw new InvalidOperationException();
}
catch (Exception ex)
{
    // Handle it
}
```

**Rust panic usually can't be caught:**

```rust
panic!("Something went wrong!");  // Program terminates
```

Panics are for unrecoverable errors. They unwind the stack (by default) and terminate the program. You can catch panics with `std::panic::catch_unwind`, but it's uncommon and not recommended for control flow.

### Unwrap and Expect

For quick prototyping or when you know a value can't fail:

```rust
let x = Some(5);
let y = x.unwrap();  // Panics if x is None

let z = x.expect("x should have a value");  // Panics with custom message
```

In production code, prefer `match`, `if let`, or `?` to handle errors explicitly.

### Error Crates

For more sophisticated error handling, the Rust ecosystem has:

**anyhow** - for application-level error handling:

```rust
use anyhow::{Result, Context};

pub fn read_config() -> Result<Config> {
    let content = fs::read_to_string("config.txt")
        .context("Failed to read config file")?;
    let config = serde_json::from_str(&content)
        .context("Failed to parse config")?;
    Ok(config)
}
```

**thiserror** - for library error types:

```rust
use thiserror::Error;

#[derive(Error, Debug)]
pub enum ConfigError {
    #[error("Config file not found")]
    NotFound,
    #[error("Invalid format: {0}")]
    InvalidFormat(String),
}
```

This is similar to C#'s custom exception types:

```csharp
public class ConfigException : Exception
{
    public ConfigException(string message) : base(message) { }
}
```

## Collections

### Vectors vs Lists

**C# List:**

```csharp
var numbers = new List<int>();
numbers.Add(1);
numbers.Add(2);
numbers.Add(3);
Console.WriteLine(numbers[0]);
Console.WriteLine(numbers.Count);
```

**Rust Vec:**

```rust
let mut numbers = Vec::new();
numbers.push(1);
numbers.push(2);
numbers.push(3);
println!("{}", numbers[0]);
println!("{}", numbers.len());
```

They're nearly identical in functionality. Both are dynamically sized, heap-allocated sequences.

**Creating with initial values:**

```csharp
var numbers = new List<int> { 1, 2, 3 };
```

```rust
let numbers = vec![1, 2, 3];  // vec! macro
```

### HashMaps vs Dictionaries

**C# Dictionary:**

```csharp
var scores = new Dictionary<string, int>();
scores["Alice"] = 10;
scores["Bob"] = 8;

if (scores.TryGetValue("Alice", out var score))
{
    Console.WriteLine(score);
}
```

**Rust HashMap:**

```rust
use std::collections::HashMap;

let mut scores = HashMap::new();
scores.insert(String::from("Alice"), 10);
scores.insert(String::from("Bob"), 8);

if let Some(score) = scores.get("Alice") {
    println!("{}", score);
}
```

The APIs are very similar. Rust's `get` returns `Option<&V>`, forcing you to handle the "key not found" case.

### Iterators vs LINQ

This is where the two languages feel most similar.

**C# LINQ:**

```csharp
var numbers = new List<int> { 1, 2, 3, 4, 5 };
var doubled = numbers
    .Where(x => x % 2 == 0)
    .Select(x => x * 2)
    .ToList();
```

**Rust iterators:**

```rust
let numbers = vec![1, 2, 3, 4, 5];
let doubled: Vec<i32> = numbers
    .iter()
    .filter(|x| *x % 2 == 0)
    .map(|x| x * 2)
    .collect();
```

Both use lazy evaluation. The operations don't execute until you consume the iterator (with `ToList()` in C# or `collect()` in Rust).

**More examples:**

```csharp
// C# LINQ
var sum = numbers.Sum();
var max = numbers.Max();
var any = numbers.Any(x => x > 10);
```

```rust
// Rust iterators
let sum: i32 = numbers.iter().sum();
let max = numbers.iter().max();
let any = numbers.iter().any(|x| *x > 10);
```

The biggest difference is that Rust iterators are methods on the `Iterator` trait, not extension methods like LINQ. But functionally, they're remarkably similar.

## Generics

Both languages have robust generic systems.

### Basic Generics

**C#:**

```csharp
public class Container<T>
{
    private T _value;
    
    public Container(T value)
    {
        _value = value;
    }
    
    public T Get() => _value;
}
```

**Rust:**

```rust
pub struct Container<T> {
    value: T,
}

impl<T> Container<T> {
    pub fn new(value: T) -> Self {
        Container { value }
    }
    
    pub fn get(&self) -> &T {
        &self.value
    }
}
```

### Generic Constraints

**C# constraints:**

```csharp
public class Processor<T> where T : IComparable<T>
{
    public T Max(T a, T b)
    {
        return a.CompareTo(b) > 0 ? a : b;
    }
}
```

**Rust trait bounds:**

```rust
pub struct Processor<T: PartialOrd> {
    // struct fields
}

impl<T: PartialOrd> Processor<T> {
    pub fn max(&self, a: T, b: T) -> T {
        if a > b { a } else { b }
    }
}
```

Alternative syntax using `where`:

```rust
impl<T> Processor<T> 
where 
    T: PartialOrd 
{
    // implementation
}
```

### Monomorphization vs JIT

**C# generics** use runtime type erasure (with some reification). Generic types are compiled once, and the JIT fills in type parameters at runtime.

**Rust generics** use monomorphization. The compiler generates separate code for each concrete type used. This results in faster code but larger binaries.

```rust
let int_container = Container::new(5);
let string_container = Container::new(String::from("hello"));
// Compiler generates two different versions of Container
```

## Traits vs Interfaces

Traits are Rust's primary abstraction mechanism, similar to C# interfaces but more powerful.

### Defining and Implementing

**C# interface:**

```csharp
public interface IDrawable
{
    void Draw();
}

public class Circle : IDrawable
{
    public void Draw()
    {
        Console.WriteLine("Drawing circle");
    }
}
```

**Rust trait:**

```rust
pub trait Drawable {
    fn draw(&self);
}

pub struct Circle;

impl Drawable for Circle {
    fn draw(&self) {
        println!("Drawing circle");
    }
}
```

### Default Implementations

Both support default implementations:

**C#:**

```csharp
public interface ILogger
{
    void Log(string message);
    
    void LogError(string message)
    {
        Log($"ERROR: {message}");
    }
}
```

**Rust:**

```rust
pub trait Logger {
    fn log(&self, message: &str);
    
    fn log_error(&self, message: &str) {
        self.log(&format!("ERROR: {}", message));
    }
}
```

### Trait Objects vs Interface References

**C# interface references:**

```csharp
IDrawable shape = new Circle();
shape.Draw();  // Virtual dispatch at runtime
```

**Rust trait objects:**

```rust
let shape: Box<dyn Drawable> = Box::new(Circle);
shape.draw();  // Dynamic dispatch at runtime
```

The `dyn` keyword indicates dynamic dispatch. Without it, Rust uses static dispatch (monomorphization).

**Static dispatch in Rust:**

```rust
fn draw_shape<T: Drawable>(shape: &T) {
    shape.draw();  // Resolved at compile time
}
```

C# doesn't have a direct equivalent to this compile-time polymorphism for interfaces. Generics with constraints come close but still involve runtime dispatch.

### Derive Macros vs Source Generators

**C# source generators:**

```csharp
// Requires custom source generator
[AutoNotify]
public partial class Person
{
    private string _name;
}
```

**Rust derive macros:**

```rust
#[derive(Debug, Clone, PartialEq, Eq, Hash)]
pub struct Person {
    name: String,
}
```

Rust's derive macros are more common and built into the language ecosystem. Many standard traits can be automatically implemented.

### Orphan Rule

Rust has a restriction that C# doesn't have: you can only implement a trait for a type if either the trait or the type is local to your crate.

```rust
// Can't do this:
impl Display for Vec<i32> {  // ERROR: both Display and Vec are from std
    // ...
}
```

This prevents conflicts when combining multiple dependencies. C# doesn't have this restriction—you can add interface implementations to any type through extension methods or wrappers.

### Extension Traits vs Extension Methods

**C# extension methods:**

```csharp
public static class StringExtensions
{
    public static bool IsEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }
}

"hello".IsEmpty();  // false
```

**Rust extension traits:**

```rust
pub trait StringExt {
    fn is_empty_ext(&self) -> bool;
}

impl StringExt for String {
    fn is_empty_ext(&self) -> bool {
        self.is_empty()
    }
}

String::from("hello").is_empty_ext();  // false
```

Both achieve similar results but with different mechanisms. C# extension methods are syntactic sugar for static method calls. Rust extension traits are actual trait implementations.

## Lifetimes

Lifetimes are Rust's most unique and challenging feature. C# developers have never dealt with anything like this.

### Why C# Doesn't Need Lifetimes

In C#, the garbage collector tracks references for you:

```csharp
string GetFirstWord(string s)
{
    var space = s.IndexOf(' ');
    if (space == -1) return s;
    return s.Substring(0, space);
}

var sentence = "hello world";
var word = GetFirstWord(sentence);
// Both sentence and word are valid, GC handles lifetime
```

The GC ensures `sentence` stays alive as long as any part of it is referenced. You never think about this.

### Why Rust Needs Lifetimes

Rust doesn't have a GC. The compiler needs to verify that references are always valid:

```rust
fn get_first_word(s: &str) -> &str {
    let space = s.find(' ');
    match space {
        Some(pos) => &s[..pos],
        None => s,
    }
}
```

The compiler needs to know: how long is the returned reference valid? The answer is: as long as the input reference is valid. This is expressed with lifetimes.

### Lifetime Annotations

```rust
fn get_first_word<'a>(s: &'a str) -> &'a str {
    // ...
}
```

The `'a` is a lifetime parameter. It says "the output reference lives as long as the input reference."

**More complex example:**

```rust
fn longest<'a>(x: &'a str, y: &'a str) -> &'a str {
    if x.len() > y.len() { x } else { y }
}
```

This says "the returned reference lives as long as the shortest of x and y."

### Lifetime Elision

In many cases, Rust can infer lifetimes:

```rust
// Explicit lifetimes
fn first_word<'a>(s: &'a str) -> &'a str { ... }

// Elided lifetimes (same meaning)
fn first_word(s: &str) -> &str { ... }
```

The compiler applies rules to infer lifetimes in common patterns. But when it can't figure it out, you must annotate explicitly.

### Lifetimes in Structs

```rust
pub struct Excerpt<'a> {
    text: &'a str,
}
```

This says the struct contains a reference that's valid for lifetime `'a`. The struct can't outlive the data it references:

```rust
let excerpt;
{
    let text = String::from("hello");
    excerpt = Excerpt { text: &text };
}  // ERROR: text is dropped, but excerpt holds a reference to it
```

### The 'static Lifetime

```rust
let s: &'static str = "hello";
```

The `'static` lifetime means the reference is valid for the entire program duration. String literals are always `'static` because they're embedded in the binary.

### Why This Matters

Lifetimes enable Rust to provide memory safety without garbage collection. They prevent:
- Use after free
- Dangling pointers
- Data races

All at compile time, with zero runtime cost. C# provides these guarantees at runtime through the GC, which has performance overhead. Rust provides them at compile time through the borrow checker, which has zero runtime cost but a steep learning curve.

## Modules and Project Organization

### Namespaces vs Modules

**C# namespaces:**

```csharp
// File: User.cs
namespace MyApp.Models
{
    public class User
    {
        public string Name { get; set; }
    }
}

// File: Program.cs
using MyApp.Models;

var user = new User { Name = "Alice" };
```

**Rust modules:**

```rust
// File: src/models.rs
pub struct User {
    pub name: String,
}

// File: src/main.rs
mod models;
use models::User;

fn main() {
    let user = User { name: String::from("Alice") };
}
```

In C#, namespaces are logical groupings independent of file structure. In Rust, the module system is tied to the file system. Each file is a module.

### Nested Modules

**C# nested namespaces:**

```csharp
namespace MyApp.Models.Users
{
    public class UserService { }
}
```

**Rust nested modules:**

```rust
// src/models/mod.rs
pub mod users;

// src/models/users.rs
pub struct UserService;

// Usage:
use models::users::UserService;
```

Or inline:

```rust
pub mod models {
    pub mod users {
        pub struct UserService;
    }
}
```

### Visibility

**C#:**

```csharp
public class User { }        // Visible everywhere
internal class Config { }    // Visible within assembly
private class Helper { }     // Visible within class
protected class Base { }     // Visible to derived classes
```

**Rust:**

```rust
pub struct User { }          // Public, visible everywhere
pub(crate) struct Config { } // Visible within crate
struct Helper { }            // Private to module (default)
// No protected equivalent
```

Rust is private by default. Everything is module-private unless marked `pub`.

### Crates vs Assemblies

**C# assemblies:**

```csharp
// MyApp.dll contains compiled code
// Reference in .csproj:
// <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
```

**Rust crates:**

```toml
# Cargo.toml
[dependencies]
serde_json = "1.0"
```

A crate is a compilation unit, similar to a C# assembly. The `Cargo.toml` file is your project manifest, like `.csproj`.

### Workspaces

**C# solutions:**

```xml
<!-- MyApp.sln references multiple projects -->
<Project Include="MyApp.Core\MyApp.Core.csproj" />
<Project Include="MyApp.Api\MyApp.Api.csproj" />
```

**Rust workspaces:**

```toml
# Cargo.toml in root
[workspace]
members = [
    "core",
    "api",
]
```

Both provide a way to group multiple related projects/crates.

## Closures and Functional Patterns

### Closure Syntax

**C# lambda:**

```csharp
Func<int, int> add_one = x => x + 1;
var result = add_one(5);  // 6

Action<string> print = s => Console.WriteLine(s);
print("hello");
```

**Rust closures:**

```rust
let add_one = |x| x + 1;
let result = add_one(5);  // 6

let print = |s: &str| println!("{}", s);
print("hello");
```

The syntax is similar. Rust uses `||` instead of `()` for parameters.

### Closure Traits

Rust has three closure traits that C# doesn't distinguish:

**Fn** - Borrows values immutably:

```rust
let x = 5;
let add_x = |y| y + x;  // Borrows x
println!("{}", add_x(10));  // 15
```

**FnMut** - Borrows values mutably:

```rust
let mut total = 0;
let mut add_to_total = |y| total += y;  // Mutable borrow of total
add_to_total(5);
println!("{}", total);  // 5
```

**FnOnce** - Takes ownership:

```rust
let s = String::from("hello");
let consume = || drop(s);  // Takes ownership of s
consume();
// s is no longer valid
```

C# captures variables by reference (or by value for value types), but doesn't distinguish these three cases at the type level.

### Move Closures

Force a closure to take ownership:

```rust
let s = String::from("hello");
let closure = move || println!("{}", s);
closure();
// s is moved, no longer valid here
```

**C# equivalent requires explicit copying:**

```csharp
var s = "hello";
Action closure = () => Console.WriteLine(s);  // Captures by reference
closure();
// s still valid
```

### Functional Patterns

**C# LINQ:**

```csharp
var numbers = new List<int> { 1, 2, 3, 4, 5 };
var result = numbers
    .Where(x => x % 2 == 0)
    .Select(x => x * x)
    .Sum();
```

**Rust iterators:**

```rust
let numbers = vec![1, 2, 3, 4, 5];
let result: i32 = numbers
    .iter()
    .filter(|x| *x % 2 == 0)
    .map(|x| x * x)
    .sum();
```

Both languages support functional programming patterns well.

## Smart Pointers

### Heap Allocation with Box

**C# reference types** are automatically heap-allocated:

```csharp
var data = new List<int> { 1, 2, 3 };  // Heap-allocated
```

**Rust Box** explicitly heap-allocates:

```rust
let data = Box::new(vec![1, 2, 3]);  // Heap-allocated
```

`Box<T>` is useful for:
- Allocating large data on the heap
- Recursive types (e.g., trees)
- Trait objects

```rust
struct Node {
    value: i32,
    next: Option<Box<Node>>,  // Recursive type
}
```

### Reference Counting with Rc and Arc

**C# relies on GC** for shared ownership:

```csharp
var data = new List<int> { 1, 2, 3 };
var ref1 = data;
var ref2 = data;
// GC tracks all references
```

**Rust Rc** for single-threaded reference counting:

```rust
use std::rc::Rc;

let data = Rc::new(vec![1, 2, 3]);
let ref1 = Rc::clone(&data);
let ref2 = Rc::clone(&data);
// Reference count is 3
```

**Rust Arc** for thread-safe reference counting:

```rust
use std::sync::Arc;

let data = Arc::new(vec![1, 2, 3]);
let ref1 = Arc::clone(&data);
// Can share across threads
```

`Arc` is atomic reference counting, similar to C#'s reference counting but explicit.

### Interior Mutability with RefCell

**C# can always mutate through references:**

```csharp
var data = new List<int> { 1, 2, 3 };
var ref1 = data;
ref1.Add(4);  // Mutates through reference
```

**Rust RefCell** allows mutation through immutable reference:

```rust
use std::cell::RefCell;

let data = RefCell::new(vec![1, 2, 3]);
data.borrow_mut().push(4);  // Runtime borrow checking
```

`RefCell<T>` moves borrow checking from compile time to runtime. Use it when you need to mutate through a shared reference but can't use `&mut` due to ownership rules.

## Concurrency

### Spawning Threads

**C# Task/Thread:**

```csharp
var thread = new Thread(() => {
    Console.WriteLine("Hello from thread");
});
thread.Start();
thread.Join();

// Or with Task
await Task.Run(() => {
    Console.WriteLine("Hello from task");
});
```

**Rust threads:**

```rust
use std::thread;

let handle = thread::spawn(|| {
    println!("Hello from thread");
});
handle.join().unwrap();
```

### Message Passing

**C# channels:**

```csharp
using System.Threading.Channels;

var channel = Channel.CreateUnbounded<int>();
var writer = channel.Writer;
var reader = channel.Reader;

await writer.WriteAsync(42);
var value = await reader.ReadAsync();
```

**Rust channels:**

```rust
use std::sync::mpsc;

let (tx, rx) = mpsc::channel();
tx.send(42).unwrap();
let value = rx.recv().unwrap();
```

Both support message passing between threads. Rust's channels are part of the standard library by default.

### Shared State with Mutex

**C# lock:**

```csharp
private object _lock = new object();
private int _counter = 0;

lock (_lock)
{
    _counter++;
}
```

**Rust Mutex:**

```rust
use std::sync::Mutex;

let counter = Mutex::new(0);
{
    let mut num = counter.lock().unwrap();
    *num += 1;
}
```

For thread-safe shared ownership, combine `Arc` and `Mutex`:

```rust
use std::sync::{Arc, Mutex};
use std::thread;

let counter = Arc::new(Mutex::new(0));
let mut handles = vec![];

for _ in 0..10 {
    let counter = Arc::clone(&counter);
    let handle = thread::spawn(move || {
        let mut num = counter.lock().unwrap();
        *num += 1;
    });
    handles.push(handle);
}

for handle in handles {
    handle.join().unwrap();
}
```

### Send and Sync Traits

Rust has compile-time thread safety through marker traits:

- `Send` - Type can be transferred between threads
- `Sync` - Type can be shared between threads

Most types automatically implement these. The compiler prevents data races:

```rust
use std::rc::Rc;
use std::thread;

let rc = Rc::new(5);
thread::spawn(move || {  // ERROR: Rc is not Send
    println!("{}", rc);
});
```

C# doesn't have compile-time enforcement like this. Thread safety is enforced at runtime or not at all.

## Async Programming

Both languages have async/await, but with different underlying models.

### Async Functions

**C# async/await:**

```csharp
public async Task<string> FetchDataAsync()
{
    var client = new HttpClient();
    var response = await client.GetStringAsync("https://example.com");
    return response;
}
```

**Rust async/await:**

```rust
use reqwest;

pub async fn fetch_data() -> Result<String, reqwest::Error> {
    let response = reqwest::get("https://example.com")
        .await?
        .text()
        .await?;
    Ok(response)
}
```

### Runtimes

**C# has a built-in runtime.** `Task` and async/await work out of the box.

**Rust requires an async runtime.** Common choices:
- **Tokio** - Most popular, full-featured
- **async-std** - Standard library-like API
- **smol** - Lightweight runtime

```toml
[dependencies]
tokio = { version = "1.0", features = ["full"] }
```

```rust
#[tokio::main]
async fn main() {
    let result = fetch_data().await;
}
```

### Future vs Task

**C# Task:**

```csharp
Task<int> task = Task.Run(() => {
    Thread.Sleep(1000);
    return 42;
});
var result = await task;
```

**Rust Future:**

```rust
use tokio::time::{sleep, Duration};

async fn delayed_value() -> i32 {
    sleep(Duration::from_secs(1)).await;
    42
}

let result = delayed_value().await;
```

Rust's `Future` is lazy—it doesn't start executing until you await it or explicitly poll it. C#'s `Task` starts executing immediately when created.

## Testing

Both languages have excellent built-in testing support.

### Unit Tests

**C# with xUnit:**

```csharp
public class CalculatorTests
{
    [Fact]
    public void Add_TwoNumbers_ReturnsSum()
    {
        var result = Calculator.Add(2, 3);
        Assert.Equal(5, result);
    }
}
```

**Rust tests:**

```rust
pub fn add(a: i32, b: i32) -> i32 {
    a + b
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_add() {
        assert_eq!(add(2, 3), 5);
    }
}
```

Rust tests live in the same file as the code they test, inside a `tests` module. The `#[cfg(test)]` attribute ensures the test code is only compiled when running tests.

### Test Assertions

**C#:**

```csharp
Assert.True(condition);
Assert.False(condition);
Assert.Equal(expected, actual);
Assert.Throws<Exception>(() => code());
```

**Rust:**

```rust
assert!(condition);
assert_eq!(expected, actual);
assert_ne!(a, b);

#[should_panic]
#[test]
fn test_panic() {
    panic!("Expected panic");
}
```

### Integration Tests

**C# test projects** are separate projects that reference your main project.

**Rust integration tests** go in the `tests/` directory:

```
my_project/
├── src/
│   └── lib.rs
└── tests/
    └── integration_test.rs
```

```rust
// tests/integration_test.rs
use my_project;

#[test]
fn integration_test() {
    assert!(my_project::function_works());
}
```

## Macros vs Metaprogramming

### Declarative Macros

Rust has powerful compile-time metaprogramming with macros.

**Simple macro:**

```rust
macro_rules! say_hello {
    () => {
        println!("Hello!");
    };
}

say_hello!();  // Expands to println!("Hello!");
```

**Macro with arguments:**

```rust
macro_rules! create_function {
    ($func_name:ident) => {
        fn $func_name() {
            println!("Called {:?}", stringify!($func_name));
        }
    };
}

create_function!(foo);
foo();  // Called "foo"
```

C# doesn't have a direct equivalent to declarative macros. The closest is preprocessor directives, but those are much more limited.

### Procedural Macros

Procedural macros are more powerful:

```rust
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct User {
    pub name: String,
    pub age: i32,
}
```

The `#[derive]` attribute is a procedural macro that generates code.

### C# Source Generators

C# has source generators, which are similar in spirit:

```csharp
[AutoNotify]
public partial class Person
{
    private string _name;
    // Source generator creates Name property
}
```

But they're less common and require more setup than Rust's derive macros.

### Reflection

**C# reflection:**

```csharp
var type = typeof(User);
var properties = type.GetProperties();
foreach (var prop in properties)
{
    Console.WriteLine(prop.Name);
}
```

**Rust doesn't have runtime reflection.** All metaprogramming happens at compile time. For runtime inspection, you need to generate code with macros or procedural macros.

## Tooling and Ecosystem

### Command-Line Tools

| Task               | C#                   | Rust              |
| ------------------ | -------------------- | ----------------- |
| Project creation   | `dotnet new console` | `cargo new myapp` |
| Build              | `dotnet build`       | `cargo build`     |
| Run                | `dotnet run`         | `cargo run`       |
| Test               | `dotnet test`        | `cargo test`      |
| Format             | `dotnet format`      | `cargo fmt`       |
| Lint               | Roslyn analyzers     | `cargo clippy`    |
| Documentation      | XML docs             | `cargo doc`       |
| Package publishing | `dotnet nuget push`  | `cargo publish`   |

### IDE Support

**C#:**
- Visual Studio - Best-in-class IDE experience
- Visual Studio Code - Excellent with C# extension
- Rider - Great alternative from JetBrains

**Rust:**
- Visual Studio Code with rust-analyzer - Excellent
- IntelliJ IDEA with Rust plugin - Good
- Rust Rover from JetBrains - Dedicated Rust IDE

The rust-analyzer language server provides IDE features comparable to C#'s OmniSharp/Roslyn.

### Package Repositories

**C# NuGet.org:**
- Centralized package repository
- Strong versioning support
- Corporate-friendly package hosting

**Rust crates.io:**
- Centralized package repository
- Strong semantic versioning culture
- Excellent documentation hosting on docs.rs

Both ecosystems are mature with thousands of high-quality packages.

### Documentation

**C# XML docs:**

```csharp
/// <summary>
/// Adds two numbers.
/// </summary>
/// <param name="a">First number</param>
/// <param name="b">Second number</param>
/// <returns>The sum</returns>
public int Add(int a, int b) => a + b;
```

**Rust doc comments:**

```rust
/// Adds two numbers.
///
/// # Examples
///
/// ```
/// let result = add(2, 3);
/// assert_eq!(result, 5);
/// ```
pub fn add(a: i32, b: i32) -> i32 {
    a + b
}
```

Rust's doc comments can include runnable code examples that are automatically tested with `cargo test`.

## Features Unique to Rust

These features exist in Rust but have no equivalent in C#:

1. **Ownership and the borrow checker** - Compile-time memory safety without GC
2. **Lifetime annotations** - Explicit control over reference lifetimes
3. **Zero-cost abstractions** - High-level code with low-level performance
4. **Algebraic data types** - Enums that carry data
5. **Pattern matching exhaustiveness** - Compiler-enforced case handling
6. **Send/Sync traits** - Compile-time thread safety guarantees
7. **No null** - `Option` forces explicit handling
8. **No runtime** - No VM, no GC, direct machine code
9. **Move semantics by default** - Explicit ownership transfer
10. **Result-based errors** - No exceptions, explicit error propagation
11. **Declarative macros** - Compile-time code generation with macro_rules!
12. **unsafe blocks** - Explicit opt-out of safety guarantees

## Features Unique to C#

These features exist in C# but have no equivalent in Rust:

1. **Garbage collection** - Automatic memory management
2. **Class inheritance** - Traditional OOP with base classes
3. **Abstract classes** - Partial implementations with inheritance
4. **Properties** - Get/set accessors as language feature
5. **Events and delegates** - Built-in observer pattern
6. **LINQ query syntax** - SQL-like query expressions
7. **Reflection** - Runtime type inspection and manipulation
8. **Dynamic typing** - Runtime type resolution with `dynamic`
9. **Nullable reference types** - Compiler analysis for null safety
10. **Partial classes** - Split class definitions across files
11. **Extension methods** - Add methods to existing types without inheritance
12. **Runtime attributes** - Metadata discoverable via reflection
13. **Hot reload** - Modify code while running
14. **Implicit interface implementation** - No explicit interface syntax required
15. **Indexers** - Overload the `[]` operator for custom types
16. **yield return** - Iterator methods with state machines
17. **try/catch/finally** - Exception-based error handling
18. **async streams** - `IAsyncEnumerable<T>` for async iteration
19. **Covariance and contravariance** - Type parameter variance for interfaces/delegates

## Conclusion

After spending two decades in C#, learning Rust has been both challenging and rewarding. Rust forces you to think about memory, ownership, and lifetimes in ways that C# abstracts away. The borrow checker can be frustrating at first, but it catches bugs at compile time that would be runtime errors or subtle data races in C#.

For C# developers, the biggest conceptual shifts are:

1. **Ownership** - Every value has a clear owner, and ownership transfers are explicit
2. **Immutability by default** - Variables are immutable unless explicitly marked `mut`
3. **No null** - `Option<T>` forces you to handle the "nothing" case
4. **Result-based errors** - No exceptions, explicit error propagation with `Result<T, E>`
5. **Lifetimes** - The compiler tracks reference validity at compile time
6. **Explicit memory management** - No garbage collector, deterministic cleanup

What surprised me most is how similar many things are. Iterators work almost identically to LINQ. Generics are conceptually the same. Traits feel like interfaces with superpowers. The async/await syntax is nearly identical.

**When to use Rust:**
- Systems programming where you need maximum performance
- Applications with strict latency requirements (no GC pauses)
- When you need guaranteed memory safety without runtime overhead
- Command-line tools and system utilities
- Embedded systems and resource-constrained environments

**When to use C#:**
- Rapid application development with strong productivity tools
- Enterprise applications with large teams
- Applications where GC pauses are acceptable
- When you need extensive libraries and frameworks (.NET ecosystem)
- GUI applications (Windows Forms, WPF, MAUI)

**Resources for Learning:**

- [The Rust Programming Language](https://doc.rust-lang.org/book/) (The Book) - Official comprehensive guide
- [Rust by Example](https://doc.rust-lang.org/rust-by-example/) - Learn by examples
- [rustlings](https://github.com/rust-lang/rustlings) - Interactive exercises
- [Exercism Rust Track](https://exercism.org/tracks/rust) - Practice problems with mentoring
- [docs.rs](https://docs.rs/) - Crate documentation
- [The Cargo Book](https://doc.rust-lang.org/cargo/) - Package manager deep dive

For C# developers, I recommend reading The Rust Programming Language cover to cover. The ownership and borrowing chapters are critical. Don't skip them or skim them—these concepts are foreign to C# developers and require careful study.

The learning curve is steep, but the payoff is understanding a fundamentally different approach to memory safety and systems programming. Rust won't replace C# in most domains, but it opens up possibilities that C# simply can't provide without compromising on performance or safety guarantees.

After 20 years of C#, learning Rust has made me a better programmer in both languages. It's forced me to think more carefully about resource management, error handling, and API design. Whether you adopt Rust for production use or not, the concepts are worth learning for any serious software engineer.
