# DrawModePlusMLS

[English](#english) | [中文](#中文)

A Unity Editor plugin that extends SceneView with additional debug draw modes, helping artists and technical artists inspect depth, normals, UVs, material properties, and more — all within the SceneView camera dropdown.

GitHub: https://github.com/MlsMoon/DrawModePlusMLS

---

## English

### Supported Unity Versions

- Unity 2022.3.62f3 (tested)
- **URP (Universal Render Pipeline) only** — Built-in and HDRP are not supported
- Both **Forward** and **Deferred** rendering paths

### Features

| Mode | Rendering Path | Description |
|---|---|---|
| **Depth** | Forward / Deferred | Fullscreen depth visualization with adjustable depth range slider (1–500m) |
| **WorldNormal (Forward)** | Forward | World-space normal visualization via fullscreen pass |
| **WorldNormal (Deferred)** | Deferred | Deferred GBuffer normal buffer visualization (supports normal maps) |
| **BaseColor (Deferred)** | Deferred | Deferred GBuffer base color (albedo) visualization |
| **MaterialAO** | Deferred | Material ambient occlusion pass from GBuffer |
| **Metallic (Deferred)** | Deferred | Deferred GBuffer metallic channel visualization |
| **Roughness (Deferred)** | Deferred | Deferred GBuffer roughness (smoothness) visualization |
| **TexelDensity** | Forward / Deferred | Re-renders scene objects to visualize texel density. Color legend: ≤128 red, 256 orange, 512 green, 1024 cyan, ≥2048 blue. Gray = non-Common.shader objects. **Requires project shader integration — see below.** |
| **UV0** | Forward / Deferred | UV0 checker pattern overlay on all scene objects |
| **Stencil** | Forward / Deferred | Stencil buffer debug: writes stencil values then visualizes them via post-process blit |

### How to Use

#### Method 1: SceneView Camera Dropdown

1. Open a SceneView
2. Click the **Draw Mode** dropdown (top-left of SceneView, usually says "Shaded")
3. Scroll down to the **DrawModePlusMLS** section
4. Select the desired debug mode

#### Method 2: Control Window

`Tools > DrawModePlus > DrawMode 显示控制面板`

A dockable EditorWindow that lets you switch modes via dropdown, with a depth range slider when Depth mode is active.

### Installation

1. Copy the `DrawModePlusMLS` folder into your project's `Assets/Plugins/` (or any `Assets/` folder)
2. The plugin auto-injects `DrawModePlusRendererFeature` into all URP RendererData assets on first load
3. Switch to a SceneView and select a draw mode from the camera dropdown

### TexelDensity — Project Shader Integration

The TexelDensity mode requires **manual shader modifications** in your project. Without this, all your project objects will render as flat gray.

#### Step 1: Add a LightMode Tag to your project's main shader

In **each shader** that should participate in texel density visualization, add a pass with `LightMode = "DrawModePlusTexelDensity"`:

```hlsl
Pass
{
    Name "DrawModePlusTexelDensity"
    Tags { "LightMode" = "DrawModePlusTexelDensity" }

    HLSLPROGRAM
    #pragma vertex Vert
    #pragma fragment Frag

    // ... include your common HLSL headers ...

    // The key: output texel density color in the fragment shader
    half4 Frag(Varyings input) : SV_Target
    {
        // Calculate texel density
        float2 uv = input.uv0;                      // or your mesh's primary UV
        float2 texelSize = ...;                     // from your main texture's TexelSize
        float2 ddxUV = ddx(uv);
        float2 ddyUV = ddy(uv);
        float texelDensity = ...;                   // your texel density formula (e.g., texels per world-unit)

        // Normalize to the 512 reference scale and output as color
        float normalizedDensity = texelDensity / 512.0;
        return half4(normalizedDensity, normalizedDensity, normalizedDensity, 1.0);
    }
    ENDHLSL
}
```

**How it works**: The `TexelDensityDebugPass` calls `context.DrawRenderers()` with `ShaderTagId("DrawModePlusTexelDensity")`. Unity only renders objects whose shaders contain a pass with this LightMode tag. Objects without this tag fall back to the `FlatGray` shader.

#### Step 2: Verify the result

Switch to TexelDensity mode in SceneView. A legend bar appears at the bottom:

```
Texel Density 512/m | <=128 Low | 256 Low | 512 OK | 1024 High | >=2048 High | Gray = non-Common.shader
```

- **Red (≤128)**: Texel density too low — texture appears blurry, needs higher-res texture or larger UV scale
- **Orange (256)**: Below reference
- **Green (512)**: Matches the reference density — good
- **Cyan (1024)**: Above reference
- **Blue (≥2048)**: Much higher than reference — texture memory waste
- **Gray**: Non-Common.shader objects that haven't been integrated

### Shader Requirements

The following shaders must be present in the project (included in `Arts/Shaders/`):

| Shader | Purpose |
|---|---|
| `DrawModePlus/DepthView` | Depth visualization fullscreen pass |
| `DrawModePlus/WorldNormal` | Forward normal visualization |
| `DrawModePlus/DeferredNormalBuffer` | Deferred normal buffer read |
| `DrawModePlus/DeferredDebugView` | Deferred GBuffer debug (BaseColor/Metallic/Roughness/AO) |
| `DrawModePlus/UV0Checker` | UV0 checker pattern |
| `DrawModePlus/FlatGray` | Fallback for non-integrated objects in TexelDensity |
| `DrawModePlus/StencilWriter` | Stencil write pass |
| `DrawModePlus/StencilChecker` | Stencil visualization pass |

These are automatically referenced by name via `Shader.Find()`. Ensure they are in a `Resources` folder or always-included in project settings.

### Architecture

```
DrawModePlusMLS/
├── Runtime/
│   ├── DrawModePlusRendererFeature.cs    — URP RendererFeature (auto-injected)
│   ├── DrawModePlusRuntimeState.cs       — Global state + DrawModePlusMode enum
│   ├── DrawModePlusRenderPipelineBridge.cs — URP pipeline reflection helpers
│   └── Passes/
│       ├── FullscreenDebugPass.cs        — Depth / Normal fullscreen blit
│       ├── SceneObjectDebugPass.cs       — Base class: re-draw scene objects
│       ├── Uv0DebugPass.cs              — UV0 checker (override material)
│       ├── TexelDensityDebugPass.cs     — Texel density (override material + DrawModePlusTexelDensity tag)
│       ├── StencilDebugPass.cs          — Stencil write + view blit
│       ├── MaterialAOCapturePass.cs     — Deferred GBuffer capture (procedural quad)
│       └── MaterialAOCompositePass.cs   — Deferred debug composite
├── Editor/
│   ├── CustomDrawModeInitializer.cs     — [InitializeOnLoad] entry point, auto-injects Feature
│   ├── DrawModePlusModeRegistry.cs      — SceneView camera mode registration
│   ├── DrawModePlusControlWindow.cs     — EditorWindow for mode switching
│   ├── DrawModePlusRendererFeatureEditor.cs — Feature inspector GUI
│   ├── ResourceFinder.cs                — Loads textures from package
│   ├── SceneViewDebugOverLayer.cs       — Overlay for debug info
│   └── DrawModes/
│       ├── CustomDrawModeBase.cs        — Base class for draw modes
│       ├── DepthDrawMode.cs
│       ├── WorldNormalDrawMode.cs
│       ├── DeferredNormalBufferDrawMode.cs
│       ├── BaseColorDeferredDrawMode.cs
│       ├── DeferredAmbientOcclusionDrawMode.cs
│       ├── MetallicDeferredDrawMode.cs
│       ├── RoughnessDeferredDrawMode.cs
│       ├── TexelDensityDrawMode.cs
│       ├── UV0Checker.cs
│       └── StencilDrawMode.cs
├── Arts/                                — Shaders, materials, textures, demo assets
├── Demo.unity                           — Demo scene
└── Images/                              — GIF screenshots for docs
```

### Auto-Injection

On editor load, `CustomDrawModeInitializer` automatically:
1. Finds the active URP asset
2. Checks all RendererData for existing `DrawModePlusRendererFeature`
3. If missing, creates and injects a new Feature instance into the RendererData asset

This means the plugin works immediately after import — no manual RendererFeature configuration needed.

---

## 中文

### 支持的 Unity 版本

- 已测试 Unity 2022.3.62f3
- **仅支持 URP（通用渲染管线）** — 不支持 Built-in 和 HDRP
- 同时支持 **Forward** 和 **Deferred** 渲染路径

### 功能列表

| 模式 | 渲染路径 | 说明 |
|---|---|---|
| **Depth** | Forward / Deferred | 全屏深度可视化，可调节深度范围滑块（1–500m） |
| **WorldNormal (Forward)** | Forward | 世界空间法线可视化（全屏后处理） |
| **WorldNormal (Deferred)** | Deferred | 延迟渲染 GBuffer 法线可视化（支持法线贴图） |
| **BaseColor (Deferred)** | Deferred | 延迟渲染 GBuffer 基础色（Albedo）可视化 |
| **MaterialAO** | Deferred | 材质环境光遮蔽（GBuffer AO 通道） |
| **Metallic (Deferred)** | Deferred | 延迟渲染金属度通道 |
| **Roughness (Deferred)** | Deferred | 延迟渲染粗糙度（光滑度）通道 |
| **TexelDensity** | Forward / Deferred | 重新绘制场景物体以可视化纹理密度。图例：≤128 红、256 橙、512 绿、1024 青、≥2048 蓝。灰色 = 未适配的 shader。**需要项目 shader 适配 — 见下方说明。** |
| **UV0** | Forward / Deferred | UV0 棋盘格叠加显示 |
| **Stencil** | Forward / Deferred | Stencil 缓冲调试：写入模板值后通过后处理可视化 |

### 使用方式

#### 方式一：SceneView 相机下拉菜单

1. 打开 SceneView
2. 点击左上角 **Draw Mode** 下拉菜单（通常显示 "Shaded"）
3. 滚动到 **DrawModePlusMLS** 分组
4. 选择需要的调试模式

#### 方式二：控制面板

`Tools > DrawModePlus > DrawMode 显示控制面板`

可停靠的 EditorWindow，通过下拉菜单切换模式。Depth 模式下提供深度范围滑块。

### 安装

1. 将 `DrawModePlusMLS` 文件夹放入项目的 `Assets/Plugins/`（或任意 `Assets/` 目录）
2. 插件首次加载时自动向所有 URP RendererData 注入 `DrawModePlusRendererFeature`
3. 切换到 SceneView，从相机下拉菜单选择 draw mode 即可

### TexelDensity — 项目 Shader 适配指南

TexelDensity 模式需要**手动修改项目 shader**。未适配时，所有项目物体会渲染为灰色。

#### 第一步：为项目主 Shader 添加 LightMode Tag

在需要参与纹理密度检测的**每个 shader** 中添加一个 `LightMode = "DrawModePlusTexelDensity"` 的 Pass：

```hlsl
Pass
{
    Name "DrawModePlusTexelDensity"
    Tags { "LightMode" = "DrawModePlusTexelDensity" }

    HLSLPROGRAM
    #pragma vertex Vert
    #pragma fragment Frag

    // ... 引入项目的公共 HLSL 头文件 ...

    half4 Frag(Varyings input) : SV_Target
    {
        // 计算纹理密度
        float2 uv = input.uv0;                      // 或用网格的主 UV
        float2 texelSize = ...;                     // 从主纹理的 TexelSize 获取
        float2 ddxUV = ddx(uv);
        float2 ddyUV = ddy(uv);
        float texelDensity = ...;                   // 项目自己的纹理密度公式（如每世界单位的 texel 数）

        // 以 512 为基准归一化，输出为颜色
        float normalizedDensity = texelDensity / 512.0;
        return half4(normalizedDensity, normalizedDensity, normalizedDensity, 1.0);
    }
    ENDHLSL
}
```

**原理**：`TexelDensityDebugPass` 调用 `context.DrawRenderers()` 时传入 `ShaderTagId("DrawModePlusTexelDensity")`。Unity 只会渲染包含此 LightMode 标签 Pass 的 shader 物体。没有此标签的物体将使用 `FlatGray` shader 回退渲染为灰色。

#### 第二步：验证效果

在 SceneView 中切换到 TexelDensity 模式，底部会出现图例条：

```
Texel Density 512/m | <=128 Low | 256 Low | 512 OK | 1024 High | >=2048 High | Gray = 未适配shader
```

- **红 (≤128)**：纹理密度过低，贴图模糊，需提高分辨率或增大 UV 缩放
- **橙 (256)**：低于基准
- **绿 (512)**：符合基准密度 — 良好
- **青 (1024)**：高于基准
- **蓝 (≥2048)**：远高于基准 — 纹理内存浪费
- **灰**：未适配的 shader 物体

### Shader 依赖

以下 shader 必须存在于项目中（已包含在 `Arts/Shaders/` 中）：

| Shader | 用途 |
|---|---|
| `DrawModePlus/DepthView` | 深度可视化全屏 Pass |
| `DrawModePlus/WorldNormal` | Forward 法线可视化 |
| `DrawModePlus/DeferredNormalBuffer` | Deferred 法线缓冲读取 |
| `DrawModePlus/DeferredDebugView` | Deferred GBuffer 调试（BaseColor/Metallic/Roughness/AO） |
| `DrawModePlus/UV0Checker` | UV0 棋盘格 |
| `DrawModePlus/FlatGray` | TexelDensity 未适配物体回退 |
| `DrawModePlus/StencilWriter` | Stencil 写入 |
| `DrawModePlus/StencilChecker` | Stencil 可视化 |

这些 shader 通过 `Shader.Find()` 按名称引用，确保它们在 Resources 文件夹中或已加入 always-included shaders。

### 架构

```
DrawModePlusMLS/
├── Runtime/
│   ├── DrawModePlusRendererFeature.cs     — URP RendererFeature（自动注入）
│   ├── DrawModePlusRuntimeState.cs        — 全局状态 + DrawModePlusMode 枚举
│   ├── DrawModePlusRenderPipelineBridge.cs — URP 管线反射工具
│   └── Passes/
│       ├── FullscreenDebugPass.cs         — Depth / Normal 全屏 Blit
│       ├── SceneObjectDebugPass.cs        — 基类：重新绘制场景物体
│       ├── Uv0DebugPass.cs               — UV0 棋盘格（材质覆盖）
│       ├── TexelDensityDebugPass.cs      — 纹理密度（材质覆盖 + DrawModePlusTexelDensity 标签）
│       ├── StencilDebugPass.cs           — Stencil 写入 + 查看 Blit
│       ├── MaterialAOCapturePass.cs      — Deferred GBuffer 捕获（程序化四边形）
│       └── MaterialAOCompositePass.cs    — Deferred 调试合成
├── Editor/
│   ├── CustomDrawModeInitializer.cs      — [InitializeOnLoad] 入口，自动注入 Feature
│   ├── DrawModePlusModeRegistry.cs       — SceneView 相机模式注册
│   ├── DrawModePlusControlWindow.cs      — 模式切换 EditorWindow
│   ├── DrawModePlusRendererFeatureEditor.cs — Feature Inspector GUI
│   ├── ResourceFinder.cs                 — 资源加载
│   ├── SceneViewDebugOverLayer.cs        — 调试信息 Overlay
│   └── DrawModes/
│       ├── CustomDrawModeBase.cs         — DrawMode 基类
│       ├── DepthDrawMode.cs
│       ├── WorldNormalDrawMode.cs
│       ├── DeferredNormalBufferDrawMode.cs
│       ├── BaseColorDeferredDrawMode.cs
│       ├── DeferredAmbientOcclusionDrawMode.cs
│       ├── MetallicDeferredDrawMode.cs
│       ├── RoughnessDeferredDrawMode.cs
│       ├── TexelDensityDrawMode.cs
│       ├── UV0Checker.cs
│       └── StencilDrawMode.cs
├── Arts/                                 — Shader、材质、贴图、Demo 资产
├── Demo.unity                            — Demo 场景
└── Images/                               — 文档用 GIF 截图
```

### 自动注入

编辑器启动时，`CustomDrawModeInitializer` 自动：
1. 找到当前 URP 管线资产
2. 检查所有 RendererData 是否已包含 `DrawModePlusRendererFeature`
3. 如缺失，自动创建并注入 Feature 实例到 RendererData

这意味着导入插件后即可使用，无需手动配置 RendererFeature。
