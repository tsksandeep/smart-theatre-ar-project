# Smart Theatre AR - Modernization Plan (2025)

## Executive Summary

This document outlines a comprehensive modernization strategy for the Smart Theatre AR project, originally developed ~5 years ago. The goal remains the same: **display synchronized AR subtitles for movies across multiple devices** - but with modern technology that provides better performance, cross-platform support, improved developer experience, and production-ready infrastructure.

---

## 1. Legacy System Analysis

### Original Architecture (2019-2020)

```
┌─────────────────────────────────────────────────────────────────┐
│                         ORIGINAL SYSTEM                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────────┐          ┌──────────────────────────┐  │
│  │     Go Server        │   gRPC   │    Unity AR Client       │  │
│  │   (Port 50051)       │◄────────►│    (Android Only)        │  │
│  │                      │          │                          │  │
│  │  • SRT Parser        │          │  • ARCore SDK            │  │
│  │  • Pub/Sub Broker    │          │  • TextMesh Subtitles    │  │
│  │  • Anchor Storage    │          │  • gRPC Client           │  │
│  │  • GOPATH structure  │          │  • Hardcoded IPs         │  │
│  └──────────────────────┘          └──────────────────────────┘  │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

### Legacy Technology Stack

| Component | Old Technology | Version | Status in 2025 |
|-----------|---------------|---------|----------------|
| Game Engine | Unity | 2018.3.5f1 | Severely outdated (6+ years) |
| AR Framework | ARCore SDK for Unity | Legacy | **Deprecated** - replaced by AR Foundation |
| Backend Language | Go | 1.13 | Outdated (no modules, old patterns) |
| RPC Framework | gRPC | Legacy | Works but lacks modern features |
| gRPC Client (Unity) | Grpc.Core | Legacy | **Deprecated** - use grpc-dotnet |
| Protobuf | proto3 | Basic | Still valid, needs enhancement |
| Subtitle Format | SRT only | N/A | Limited format support |
| Platform Support | Android only | N/A | No iOS, Web, or cross-platform |
| Deployment | Local machine | N/A | No containerization or cloud |

### Critical Issues Identified

1. **Deprecated Dependencies**: ARCore SDK for Unity and Grpc.Core are deprecated
2. **Single Platform**: Android-only limits audience reach
3. **Hardcoded Configuration**: IP addresses embedded in source code
4. **No Modern DevOps**: No CI/CD, containerization, or cloud deployment
5. **Limited Subtitle Support**: Only SRT format, hardcoded file path
6. **No Authentication**: Insecure gRPC channel with no auth
7. **Poor Scalability**: In-memory anchor storage, single server instance
8. **No UI/UX**: Basic TextMesh, no styling or accessibility features

---

## 2. Modernized Architecture (2025)

### High-Level System Design

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        MODERNIZED SYSTEM (2025)                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                         CLOUD BACKEND                                │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌────────────┐  │    │
│  │  │   API       │  │  Subtitle   │  │   Anchor    │  │   Auth     │  │    │
│  │  │   Gateway   │  │   Service   │  │   Service   │  │   Service  │  │    │
│  │  │  (GraphQL)  │  │  (Rust/Go)  │  │  (Rust/Go)  │  │  (OAuth2)  │  │    │
│  │  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘  └─────┬──────┘  │    │
│  │         │                │                │                │         │    │
│  │         └────────────────┴────────────────┴────────────────┘         │    │
│  │                                   │                                   │    │
│  │  ┌────────────────────────────────┼────────────────────────────────┐ │    │
│  │  │              MESSAGE BROKER (NATS / Redis Pub/Sub)              │ │    │
│  │  └────────────────────────────────┬────────────────────────────────┘ │    │
│  │                                   │                                   │    │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐                   │    │
│  │  │   Redis     │  │  PostgreSQL │  │  S3/MinIO   │                   │    │
│  │  │   (Cache)   │  │  (Anchors)  │  │  (Subtitles)│                   │    │
│  │  └─────────────┘  └─────────────┘  └─────────────┘                   │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                   │                                          │
│                    WebSocket / gRPC-Web / SSE                                │
│                                   │                                          │
│  ┌────────────────────────────────┴────────────────────────────────────┐    │
│  │                         CLIENT APPLICATIONS                          │    │
│  │                                                                      │    │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────┐ │    │
│  │  │   Unity      │  │   WebXR      │  │   Native     │  │  Vision  │ │    │
│  │  │   (AR Found.)│  │   (Browser)  │  │   Swift/ARKit│  │   Pro    │ │    │
│  │  │   iOS+Android│  │   Any Device │  │   iOS        │  │   visionOS│ │    │
│  │  └──────────────┘  └──────────────┘  └──────────────┘  └──────────┘ │    │
│  │                                                                      │    │
│  └──────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Technology Stack Recommendations

### Backend Services

| Component | Recommended Technology | Rationale |
|-----------|----------------------|-----------|
| **Primary Language** | **Rust** or **Go 1.23+** | Rust for performance-critical services; Go for rapid development |
| **API Layer** | GraphQL (async-graphql/gqlgen) + gRPC | GraphQL for flexibility, gRPC for real-time streaming |
| **Message Broker** | **NATS JetStream** | Cloud-native, lightweight, perfect for real-time pub/sub |
| **Database** | PostgreSQL 16+ with pgvector | Relational data + vector search for future AI features |
| **Cache** | Redis 7+ / Valkey | Session management, anchor caching, pub/sub fallback |
| **Object Storage** | S3 / MinIO | Subtitle file storage with CDN support |
| **Container Runtime** | Docker + Kubernetes | Production orchestration |
| **Service Mesh** | Linkerd or Istio | mTLS, observability, traffic management |

### Frontend/Client Applications

| Platform | Recommended Technology | Rationale |
|----------|----------------------|-----------|
| **Cross-Platform AR** | **Unity 6 (2024 LTS)** + **AR Foundation 6.x** | Industry standard, ARCore + ARKit + Meta Quest support |
| **Web AR** | **WebXR Device API** + Three.js/A-Frame | Browser-based AR without app installation |
| **Native iOS** | **Swift** + **ARKit 6** + **RealityKit** | Best-in-class iOS AR experience |
| **Apple Vision Pro** | **visionOS** + **RealityKit** | Spatial computing for premium experience |
| **UI Framework** | TextMeshPro + Unity UI Toolkit | Modern, performant text rendering |

### Communication Protocols

| Use Case | Protocol | Library |
|----------|----------|---------|
| Real-time Subtitles | **WebSocket** + JSON | SignalR / Socket.IO / Native WS |
| Anchor Sync | **gRPC Streaming** | grpc-dotnet (Unity), tonic (Rust) |
| Web Clients | **gRPC-Web** or **SSE** | Connect-Web, EventSource |
| REST Fallback | HTTP/3 + JSON | Standard REST APIs |

### DevOps & Infrastructure

| Component | Recommended Technology |
|-----------|----------------------|
| **CI/CD** | GitHub Actions / GitLab CI |
| **Container Registry** | GitHub Container Registry / AWS ECR |
| **Kubernetes** | EKS / GKE / self-hosted K3s |
| **Secrets Management** | HashiCorp Vault / AWS Secrets Manager |
| **Monitoring** | Prometheus + Grafana |
| **Tracing** | OpenTelemetry + Jaeger |
| **Logging** | Loki / ELK Stack |

---

## 4. Detailed Component Design

### 4.1 Backend Services (Microservices Architecture)

#### Subtitle Service (Rust)

```rust
// Modern Rust service with async runtime
// Dependencies: tokio, tonic, async-graphql, sqlx

pub struct SubtitleService {
    db: PgPool,
    nats: async_nats::Client,
    storage: S3Client,
}

impl SubtitleService {
    // Parse multiple subtitle formats (SRT, VTT, ASS, SSA, TTML)
    pub async fn parse_subtitle(&self, file: Bytes, format: SubtitleFormat) -> Result<SubtitleTrack>;

    // Stream subtitles with precise timing via NATS
    pub async fn stream_subtitles(&self, session_id: Uuid, track_id: Uuid) -> impl Stream<Item = Subtitle>;

    // AI-powered subtitle generation (Whisper integration)
    pub async fn generate_subtitles(&self, audio_url: &str, language: &str) -> Result<SubtitleTrack>;
}
```

#### Anchor Service (Go 1.23+)

```go
// Modern Go with generics and structured logging
package anchor

type AnchorService struct {
    db     *pgxpool.Pool
    redis  *redis.Client
    nats   *nats.Conn
}

type Anchor struct {
    ID        uuid.UUID `json:"id"`
    SessionID uuid.UUID `json:"session_id"`
    Position  Vector3   `json:"position"`
    Rotation  Quaternion `json:"rotation"`
    CloudAnchorID string `json:"cloud_anchor_id,omitempty"` // ARCore Cloud Anchors
    CreatedAt time.Time `json:"created_at"`
}

// SetAnchor stores anchor with Redis caching and NATS broadcast
func (s *AnchorService) SetAnchor(ctx context.Context, anchor Anchor) error

// GetAnchor retrieves anchor with cache-first strategy
func (s *AnchorService) GetAnchor(ctx context.Context, sessionID uuid.UUID) (*Anchor, error)

// WatchAnchor subscribes to anchor updates via NATS
func (s *AnchorService) WatchAnchor(ctx context.Context, sessionID uuid.UUID) (<-chan Anchor, error)
```

#### Protocol Buffers (Enhanced)

```protobuf
syntax = "proto3";

package smarttheatre.v1;

import "google/protobuf/timestamp.proto";

// Enhanced subtitle message with styling support
message Subtitle {
    string id = 1;
    string text = 2;
    google.protobuf.Timestamp start_time = 3;
    google.protobuf.Timestamp end_time = 4;
    SubtitleStyle style = 5;
    string language = 6;
}

message SubtitleStyle {
    string font_family = 1;
    float font_size = 2;
    string color = 3;
    string background_color = 4;
    TextAlignment alignment = 5;
    Vector3 offset = 6; // 3D positioning offset
}

message Vector3 {
    float x = 1;
    float y = 2;
    float z = 3;
}

message Quaternion {
    float x = 1;
    float y = 2;
    float z = 3;
    float w = 4;
}

// Cloud Anchors support for cross-device AR
message Anchor {
    string id = 1;
    string session_id = 2;
    Vector3 position = 3;
    Quaternion rotation = 4;
    string cloud_anchor_id = 5; // ARCore/ARKit Cloud Anchor ID
    string creator_device_id = 6;
    google.protobuf.Timestamp created_at = 7;
}

message Session {
    string id = 1;
    string name = 2;
    string movie_id = 3;
    repeated string participant_ids = 4;
    Anchor anchor = 5;
    SessionState state = 6;
}

enum SessionState {
    SESSION_STATE_UNSPECIFIED = 0;
    SESSION_STATE_WAITING = 1;
    SESSION_STATE_ACTIVE = 2;
    SESSION_STATE_PAUSED = 3;
    SESSION_STATE_ENDED = 4;
}

// Streaming service definition
service SubtitleStreamService {
    // Bidirectional streaming for real-time sync
    rpc StreamSubtitles(stream SubtitleRequest) returns (stream Subtitle);

    // Server-streaming for anchor updates
    rpc WatchAnchor(WatchAnchorRequest) returns (stream Anchor);

    // Unary calls for session management
    rpc CreateSession(CreateSessionRequest) returns (Session);
    rpc JoinSession(JoinSessionRequest) returns (Session);
    rpc SetAnchor(SetAnchorRequest) returns (Anchor);
}
```

### 4.2 Unity Client (AR Foundation 6.x)

#### Project Structure

```
SmartTheatreAR/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── ServiceLocator.cs
│   │   │   ├── Configuration.cs
│   │   │   └── EventBus.cs
│   │   ├── AR/
│   │   │   ├── ARSessionManager.cs
│   │   │   ├── ImageTrackingController.cs
│   │   │   ├── AnchorManager.cs
│   │   │   └── CloudAnchorService.cs
│   │   ├── Networking/
│   │   │   ├── GrpcClient.cs
│   │   │   ├── WebSocketClient.cs
│   │   │   └── ConnectionManager.cs
│   │   ├── Subtitles/
│   │   │   ├── SubtitleRenderer.cs
│   │   │   ├── SubtitleStyler.cs
│   │   │   └── SubtitleSynchronizer.cs
│   │   └── UI/
│   │       ├── SettingsPanel.cs
│   │       ├── SessionJoinUI.cs
│   │       └── SubtitleOverlay.cs
│   ├── Prefabs/
│   │   ├── ARSession.prefab
│   │   ├── SubtitleCanvas.prefab
│   │   └── AnchorVisualizer.prefab
│   ├── Settings/
│   │   └── SmartTheatreSettings.asset
│   └── Scenes/
│       ├── Bootstrap.unity
│       ├── MainAR.unity
│       └── Settings.unity
├── Packages/
│   └── manifest.json
└── ProjectSettings/
```

#### Core AR Controller (Modern C#)

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Grpc.Net.Client;
using SmartTheatre.V1;

namespace SmartTheatre.AR
{
    public class ARSessionController : MonoBehaviour
    {
        [SerializeField] private ARSession arSession;
        [SerializeField] private ARTrackedImageManager imageManager;
        [SerializeField] private ARAnchorManager anchorManager;
        [SerializeField] private SubtitleRenderer subtitleRenderer;

        private GrpcChannel _channel;
        private SubtitleStreamService.SubtitleStreamServiceClient _client;
        private CancellationTokenSource _cts;

        private async void Start()
        {
            // Load configuration from ScriptableObject or remote config
            var config = await ConfigurationService.LoadAsync();

            // Initialize gRPC with modern grpc-dotnet
            _channel = GrpcChannel.ForAddress(config.ServerUrl, new GrpcChannelOptions
            {
                HttpHandler = new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30)
                }
            });

            _client = new SubtitleStreamService.SubtitleStreamServiceClient(_channel);

            // Subscribe to image tracking events
            imageManager.trackedImagesChanged += OnTrackedImagesChanged;

            _cts = new CancellationTokenSource();
        }

        private async void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
        {
            foreach (var image in args.added)
            {
                if (image.trackingState == TrackingState.Tracking)
                {
                    await HandleImageDetected(image);
                }
            }
        }

        private async Task HandleImageDetected(ARTrackedImage image)
        {
            // Check if session already has an anchor
            var session = await _client.JoinSessionAsync(new JoinSessionRequest
            {
                SessionId = GetSessionId(),
                DeviceId = SystemInfo.deviceUniqueIdentifier
            });

            if (session.Anchor != null)
            {
                // Use existing anchor (resolve cloud anchor)
                await ResolveCloudAnchor(session.Anchor.CloudAnchorId);
            }
            else
            {
                // First device - create and host anchor
                await CreateAndHostAnchor(image.transform.position, image.transform.rotation);
            }

            // Start subtitle streaming
            await StartSubtitleStream(session.Id);
        }

        private async Task StartSubtitleStream(string sessionId)
        {
            using var stream = _client.StreamSubtitles(cancellationToken: _cts.Token);

            // Send initial request
            await stream.RequestStream.WriteAsync(new SubtitleRequest
            {
                SessionId = sessionId,
                DeviceId = SystemInfo.deviceUniqueIdentifier
            });

            // Receive subtitle updates
            await foreach (var subtitle in stream.ResponseStream.ReadAllAsync(_cts.Token))
            {
                await UniTask.SwitchToMainThread();
                subtitleRenderer.DisplaySubtitle(subtitle);
            }
        }

        private async Task CreateAndHostAnchor(Vector3 position, Quaternion rotation)
        {
            // Create local anchor
            var anchorGO = new GameObject("SessionAnchor");
            anchorGO.transform.SetPositionAndRotation(position, rotation);
            var anchor = anchorGO.AddComponent<ARAnchor>();

            // Host to ARCore Cloud Anchors for cross-device sharing
            var cloudAnchorId = await CloudAnchorService.HostAnchorAsync(anchor);

            // Save to server
            await _client.SetAnchorAsync(new SetAnchorRequest
            {
                SessionId = GetSessionId(),
                Position = new Vector3Proto { X = position.x, Y = position.y, Z = position.z },
                Rotation = new QuaternionProto { X = rotation.x, Y = rotation.y, Z = rotation.z, W = rotation.w },
                CloudAnchorId = cloudAnchorId
            });
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _channel?.Dispose();
        }
    }
}
```

#### Modern Subtitle Renderer

```csharp
using UnityEngine;
using TMPro;
using DG.Tweening;
using SmartTheatre.V1;

namespace SmartTheatre.Subtitles
{
    public class SubtitleRenderer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform container;

        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private float fadeOutDuration = 0.15f;

        [Header("Accessibility")]
        [SerializeField] private bool useHighContrast = false;
        [SerializeField] private float minimumFontSize = 24f;

        private Tween _currentTween;

        public void DisplaySubtitle(Subtitle subtitle)
        {
            _currentTween?.Kill();

            if (string.IsNullOrEmpty(subtitle.Text))
            {
                // Fade out
                _currentTween = canvasGroup.DOFade(0, fadeOutDuration);
                return;
            }

            // Apply styling
            ApplyStyle(subtitle.Style);

            // Set text and fade in
            subtitleText.text = subtitle.Text;
            _currentTween = canvasGroup.DOFade(1, fadeInDuration);
        }

        private void ApplyStyle(SubtitleStyle style)
        {
            if (style == null) return;

            // Font size with accessibility minimum
            subtitleText.fontSize = Mathf.Max(style.FontSize, minimumFontSize);

            // Colors
            if (ColorUtility.TryParseHtmlString(style.Color, out var textColor))
                subtitleText.color = textColor;

            // High contrast mode for accessibility
            if (useHighContrast)
            {
                subtitleText.color = Color.white;
                subtitleText.outlineWidth = 0.3f;
                subtitleText.outlineColor = Color.black;
            }

            // 3D positioning offset
            if (style.Offset != null)
            {
                container.anchoredPosition3D = new Vector3(
                    style.Offset.X,
                    style.Offset.Y,
                    style.Offset.Z
                );
            }
        }
    }
}
```

### 4.3 WebXR Client (Browser-Based)

```typescript
// Modern TypeScript WebXR implementation
import { WebXRButton } from 'three/examples/jsm/webxr/WebXRButton';
import { ARButton } from 'three/examples/jsm/webxr/ARButton';
import * as THREE from 'three';

interface SubtitleMessage {
  id: string;
  text: string;
  startTime: number;
  endTime: number;
  style?: SubtitleStyle;
}

class SmartTheatreWebXR {
  private scene: THREE.Scene;
  private camera: THREE.PerspectiveCamera;
  private renderer: THREE.WebGLRenderer;
  private subtitleMesh: THREE.Mesh;
  private ws: WebSocket;

  constructor(private serverUrl: string) {
    this.initThreeJS();
    this.initWebSocket();
  }

  private async initThreeJS() {
    this.scene = new THREE.Scene();
    this.camera = new THREE.PerspectiveCamera(70, window.innerWidth / window.innerHeight, 0.01, 20);

    this.renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
    this.renderer.setSize(window.innerWidth, window.innerHeight);
    this.renderer.xr.enabled = true;

    document.body.appendChild(this.renderer.domElement);
    document.body.appendChild(ARButton.createButton(this.renderer, {
      requiredFeatures: ['hit-test', 'dom-overlay'],
      optionalFeatures: ['image-tracking']
    }));

    // Create subtitle text geometry
    this.createSubtitleDisplay();

    this.renderer.setAnimationLoop(this.render.bind(this));
  }

  private initWebSocket() {
    this.ws = new WebSocket(`${this.serverUrl}/ws/subtitles`);

    this.ws.onmessage = (event) => {
      const subtitle: SubtitleMessage = JSON.parse(event.data);
      this.updateSubtitle(subtitle);
    };
  }

  private updateSubtitle(subtitle: SubtitleMessage) {
    // Update Three.js text geometry with new subtitle
    // Using troika-three-text for performant text rendering
  }

  private render(timestamp: number, frame: XRFrame) {
    this.renderer.render(this.scene, this.camera);
  }
}
```

---

## 5. Infrastructure & Deployment

### Docker Compose (Development)

```yaml
version: '3.8'

services:
  subtitle-service:
    build:
      context: ./services/subtitle
      dockerfile: Dockerfile
    ports:
      - "50051:50051"
    environment:
      - DATABASE_URL=postgres://postgres:password@db:5432/smarttheatre
      - NATS_URL=nats://nats:4222
      - S3_ENDPOINT=http://minio:9000
    depends_on:
      - db
      - nats
      - minio

  anchor-service:
    build:
      context: ./services/anchor
      dockerfile: Dockerfile
    ports:
      - "50052:50052"
    environment:
      - DATABASE_URL=postgres://postgres:password@db:5432/smarttheatre
      - REDIS_URL=redis://redis:6379
      - NATS_URL=nats://nats:4222
    depends_on:
      - db
      - redis
      - nats

  api-gateway:
    build:
      context: ./services/gateway
      dockerfile: Dockerfile
    ports:
      - "8080:8080"
      - "8443:8443"
    environment:
      - SUBTITLE_SERVICE_URL=subtitle-service:50051
      - ANCHOR_SERVICE_URL=anchor-service:50052

  db:
    image: postgres:16-alpine
    environment:
      - POSTGRES_DB=smarttheatre
      - POSTGRES_PASSWORD=password
    volumes:
      - postgres_data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    volumes:
      - redis_data:/data

  nats:
    image: nats:2.10-alpine
    command: ["--jetstream"]
    ports:
      - "4222:4222"
      - "8222:8222"

  minio:
    image: minio/minio:latest
    command: server /data --console-address ":9001"
    ports:
      - "9000:9000"
      - "9001:9001"
    volumes:
      - minio_data:/data

volumes:
  postgres_data:
  redis_data:
  minio_data:
```

### Kubernetes Deployment (Production)

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: subtitle-service
  namespace: smarttheatre
spec:
  replicas: 3
  selector:
    matchLabels:
      app: subtitle-service
  template:
    metadata:
      labels:
        app: subtitle-service
    spec:
      containers:
        - name: subtitle-service
          image: ghcr.io/smarttheatre/subtitle-service:latest
          ports:
            - containerPort: 50051
          resources:
            requests:
              memory: "256Mi"
              cpu: "250m"
            limits:
              memory: "512Mi"
              cpu: "500m"
          livenessProbe:
            grpc:
              port: 50051
            initialDelaySeconds: 10
          readinessProbe:
            grpc:
              port: 50051
            initialDelaySeconds: 5
          env:
            - name: DATABASE_URL
              valueFrom:
                secretKeyRef:
                  name: db-credentials
                  key: url
---
apiVersion: v1
kind: Service
metadata:
  name: subtitle-service
  namespace: smarttheatre
spec:
  selector:
    app: subtitle-service
  ports:
    - port: 50051
      targetPort: 50051
  type: ClusterIP
```

---

## 6. Feature Enhancements

### 6.1 AI-Powered Features (New)

| Feature | Technology | Description |
|---------|------------|-------------|
| **Auto-Subtitle Generation** | OpenAI Whisper | Generate subtitles from audio in real-time |
| **Multi-Language Translation** | DeepL / Google Translate API | Real-time subtitle translation |
| **Speech Recognition Sync** | Whisper + FFmpeg | Sync subtitles with actual audio |
| **Smart Positioning** | ML-based gaze tracking | Auto-position subtitles based on user gaze |

### 6.2 Enhanced AR Features

| Feature | Technology | Description |
|---------|------------|-------------|
| **Cloud Anchors** | ARCore/ARKit Cloud Anchors | Persistent cross-device AR anchoring |
| **Spatial Audio** | Unity Spatial Audio | 3D audio for enhanced immersion |
| **Hand Tracking** | AR Foundation Hand Tracking | Gesture-based controls |
| **Eye Tracking** | Vision Pro / Quest Pro | Gaze-based subtitle positioning |

### 6.3 Accessibility Features

- High contrast mode
- Customizable font sizes (up to 200%)
- Background opacity control
- Position adjustment
- Multiple font options (including dyslexia-friendly fonts)
- Screen reader support
- Voice control

---

## 7. Migration Strategy

### Phase 1: Backend Modernization (Weeks 1-3)
1. Set up new Go module structure with `go mod`
2. Implement Rust subtitle service
3. Add PostgreSQL and Redis
4. Set up NATS for pub/sub
5. Create Docker Compose development environment
6. Write comprehensive tests

### Phase 2: Unity Client Upgrade (Weeks 4-6)
1. Create new Unity 6 project
2. Integrate AR Foundation 6.x
3. Replace Grpc.Core with grpc-dotnet
4. Implement modern UI with TextMeshPro
5. Add Cloud Anchors support
6. Test on iOS and Android

### Phase 3: Web Client (Weeks 7-8)
1. Develop WebXR client with Three.js
2. Implement WebSocket-based subtitle streaming
3. Create responsive web UI
4. Test across browsers

### Phase 4: Production Deployment (Weeks 9-10)
1. Set up Kubernetes cluster
2. Configure CI/CD pipelines
3. Implement monitoring and logging
4. Security hardening
5. Load testing
6. Documentation

---

## 8. Directory Structure (New Project)

```
smart-theatre-ar-v2/
├── services/
│   ├── subtitle-service/          # Rust
│   │   ├── src/
│   │   ├── Cargo.toml
│   │   └── Dockerfile
│   ├── anchor-service/            # Go
│   │   ├── cmd/
│   │   ├── internal/
│   │   ├── go.mod
│   │   └── Dockerfile
│   ├── gateway/                   # Go/Node
│   │   ├── cmd/
│   │   └── Dockerfile
│   └── proto/                     # Shared protobufs
│       └── smarttheatre/
│           └── v1/
│               ├── subtitle.proto
│               ├── anchor.proto
│               └── session.proto
├── clients/
│   ├── unity/                     # Unity 6 + AR Foundation
│   │   ├── Assets/
│   │   ├── Packages/
│   │   └── ProjectSettings/
│   ├── webxr/                     # TypeScript + Three.js
│   │   ├── src/
│   │   ├── package.json
│   │   └── vite.config.ts
│   └── visionos/                  # Swift + RealityKit
│       └── SmartTheatre.xcodeproj
├── infrastructure/
│   ├── docker/
│   │   └── docker-compose.yml
│   ├── kubernetes/
│   │   ├── base/
│   │   └── overlays/
│   └── terraform/                 # Cloud infrastructure
├── docs/
│   ├── architecture.md
│   ├── api.md
│   └── deployment.md
├── .github/
│   └── workflows/
│       ├── ci.yml
│       └── deploy.yml
└── README.md
```

---

## 9. Estimated Effort & Priorities

| Priority | Component | Effort | Impact |
|----------|-----------|--------|--------|
| P0 | Backend Services (Go/Rust) | High | Critical - foundation for all clients |
| P0 | Unity Client (AR Foundation) | High | Critical - primary user experience |
| P1 | WebXR Client | Medium | High - enables no-install experience |
| P1 | Cloud Deployment | Medium | High - production readiness |
| P2 | Vision Pro Client | Medium | Medium - premium experience |
| P2 | AI Subtitle Generation | Medium | Medium - enhanced features |
| P3 | Advanced Accessibility | Low | Medium - inclusive design |

---

## 10. Conclusion

This modernization plan transforms the Smart Theatre AR project from a proof-of-concept into a production-ready, cross-platform solution. Key improvements include:

- **Cross-Platform Support**: iOS, Android, Web, and Vision Pro
- **Scalable Architecture**: Microservices with Kubernetes
- **Modern AR**: AR Foundation with Cloud Anchors for shared experiences
- **Real-Time Performance**: NATS + gRPC for low-latency streaming
- **AI Integration**: Whisper for auto-subtitle generation
- **Accessibility**: Comprehensive accessibility features
- **Developer Experience**: Modern tooling, CI/CD, and comprehensive docs

The same core goal - synchronized AR subtitles for movies - but implemented with 2025's best practices and technologies.
