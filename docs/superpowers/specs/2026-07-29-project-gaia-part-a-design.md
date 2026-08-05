# Project Gaia Part A Design

**Date:** 2026-07-29  
**Status:** Approved direction; implementation has not started  
**Source:** `פרויקט גאיה - תרגיל פיתוח תוכנה.docx`

## 1. Purpose

Part A will deliver a small but complete operations platform. A user selects an
operation, supplies two input fields, executes the operation, and sees the
result. The same capabilities are exposed through an HTTP API for external
clients and through a React web client.

The project is also a learning exercise. Its design must demonstrate modern
.NET architecture, understandable object-oriented design, explicit boundaries,
and a real client/server contract without introducing enterprise ceremony that
the assignment does not need.

## 2. Goals

- Complete Part A independently before implementing the optional database
  bonus.
- Build the server in C# on .NET 10 and ASP.NET Core 10.
- Expose a documented HTTP API.
- Build a separate React and TypeScript client.
- Support arithmetic and string operations through one common abstraction.
- Allow a new operation to be added without changing the executor, controllers,
  API contract, or React form.
- Generate TypeScript API types and endpoint functions from the server's
  OpenAPI document.
- Return predictable, non-leaking HTTP errors.
- Keep the business logic independent from ASP.NET Core, React, EF Core, and
  SQL Server.
- Ensure the user can explain every submitted line and architectural decision.

## 3. Non-Goals for Part A

- Runtime scripting or expression evaluation.
- Loading third-party operation plug-ins from DLL files.
- Creating operations through an administration interface.
- Authentication or authorization.
- Database persistence.
- Operation history.
- A full component design system.
- Client-side routing.
- Complex state-management or data-fetching libraries.
- A mandatory automated test suite.
- Production deployment infrastructure.

## 4. Technology Stack

### Server

- .NET 10 LTS
- C# 14
- ASP.NET Core 10 controller-based Web API
- Built-in dependency injection
- Built-in OpenAPI document generation
- Build-time OpenAPI JSON generation using the official
  `Microsoft.Extensions.ApiDescription.Server` package
- A lightweight OpenAPI browser UI such as Scalar for development-time manual
  inspection

### Client

- React 19
- TypeScript
- Vite 8
- Tailwind CSS
- Node.js 22.18 or newer for client generation tooling
- Hey API (`@hey-api/openapi-ts`) for generated TypeScript contract types and
  endpoint functions
- Hey API's TypeScript, SDK, and Fetch client plugins

### Future Part B

- SQL Server 2025 Developer or Express
- Entity Framework Core 10

Part B technologies are selected for future compatibility but will not be
installed or referenced in Part A.

## 5. Repository and Solution Structure

The Git repository will contain both applications. This is a small monorepo in
the literal sense, but it will not use Nx, Turborepo, or other monorepo
orchestration.

```text
repository/
  AGENTS.md
  README.md

  server/
    Gaia.slnx
    Gaia.Api/
    Gaia.Operations/

  client/
    package.json
    src/

  docs/
    superpowers/
      specs/
      plans/
```

Only the .NET projects belong to `Gaia.slnx`. The React client remains an
independent npm/Vite application in the same repository.

## 6. Dependency Direction

```text
Gaia.Api  ──references──>  Gaia.Operations

React client  ──HTTP/OpenAPI──>  Gaia.Api
```

`Gaia.Operations` must not reference ASP.NET Core, EF Core, SQL Server, or the
React client. The separate class library makes this a compile-time boundary,
not merely a folder convention.

## 7. Server Responsibilities

### `Gaia.Operations`

This class library owns:

- The common binary-operation contract.
- Operation metadata: a stable key and a display name.
- The four initial operation implementations.
- The `OperationExecutor`.
- Operation success and expected failure representations.
- Business-level errors that have no knowledge of HTTP status codes.

### `Gaia.Api`

This web project owns:

- HTTP controllers.
- Request and response DTOs.
- HTTP-level structural validation.
- Mapping business outcomes to HTTP responses.
- Problem Details responses.
- Global handling of unexpected exceptions.
- OpenAPI document generation.
- Development-time API documentation UI.
- CORS policy.
- Dependency-injection registration and application composition.

## 8. Operation Model

Every operation receives exactly two string operands. Each concrete operation
decides how to interpret them.

The shared operation contract exposes:

- A stable machine key, such as `add`.
- A human-readable display name, such as `Addition`.
- Execution using a left and right string operand.
- A success or expected-failure outcome.

The initial operations are:

| Key | Display name | Semantics |
|---|---|---|
| `add` | Addition | Parses both operands as decimal numbers and adds them. |
| `subtract` | Subtraction | Parses both operands as decimal numbers and subtracts right from left. |
| `concatenate` | Concatenate | Concatenates the left and right strings. |
| `contains` | Contains | Performs an ordinal, case-sensitive check that the left string contains the right string. |

Numeric parsing and formatting use invariant culture. The API therefore uses a
dot as the decimal separator regardless of the machine's locale.

Results are represented as strings because Part A only needs to display them.
This gives the HTTP contract one stable response shape for numeric, textual, and
Boolean operations.

## 9. `OperationExecutor`

`OperationExecutor` is a coordinator, not an operation implementation.

It receives the registered operation implementations through dependency
injection and is responsible for:

- Building a lookup by operation key.
- Rejecting duplicate keys as application-configuration errors.
- Returning operation metadata for the available-operations endpoint.
- Resolving a requested operation key.
- Delegating execution to the resolved operation.

It does not:

- Implement arithmetic or string behavior.
- Parse numeric operands.
- Know about HTTP.
- Return ASP.NET Core result types.
- Access a database.
- Log or persist operation history.

The four operation implementations will be registered explicitly in the
application composition root. This keeps composition visible and avoids
reflection, assembly scanning, service-location, or another dependency merely
to remove a few registration lines.

Adding an operation requires a new implementation and one explicit registration
line. It does not require modifying the executor, controllers, API DTOs, or
React form.

## 10. HTTP API

### List available operations

```http
GET /api/operations
```

Successful response:

```json
[
  {
    "key": "add",
    "displayName": "Addition"
  },
  {
    "key": "concatenate",
    "displayName": "Concatenate"
  }
]
```

The response is derived from registered operation metadata. It is not a
hard-coded controller list.

### Execute an operation

```http
POST /api/calculations
Content-Type: application/json
```

Request:

```json
{
  "operation": "add",
  "left": "12.5",
  "right": "7.5"
}
```

Successful response:

```json
{
  "result": "20"
}
```

The request must contain:

- A non-empty operation key.
- A present left operand.
- A present right operand.

Left and right may be empty strings because empty strings are valid inputs for
some string operations. Each operation performs its own semantic validation.

## 11. Validation and Error Handling

Validation has two layers.

### HTTP structural validation

`Gaia.Api` validates the JSON body and required DTO fields. Invalid request
shape produces HTTP 400 with `ValidationProblemDetails`.

### Operation semantic validation

Each operation validates the meaning of its own operands. Numeric operations
return an expected failure when parsing fails. String operations accept the
raw strings according to their documented semantics.

Expected invalid input is represented as data rather than thrown as an
exception. Unexpected system failures remain exceptions.

Response mapping:

| Condition | HTTP response |
|---|---|
| Successful operation | `200 OK` |
| Unknown operation key | `400 Bad Request` with Problem Details |
| Invalid operands | `400 Bad Request` with Problem Details |
| Malformed request DTO | `400 Bad Request` with Validation Problem Details |
| Unexpected internal failure | `500 Internal Server Error` with generic Problem Details |

Production responses must not expose stack traces, internal exception messages,
filesystem paths, or other implementation details.

## 12. React Client

Part A uses one page and does not require routing.

The page contains:

- A title.
- An operation selector.
- A labelled Field A input.
- A labelled Field B input.
- A Calculate button.
- A result or error panel.

On startup, the client loads available operations from
`GET /api/operations`. The selector must not contain a hard-coded operation
list.

On submission, the client sends the selected operation and both operands to
`POST /api/calculations`.

The client handles:

- Initial operation loading.
- Operation-load failure.
- Ready state.
- Calculation in progress.
- Successful result.
- Problem Details response.

The Calculate button is disabled while a calculation request is in progress.

React hooks are sufficient for this state. Part A will not add Redux, Zustand,
TanStack Query, or another state-management/data-fetching library.

## 13. OpenAPI and Generated Client

ASP.NET Core exposes an OpenAPI document during development and generates the
same contract as a local JSON build artifact. The build artifact lets the
frontend regenerate types without requiring a running backend process.

The client has a repeatable generation script that:

1. Reads the build-generated server OpenAPI JSON file.
2. Uses Hey API to generate TypeScript DTO types and endpoint functions.
3. Uses the Hey API Fetch client for browser HTTP transport.
4. Stores the generated output under `client/src/api/generated/`.

The generated files:

- Are never edited manually.
- Are committed to Git so a frontend build does not require a running backend.
- Are regenerated whenever the HTTP contract changes.

The server's API DTOs remain the source of truth for the HTTP contract. Server
domain entities stay private and are not shared with or generated for the
client. OpenAPI is the language-neutral boundary between the applications.

Hey API uses the `@hey-api/typescript`, `@hey-api/sdk`, and
`@hey-api/client-fetch` plugins. A thin handwritten `src/api/client.ts` module
configures and exposes the generated client to React. React components import
only from this handwritten boundary; they do not import directly from
`src/api/generated/`, construct API URLs, or call `fetch` independently.

The API base URL is supplied through `VITE_API_BASE_URL`. The value is public
browser configuration and must never contain secrets.

## 14. CORS

During local development, the API allows the exact Vite development origin.
The policy must not use `AllowAnyOrigin`.

This explicit browser boundary demonstrates that the React client is a real
external API consumer rather than server-rendered UI hidden inside the .NET
application.

## 15. Styling and Accessibility

Tailwind CSS alone is sufficient for Part A.

The UI must have:

- Explicit labels associated with both inputs and the selector.
- Visible keyboard focus states.
- A disabled state for in-progress submission.
- Readable success and error states.
- A responsive single-column layout suitable for desktop and mobile widths.

Part A will not introduce shadcn/ui, daisyUI, or another component library. A
component library may be reconsidered only after the working flow is visually
reviewed.

## 16. Verification

Automated tests are not required by the assignment and are not a Part A
completion gate.

Part A verification is a documented manual workflow:

1. Build the server successfully.
2. Build and type-check the client successfully.
3. Inspect the OpenAPI document and development API UI.
4. Call both endpoints through the API UI.
5. Verify all four operations with valid inputs.
6. Verify invalid numeric operands return HTTP 400 Problem Details.
7. Verify an unknown operation returns HTTP 400 Problem Details.
8. Generate the TypeScript API contract and rebuild the client.
9. Run the real React client and API together.
10. Verify that operations load from the API.
11. Verify calculation success and error display in the browser.
12. Verify refresh returns the client to a valid initial state.

After Part A works, a small `Gaia.Operations.Tests` learning step may be
considered separately. It is not part of the approved required scope.

## 17. Part A Acceptance Criteria

Part A is complete only when:

- The server is implemented in C# and .NET 10.
- The API and React UI both execute operations successfully.
- `Gaia.Operations` has no ASP.NET Core or database dependency.
- No controller contains a switch or conditional chain selecting concrete
  operations.
- The React client does not hard-code the available operations.
- A new operation requires no executor, controller, DTO, API-contract, or form
  changes.
- API errors use clean Problem Details responses.
- OpenAPI-generated TypeScript DTO types and endpoint functions are used by the
  client behind the handwritten API boundary.
- Server and client builds pass.
- The end-to-end browser flow is manually verified.
- The exact comment marker `A34D` appears in submitted code.
- No EF Core package, SQL Server connection, migration, or history code exists
  in Part A.

## 18. Future Part B Direction

Part B will receive a separate design review before implementation.

The approved direction is:

- SQL Server 2025 Developer or Express.
- EF Core 10.
- A dedicated `Gaia.Infrastructure` project.
- A persisted successful-operation record containing an identity key,
  operation key, both operands, result, and UTC execution time.
- `DateTimeOffset` in .NET and `datetimeoffset` in SQL Server.
- A composite index beginning with operation key and followed by execution time
  in descending order.
- Querying the latest three successful operations of the same type.
- Counting successful operations of the same type since the start of the
  current month.
- Inspecting EF Core migrations and generated SQL rather than treating the ORM
  as a black box.

The calculation response may be extended in Part B to include the current
result, the recent operations, and the monthly count. The current execution is
included in both history values because it is persisted before the summary is
returned.

No future persistence abstraction or no-op implementation will be added to
Part A merely in anticipation of this bonus.
