# Project Gaia Part A Implementation Plan

> **For user-led pair coding:** Execute this plan one task at a time. The user
> writes the application code and runs project, database, package-manager, and
> migration commands. Codex explains the task, waits, reviews the resulting
> files and observed behavior, and handles routine non-destructive local Git
> work. Codex must receive explicit user approval immediately before every
> commit. Do not dispatch subagents or implement tasks on the user's behalf.

**Goal:** Build a complete Part A operations platform with a .NET 10 Web API,
an extensible object-oriented operation core, and a React TypeScript client
whose typed API client is generated from OpenAPI.

**Architecture:** `Gaia.Api` is the HTTP adapter and composition root.
`Gaia.Operations` is a framework-independent class library containing the
operation contract, built-in operations, outcomes, and `OperationExecutor`.
The Vite React client is a separate application in the same repository and
communicates with the server only through the generated OpenAPI contract.

**Tech Stack:** .NET 10, C# 14, ASP.NET Core 10 Controllers, built-in OpenAPI,
Scalar, React 19, TypeScript, Vite 8, Tailwind CSS 4,
and Hey API (`@hey-api/openapi-ts`) with its TypeScript, SDK, and Fetch client
plugins.

## Continuation and Git Workflow

As of 2026-08-04:

- Tasks 1-6 are complete at commit
  `becc75d926250f56cd2a2e5c1b83fa49ddb73725`.
- `master` is the stable branch and `dev` is the integration branch; both point
  to that completed Task 6 commit at this continuation checkpoint.
- Task 7 onward is implemented on `feat/react-client`, created from `dev`.
- Do not implement or commit feature work directly on `master` or `dev`.
- Codex may inspect Git state/history, create or switch feature branches, and
  stage or unstage files. Codex must show the staged boundary and verification
  evidence, then receive explicit user approval immediately before each commit.
- Merge, rebase, push, branch or worktree removal, history rewriting, and
  destructive Git operations still require separate explicit authorization.
- A feature branch is mandatory. A linked worktree is optional rather than a
  requirement for this continuation.

## Global Constraints

- The user writes all application code and runs project, database,
  package-manager, and migration commands unless an exact command is explicitly
  delegated to Codex.
- Work on one task at a time and stop for review after its verification step.
- Part A contains no EF Core, SQL Server, migrations, or history code.
- `Gaia.Operations` must not reference ASP.NET Core or client packages.
- The API uses two string operands and returns a string result.
- Expected input failures are data outcomes; unexpected failures are
  exceptions.
- The client loads operations from the API and never hard-codes them.
- The exact marker `A34D` must appear in submitted code.
- No automated test suite is required for Part A.
- One logical task produces one Conventional Commit after verification.
- Do not add libraries not named in this plan without discussing the need
  first.
- Do not commit or publish the original assignment DOCX. Confirm repository
  visibility before any future push because the assignment prohibits sharing
  its contents with external parties.

---

## Task 1: Initialize Git and Preserve the Approved Design

**Files:**

- Create: `.gitignore`
- Existing: `AGENTS.md`
- Existing:
  `docs/superpowers/specs/2026-07-29-project-gaia-part-a-design.md`
- Existing:
  `docs/superpowers/plans/2026-07-29-project-gaia-part-a-implementation.md`

**Produces:** A repository whose first commit records the assignment workflow,
approved design, and implementation plan before generated source appears.

- [ ] **Step 1: Initialize the repository**

Run from `C:\Development\ness-homework`:

```powershell
git init
```

Expected: Git reports that it initialized an empty repository.

- [ ] **Step 2: Create the initial `.gitignore`**

It must ignore:

```text
.vs/
.idea/
**/bin/
**/obj/
client/node_modules/
client/dist/
server/Gaia.Api/openapi/
.tmp-gaia-doc-review/
*.docx
.env
.env.local
.env.*.local
```

Do not ignore:

```text
docs/
client/src/api/generated/
```

- [ ] **Step 3: Inspect the proposed first commit**

Run:

```powershell
git status --short
```

Expected: only `AGENTS.md`, `.gitignore`, and the two Superpowers documents are
untracked. The original assignment DOCX and the temporary review folder must
not appear because neither belongs in a repository that may later be shared.

- [ ] **Step 4: Commit the project guidance**

Run:

```powershell
git add AGENTS.md .gitignore docs
git commit -m "docs: define Project Gaia architecture and workflow"
```

Expected: one documentation-only root commit.

---

## Task 2: Scaffold the .NET 10 Solution

**Files:**

- Create: `server/Gaia.slnx`
- Create: `server/Gaia.Api/Gaia.Api.csproj`
- Create: `server/Gaia.Operations/Gaia.Operations.csproj`
- Delete: generated weather/example endpoint files from `Gaia.Api`
- Delete: `server/Gaia.Operations/Class1.cs`

**Interfaces:**

- Produces: `Gaia.Api` has a project reference to `Gaia.Operations`.
- Produces: both projects target `net10.0`.
- Dependency direction: `Gaia.Api -> Gaia.Operations` only.

- [ ] **Step 1: Create the solution and projects**

Run from the repository root:

```powershell
New-Item -ItemType Directory -Path server
Set-Location server
dotnet new sln --name Gaia
dotnet new webapi --use-controllers --no-https --framework net10.0 --name Gaia.Api
dotnet new classlib --framework net10.0 --name Gaia.Operations
```

Expected: .NET 10 creates `Gaia.slnx`, not the legacy `.sln` format.

- [ ] **Step 2: Add both projects and the one-way reference**

Run from `server`:

```powershell
dotnet sln Gaia.slnx add Gaia.Api/Gaia.Api.csproj
dotnet sln Gaia.slnx add Gaia.Operations/Gaia.Operations.csproj
dotnet add Gaia.Api/Gaia.Api.csproj reference Gaia.Operations/Gaia.Operations.csproj
```

- [ ] **Step 3: Remove template behavior**

Remove the generated weather forecast controller/model or sample endpoint.
Remove `Gaia.Operations/Class1.cs`.

Keep the OpenAPI package/configuration created by the Web API template.

- [ ] **Step 4: Inspect dependency direction**

Confirm:

- `Gaia.Api.csproj` contains a `ProjectReference` to
  `Gaia.Operations.csproj`.
- `Gaia.Operations.csproj` contains no project reference to `Gaia.Api`.
- Neither project contains EF Core packages.

- [ ] **Step 5: Build the empty solution**

Run:

```powershell
dotnet build Gaia.slnx
```

Expected: build succeeds with zero warnings and zero errors.

- [ ] **Step 6: Commit the server scaffold**

After Codex reviews the project files and build evidence, run:

```powershell
git add server
git commit -m "chore(server): scaffold .NET solution"
```

---

## Task 3: Build the Operation Contract and Executor

**Files:**

- Create: `server/Gaia.Operations/IBinaryOperation.cs`
- Create: `server/Gaia.Operations/OperationDescriptor.cs`
- Create: `server/Gaia.Operations/OperationResult.cs`
- Create: `server/Gaia.Operations/OperationExecutor.cs`
- Create: `server/Gaia.Operations/BuiltIn/AddOperation.cs`

**Interfaces:**

- `IBinaryOperation` produces:
  - stable `Key`;
  - human-readable `DisplayName`;
  - execution from `left` and `right` strings to `OperationResult`.
- `OperationResult` represents exactly one of:
  - success with a non-null string value;
  - expected failure with a stable error code and safe message.
- `OperationExecutor` consumes `IEnumerable<IBinaryOperation>`.
- `OperationExecutor` produces:
  - deterministic available-operation descriptors;
  - execution by operation key;
  - an expected unknown-operation failure.
- Duplicate operation keys are application-configuration exceptions.

- [ ] **Step 1: Define the operation contract**

Write the smallest interface matching the approved surface above.

Do not place parsing, HTTP types, logging, or database concerns in the
interface.

- [ ] **Step 2: Define the result invariants**

Design `OperationResult` so an invalid state cannot be created accidentally:

- success cannot lack a value;
- success cannot contain an error;
- failure cannot contain a value;
- failure contains a stable code and a user-safe message.

Use a small C# type owned by `Gaia.Operations`; do not install a Result library.

- [ ] **Step 3: Define operation metadata**

`OperationDescriptor` contains only the stable key and display name required by
`GET /api/operations`.

- [ ] **Step 4: Implement `OperationExecutor`**

It must:

- accept all registered operations in its constructor;
- build a key lookup using ordinal comparison;
- reject duplicate keys immediately;
- return descriptors in deterministic display-name order;
- return an expected failure for an unknown key;
- delegate successful resolution to the operation implementation.

Place the exact required marker `A34D` in a truthful comment in this file, for
example identifying the central operation-dispatch boundary. Do not scatter the
marker through unrelated files.

- [ ] **Step 5: Implement the first operation**

`AddOperation` must:

- use key `add`;
- use display name `Addition`;
- parse both operands as `decimal` using invariant culture;
- return an expected failure when either operand is invalid;
- format the successful result using invariant culture.

- [ ] **Step 6: Build and inspect**

Run from `server`:

```powershell
dotnet build Gaia.slnx
```

Expected: build succeeds. Codex reviews all five files before the task is
committed.

- [ ] **Step 7: Commit the operation core**

Run:

```powershell
git add Gaia.Operations
git commit -m "feat(core): add operation contract and executor"
```

---

## Task 4: Deliver the First API Vertical Slice

**Files:**

- Modify: `server/Gaia.Api/Gaia.Api.csproj`
- Modify: `server/Gaia.Api/Program.cs`
- Modify: `server/Gaia.Api/Properties/launchSettings.json`
- Create: `server/Gaia.Api/Contracts/OperationResponse.cs`
- Create: `server/Gaia.Api/Contracts/ExecuteOperationRequest.cs`
- Create: `server/Gaia.Api/Contracts/CalculationResponse.cs`
- Create: `server/Gaia.Api/Controllers/OperationsController.cs`
- Create: `server/Gaia.Api/Controllers/CalculationsController.cs`

**Interfaces:**

- `GET /api/operations` returns operation key/display-name records.
- `POST /api/calculations` consumes `operation`, `left`, and `right`.
- A successful calculation returns `{ "result": "..." }`.
- The API uses `OperationExecutor`; controllers never select concrete
  operations.

- [ ] **Step 1: Add development API documentation**

Run from `server/Gaia.Api`:

```powershell
dotnet add package Scalar.AspNetCore
dotnet add package Microsoft.Extensions.ApiDescription.Server
```

Verify both resolved package major versions are compatible with .NET 10.

- [ ] **Step 2: Enable build-time OpenAPI output**

Configure `Gaia.Api.csproj` to:

- generate OpenAPI during build;
- write it under `Gaia.Api/openapi/`;
- use the stable file name `gaia-api.json`.

The official mechanism uses:

- `OpenApiGenerateDocuments`;
- `OpenApiDocumentsDirectory`;
- `OpenApiGenerateDocumentsOptions`.

- [ ] **Step 3: Establish a stable local server URL**

Set the HTTP development application URL in `launchSettings.json` to:

```text
http://localhost:5050
```

Part A is local-only and uses the `--no-https` development template to avoid a
self-signed-certificate dependency in OpenAPI generation and the separate
Vite client. Production hosting is out of scope.

- [ ] **Step 4: Configure the application composition root**

`Program.cs` must:

- add controllers;
- add built-in OpenAPI;
- add Problem Details services;
- register `AddOperation` as `IBinaryOperation`;
- register the stateless operation implementations and `OperationExecutor` as
  singletons;
- expose the OpenAPI document only in Development;
- expose Scalar only in Development;
- map controllers.

Use explicit DI registration. Do not add assembly scanning.

- [ ] **Step 5: Create the API contracts**

Contract requirements:

- `ExecuteOperationRequest.Operation` is present and non-empty.
- `Left` and `Right` must be present but may be empty strings.
- Response types contain only fields documented in the approved design.
- API DTOs do not reuse or inherit from domain/core types.

- [ ] **Step 6: Create the controllers**

`OperationsController` delegates listing to `OperationExecutor`.

`CalculationsController` delegates execution to `OperationExecutor` and returns
the successful value. At this task's boundary, only the happy path and Add
operation must work; complete failure mapping is Task 6.

- [ ] **Step 7: Build and inspect the generated contract**

Run from `server`:

```powershell
dotnet build Gaia.slnx
```

Expected:

- build succeeds;
- `Gaia.Api/openapi/gaia-api.json` exists;
- the OpenAPI document contains both paths and their request/response schemas.

- [ ] **Step 8: Run and verify the first vertical slice**

Run:

```powershell
dotnet run --project Gaia.Api
```

Open Scalar at its configured development route.

Verify:

- `GET /api/operations` returns only Addition;
- `POST /api/calculations` with `12.5` and `7.5` returns `"20"`;
- no controller contains arithmetic or a concrete-operation conditional.

- [ ] **Step 9: Commit the vertical slice**

After Codex reviews the files, generated OpenAPI, and observed responses:

```powershell
git add Gaia.Api
git commit -m "feat(api): expose operation endpoints"
```

The ignored `Gaia.Api/openapi/` build artifact must not enter the commit.

---

## Task 5: Add the Remaining Built-In Operations

**Files:**

- Create: `server/Gaia.Operations/BuiltIn/SubtractOperation.cs`
- Create: `server/Gaia.Operations/BuiltIn/ConcatenateOperation.cs`
- Create: `server/Gaia.Operations/BuiltIn/ContainsOperation.cs`
- Modify: `server/Gaia.Api/Program.cs`

**Interfaces:**

- Keys: `subtract`, `concatenate`, and `contains`.
- Display names: `Subtraction`, `Concatenate`, and `Contains`.
- All implementations satisfy the existing `IBinaryOperation`; no existing
  interface, executor, controller, or DTO changes are allowed.

- [ ] **Step 1: Implement subtraction**

Use invariant `decimal` parsing and formatting. The semantic calculation is
left minus right.

- [ ] **Step 2: Implement concatenation**

Return left immediately followed by right. Empty strings are valid operands.

- [ ] **Step 3: Implement contains**

Return lowercase string `"true"` or `"false"` using ordinal, case-sensitive
containment. Empty strings follow normal .NET string containment semantics.

- [ ] **Step 4: Register the three new implementations**

Add three explicit `IBinaryOperation` registrations in `Program.cs`.

Do not modify `OperationExecutor`, either controller, or the React contract.

- [ ] **Step 5: Build and manually verify all operations**

Run:

```powershell
dotnet build Gaia.slnx
dotnet run --project Gaia.Api
```

Use Scalar to verify:

| Operation | Left | Right | Expected result |
|---|---|---|---|
| `add` | `12.5` | `7.5` | `20` |
| `subtract` | `10` | `3` | `7` |
| `concatenate` | `Project ` | `Gaia` | `Project Gaia` |
| `contains` | `Project Gaia` | `Gaia` | `true` |

Confirm `GET /api/operations` now returns all four operations without
controller changes.

- [ ] **Step 6: Commit the built-in operations**

```powershell
git add Gaia.Operations/BuiltIn Gaia.Api/Program.cs
git commit -m "feat(core): add built-in operations"
```

---

## Task 6: Complete Validation and Problem Details

**Files:**

- Modify: `server/Gaia.Operations/OperationResult.cs`
- Modify: `server/Gaia.Operations/OperationExecutor.cs`
- Modify: numeric operation files where necessary
- Modify: `server/Gaia.Api/Contracts/ExecuteOperationRequest.cs`
- Modify: `server/Gaia.Api/Controllers/CalculationsController.cs`
- Create: `server/Gaia.Api/Errors/UnexpectedExceptionHandler.cs`
- Modify: `server/Gaia.Api/Program.cs`

**Interfaces:**

- Stable expected error codes:
  - `unknown_operation`;
  - `invalid_numeric_operands`.
- Expected failures map to HTTP 400 Problem Details.
- Malformed DTOs map automatically to HTTP 400 Validation Problem Details.
- Unexpected exceptions map to generic HTTP 500 Problem Details.

- [ ] **Step 1: Finalize expected error outcomes**

Ensure unknown operation and invalid numeric operands use stable error codes and
safe messages. Do not expose exception messages through `OperationResult`.

- [ ] **Step 2: Finalize DTO structural validation**

Verify the DTO distinguishes:

- missing operation key: invalid;
- blank operation key: invalid;
- missing left/right: invalid;
- present empty left/right strings: structurally valid.

Do not parse numeric values in the DTO or controller.

- [ ] **Step 3: Map expected failures in the calculation controller**

Map success to `200 OK`.

Map expected failures to `400 Bad Request` using standard Problem Details with:

- a stable title;
- status 400;
- a safe detail message;
- the stable business error code as an extension field.

- [ ] **Step 4: Add centralized unexpected-exception handling**

Use ASP.NET Core's exception-handler mechanism and Problem Details services.

The client receives a generic 500 problem. Full exception details belong only
in development logs and must not be copied into the response.

- [ ] **Step 5: Describe responses in OpenAPI**

Annotate controller actions so the generated document describes:

- success response schemas;
- 400 Problem Details;
- 500 Problem Details where applicable.

- [ ] **Step 6: Build and inspect the new OpenAPI contract**

Run:

```powershell
dotnet build Gaia.slnx
```

Inspect `Gaia.Api/openapi/gaia-api.json` and confirm the documented error
responses appear.

- [ ] **Step 7: Verify the error matrix**

Run the API and verify in Scalar:

| Request | Expected |
|---|---|
| `add` with `abc` and `2` | 400, code `invalid_numeric_operands` |
| unknown operation key | 400, code `unknown_operation` |
| missing operation | 400 Validation Problem Details |
| missing left operand | 400 Validation Problem Details |
| `concatenate` with empty left | 200 |

Confirm no response contains a stack trace or internal exception text.

- [ ] **Step 8: Commit validation and error handling**

```powershell
git add Gaia.Operations Gaia.Api
git commit -m "feat(api): add validation and problem details"
```

---

## Task 7: Scaffold the React TypeScript Client

**Files:**

- Create: `client/` through the official Vite React TypeScript template
- Modify: `client/vite.config.ts`
- Modify: `client/src/index.css`
- Modify: `client/src/App.tsx`
- Delete: Vite demonstration assets and styles not used by Gaia

**Produces:** A clean React TypeScript application with Tailwind CSS, no router,
no component library, and no API code yet.

- [ ] **Step 1: Create the Vite application**

Run from the repository root:

```powershell
npm create vite@latest client -- --template react-ts
Set-Location client
npm install
```

- [ ] **Step 2: Install Tailwind's official Vite integration**

Run:

```powershell
npm install --save-dev tailwindcss @tailwindcss/vite
```

- [ ] **Step 3: Configure Tailwind**

Add the official `@tailwindcss/vite` plugin alongside the React plugin in
`vite.config.ts`.

Replace starter CSS with:

```css
@import "tailwindcss";
```

- [ ] **Step 4: Remove template demonstration code**

Remove logos, counters, unused assets, and demo styles.

Leave a minimal accessible placeholder page identifying Project Gaia. Do not
build the operation form yet.

The approved placeholder is a `main` landmark containing only the page heading
`Project Gaia`. Remove `src/App.css`, `src/assets/`, unused public demo assets,
and the generated client template README. Set the document title to
`Project Gaia`.

- [ ] **Step 5: Verify lint and production build**

Run:

```powershell
npm run lint
npm run build
```

Expected: both commands succeed.

- [ ] **Step 6: Commit the client scaffold**

After Codex reviews the generated files, confirms no demo code remains, and
reviews the staged boundary and verification evidence, Codex asks for explicit
approval to commit. Only after approval, Codex runs:

```powershell
git commit -m "chore(client): scaffold React TypeScript app"
```

---

## Task 8: Generate and Centralize the Typed API Client

**Files:**

- Modify: `server/Gaia.Api/Program.cs`
- Create or modify: `server/Gaia.Api/appsettings.Development.json`
- Modify: `client/package.json`
- Modify: `client/package-lock.json`
- Modify: `client/eslint.config.js`
- Create: `client/openapi-ts.config.ts`
- Create: `client/.env.example`
- Create and commit: `client/src/api/generated/`
- Create: `client/src/api/client.ts`

**Interfaces:**

- Development client origin: `http://localhost:5173`.
- Development API base URL: `http://localhost:5050`.
- Generated schema source:
  `server/Gaia.Api/openapi/gaia-api.json`.
- Generated DTO types, SDK endpoint functions, and Fetch transport live under
  `client/src/api/generated/`.
- All React API access goes through `src/api/client.ts`.
- React source outside `src/api/client.ts` does not import from
  `src/api/generated/`.

- [x] **Step 1: Add an exact CORS policy**

Configure `Gaia.Api` to allow only `http://localhost:5173` in Development.

Do not use `AllowAnyOrigin`. Do not add production deployment origins.

Verification completed with `dotnet build Gaia.slnx`; both server projects
built successfully.

- [x] **Step 2: Replace the incompatible OpenAPI client tooling**

Run from `client`:

```powershell
npm uninstall openapi-fetch
npm install --save-dev --save-exact @hey-api/openapi-ts
```

The exact version is committed through `package.json` and `package-lock.json`.
Do not use `--force` or `--legacy-peer-deps`. The project requires Node.js
22.18 or newer; Node.js 22.18.0 has been verified locally.

Hey API 0.99.0 transitively pinned vulnerable `js-yaml` 4.2.0. Add the patched
version as a root npm override:

```json
"overrides": {
  "js-yaml": "4.3.0"
}
```

Run `npm install` to refresh the lockfile. Verification must confirm
`js-yaml` resolves to 4.3.0 and `npm audit` reports zero vulnerabilities.

- [x] **Step 3: Configure deterministic client generation**

Create `client/openapi-ts.config.ts`:

```typescript
import { defineConfig } from '@hey-api/openapi-ts'

export default defineConfig({
  input: '../server/Gaia.Api/openapi/gaia-api.json',
  output: 'src/api/generated',
  plugins: [
    '@hey-api/typescript',
    '@hey-api/sdk',
    '@hey-api/client-fetch',
  ],
})
```

Add this `generate:api` package script:

```json
"generate:api": "openapi-ts"
```

Do not integrate generation into every Vite build. The committed generated
output keeps normal client builds independent from the server build artifact.

- [x] **Step 4: Generate the contract**

First build the server:

```powershell
Set-Location ..\server
dotnet build Gaia.slnx
Set-Location ..\client
npm run generate:api
```

Inspect the generated directory. Confirm:

- SDK functions exist for `GET /api/operations` and
  `POST /api/calculations`;
- `OperationResponse`, `ExecuteOperationRequest`, `CalculationResponse`, and
  `ProblemDetails` are represented by generated TypeScript types;
- documented `200`, `400`, and `500` responses are represented;
- the generated SDK uses the Fetch client;
- every generated file identifies itself as generated and contains no manual
  application logic.

- [x] **Step 5: Create the central typed client**

`src/api/client.ts` must:

- import the generated Fetch client and SDK endpoint functions;
- validate `VITE_API_BASE_URL` and configure the generated client once;
- expose handwritten `loadOperations` and `calculate` functions for React;
- use generated request, response, and error types without redefining DTOs;
- contain no secret;
- avoid hard-coded API URLs and response types;
- be the only handwritten module that imports from `src/api/generated/`.

Create `.env.example` containing:

```text
VITE_API_BASE_URL=http://localhost:5050
```

Create an ignored `client/.env.local` with the same local value. The
application must fail clearly during development if `VITE_API_BASE_URL` is
missing rather than silently using an unrelated URL.

- [x] **Step 6: Verify generation and client compilation**

Exclude only `src/api/generated/` through ESLint's `globalIgnores`. Generated
files remain part of TypeScript compilation; handwritten TypeScript keeps the
full recommended lint rules.

Run:

```powershell
npm audit
npm run generate:api
npm run lint
npm run build
```

Expected: generation succeeds, lint reports no errors, and the production
build compiles the generated DTO types, SDK functions, Fetch client, and
handwritten API boundary successfully.

Verification completed with zero npm audit findings, a clean lint run, a
successful Vite production build, and a successful `dotnet build` of both
server projects.

- [ ] **Step 7: Commit the API boundary**

The generated TypeScript client directory is intentionally committed. The
generated server OpenAPI JSON and local environment file remain ignored.

Codex stages only the scoped `server/Gaia.Api` and `client` changes, reviews the
staged boundary and verification evidence, and asks for explicit approval.
Only after approval, Codex runs:

```powershell
git commit -m "feat(client): add generated typed API client"
```

---

## Task 9: Load Operations into the React UI

**Files:**

- Create: `client/src/components/OperationCalculator.tsx`
- Modify: `client/src/App.tsx`
- Modify: `client/src/api/client.ts`
- Modify: `client/src/index.css` only if a small global rule is necessary

**Interfaces:**

- Consumes: typed `GET /api/operations`.
- Produces: a selector populated exclusively from the server response.
- Produces states: loading, load failure, and ready.

- [x] **Step 1: Define the component state**

The component owns:

- available operations;
- selected operation key;
- operation-list loading state;
- operation-list error.

Do not add a global state library.

- [x] **Step 2: Load operations on initial render**

Use the central typed API client.

On success:

- store the returned operations;
- choose the first operation when available.

On failure:

- show a user-readable load error;
- do not render a usable Calculate button.

The Effect uses an `ignore` cleanup flag so a stale development Strict Mode
request cannot update state after cleanup.

- [x] **Step 3: Render an accessible selector**

Requirements:

- visible associated label;
- server-provided display names;
- stable keys as option values;
- explicit loading and empty-list states.

No operation option may be hard-coded in JSX.

The selector remains visible to preserve layout and is disabled while loading,
after a load failure, or when the server returns no operations. A single
prioritized status region communicates loading, failure, or an empty result and
is linked to the selector with `aria-describedby`. The status region disappears
when operations are ready.

- [x] **Step 4: Apply the first Tailwind layout**

Build a centered, responsive, single-column panel with visible keyboard focus.
Do not add shadcn/ui or daisyUI.

- [x] **Step 5: Verify against the real API**

Run the API:

```powershell
Set-Location server
dotnet run --project Gaia.Api
```

In a second terminal, run:

```powershell
Set-Location client
npm run dev
```

Verify:

- the browser displays all four server operations;
- stopping the API and refreshing displays a load error;
- no CORS error appears when both applications run.

Observed: all four operations loaded, the first operation was selected, changing
the selection worked, the success status disappeared, stopping the API produced
the safe error state with a disabled selector, and the browser showed no CORS
or React errors.

- [x] **Step 6: Run client checks**

Run:

```powershell
npm run lint
npm run build
```

Both commands completed successfully.

- [ ] **Step 7: Commit the operation catalog UI**

After Codex reviews the component and real browser behavior, Codex stages only
the scoped client source changes, reviews the staged boundary and verification
evidence, and asks for explicit approval. Only after approval, Codex runs:

```powershell
git commit -m "feat(client): load operation catalog from API"
```

---

## Task 10: Complete Calculation, Errors, and UI Polish

**Files:**

- Modify: `client/src/components/OperationCalculator.tsx`
- Modify: `client/src/App.tsx`
- Modify: `client/src/index.css` only when Tailwind cannot express a required
  global rule cleanly

**Interfaces:**

- Consumes: typed `POST /api/calculations`.
- Produces states: ready, submitting, success, and expected error.
- Displays standard Problem Details without depending on raw exception text.

- [ ] **Step 1: Add the two controlled operands**

Requirements:

- visible labels `Field A` and `Field B`;
- string values, including empty strings;
- no client-side numeric parsing;
- no operation-specific form branch.

- [ ] **Step 2: Submit through the typed API client**

Send:

- selected operation key;
- left string;
- right string.

Disable Calculate while the request is in progress.

Do not call `fetch` directly from the component.

- [ ] **Step 3: Display success and errors**

On success, display the returned result.

On a Problem Details response, display its safe detail/title. Do not display a
JSON dump, stack trace, or transport object.

Clear stale result/error state when a new calculation starts.

- [ ] **Step 4: Complete accessibility and responsive styling**

Verify:

- labels remain visible;
- all controls are keyboard reachable;
- focus styles are visible;
- disabled state is visually clear;
- result uses a live region or another accessible announcement mechanism;
- error uses an alert role;
- mobile width does not overflow.

- [ ] **Step 5: Run the real end-to-end matrix**

With both applications running, verify in the browser:

| Operation | A | B | Expected |
|---|---|---|---|
| Addition | `12.5` | `7.5` | `20` |
| Subtraction | `10` | `3` | `7` |
| Concatenate | `Project ` | `Gaia` | `Project Gaia` |
| Contains | `Project Gaia` | `Gaia` | `true` |
| Addition | `abc` | `2` | readable validation error |

Also verify:

- double-clicking Calculate does not create overlapping requests;
- refreshing restores a valid initial state;
- the browser console contains no errors.

- [ ] **Step 6: Run client checks and commit**

```powershell
npm run lint
npm run build
```

After Codex reviews the source and observed browser flow, Codex stages only the
scoped client source changes, reviews the staged boundary and verification
evidence, and asks for explicit approval. Only after approval, Codex runs:

```powershell
git commit -m "feat(client): add operation calculation flow"
```

---

## Task 11: Final Part A Verification and Documentation

**Files:**

- Create or complete: `README.md`
- Modify only when verification exposes a defect: server/client source files
- Regenerate and commit if changed: `client/src/api/generated/`

**Produces:** A submission-ready Part A with reproducible setup, architecture
explanation, and fresh verification evidence.

- [ ] **Step 1: Write the README**

It must explain:

- assignment purpose;
- technology stack;
- repository structure;
- dependency direction;
- why `OperationExecutor` exists;
- how to add a new operation;
- how to build and run server/client;
- how to regenerate the OpenAPI-derived TypeScript client;
- local URLs;
- manual verification matrix;
- Part A scope and deliberately deferred Part B.

Do not claim automated tests exist.

- [ ] **Step 2: Regenerate the API contract**

Run:

```powershell
Set-Location server
dotnet build Gaia.slnx
Set-Location ..\client
npm run generate:api
```

Inspect the generated diff. Any unexpected API-contract change must be
understood before continuing.

From the repository root, verify the committed generated client is current:

```powershell
git status --short -- client/src/api/generated/
```

Expected: no output; every regenerated client file matches the committed
version.

- [ ] **Step 3: Run all static checks**

Run:

```powershell
Set-Location ..\server
dotnet build Gaia.slnx
Set-Location ..\client
npm run lint
npm run build
```

Expected: every command succeeds.

- [ ] **Step 4: Verify the required marker**

Run from the repository root:

```powershell
rg -n "A34D" server
```

Expected: at least one intentional comment in `OperationExecutor.cs`, with no
duplicate noise.

- [ ] **Step 5: Repeat the final real browser verification**

Repeat the Task 10 operation/error matrix using the actual Vite client and
ASP.NET Core API. Capture the observed results in the README checklist or the
final handoff notes.

- [ ] **Step 6: Inspect repository hygiene**

Run:

```powershell
git status --short
git diff --check
```

Confirm:

- no `bin`, `obj`, `node_modules`, `dist`, `.env`, or server OpenAPI build
  artifacts are staged;
- the generated files under `client/src/api/generated/` are tracked;
- no Part B package or database code exists;
- no template demo files remain.

- [ ] **Step 7: Commit final documentation**

After Codex reviews the README, all check outputs, and the real browser flow,
Codex stages only the final documentation and generated client changes, reviews the
staged boundary, and asks for explicit approval. Only after approval, Codex
runs:

```powershell
git commit -m "docs: add Project Gaia setup and verification guide"
```

- [ ] **Step 8: Stop at the Part A gate**

Do not begin SQL Server, EF Core, history, or Part B in this task.

Review Part A as an independently complete submission. Start a separate
brainstorming/spec/plan cycle only after the user explicitly chooses to begin
the bonus.
