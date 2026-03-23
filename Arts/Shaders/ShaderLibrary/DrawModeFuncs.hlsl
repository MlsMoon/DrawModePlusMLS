#pragma once

float _DrawModeIsForward;

void get_rendering_path_half(out float is_forward_rendering)
{
    // 由 Editor 端 C# 通过 Shader.SetGlobalInt/_Float 设置：
    // _DrawModeIsForward = 1 (Forward / Forward+), 0 (Deferred)
    is_forward_rendering = _DrawModeIsForward;
}

void get_rendering_path_float(out float is_forward_rendering)
{
    get_rendering_path_half(is_forward_rendering);
}