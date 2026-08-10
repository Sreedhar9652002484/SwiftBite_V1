# Contributing to SwiftBite

Thanks for considering contributing. A few guidelines:

## Setup

Follow the [Getting Started](README.md#-getting-started) section in the README to run the stack locally.

## Making changes

1. Fork the repo and create a branch from `main`.
2. Keep changes scoped — one feature/fix per PR.
3. Run `dotnet build SwiftBite.sln` and `dotnet test SwiftBite.sln` before opening a PR.
4. For frontend changes, run `ng build` and `ng test` in `frontend/swiftbite-ui`.
5. Never commit real secrets — see [SECURITY.md](SECURITY.md).

## Pull requests

Describe what changed and why. Link any related issue. CI must pass before merge.
