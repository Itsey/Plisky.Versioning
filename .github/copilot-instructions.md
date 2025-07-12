# Copilot Instructions for Code Suggestions

## General Guidelines for Code Suggestions
- Always use British English for spelling and grammar in code comments, documentation, commit messages, and any other text, including class, method and variable names.
- Prefer descriptive, explicit variable names for readability.
- Always provide clear, concise explanations for your code suggestions.
- Ensure that your code adheres to the project's coding standards and style guidelines.
- Use modern technologies and best practices relevant to the programming language and framework in use.
- When suggesting changes, consider the impact on performance, security, readability and maintainability.
- If a code suggestion involves a significant change, provide a detailed explanation of the rationale behind the change.
- Avoid using magic numbers; use constants or enums instead.
- Don't suggest whitespace changes.
- Only implement changes explicitly requested; do not invent changes.
- Suggest or include unit tests for new or modified code.
- Use comments sparingly and only when meaningful.
- Avoid suggesting changes that would break existing functionality unless explicitly requested.
- Always ensure that the code compiles and passes existing tests after your changes.

### Making Edits
- Focus on one conceptual change at a time.
- Show clear "before" and "after" snippets when proposing changes.
- Include concise explanations of what changed and why.
- Always check if the edit maintains the project's coding style.
- Only change code that is directly relevant to the task at hand. Do not refactor unrelated code unless it is necessary for the change.

###  Instructions for Code Suggestions
When working with large files (>300 lines) or complex changes:
1. ALWAYS start by creating a detailed plan BEFORE making any edits
2. Your plan MUST include:
   - All functions/sections that need modification
   - The order in which changes should be applied
   - Dependencies between changes
   - Estimated number of separate edits required
3. Format your plan as:
## Proposed Edit Plan
	Working with: [filename]
	Total planned edits: [number]

## Dot Net Project Requirements
- Use C# as the primary programming language.
- Use .NET 6 or later for all new projects.
- Use ASP.NET Core for web applications.
- Use Dependency Injection for managing dependencies.
- Use One True Brace Style for formatting code.

## Test project Requirements
- .ITest projects are Integration Tests.
- .Test projects are Unit Tests.
- Do not include comments for Arrange, Act, Assert sections.
- Leave a single blank new line between the Arrange, Act, and Assert sections.
- Use Shouldly for assertions.
- Include a short custom message in Shouldly assertions to provide context for test failures.
- Use the ExploratoryTests class for new exploratory unit tests.
- Use the [Fact] attribute for unit tests.
- Use the [Theory] attribute for parameterised tests to reduce repetition and improve maintainability.
- Use the [InlineData] attribute for providing parameters to parameterised tests.
- Avoid creating unit tests that hit the disk or require external resources.
- Make suggestions for unit tests that cover edge cases and error handling.