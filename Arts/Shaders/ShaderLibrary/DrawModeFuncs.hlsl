#pragma once

float _DrawModeIsForward;

void get_rendering_path_half(out float is_forward_rendering)
{
    // Set from editor C# via Shader.SetGlobalInt / SetGlobalFloat:
    // _DrawModeIsForward = 1 (Forward / Forward+), 0 (Deferred)
    is_forward_rendering = _DrawModeIsForward;
}

void get_rendering_path_float(out float is_forward_rendering)
{
    get_rendering_path_half(is_forward_rendering);
}