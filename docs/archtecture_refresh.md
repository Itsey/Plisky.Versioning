# Objective: Generate a C4 Architecture Model for Plisky.Versioning & Versonify

You are acting as a Principal Software Architect. Your task is to inspect the codebase at `src/` and generate a complete, publication-ready **C4 Architectural Model** (Context, Container, Component, and Code) formatted in GitHub-flavored Markdown.

---

### Step 1: Codebase Discovery & Verification

Before generating the model, inspect the codebase to establish architectural boundaries:
1. **Core Projects:**
   - `Plisky.Versioning` (`src/Plisky.Versioning/Plisky.Versioning.csproj`): Core domain class library (`net8.0; net9.0; net10.0`).
   - `Versonify` (`src/Versonify/Versonify.csproj`): CLI application and global tool packaging.
   - `Versonify.build` (`src/Versonify.build/versonify.build.csproj`): Nuke-based pipeline build automation.
2. **Key Domain Concepts:**
   - Multi-digit version abstraction (`CompleteVersion.cs`, `VersionUnit.cs`).
   - Digit increment rules (`DigitIncrementBehaviour.cs`) and display formats (`DisplayType.cs`).
   - Storage abstraction (`VersionStorage.cs`) supporting local/UNC JSON (`JsonVersionPersister.cs`) and Sonatype Nexus HTTP (`NexusVersionPersister.cs`).
   - File updating engine (`VersionFileUpdater.cs`) supporting XML tags, Regex attributes, and literal token replacements (`XXX-VERSION-XXX`).
   - Diagnostic tracing via `Plisky.Diagnostics.Bilge` and message dispatching via `Plisky.Plumbing.Hub`.
3. **Guardrails & Architectural Exclusions:**
   - **Do NOT** include `VersioningTask` as an independent container or MSBuild task. It is a legacy/retired helper class, not a standalone deployable container.

---

### Step 2: C4 Document Structure & Design Specifications

Generate the architectural document with the following four levels:

#### Level 1: System Context Diagram
- **Actors:**
  - `Developer`: Runs CLI commands (`passive`, `set`, `get`, `override`) locally.
  - `CI/CD Automation`: GitHub Actions, Azure DevOps, or Nuke executing build pipelines (`updatefiles`, `increment`).
- **Core System Boundary:**
  - `Plisky.Versioning & Versonify`: Unified version management and synchronization engine.
- **External Systems (Consolidated):**
  - **`Version Storage`** (MUST be a single consolidated element): Represents state persistence either over HTTP (e.g. Sonatype Nexus) or to local/remote (UNC) file systems.
  - **`Solution & Source Files`**: Codebase target files (.csproj, AssemblyInfo.cs, .nuspec, .wxs, markdown/text).
  - **`CI/CD Environments`**: Downstream consumers of outputs (Azure DevOps task variables, process environment variables, stdout).
- Provide a summary table detailing Context element types and responsibilities.

#### Level 2: Container Diagram
- **System Containers:**
  - `Versonify CLI Tool`: .NET console app / global tool; argument parsing, command dispatch, structured output formatting.
  - `Plisky.Versioning Library`: .NET class library containing the core domain model, persistence providers, and file updaters.
  - `Versonify.build Automation`: Nuke build application orchestrating Arrange, Construct, Examine, Package, Release.
- **Storage Subsystem:**
  - `JSON Version Store` (local/UNC file) and `Nexus Raw Repository` (remote HTTP service).
- **Target Files (Consolidated):**
  - **`Known File Implementations ( Wix,csproj,nuspec )`** (MUST be consolidated into a single element): Represents known project/manifest formats (.csproj, AssemblyInfo.cs, .nuspec, .wxs).
  - **`Text / Markdown Files`**: Generic token replacements (`XXX-VERSION-XXX`, etc.).
- **Downstream Sinks (Consolidated):**
  - **`CI/CD Feedback (Json/Environment)`** (MUST be consolidated into a single element): Covers Azure DevOps variables (`##vso`), environment variables (`PVER-LATEST`, `PVER-RELEASE`), and plain/`jcon` console streams.

#### Level 3: Component Diagrams
Generate two component breakdowns:
1. **Versonify CLI Components:**
   - `Program` (entry point / orchestrator), `CommandLineParser` (System.CommandLine configuration), `ArgumentValidator` (constraint checks), `VersonifyOptions` (command and output flags), `Hub & Event Router` (in-memory bus), `JsonOutputMessage` (structured serializer), `DiagnosticsConfig` (Bilge tracing).
2. **Plisky.Versioning Core Library Components:**
   - Facade: `Versioning`.
   - Domain Model: `CompleteVersion`, `VersionUnit`, `DigitIncrementBehaviour`, `DisplayType`.
   - Storage Subsystem: `VersionStorage`, `JsonVersionPersister`, `NexusVersionPersister`, `NexusSupport`.
   - File Updating Subsystem: `VersionFileUpdater`, `DryRunVersionFileUpdater`, `FileUpdateType`.
   - Output Subsystem: `VersioningOutputter`, `OutputPossibilities`.
3. **Component Responsibilities Table:**
   - Create a markdown table mapping each component to its source file with clickable markdown links (`file:///...`) and key responsibilities.

#### Level 4: Code & Implementation Models
1. **Domain Class Diagram (`classDiagram`):**
   - Model the relationships: `Versioning` coordinates `CompleteVersion`, `VersionStorage`, and `VersionFileUpdater`. Show inheritance for persisters and updaters, and composition of `VersionUnit` inside `CompleteVersion`.
2. **Sequence Diagram (`sequenceDiagram`):**
   - Trace the end-to-end execution of `updatefiles -i -v .version -m "**/*.csproj|StdAssembly" --root .`:
     - CLI parse -> Storage load (`ActualLoad`) -> Facade increment -> Domain evaluation -> Glob search -> File updates -> Storage persist (`ActualPersist`) -> Output feedback.

#### Section 5: Architectural Summary & Cross-Cutting Concerns
- Document pluggable storage, deterministic multi-digit behaviours, multi-format file updaters, dry-run safety simulation, and diagnostics.

---

### Step 3: Mermaid Diagramming Syntax Rules

To prevent syntax and rendering errors in Markdown viewers and IDE extensions:
1. In `flowchart TD` / `flowchart LR` blocks:
   - **NEVER** use class-diagram inheritance (`<|--`) or composition (`*-->`). An asterisk (`*`) is tokenized as `MULT` (multiplication) and will crash the parser. Always use standard directed arrows (`-->`, `-.->`).
   - Quote all node labels containing brackets, parentheses, or punctuation (e.g. `KnownFiles["Known File Implementations ( Wix,csproj,nuspec )"]`).
   - Do NOT use raw HTML tags inside node text; use `<br/>` and `<i>...</i>` safely.
2. In `classDiagram` blocks:
   - Use standard class diagram inheritance (`<|--`) and composition (`*--`).
3. In all diagrams:
   - Use descriptive, clean edge descriptions.

---

### Step 4: Output Requirements
- Output the entire document as a single clean Markdown file or artifact.
- Ensure all source code references use clickable GitHub-style markdown file links using the `file:///` URI scheme with forward slashes.
