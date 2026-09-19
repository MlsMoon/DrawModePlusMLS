# DrawModePlusMLS

[English](../README.md) | 简体中文

为 [Universal Render Pipeline](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/index.html) 增加 SceneView 调试绘制模式。可在 SceneView 相机下拉菜单中查看深度、法线、UV、GBuffer 通道、纹理密度、Stencil 和反射探针。

> 本插件仅用于编辑器。它会自动注入 URP Renderer Feature，再重绘或 Blit 调试视图，不改玩法材质。

**给 Agent：** 先读仓库根目录 [`AGENTS.md`](../AGENTS.md)。技能在 [`Skills~/`](../Skills~/README.md)（Unity 会忽略 `~` 目录）。机器可读索引：[`llms.txt`](../llms.txt)。不要按旧 README 猜测 API，以 `AGENTS.md` 的当前合同为准。

## 环境要求

- Unity 2022.3+（已在 2022.3.62f3 验证）
- Universal Render Pipeline (URP) 14
- **仅支持 URP** — 不支持 Built-in 和 HDRP
- 同时支持 Forward 和 Deferred

## 快速开始

1. 安装插件（见[安装](#安装)）。
2. 打开 SceneView。
3. 点击左上角 **Draw Mode** 下拉菜单（通常显示 `Shaded`）。
4. 滚动到 **DrawModePlusMLS** 分组并选择模式。

可选：`Tools > DrawModePlus > Draw Mode Control Panel` 提供可停靠切换器、Depth 范围滑块，以及 Reflection 的 Force Mirror / Global Roughness。

首次加载时，插件会向当前管线的每个 URP Renderer Data 注入 `DrawModePlusRendererFeature`，无需手动配置。

## 安装

### 复制到 `Assets/`

把 `DrawModePlusMLS` 文件夹放到工程任意 `Assets/` 路径下（例如 `Assets/Plugins/DrawModePlusMLS`）。

### Unity Package Manager（Git）

在 Unity：**Window > Package Manager > + > Add package from git URL**

```text
https://github.com/MlsMoon/DrawModePlusMLS.git
```

需要固定版本时加上 commit 或 tag：

```text
https://github.com/MlsMoon/DrawModePlusMLS.git#v0.2.0
```

包名是 `com.mlsmoon.drawmodeplus`。导入并编译后打开 SceneView 即可。

### 安装后

1. 控制台应出现一次 `DrawModePlusMLS: Injected DrawModePlusRendererFeature into ...`。
2. 从 SceneView 下拉菜单或控制面板选择模式。
3. 如需 Game 相机也显示调试视图，在注入的 Feature 上勾选 **Enable Game View**。

## 功能

| 模式 | 路径 | 说明 |
|---|---|---|
| **Depth** | Forward / Deferred | 全屏深度，范围滑块 1–500 m |
| **WorldNormal (Forward)** | Forward | 世界空间法线（全屏 Pass） |
| **WorldNormal (Deferred)** | Deferred | GBuffer 法线，含法线贴图 |
| **BaseColor (Deferred)** | Deferred | GBuffer 基础色 |
| **MaterialAO** | Deferred | GBuffer 环境光遮蔽 |
| **Metallic (Deferred)** | Deferred | GBuffer 金属度 |
| **Roughness (Deferred)** | Deferred | GBuffer 粗糙度（由光滑度得出） |
| **TexelDensity** | Forward / Deferred | 重绘带 `DrawModePlusTexelDensity` 的物体。图例：≤128 红、256 橙、512 绿、1024 青、≥2048 蓝。灰色 = 未适配 shader。**宿主 shader 必须自行加 Pass。** |
| **UV0** | Forward / Deferred | UV0 棋盘格叠加 |
| **Stencil** | Forward / Deferred | 写入 Stencil 后再 Blit 可视化 |
| **Reflection** | Forward / Deferred | 清屏后用 `DrawModePlus/ReflectionView` 重绘。按网格法线采样每个物体自己的 Probe/Sky。默认 **Force Mirror** 为 chrome；关闭后用全局 roughness，不是原材质粗糙度。没有 `UniversalForward` / `UniversalGBuffer` / `UniversalForwardOnly` / `SRPDefaultUnlit` 的物体会被跳过，与 UV0 相同。 |

## 截图

| Depth | Depth 范围 | World Normal | UV0 |
|---|---|---|---|
| ![Depth](images/DepthView.gif) | ![Depth slider](images/DepthViewSlider.gif) | ![World Normal](images/WorldNormal.gif) | ![UV0](images/UV0Checker.gif) |

## Reflection 模式

Reflection 是 Renderer Feature override，不是 URP Lighting Debug：

- 清掉颜色后再用 `DrawModePlus/ReflectionView` 做 `DrawRenderers`
- 原材质和其他宿主调试视图不会写进这个缓冲
- **Force Mirror**（默认）：perceptual roughness = 0，所有物体看同一套 chrome IBL
- 关闭 Force Mirror：用全局 roughness 滑条，仍然不是原材质粗糙度
- 使用网格法线和每个物体自己的 `unity_SpecCube0` / Box Projection
- 不包含其他 Feature 的屏幕空间反射。需要隔离 SSR 时，请用那个 Feature 自己的调试视图

## TexelDensity — 宿主 Shader 适配

没有 `LightMode = "DrawModePlusTexelDensity"` 的 Pass 时，物体会画成灰色。

```hlsl
Pass
{
    Name "DrawModePlusTexelDensity"
    Tags { "LightMode" = "DrawModePlusTexelDensity" }

    HLSLPROGRAM
    #pragma vertex Vert
    #pragma fragment Frag

    half4 Frag(Varyings input) : SV_Target
    {
        float2 uv = input.uv0;
        float2 ddxUV = ddx(uv);
        float2 ddyUV = ddy(uv);
        float texelDensity = /* 每世界单位的 texel 数 */;
        float normalizedDensity = texelDensity / 512.0;
        return half4(normalizedDensity, normalizedDensity, normalizedDensity, 1.0);
    }
    ENDHLSL
}
```

`TexelDensityDebugPass` 使用 `ShaderTagId("DrawModePlusTexelDensity")` 绘制。没有该标签的物体回退到 `DrawModePlus/FlatGray`。

SceneView 底部图例：

```text
Texel Density 512/m | <=128 Low | 256 Low | 512 OK | 1024 High | >=2048 High | Gray = 未适配 shader
```

完整说明见 `Skills~/drawmodeplus-use-plugin/references/texel-density.md`。

## Shader

位于 `Arts/Shaders/`，通过 `Shader.Find` 按名称查找。**不必**放进 `Resources` 文件夹，但必须留在已导入的插件内。

| Shader | 用途 |
|---|---|
| `DrawModePlus/DepthView` | 深度全屏 Pass |
| `DrawModePlus/WorldNormal` | Forward 法线 |
| `DrawModePlus/DeferredNormalBuffer` | Deferred 法线缓冲 |
| `DrawModePlus/DeferredDebugView` | Deferred GBuffer（BaseColor / Metallic / Roughness / AO） |
| `DrawModePlus/UV0Checker` | UV0 棋盘格 |
| `DrawModePlus/FlatGray` | TexelDensity 回退 |
| `DrawModePlus/StencilWriter` | Stencil 写入 |
| `DrawModePlus/StencilChecker` | Stencil 可视化 |
| `DrawModePlus/ReflectionView` | override chrome / IBL 重绘 |

## 示例

等 Renderer Feature 注入后打开 `Example/Demo.unity`。示例网格、材质和 NormalMap Prefab 在 `Example/` 与 `Arts/`。详见 `Example/README.md`。

## 目录

```text
DrawModePlusMLS/
├── Runtime/                 URP Renderer Feature、状态、调试 Pass
├── Editor/                  SceneView 模式、控制面板、自动注入
├── Arts/                    Shader、材质、贴图
├── Example/                 示例场景和 Prefab
├── Docs/                    翻译和截图 GIF
├── Skills~/                 Agent 技能（Unity 会忽略）
├── .mlsmoon/                给 Moon Game Dev Tool Manager 的路由 Skill
├── AGENTS.md                Agent 路由和当前合同
└── README.md
```

## Agent 技能

`Skills~/`（Unity 会忽略 `~` 目录）：

- `drawmodeplus-use-plugin` — 安装、模式、纹理密度适配、排错
- `drawmodeplus-develop-plugin` — 架构、新增模式、验证

Moon Game Dev Tool Manager 会从 `.mlsmoon/` 安装薄路由 Skill `draw-mode-plus-mls-skill`。改 `Skills~/`，不要改路由稿。不要把 `Skills~/` 当独立 Skill 拷进宿主 `.agents/skills`。在本仓库请先读 `AGENTS.md`。

## 参与贡献

见 [`CONTRIBUTING.md`](../CONTRIBUTING.md)。改 Runtime 或 Editor 前请先读 `AGENTS.md`。

## 许可

MIT — 见 [`LICENSE`](../LICENSE)。第三方示例资源见 [`THIRD_PARTY.md`](../THIRD_PARTY.md)。
