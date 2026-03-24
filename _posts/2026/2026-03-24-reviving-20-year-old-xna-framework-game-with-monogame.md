---
layout: post
title: Reviving an 20 year old XNA Framework game with MonoGame
date: 2026-03-24
author: Christian Helle
tags:
  - MonoGame
  - XNA
  - Copilot
  - Agents
  - .NET
redirect_from:
  - /2026/03/24/reviving-20-year-old-xna-framework-game-with-monogame
  - /2026/03/24/reviving-20-year-old-xna-framework-game-with-monogame/
  - /2026/03/reviving-20-year-old-xna-framework-game-with-monogame
  - /2026/03/reviving-20-year-old-xna-framework-game-with-monogame/
  - /2026/reviving-20-year-old-xna-framework-game-with-monogame
  - /2026/reviving-20-year-old-xna-framework-game-with-monogame/
  - /reviving-20-year-old-xna-framework-game-with-monogame
  - /reviving-20-year-old-xna-framework-game-with-monogame/
---

I recently revived [Chris' Puzzle Game](https://github.com/christianhelle/xnapuzzlegame), an old sliding puzzle game I originally built with XNA for Windows Phone. The result is a modern [MonoGame port](https://github.com/christianhelle/puzzlegame-mono) that targets current versions of .NET and runs as a desktop application on Windows.

What made this project especially interesting is that I did not port it the traditional way. This was a pure agentic engineering exercise using GitHub Copilot CLI together with [Squad](https://github.com/christianhelle/blog/tree/master/.squad). I gave the agents a goal, pointed them at the original repository, and let them work through the migration with the old game as the behavior reference.

This post is a walk through of that rewrite: what the original XNA code looked like, what changed in the MonoGame version, how the gameplay logic was modernized, and why this was an ideal kind of project for agentic engineering.

## The prompt

The prompt I used was intentionally short:

```text
Build a MonoGame port of my old XNA Framework based game for Windows Phone called Chris' Puzzle Game. My old puzzle game is open source and the code is available at https://github.com/christianhelle/xnapuzzlegame
```

That is not much of a specification, but it contains the most important part: a concrete reference implementation.

The original repository gave the agents a playable target to aim for. That meant the job was not to invent a puzzle game from scratch. The job was to preserve the feel of an existing game while replacing the platform underneath it. In practice that meant keeping the screen flow, puzzle behavior, preview mode, and overall structure intact while adapting input, window management, persistence, and the build pipeline for modern desktop .NET.

## Revisiting the original XNA codebase

The original XNA repository is more complete than the very first prototype I wrote about in my old post [Writing a puzzle game for Windows Phone 7](https://christianhelle.com/2010/09/writing-puzzle-game-for-windows-phone-7.html). The open source version includes a proper screen manager, a main menu, credits, preview mode, in-game options, and save/resume support through isolated storage.

At the top level, the startup flow is very XNA-era in spirit: create the graphics device, attach a screen manager component, and then restore the previous screen stack or fall back to the main menu.

```csharp
public PuzzleGame()
{
    Content.RootDirectory = "Content";

    graphics = new GraphicsDeviceManager(this);
    TargetElapsedTime = TimeSpan.FromTicks(333333);

    screenManager = new ScreenManager(this);
    Components.Add(screenManager);

#if WINDOWS_PHONE
    if (!screenManager.DeserializeState())
    {
        screenManager.AddScreen(GameScreenFactory.Create<BackgroundScreen>(), null);
        screenManager.AddScreen(GameScreenFactory.Create<MainMenuScreen>(), null);
    }
#else
    screenManager.AddScreen(GameScreenFactory.Create<BackgroundScreen>(), null);
    screenManager.AddScreen(GameScreenFactory.Create<MainMenuScreen>(), null);
#endif
}
```

The main menu itself was simple and direct, which is exactly what made it easy to preserve in the port:

```csharp
public MainMenuScreen()
    : base("Chris' Puzzle Game")
{
    var playGameMenuEntry = new MenuEntry("New Game");
    var aboutMenuEntry = new MenuEntry("About");
    var exitMenuEntry = new MenuEntry("Exit");

    playGameMenuEntry.Selected += PlayGameMenuEntrySelected;
    aboutMenuEntry.Selected += AboutMenuEntrySelected;
    exitMenuEntry.Selected += OnCancel;

    MenuEntries.Add(playGameMenuEntry);
    MenuEntries.Add(aboutMenuEntry);
    MenuEntries.Add(exitMenuEntry);
}
```

Most of the interesting logic lived in `GameplayScreen`. That class was responsible for almost everything: loading the puzzle image, slicing it into tiles, scrambling the board, reading input, tracking time, checking for completion, drawing the puzzle, and serializing progress. It worked, but it also mixed game rules, rendering, and persistence into one large screen class.

That combination is very typical of small XNA projects. It is also exactly the kind of codebase that benefits from a rewrite rather than a literal line-by-line port.

## Moving from XNA and Windows Phone to MonoGame and .NET 9

One of the first changes in the MonoGame port was simply choosing a modern runtime and packaging model. Instead of an XNA project targeting Windows Phone, the new project targets `net9.0-windows` and references the MonoGame desktop framework directly.

```xml
<PropertyGroup>
  <OutputType>WinExe</OutputType>
  <TargetFramework>net9.0-windows</TargetFramework>
  <UseWindowsForms>true</UseWindowsForms>
  <ApplicationHighDpiMode>PerMonitorV2</ApplicationHighDpiMode>
  <Nullable>enable</Nullable>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="MonoGame.Framework.WindowsDX" Version="3.8.*"/>
</ItemGroup>
```

That change alone immediately moves the game from the XNA/Windows Phone world into something that can be built with current .NET tooling. The port also wires the MonoGame content pipeline into MSBuild so content is rebuilt as part of the project build:

```xml
<Target Name="BuildMonoGameContent"
        Condition="'$(DesignTimeBuild)' != 'true'"
        DependsOnTargets="RestoreMonoGameTools"
        BeforeTargets="CopyFilesToOutputDirectory">
  <Exec WorkingDirectory="$(MSBuildProjectDirectory)\Content"
        Command="dotnet mgcb /@:Content.mgcb /rebuild /quiet"
        StandardOutputImportance="Low"/>
</Target>
```

The new startup code still feels recognizably like the original game, but it is now clearly desktop-first:

```csharp
public PuzzleGame()
{
    Content.RootDirectory = "Content";
    gameplayPersistence = new();

    _ = new GraphicsDeviceManager(this)
    {
        PreferredBackBufferWidth = 1200,
        PreferredBackBufferHeight = 720,
        SynchronizeWithVerticalRetrace = true,
    };

    TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
    IsMouseVisible = true;
    Window.AllowUserResizing = true;
    Window.Title = "Chris' Puzzle Game";

    screenManager = new(this);
    Components.Add(screenManager);
    Exiting += HandleGameExiting;

    ConfigureStartupScreens();
}
```

The shape is familiar, but the assumptions are different. The port is not pretending to be a phone game anymore. It is a Windows desktop game with a resizable window, visible mouse cursor, high DPI support, and an explicit persistence service that saves state on exit.

## Extracting the puzzle rules into a real board model

One of the most useful improvements in the rewrite was separating the board logic from the screen class.

In the original `GameplayScreen`, the puzzle board was represented indirectly through dictionaries of textures and scrambled pieces. The screen owned the tile arrangement, input queue, timing, movement rules, and drawing logic all in one place. The MonoGame port instead introduces a dedicated `PuzzleBoard` type that focuses only on board state and valid moves:

```csharp
internal sealed class PuzzleBoard
{
    private readonly int[] tiles;

    public PuzzleBoard(int size = 4)
    {
        Size = size;
        tiles = new int[size * size];
        Reset();
    }

    public int Size { get; }
    public int BlankIndex { get; private set; }

    public void Shuffle(int moveCount, Random random)
    {
        Reset();
        // Shuffle by legal moves so the board always remains solvable.
    }

    public bool TryMovePosition(int positionIndex)
    {
        // Only move tiles adjacent to the blank slot.
    }

    public void Restore(int[] tileState)
    {
        // Validate length, duplicates, blank tile, and solvability.
    }
}
```

This is a much better shape for a long-lived codebase. The board can validate solvability, restore saved state safely, and expose the current tile layout without knowing anything about textures, sprite batches, or screen transitions.

That small design change has a big effect. It makes the MonoGame `GameplayScreen` much easier to reason about because it no longer owns the game rules directly. Instead, it becomes the layer that translates keyboard and mouse input into board operations and then renders the current board state.
