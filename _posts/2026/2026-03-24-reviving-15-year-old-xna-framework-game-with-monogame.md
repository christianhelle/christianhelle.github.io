---
layout: post
title: Reviving a 15 year old XNA Framework game with MonoGame
date: 2026-03-24
author: Christian Helle
tags:
  - MonoGame
  - XNA
  - Copilot
  - Agents
  - .NET
redirect_from:
  - /2026/03/24/reviving-15-year-old-xna-framework-game-with-monogame
  - /2026/03/24/reviving-15-year-old-xna-framework-game-with-monogame/
  - /2026/03/reviving-15-year-old-xna-framework-game-with-monogame
  - /2026/03/reviving-15-year-old-xna-framework-game-with-monogame/
  - /2026/reviving-15-year-old-xna-framework-game-with-monogame
  - /2026/reviving-15-year-old-xna-framework-game-with-monogame/
  - /reviving-15-year-old-xna-framework-game-with-monogame
  - /reviving-15-year-old-xna-framework-game-with-monogame/
---

I recently revived [Chris' Puzzle Game](https://github.com/christianhelle/xnapuzzlegame), an old sliding puzzle game I originally built with XNA for Windows Phone. The result is a modern [MonoGame port](https://github.com/christianhelle/puzzlegame-mono) that targets current versions of .NET and runs as a desktop application on Windows.

What made this project especially interesting is that I did not port it the traditional way. This was a pure agentic engineering exercise using GitHub Copilot CLI together with [Squad](https://github.com/christianhelle/blog/tree/master/.squad). I gave the agents a goal, pointed them at the original repository, and let them work through the migration with the old game as the behavior reference.

This post is a walkthrough of that rewrite: what the original XNA code looked like, what changed in the MonoGame version, how the gameplay logic was modernized, and why this was an ideal kind of project for agentic engineering.

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
        var previousBlankIndex = -1;

        for (var move = 0; move < moveCount; move++)
        {
            var neighbors = GetNeighborIndexes(BlankIndex);
            if (previousBlankIndex >= 0 && neighbors.Count > 1)
            {
                neighbors.Remove(previousBlankIndex);
            }

            var nextBlankIndex = neighbors[random.Next(neighbors.Count)];
            previousBlankIndex = BlankIndex;
            Swap(BlankIndex, nextBlankIndex);
            BlankIndex = nextBlankIndex;
        }
    }
}
```

This is a much better shape for a long-lived codebase. The board can guarantee that shuffles stay solvable, validate restored state, and expose the current tile layout without knowing anything about textures, sprite batches, or screen transitions.

That small design change has a big effect. It makes the MonoGame `GameplayScreen` much easier to reason about because it no longer owns the game rules directly. Instead, it becomes the layer that translates keyboard and mouse input into board operations and then renders the current board state.

## Reworking the UI for desktop input

The biggest behavioral difference between the original game and the port is not the puzzle itself. It is the environment around it.

The Windows Phone version naturally centered around phone-era assumptions. The MonoGame port had to feel good on a desktop with a mouse, a keyboard, a resizable window, and varying screen sizes. That shows up very clearly in the new menu infrastructure.

The base `MenuScreen` now understands keyboard navigation, Tab navigation, mouse wheel scrolling, hover selection, left click selection, and right click cancel behavior:

```csharp
public override void HandleInput(InputState input)
{
    var hoveredEntryIndex = GetHoveredEntryIndex(input);
    if (hoveredEntryIndex.HasValue)
        selectedEntry = hoveredEntryIndex.Value;

    if (input.IsMenuUp(ControllingPlayer) || input.MouseWheelDelta >= 120)
        selectedEntry = (selectedEntry - 1 + menuEntries.Count) % menuEntries.Count;
    else if (input.IsMenuDown(ControllingPlayer) || input.MouseWheelDelta <= -120)
        selectedEntry = (selectedEntry + 1) % menuEntries.Count;

    if (input.IsMenuSelect(ControllingPlayer, out var playerIndex))
        OnSelectEntry(selectedEntry, playerIndex);
    else if (input.IsNewLeftClick(ControllingPlayer, out playerIndex)
          && hoveredEntryIndex.HasValue)
        OnSelectEntry(hoveredEntryIndex.Value, playerIndex);
    else if (input.IsNewRightClick(ControllingPlayer, out playerIndex))
        OnCancel(playerIndex);
}
```

That is a much better fit for a desktop game than a straight preservation of the old input model. The menu also scales itself based on viewport size, uses safe area calculations, and draws a proper desktop-style panel chrome rather than assuming a fixed phone screen.

The gameplay screen follows the same philosophy. It preserves the original controls, but widens them for desktop use:

```csharp
public override void HandleInput(InputState input)
{
    if (input.IsPauseGame(ControllingPlayer))
    {
        ShowOptions(playerIndex);
        return;
    }

    if (input.IsNewKeyPress(Keys.R, ControllingPlayer, out _)
     || input.IsNewKeyPress(Keys.F5, ControllingPlayer, out _))
    {
        ScrambleBoard();
        return;
    }

    if (!input.IsNewLeftClick(ControllingPlayer, out _))
    {
        return;
    }

    if (TryMoveFromMouse(input.MousePosition))
    {
        moveCount++;
    }
}
```

On top of that, the port keeps the live preview mechanic from the old desktop build: holding `Enter` or `F1` shows the solved image. That is a nice example of the project’s overall approach. The game did not need to become a different puzzle game. It just needed a better shell around the same core behavior.

## Persistence without Windows Phone isolated storage

The original game persisted progress using `IsolatedStorageFile` and XML serialization. That made perfect sense on Windows Phone:

```csharp
public static void Save(SaveState state)
{
    using (var userStore = IsolatedStorageFile.GetUserStoreForApplication())
    {
        if (userStore.FileExists(FILENAME))
            userStore.DeleteFile(FILENAME);

        using (var stream = new IsolatedStorageFileStream(FILENAME, FileMode.OpenOrCreate, userStore))
        {
            var serializer = new XmlSerializer(typeof(SaveState));
            serializer.Serialize(stream, state);
        }
    }
}
```

The MonoGame port keeps the resume-on-exit experience, but modernizes the implementation. Instead of serializing the entire screen stack, it persists the active gameplay state to `%LocalAppData%`, writes a simple versioned binary header, and uses a temporary file before replacing the real save file:

```csharp
using (var stream = File.Open(
    temporarySaveFilePath,
    FileMode.Create,
    FileAccess.Write,
    FileShare.None))
{
    WriteSaveFileHeader(stream);
    gameplayScreen.Serialize(stream);
}

File.Move(temporarySaveFilePath, saveFilePath, overwrite: true);
```

I like this design a lot more than the original one.

It is explicit about what gets saved, it is versioned, it validates the header before restoring, and it cleans up corrupted save artifacts when loading fails. The `GameplayScreen` also tracks which flow was active when the game exited, so it can resume not just the board state but also whether the player was in the pause/options flow or the win flow.

That is a great example of a rewrite preserving behavior while still making the implementation significantly better.

## Using GitHub Copilot CLI and Squad to do the rewrite

The most interesting part of this project was not MonoGame itself. It was the process.

This entire port was built through agentic engineering with GitHub Copilot CLI and Squad. The MonoGame repository is not just source code. It also contains the team structure and working agreements used to build it.

The primary implementation team is described directly in `.squad/team.md`:

```markdown
| Name    | Role         |
| ------- | ------------ |
| Keaton  | Lead         |
| McManus | Gameplay Dev |
| Fenster | Platform Dev |
| Hockney | UI Dev       |
| Redfoot | Tester       |
```

And the decision log captured the working style explicitly:

```markdown
### 2026-03-24: Preserve progress with small logical commits

**What:** During implementation, commit frequently in small, coherent groups so the repo keeps a detailed progress history.
**Why:** Incremental MonoGame porting will be easier to review, debug, and compare against the original XNA game when each change stays tightly scoped.
```

That matters because this was not a one-shot “translate this file” exercise. The agents had to:

- study the original XNA repository
- preserve the gameplay and screen flow
- choose modern .NET and MonoGame project structure
- adapt the UI to desktop input
- rebuild persistence for a different platform
- keep the work organized in small, reviewable commits

This is exactly the kind of task where agentic engineering shines.

There is a clear goal, a concrete reference implementation, and a bounded feature set. The old repository provides behavior and assets. The new repository provides the target platform and room for structural improvement. That combination lets agents do much more than boilerplate generation. They can reason about equivalence, identify where a literal port would be awkward, and reshape the code into something that fits the modern platform better.

In other words, the prompt was small, but the context was rich.

## What stayed the same

Although the implementation changed a lot, the identity of the game did not.

The MonoGame port keeps the parts that made the original game feel like Chris' Puzzle Game:

- the same 4x4 sliding tile puzzle format
- the same set of sample puzzle images
- the same main menu to gameplay to completion flow
- the same quick preview mechanic
- the same reshuffle behavior
- the same ability to resume progress on the next launch

What changed was everything around those core behaviors. The code is cleaner, the persistence story is stronger, the UI is more natural on a desktop, and the project can be built and maintained with modern tooling.

That is the best kind of rewrite. The spirit stays the same while the codebase stops fighting the platform.

## Why MonoGame was the right choice

MonoGame is a very natural destination for old XNA projects. The conceptual model is close enough that the original structure still makes sense, but the tooling and runtime are current enough that the project can live comfortably in a modern .NET ecosystem.

For this game, MonoGame was exactly the right level of change.

I did not need a full engine migration to Unity or Godot. I did not need to reinvent the gameplay loop. I just needed a runtime and framework that still respected the XNA way of building games while letting the code run on current Windows and current .NET.

That made it possible to focus the rewrite on the places where modernization actually mattered: board logic, input handling, persistence, windowing, and project structure.

## Final thoughts

Reviving an old XNA game this way was a lot of fun, but it was also a very practical demonstration of how useful agentic engineering has become.

This was not just “AI wrote some helper methods.” The agents were able to use the old repository as a reference, preserve the important gameplay behavior, improve the internal design, and ship a modern MonoGame port that still feels like the same game.

That is a very different experience from the old chat-driven copy/paste workflow. It feels much closer to delegation: define the outcome, provide the reference implementation, set constraints, and let the agents work through the migration.

If you want to compare the two versions yourself, the original XNA source code is here:

[https://github.com/christianhelle/xnapuzzlegame](https://github.com/christianhelle/xnapuzzlegame)

And the MonoGame port is here:

[https://github.com/christianhelle/puzzlegame-mono](https://github.com/christianhelle/puzzlegame-mono)

For me, this project was a nice reminder that old software does not always need to stay old. Sometimes all it needs is a modern framework, a good reference codebase, and an agent team that knows how to carry the intent forward.
