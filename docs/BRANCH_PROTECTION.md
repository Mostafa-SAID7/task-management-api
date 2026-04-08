# 🚦 Branch Protection Strategy

To maintain high code quality and prevent regressions, the following branch protection rules are recommended for the **Task Management API** repository.

## 🛡️ Recommended Rules for `main`

These rules should be configured in **Repository Settings > Branches > Add rule**.

### 1. Require Pull Request Reviews before Merging
- **Require approvals**: 1 minimum.
- **Dismiss stale pull request approvals** when new commits are pushed: ✅
- **Require review from Code Owners**: ✅ (Leverages `.github/CODEOWNERS`).

### 2. Require Status Checks to Pass before Merging
- **Require branches to be up to date before merging**: ✅
- **Status checks**:
    - `CI - Build and Test`: ✅
    - `Docker - Build and Push`: ✅
    - `Commit Lint`: ✅

### 3. Require Conversation Resolution
- All comments must be resolved before a pull request can be merged: ✅

### 4. Require Signed Commits
- Only allow commits that are GPG/SSH signed: ℹ️ (Optional but recommended for enterprise security).

### 5. Require Linear History
- Prevent merge commits and enforce squash/rebase merges: ✅ (Keeps the history clean).

### 6. Lock Branch
- Branch is read-only: ❌ (Keep disabled for `main` and `develop`).

### 7. Enforce Restrictions
- Restrict who can push to matching branches: ✅ (Only CI/CD service accounts or Lead Maintainers).

---

## 🏗️ Workflow best practices

### Feature Development
1. Create a branch from `develop` following the format: `feat/short-description`.
2. Commit changes using **Conventional Commits**.
3. Push and open a Pull Request to `develop`.
4. Passing CI and 1 approval from a **Code Owner** is required.

### Bug Fixes
- Branch format: `fix/issue-description` or `bugfix/issue-description`.
- Same PR flow as features.

### Releases
1. Create a branch from `develop`: `release/vX.Y.Z`.
2. Update versioning and double-check CHANGELOG.
3. PR to `main`.
4. Upon merging to `main`, a permanent tag `vX.Y.Z` should be created.
