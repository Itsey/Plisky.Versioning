# C4 Architectural Model: Plisky.Versioning & Versonify

This document provides a comprehensive C4 (Context, Container, Component, Code) architectural model for the **Plisky.Versioning** system and its CLI tool **Versonify**.

---

## 1. Level 1: System Context Diagram

The System Context diagram illustrates the boundary of the Plisky.Versioning software system, who interacts with it (human developers and automated build pipelines), and the external systems and targets it integrates with.

```mermaid
flowchart TD
    subgraph Users ["Actors & Stakeholders"]
        Dev["Developer<br/><i>[Person]</i><br/>Runs CLI commands locally to inspect, set, or override versions."]
        CI["CI/CD Automation & Pipelines<br/><i>[Software System]</i><br/>GitHub Actions, Azure DevOps, Nuke runners executing automated builds."]
    end

    subgraph SystemBoundary ["Plisky.Versioning System"]
        System["Plisky.Versioning & Versonify<br/><i>[Software System]</i><br/>Manages multi-digit semantic versioning, state persistence, and automated project/source file synchronization."]
    end

    subgraph ExternalSystems ["External Systems & Data Targets"]
        VersionStore["Version Storage<br/><i>[Storage / Remote Service]</i><br/>Persists version state either over HTTP or to a file system local or remote."]
        ProjectFiles["Solution & Source Files<br/><i>[Codebase Files]</i><br/>.csproj, AssemblyInfo.cs, .nuspec, .wxs, and tokenized text files."]
        CIEnv["CI/CD Environments<br/><i>[Environment Sinks]</i><br/>Azure DevOps Pipeline variables, OS environment variables, console/JSON output."]
    end

    Dev -->|"Runs CLI commands (passive, set, get, override)"| System
    CI -->|"Executes versioning & file updates (updatefiles, increment)"| System

    System -->|"Persists version state either over HTTP or to a file system (local or remote)"| VersionStore
    System -->|"Applies glob matching and updates version tokens/attributes"| ProjectFiles
    System -->|"Exports version variables (##vso commands, env vars, stdout)"| CIEnv
```

### Context Elements

| Element | Type | Description |
| :--- | :--- | :--- |
| **Developer** | Person | Software engineer maintaining version schemes, setting release names, or testing version increments locally. |
| **CI/CD Automation** | Software System | Automated orchestrators (Azure DevOps, GitHub Actions, Nuke Build) invoking versioning as a build/release step. |
| **Plisky.Versioning System** | Software System | The overall versioning solution, providing both CLI (`versonify`) and library APIs (`Plisky.Versioning`). |
| **Version Storage** | Storage / Remote Service | Persists version state either over HTTP (e.g. Sonatype Nexus) or to a file system (local disk or remote UNC share) ([`JsonVersionPersister`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/Providers/JsonVersionPersister.cs), [`NexusVersionPersister`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/Providers/NexusVersionPersister.cs)). |
| **Solution & Source Files** | File Target | Target files containing version tokens, assembly attributes, or XML elements ([`VersionFileUpdater`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/Updaters/VersionFileUpdater.cs)). |
| **CI/CD Environments** | External Sink | Consumers of version outputs such as Azure DevOps task variables (`##vso[task.setvariable]`), process environment variables, or console streams ([`VersioningOutputter`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/VersioningOutputter.cs)). |

---

## 2. Level 2: Container Diagram

The Container diagram depicts the high-level technology containers that comprise the system, their responsibilities, and how they communicate with external boundaries.

```mermaid
flowchart TD
    Dev["Developer"]
    CI["CI/CD Pipelines / Nuke Build"]

    subgraph PliskySystem ["Plisky.Versioning System Boundary"]
        CLI["Versonify CLI Tool<br/><i>[Container: .NET 8/9/10 Console Executable / Global Tool]</i><br/>Entry point for developers and automation. Parses arguments, dispatches commands, formats outputs."]
        CoreLib["Plisky.Versioning Library<br/><i>[Container: .NET 8/9/10 Class Library / NuGet]</i><br/>Domain engine managing multi-digit logic, behaviour rules, digit groups, persistence, and file updating."]
    end

    subgraph StorageContainers ["Data Stores & Remote Systems"]
        JSONStore[("JSON Version Store<br/><i>[Local / UNC File]</i><br/>Serialised CompleteVersion state")]
        NexusStore[("Nexus Raw Repository<br/><i>[Remote HTTP Service]</i><br/>REST-backed JSON version store")]
    end

    subgraph SolutionTargets ["Target Codebase Files"]
        KnownFiles["Known File Implementations ( Wix,csproj,nuspec )<br/><i>[Target Project/Manifest Files]</i><br/>.csproj, AssemblyInfo.cs, .nuspec, and .wxs files."]
        TextTokens["Text / Markdown Files<br/><i>[Tokens: XXX-VERSION-XXX, etc.]</i>"]
    end

    subgraph OutputTargets ["CI/CD Feedback (Json/Environment)"]
        Feedback["CI/CD Feedback<br/><i>[Pipeline & System Sinks]</i><br/>Azure DevOps task variables (##vso), process environment variables (PVER-*), and console/JSON streams (jcon)."]
    end

    Dev -->|"Executes versonify commands"| CLI
    CI -->|"Invokes CLI in pipeline steps"| CLI

    CLI -->|"Invokes facade & domain models"| CoreLib

    CoreLib -->|"Reads/writes JSON file via System.Text.Json"| JSONStore
    CoreLib -->|"HTTP GET/PUT/HEAD via HttpClient"| NexusStore
    CoreLib -->|"Updates XML / regex attributes in known project files"| KnownFiles
    CoreLib -->|"Replaces version placeholders"| TextTokens

    CLI -->|"Emits pipeline variables, environment variables, and console/JSON output"| Feedback
```

### Container Details

1. **Versonify CLI Tool ([`Versonify.csproj`](file:///X:/Code/ghub/Plisky.Versioning/src/Versonify/Versonify.csproj))**
   - **Type:** .NET Global Tool / Executable (`net8.0`, `net9.0`, `net10.0`).
   - **Role:** Command-line front-end. Parses CLI flags, validates arguments, triggers version lifecycle actions, formats outputs for human and automation agents (e.g. `jcon` JSON output for AI/orchestrators).
2. **Plisky.Versioning Class Library ([`Plisky.Versioning.csproj`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/Plisky.Versioning.csproj))**
   - **Type:** Reusable .NET Library (`net8.0`, `net9.0`, `net10.0`).
   - **Role:** Encapsulates the domain model ([`CompleteVersion`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/VersionNumbering/CompleteVersion.cs), [`VersionUnit`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/VersionNumbering/VersionUnit.cs)), persistence abstraction ([`VersionStorage`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/VersionStorage.cs)), glob search and file manipulation engine ([`VersionFileUpdater`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/Updaters/VersionFileUpdater.cs)), and output routing ([`VersioningOutputter`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/VersioningOutputter.cs)).
3. **Versonify.build Automation ([`Build.cs`](file:///X:/Code/ghub/Plisky.Versioning/src/Versonify.build/Build.cs))**
   - **Type:** Nuke build application.
   - **Role:** Automates the CI/CD pipeline (Arrange, Construct, Examine, Package, Release) using Versonify to version the solution itself.

---

## 3. Level 3: Component Diagrams

### 3.1 Versonify CLI Components

This diagram details the internal structure of the `Versonify` CLI container.

```mermaid
flowchart TD
    subgraph CLIContainer ["Versonify CLI Container"]
        Program["Program<br/><i>[Static Class / Entry Point]</i><br/>Bootstraps execution, handles quick-returns (--QQpnf, --version, --help), dispatches actions."]
        Parser["CommandLineParser<br/><i>[Static Class]</i><br/>Configures System.CommandLine, handles aliases, case-insensitivity, and flags."]
        Validator["ArgumentValidator<br/><i>[Static Class]</i><br/>Validates mutual exclusions (digit-group vs pre-release), paths, and option combinations."]
        Options["VersonifyOptions<br/><i>[Model Class]</i><br/>Calculates VersioningCommand and resolves OutputPossibilities flags."]
        ExecResult["ExecutionResult<br/><i>[Data Transfer Object]</i><br/>Tracks success status, error messages, and process exit code."]
        OutputHub["Hub & Event Router<br/><i>[Plisky.Plumbing.Hub]</i><br/>Internal in-memory message bus routing SimpleMessage instances."]
        JsonMsg["JsonOutputMessage<br/><i>[DTO / Serializer]</i><br/>Formats output into categorized JSON for AI and pipeline tools."]
        DiagConfig["DiagnosticsConfig<br/><i>[Config Class]</i><br/>Configures Bilge diagnostic listeners and trace levels."]
    end

    subgraph CoreLibBoundary ["Plisky.Versioning Core Library"]
        VersioningFacade["Versioning Facade"]
        StorageFactory["VersionStorage"]
    end

    CLIArgs["CLI Command Line Arguments"] --> Program
    Program -->|"Parses args"| Parser
    Parser -->|"Populates"| Options
    Program -->|"Validates"| Validator
    Validator -->|"Checks"| Options
    Program -->|"Configures tracing"| DiagConfig
    Program -->|"Initializes version store"| StorageFactory
    Program -->|"Executes actions"| VersioningFacade
    Program -->|"Publishes messages"| OutputHub
    OutputHub -->|"Formats JSON when -o jcon"| JsonMsg
    Program -->|"Populates status"| ExecResult
```

### 3.2 Plisky.Versioning Core Library Components

This diagram details the internal components of the `Plisky.Versioning` library container.

```mermaid
flowchart TD
    subgraph CoreLibrary ["Plisky.Versioning Core Library"]
        Facade["Versioning<br/><i>[Facade / Coordinator]</i><br/>High-level API for incrementing, searching files, and triggering updaters."]
        
        subgraph DomainModel ["Version Domain Model"]
            CV["CompleteVersion<br/><i>[Aggregate Root]</i><br/>Manages Digits[], digit groups, release name, and display formats."]
            VU["VersionUnit<br/><i>[Entity]</i><br/>Represents single digit (value, prefix, behaviour, group, pending override)."]
            DIB["DigitIncrementBehaviour<br/><i>[Enum]</i><br/>Fixed, AutoIncrementWithReset, AutoIncrementWithResetAny, Daily, DaysSinceDate, etc."]
            DT["DisplayType<br/><i>[Enum]</i><br/>Full, Short, ThreeDigitNumeric, FourDigitNumeric, QueuedFull, etc."]
        end

        subgraph StorageSubsystem ["Storage & Persistence Subsystem"]
            VS["VersionStorage<br/><i>[Abstract Base Class]</i><br/>Defines GetVersion(), Persist(), and factory CreateFromInitialisation()."]
            JVP["JsonVersionPersister<br/><i>[Concrete Storage]</i><br/>Local / UNC file persistence using System.Text.Json."]
            NVP["NexusVersionPersister<br/><i>[Concrete Storage]</i><br/>Nexus raw repository persister over HTTP."]
            NexusSupp["NexusSupport<br/><i>[HTTP Client / Integration]</i><br/>Performs HTTP GET, PUT, and HEAD calls against Nexus raw repos."]
        end

        subgraph FileUpdatingSubsystem ["File Updating Subsystem"]
            VFU["VersionFileUpdater<br/><i>[Engine]</i><br/>Updates CSProj XML, AssemblyInfo regex, WiX, Nuspec, and text tokens."]
            DryRun["DryRunVersionFileUpdater<br/><i>[Simulation Engine]</i><br/>Logs update actions without modifying disk contents."]
            FUT["FileUpdateType<br/><i>[Enum]</i><br/>Nuspec, TextFile, StdAssembly, NetAssembly, Wix, etc."]
        end

        subgraph OutputSubsystem ["Output & Notification Subsystem"]
            VO["VersioningOutputter<br/><i>[Output Engine]</i><br/>Generates formatted outputs for console, files, environment, and AzDo."]
            OP["OutputPossibilities<br/><i>[Flags Enum]</i><br/>Console, Json, File, Environment, NukeFusion, PliskyFusion."]
        end
    end

    Facade -->|"Coordinates"| CV
    CV -->|"Contains digits (1..*)"| VU
    VU -->|"Governed by"| DIB
    CV -->|"Formats using"| DT

    Facade -->|"Uses"| VS
    JVP -->|"Extends"| VS
    NVP -->|"Extends"| VS
    NVP -->|"Delegates to"| NexusSupp

    Facade -->|"Triggers"| VFU
    DryRun -->|"Extends"| VFU
    VFU -->|"Processes"| FUT

    Facade -.->|"Can output via"| VO
    VO -->|"Configured by"| OP
```

### Component Responsibilities & Mappings

| Component | File / Symbol | Key Responsibilities |
| :--- | :--- | :--- |
| **`Program`** | [`Program.cs`](file:///X:/Code/ghub/Plisky.Versioning/src/Versonify/Program.cs) | CLI execution flow, command dispatching (`updatefiles`, `passive`, `set`, `get`, `behaviour`, etc.), exit code handling. |
| **`CommandLineParser`** | [`CommandLineParser.cs`](file:///X:/Code/ghub/Plisky.Versioning/src/Versonify/CommandLineParser.cs) | Builds root commands via `System.CommandLine`, maps deprecated uppercase/short aliases, parses tokens into options. |
| **`ArgumentValidator`** | [`ArgumentValidator.cs`](file:///X:/Code/ghub/Plisky.Versioning/src/Versonify/ArgumentValidator.cs) | Validates argument safety (checks directories, `--digit-group` constraints, mutual exclusion with `--pre-release`). |
| **`VersonifyOptions`** | [`VersonifyOptions.cs`](file:///X:/Code/ghub/Plisky.Versioning/src/Versonify/VersonifyOptions.cs) | Holds configuration parameters, parses `-o` output options (`con`, `jcon`, `azdo`, `env`, `file`, `-nf`). |
| **`Versioning`** | [`Versioning.cs`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/VersionNumbering/Versioning.cs) | Central facade orchestrating version loading, incrementing, glob-based file searching, updating, and saving. |
| **`CompleteVersion`** | [`CompleteVersion.cs`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/VersionNumbering/CompleteVersion.cs) | Aggregate root of the version domain. Manages digits array, digit groups (`default`, `pre-release`), release names, and pending pattern overrides (`+.-..`). |
| **`VersionUnit`** | [`VersionUnit.cs`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/VersionNumbering/VersionUnit.cs) | Encapsulates single digit state, prefix separator, increment behaviour, group assignment, and overflow prevention. |
| **`VersionStorage`** | [`VersionStorage.cs`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/VersionStorage.cs) | Persistence abstraction. Factory `CreateFromInitialisation()` auto-detects `[NEXUS]` prefix vs file paths. |
| **`JsonVersionPersister`** | [`JsonVersionPersister.cs`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/Providers/JsonVersionPersister.cs) | Local/UNC file storage implementation using `System.Text.Json`. |
| **`NexusVersionPersister`** | [`NexusVersionPersister.cs`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/Providers/NexusVersionPersister.cs) | Sonatype Nexus repository storage implementation using HTTP upload/download. |
| **`VersionFileUpdater`** | [`VersionFileUpdater.cs`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/Updaters/VersionFileUpdater.cs) | Modifies target files on disk. Handles XML manipulation for `.csproj`/`.nuspec`/WiX, regex replacement for `AssemblyInfo.cs`, and token replacement (`XXX-VERSION-XXX`) for text files. |
| **`VersioningOutputter`** | [`VersioningOutputter.cs`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/VersioningOutputter.cs) | Writes version numbers to external destinations: console, text files, environment variables (`PVER-LATEST`, `PVER-RELEASE`), or Azure DevOps logging commands (`##vso[...]`). |

---

## 4. Level 4: Code & Implementation Models

### 4.1 Domain Model Class Diagram

```mermaid
classDiagram
    class Versioning {
        -Bilge b
        -CompleteVersion cv
        -VersionStorage repo
        -VersionFileUpdater vfu
        -bool testMode
        +string FileUpdateDisplayGroups
        +CompleteVersion Version
        +GetVersion() string
        +Increment(string newReleaseName) void
        +LoadMiniMatches(string[] srcFile) void
        +SearchForAllFiles(string root) List~string~
        +UpdateAllRegisteredFiles() int
        +SaveUpdatedVersion() void
        +UpdateBehaviour(string digit, DigitIncrementBehaviour beh) void
    }

    class CompleteVersion {
        -Bilge b
        +VersionUnit[] Digits
        +string ReleaseName
        +bool IsDefault
        +Dictionary~FileUpdateType, DisplayType~ DisplayTypes
        +GetDefault() CompleteVersion
        +Increment() void
        +IncrementByGroup(string groupNames) void
        +ApplyPendingVersion(string pattern) void
        +ApplyBehaviourUpdate(string digit, DigitIncrementBehaviour beh) void
        +GetVersionString(DisplayType dt) string
        +GetVersionStringByGroup(string groupNames) string
        +GetDigitsByGroup(string groupNames) int[]
    }

    class VersionUnit {
        -string actualValue
        +string PreFix
        +DigitIncrementBehaviour Behaviour
        +string GroupName
        +string IncrementOverride
        +Value string
        +PerformIncrement(bool higherChanged, bool anyHigherChanged, DateTime lastBuild, DateTime baseDate) bool
        +SetBehaviour(DigitIncrementBehaviour beh) void
        +ToString() string
    }

    class DigitIncrementBehaviour {
        <<enumeration>>
        Fixed = 0
        DaysSinceDate = 2
        DailyAutoIncrement = 3
        AutoIncrementWithReset = 4
        AutoIncrementWithResetAny = 5
        ContinualIncrement = 6
        WeeksSinceDate = 7
        ReleaseName = 8
    }

    class DisplayType {
        <<enumeration>>
        Default = 0
        Short = 1
        Full = 2
        NoDisplay = 3
        ThreeDigit = 4
        Release = 5
        FourDigitNumeric = 6
        ThreeDigitNumeric = 7
        QueuedFull = 8
        FourDigit = 9
    }

    class VersionStorage {
        <<abstract>>
        #VersionStorageOptions InitValue
        +bool IsValid
        +string StorageFailureMessage
        +CreateFromInitialisation(string vpv) VersionStorage$
        +DoesVstoreExist() bool
        +GetVersion() CompleteVersion
        +Persist(CompleteVersion cv) void
        #ActualLoad()* CompleteVersion
        #ActualPersist(CompleteVersion cv)* void
    }

    class JsonVersionPersister {
        +IsValidFileName(string fileName) bool
        #ActualLoad() CompleteVersion
        #ActualPersist(CompleteVersion cv) void
    }

    class NexusVersionPersister {
        -NexusConfig config
        -NexusSupport remote
        #ActualLoad() CompleteVersion
        #ActualPersist(CompleteVersion cv) void
    }

    class VersionFileUpdater {
        -CompleteVersion cv
        +PerformUpdate(string fl, FileUpdateType fut, DisplayType dt, string groupNames) string
        #UpdateStdCSPRoj(string fl, string version, string tag) void
        #UpdateCSFileWithAttribute(string fl, string attr, string version) void
        #UpdateNuspecFile(string fl, string version) void
        #UpdateWixFile(string fl, string version) void
        #UpdateLiteralReplacer(string fl, CompleteVersion cv, DisplayType dt, DisplayType overrideDt, string groups) string
    }

    class DryRunVersionFileUpdater {
        +PerformUpdate(string fl, FileUpdateType fut, DisplayType dt, string groupNames) string
    }

    class VersioningOutputter {
        -CompleteVersion versionToLog
        -Hub outputRouter
        +string[] Digits
        +string PassiveOutputOverride
        +string PverFileName
        +bool ReleaseRequested
        +DoOutput(OutputPossibilities oo, VersioningCommand command) void
        #SetEnvironmentWithValue() void
        #SetFileValue(string output) void
        #WriteToConsole(string output) void
    }

    Versioning --> CompleteVersion : coordinates
    Versioning --> VersionStorage : persists via
    Versioning --> VersionFileUpdater : updates files via
    CompleteVersion *-- VersionUnit : contains
    VersionUnit --> DigitIncrementBehaviour : guided by
    CompleteVersion --> DisplayType : formats with
    VersionStorage <|-- JsonVersionPersister : implements
    VersionStorage <|-- NexusVersionPersister : implements
    VersionFileUpdater <|-- DryRunVersionFileUpdater : specialises
    Versioning ..> VersioningOutputter : renders via
```

---

### 4.2 Sequence Diagram: `updatefiles` Command Execution Workflow

The sequence below illustrates the end-to-end execution path when a CI build or developer runs `versonify updatefiles -i -v .version -m "**/*.csproj|StdAssembly" --root .`:

```mermaid
sequenceDiagram
    autonumber
    actor Caller as Developer / CI Pipeline
    participant CLI as Versonify::Program
    participant Parser as CommandLineParser & Options
    participant Facade as Versioning
    participant Storage as JsonVersionPersister
    participant Domain as CompleteVersion
    participant Updater as VersionFileUpdater
    participant Outputter as VersioningOutputter

    Caller->>CLI: versonify updatefiles -i -v .version -m "**/*.csproj|StdAssembly" --root .
    CLI->>Parser: Parse(args)
    Parser-->>CLI: VersonifyOptions (UpdateFiles, Increment=true, MinMatch, Root)
    
    CLI->>Storage: VersionStorage.CreateFromInitialisation(".version")
    Storage-->>CLI: JsonVersionPersister instance
    
    CLI->>Facade: new Versioning(storage, dryRun=false)
    Facade->>Storage: GetVersion()
    Storage->>Storage: ActualLoad() (read .version JSON)
    Storage-->>Facade: CompleteVersion cv
    
    CLI->>Facade: LoadMiniMatches(minMatchPatterns)
    CLI->>Facade: Increment()
    Facade->>Domain: Increment()
    Domain->>Domain: Evaluate digit behaviours & overrides
    
    CLI->>Facade: SearchForAllFiles(Root)
    Facade->>Facade: Directory.EnumerateFiles() + Glob matching
    Facade-->>CLI: Matched file list
    
    CLI->>Facade: UpdateAllRegisteredFiles()
    loop For each matched file
        Facade->>Updater: PerformUpdate(filePath, FileUpdateType.StdAssembly)
        Updater->>Updater: Load XML & modify <AssemblyVersion> tag
        Updater->>Updater: Save modified file to disk
        Updater-->>Facade: Update status log
    end
    
    CLI->>Facade: SaveUpdatedVersion()
    Facade->>Storage: Persist(CompleteVersion)
    Storage->>Storage: ActualPersist() (write updated JSON to .version)
    
    CLI->>Outputter: DoOutput(OutputPossibilities, UpdateFiles)
    Outputter-->>Caller: Exit code 0 + console output / AzDo variable set
```

---

## 5. Architectural Summary & Cross-Cutting Concerns

1. **Pluggable Persistence Architecture:**
   - Persistence is fully decoupled via [`VersionStorage`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/VersionStorage.cs). Local files ([`JsonVersionPersister`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/Providers/JsonVersionPersister.cs)) and remote HTTP endpoints ([`NexusVersionPersister`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/Providers/NexusVersionPersister.cs)) are supported transparently.
2. **Deterministic Multi-Digit Behaviours:**
   - Each digit in [`CompleteVersion`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/VersionNumbering/CompleteVersion.cs) operates under independent rules ([`DigitIncrementBehaviour`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/VersionNumbering/DigitIncrementBehaviour.cs)), accommodating SemVer, Julian date versioning, build-counter increments, and pre-release label tagging.
3. **Multi-Target File Updating:**
   - [`VersionFileUpdater`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/Updaters/VersionFileUpdater.cs) abstracts format-specific updates across SDK-style `.csproj`, legacy .NET Framework `AssemblyInfo.cs`, NuSpec package manifests, WiX installers, and tokenized text files.
4. **Safety & Simulation (DryRun Mode):**
   - The `--dry-run` flag activates [`DryRunVersionFileUpdater`](file:///X:/Code/ghub/Plisky.Versioning/src/Plisky.Versioning/Updaters/DryRunVersionFileUpdater.cs), computing and logging version modifications without mutating files on disk or changing persistent storage.
5. **Observability & Diagnostics:**
   - Tracing is instrumented throughout the codebase using `Plisky.Diagnostics.Bilge`, configurable via `--trace` and `--debug` CLI flags.
