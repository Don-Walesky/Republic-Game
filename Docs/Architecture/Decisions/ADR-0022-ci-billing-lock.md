# ADR-0022: GitHub Actions Billing Lock and Verification Gate

## Status
Accepted

## Context
Following the cabinet appointment implementation and subsequent development commits, GitHub Actions workflow runs fail before checkout because of a platform account billing lock:
> "The job was not started because your account is locked due to a billing issue."

The failure occurs before runner allocation and before any workflow step or command can execute. It is an account-level infrastructure lock, not a failure in compilation, dependencies, National Yield, Treasury, State Capacity, development projects, or test assertions.

## Decision
1. **Billing Lock Root Cause**:
   - Actions runs since the cabinet commit fail before checkout because of the billing lock on the repository owner's GitHub account.
2. **Authoritative Verification Gate**:
   - Local `dotnet test Republic.sln` is the verification gate until the lock is cleared.
3. **Preservation of Game Domain**:
   - Do not modify `Republic.Core`, test assertions, or the national yield cycle to address or conceal this platform runner lock.

## Consequences
- Clean execution of `dotnet test Republic.sln` locally serves as authoritative proof of build integrity and regression-free test passes.
- CI workflow configurations remain intact and will resume normal automated execution once the account billing lock is cleared.
