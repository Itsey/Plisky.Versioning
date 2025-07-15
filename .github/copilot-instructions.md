# Copilot Instructions for Code Suggestions
These rules should be followed by github inline code editor and github copilot chat.

## General Guidelines for Code Suggestions
- **Frameworks:** Always use British English for spelling and grammar in code comments, documentation, commit messages, and any other text, including class, method and variable names.
- **Naming:** Prefer descriptive, explicit variable names for readability.
- **Coding Standards:** Ensure code adheres to project standards and style, and only change code relevant to the task.
- **Modernity:** Use modern technologies and best practices relevant to the programming language and framework in use.
- **Impact & Safety:** Consider performance, security, readability, maintainability, and avoid breaking changes unless requested.
- **Rationale:** If a code suggestion involves a significant change, provide a detailed explanation of the rationale behind the change.
- **Constants:** Avoid using magic numbers; use constants or enums instead.
- **Whitespace:** Don't suggest whitespace changes.
- **Modularity & Maintainability:** Encourage modular design, maintainability, and reusability; follow DRY (Do Not Repeat Yourself) principle.
- **Explicitness:** Only implement changes explicitly requested.
- **Testing:** Suggest or include unit tests for new or modified code, covering edge cases and error handling.
- **Comments:** Use comments sparingly and only when meaningful.
- **Verification:** Always ensure that the code compiles and passes existing tests after your changes.

### Making Edits
- **Change Management:** Focus on one conceptual change at a time and show clear before/after snippets.
- **Explanation:** Provide concise explanations and rationale for code changes.
- **Style:** Always check if the edit maintains the project's coding style.
- **Relevance:** Only change code that is directly relevant to the task at hand. Do not refactor unrelated code unless it is necessary for the change.

###  Instructions for Code Suggestions
When working with large files (>300 lines) or complex changes:
1. **Planning:** ALWAYS start by creating a detailed plan BEFORE making any edits
2. **PlanContent:** Your plan MUST include:
   - All functions/sections that need modification
   - The order in which changes should be applied
   - Dependencies between changes
   - Estimated number of separate edits required
3. **PlanFormat:** Format your plan as:
## Proposed Edit Plan
	Working with: [filename]
	Total planned edits: [number]

## Dot Net Project Requirements
- **Language:** Use C# as the primary programming language.
- **Framework:** Use .NET 6 or later for all new projects.
- **Web:** Use ASP.NET Core for web applications.
- **DI:** Use Dependency Injection for managing dependencies.
- **Braces:** Use One True Brace Style for formatting code.

## Test Project Requirements
- **Integration:** .ITest projects are Integration Tests.
- **Unit:** .Test projects are Unit Tests.
- **NoComments:** Do not include comments for Arrange, Act, Assert sections.
- **Spacing:** Leave a single blank new line between the Arrange, Act, and Assert sections.
- **Shouldly:** Use Shouldly for assertions.
- **AssertionMsg:** Include a short custom message in Shouldly assertions to provide context for test failures.
- **Exploratory:** Use the ExploratoryTests class for new exploratory unit tests.
- **Fact:** Use the [Fact] attribute for unit tests.
- **Theory:** Use the [Theory] attribute for parameterised tests to reduce repetition and improve maintainability.
- **InlineData:** Use the [InlineData] attribute for providing parameters to parameterised tests.
- **NoDisk:** Avoid creating unit tests that hit the disk or require external resources.
- **EdgeCases:** Make suggestions for unit tests that cover edge cases and error handling.

## Repo Specific Requirements
- **Library:** The Plisky.Versioning project is a reusable library with the versioning logic.
- **CLI:** The Versonify project is a user-facing dotnet CLI tool to interact with the versioning system.
- **CoreLogic:** Core logic should be added to the Plisky.Versioning project where it can then be referenced by the Versonify project.