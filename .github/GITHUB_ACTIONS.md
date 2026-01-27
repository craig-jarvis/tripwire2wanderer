# GitHub Actions CI/CD

This repository uses GitHub Actions to automatically build and publish Docker images to GitHub Container Registry (ghcr.io).

## Workflow Overview

The workflow (`docker-publish.yml`) automatically runs on:
- **Push to main/master branch** - Builds and publishes with `latest` tag
- **Push of version tags** (e.g., `v1.0.0`) - Builds and publishes versioned images
- **Pull requests** - Builds the image but doesn't publish (validation only)

## Published Images

Images are published to: `ghcr.io/YOUR_USERNAME/tripwire2wanderer`

### Available Tags

The workflow automatically generates multiple tags for flexibility:

| Tag Pattern | Example | When Created | Description |
|-------------|---------|--------------|-------------|
| `latest` | `latest` | Push to default branch | Always points to the latest main/master build |
| `{branch}` | `main`, `develop` | Push to any branch | Branch-specific builds |
| `{version}` | `v1.2.3` | Version tag push | Full semantic version |
| `{major}.{minor}` | `v1.2` | Version tag push | Major.minor version |
| `{major}` | `v1` | Version tag push | Major version only |
| `{branch}-sha-{sha}` | `main-sha-abc1234` | Any push | Specific commit builds |

## Usage Examples

### Pull the latest version:
```bash
docker pull ghcr.io/YOUR_USERNAME/tripwire2wanderer:latest
```

### Pull a specific version:
```bash
docker pull ghcr.io/YOUR_USERNAME/tripwire2wanderer:v1.2.3
```

### Pull a specific commit:
```bash
docker pull ghcr.io/YOUR_USERNAME/tripwire2wanderer:main-sha-abc1234
```

## Releasing a New Version

To create a new versioned release:

1. Tag your commit with a semantic version:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. The GitHub Action will automatically:
   - Build the Docker image
   - Publish with multiple tags:
     - `v1.0.0` (exact version)
     - `v1.0` (major.minor)
     - `v1` (major)
   - Generate build attestation for security

3. The image will be available at:
   ```
   ghcr.io/YOUR_USERNAME/tripwire2wanderer:v1.0.0
   ghcr.io/YOUR_USERNAME/tripwire2wanderer:v1.0
   ghcr.io/YOUR_USERNAME/tripwire2wanderer:v1
   ```

## Multi-Architecture Support

The workflow builds images for multiple architectures:
- **linux/amd64** - Standard x86_64 servers
- **linux/arm64** - ARM-based servers (AWS Graviton, Raspberry Pi, etc.)

Docker will automatically pull the correct architecture for your platform.

## Build Caching

The workflow uses GitHub Actions cache to speed up builds:
- Dependencies are cached between runs
- Reduces build time from ~2 minutes to ~30 seconds on subsequent builds

## Security Features

### Build Attestation
The workflow generates cryptographic attestations for each published image, providing:
- Proof of build provenance
- Verification that the image was built by GitHub Actions
- Transparency in the build process

### Permissions
The workflow uses minimal required permissions:
- `contents: read` - Read repository contents
- `packages: write` - Publish to GitHub Container Registry
- `id-token: write` - Generate attestations

## Accessing Published Images

### Public Access
By default, GitHub Container Registry images are private. To make them public:

1. Go to your GitHub profile → Packages
2. Find `tripwire2wanderer`
3. Click "Package settings"
4. Scroll to "Danger Zone"
5. Click "Change visibility" → "Public"

### Private Access (Authentication Required)
To pull private images, authenticate with GitHub:

```bash
# Create a Personal Access Token (PAT) with `read:packages` scope
# Then login:
echo $GITHUB_TOKEN | docker login ghcr.io -u YOUR_USERNAME --password-stdin

# Now you can pull private images
docker pull ghcr.io/YOUR_USERNAME/tripwire2wanderer:latest
```

## Workflow File Location

The workflow is defined in: `.github/workflows/docker-publish.yml`

## Monitoring Builds

View build status and logs:
1. Go to your repository on GitHub
2. Click the "Actions" tab
3. Select "Docker Build and Publish" workflow
4. Click on any workflow run to see logs

## Troubleshooting

### Build fails with "permission denied"
- Ensure the `GITHUB_TOKEN` has package write permissions (should be automatic)
- Check repository settings → Actions → General → Workflow permissions

### Image not found after successful build
- Check if the package is private (see "Accessing Published Images" above)
- Verify you're using the correct image path: `ghcr.io/YOUR_USERNAME/tripwire2wanderer`

### Build is slow
- First build will be slower (no cache)
- Subsequent builds should be faster with layer caching
- Check if the cache is being invalidated (changes to Dockerfile or dependencies)

## Local Testing

To test the workflow locally before pushing:

```bash
# Build using the same Dockerfile
docker build -t tripwire2wanderer:test .

# Test the image
docker run --rm --env-file .env tripwire2wanderer:test --debug
```
