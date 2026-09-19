#define CURRENT_PS(a) ps_3_0 a()
#define CURRENT_VS(a) vs_3_0 a()
sampler iChannel1 : register(s0);
float2 iChannelResolution1;

float randomizer;
float2 resoolution;
#define resolution float2(1920, 1080)
float time;
float scale;
float2 gamePadding;

float2 offsetOfObservation;
#define offset float2(0, 0)

struct PixelInput
{
    float2 UVMapping : TEXCOORD0;
};
struct PixelOutput
{
    float4 Color : COLOR0;
};
struct VertexInput
{
    float4 Position : POSITION0;
    float2 Down : POSITION1;
};
struct VertexOutput
{
    float4 Position : SV_Position;
    float2 UVMapping : TEXCOORD0;
};

struct VertexInput2
{
    float4 Position : POSITION0;
    float4 Offset : POSITION1;
};
struct VertexOutput2
{
    float4 Position : SV_Position;
    float2 UVMapping : TEXCOORD0;
    float2 Center : TEXCOORD1;
    float Base : TEXCOORD2;
};
struct PixelInput2
{
    float2 UVMapping : TEXCOORD0;
    float2 Center : TEXCOORD1;
    float Base : TEXCOORD2;
};


float2 Rotate2D(float2 v, float angle)
{
    float rad = radians(angle);
    float s = sin(rad);
    float c = cos(rad);
    return float2(v.x * c - v.y * s, v.x * s + v.y * c);
}

VertexOutput paddingMapper(VertexInput input)
{
    VertexOutput output;
    output.Position = input.Position;
    output.UVMapping = input.Down;
    return output;
}

// https://dekoolecentrale.nl/wgsl-fns/hash31
float hash21(float2 p)
{
    float2 _p3 = frac(p * float2(716458. / 2157763, 32192. / 54907));
    float3 p3 = float3(_p3, randomizer);
    p3 += dot(p3, p3.yxz + 33.33);
    return frac((p3.x + p3.y) * p3.z);
}

float weight(float2 from, float2 self)
{
    float f = hash21(from) * 2 * 3.1415926538;
    float a, b;
    sincos(f, a, b);
    float2 g = float2(a, b);
    return dot(self - from, g);
}

// perlin nise
float noise(float2 coord, float res)
{
    float2 as = coord / res;
    float2 root = floor(as);
    float2 dx = frac(as);
    float a = weight(root + float2(0, 0), as);
    float s = weight(root + float2(1, 0), as);
    float d = weight(root + float2(0, 1), as);
    float f = weight(root + float2(1, 1), as);

    float2 tx = dx * dx * dx * (dx * (dx * 6.0 - 15.0) + 10.0);
    float n1 = lerp(a, s, tx.x);
    float n0 = lerp(d, f, tx.x);

    return lerp(n1, n0, tx.y);
}

// https://dekoolecentrale.nl/wgsl-fns/hash31
float2 hash22(float2 p)
{
    float2 _p3 = frac(p * float2(695050. / 780583, 147474. / 156797));
    float3 p3 = float3(_p3, randomizer);
    p3 += dot(p3, p3.yxz + 33.33);
    return frac((p3.xx + p3.yx) * p3.zy);
}

float2 weight2(float2 from, float2 self)
{
    float2 f = hash22(from) * 2 * 3.1415926538;
    float2 a, b;
    sincos(f, a, b);
    float2x2 g = float2x2(a, b);
    return mul(g, self - from);
}

float2 noise2(float2 coord, float res)
{
    float2 as = coord / res;
    float2 root = floor(as);
    float2 dx = frac(as);
    float2 a = weight2(root + float2(0, 0), as);
    float2 s = weight2(root + float2(1, 0), as);
    float2 d = weight2(root + float2(0, 1), as);
    float2 f = weight2(root + float2(1, 1), as);

    float2 tx = dx * dx * dx * (dx * (dx * 6.0 - 15.0) + 10.0);
    float2 n1 = lerp(a, s, tx.x);
    float2 n0 = lerp(d, f, tx.x);

    return lerp(n1, n0, tx.y);
}

float3 parallel(float h)
{
    float3 colorB;
    if (h < 0.245)
        colorB = float3(0.99, 0.49, 0.42);
    else if (h < 0.255)
        colorB = float3(0.77, 0.38, 0.37);
    else if (h < 0.6)
        colorB = float3(0.99, 0.66, 0.45);
    else if (h < 0.608)
        colorB = float3(0.86, 0.42, 0.38);
    else
        colorB = float3(0.99, 0.77, 0.5);
    return colorB;
}

PixelOutput mainParallel(PixelInput input)
{
    float2 at = resolution * input.UVMapping.xy + offset;
    float main = noise(at + float2(40, -40) * time, 256.);
    float side = noise(at + float2(52, -28) * (time + 5.), 64.);

    float ret = main - side * 0.08;
    float3 color = parallel(0.25 + ret * 1.1);
    PixelOutput output;
    output.Color = float4(color, 1.0);
    return output;
}

float3 interference(float h)
{
    float3 colorB;
    if (h < 0.35)
        colorB = float3(0.5, 0.61, 0.98);
    else if (h < 0.4)
        colorB = float3(0.4, 0.46, 0.98);
    else if (h < 0.6)
        colorB = float3(0.41, 0.5, 0.98);
    else if (h < 0.65)
        colorB = float3(0.31, 0.33, 0.84);
    else
        colorB = float3(0.31, 0.35, 0.91);
    return colorB;
}

PixelOutput mainInterference(PixelInput input)
{
    float2 at = resolution * input.UVMapping.xy + offset;
    float main = noise(at + float2(-64, -64) * time, 256.);
    float side = noise(at + float2(160, -8) * (time + 5.), 1024.);

    float ret = main - side * .9;
    float3 color = interference(0.5 + ret * 1.1);
    PixelOutput output;
    output.Color = float4(color, 1.0);
    return output;
}

#define GOT_RADIUS 600
#define MyRotation 160
VertexOutput2 VSObservationCircle(VertexInput2 input)
{
    VertexOutput2 output;
    float2 pos = input.Position.xy - offsetOfObservation;
    output.Center = pos;
    float main = noise(input.Position.xy + float2(-64, -64) * time, 1024.);
    float scalea = clamp(main * 0.8 + 0.6, 0, 1);
    float2 at = pos + input.Offset.xy * scalea * GOT_RADIUS;
    output.UVMapping = at;
    output.Base = 1 - scalea;
    output.Position = float4(at / resolution * 2 - 1, 0, 1);
    return output;
}

PixelOutput mainObservationCircle(PixelInput2 input)
{
    float lens = length(input.Center - input.UVMapping) / GOT_RADIUS;
    float colory = clamp(1 - lens - input.Base, 0, 1);
    PixelOutput output;
    output.Color = float4(colory, colory, colory, colory);
    return output;
}

float3 observation(float h)
{
    float3 colorB;
    if (h < 0.25)
        colorB = float3(0.1, 0.37, 0.26);
    else if (h < 0.34)
        colorB = float3(0.09, 0.31, 0.32);
    else if (h < 0.43)
        colorB = float3(0.1, 0.37, 0.26);
    else if (h < 0.52)
        colorB = float3(0.15, 0.54, 0.4);
    else if (h < 0.61)
        colorB = float3(0.1, 0.37, 0.26);
    else if (h < 0.70)
        colorB = float3(0.09, 0.31, 0.32);
    else
        colorB = float3(0.1, 0.37, 0.26);
    return colorB;
}

PixelOutput mainObservation(PixelInput input)
{
    float side = noise(resolution * input.UVMapping.xy + offsetOfObservation * float2(1, -1) + float2(160, -8) * (time + 5.), 512.);
    float main = tex2D(iChannel1, input.UVMapping).x;
    float ret = main - side * .12;
    float3 color = observation(ret * 1.1);
    PixelOutput output;
    output.Color = float4(color, 1.0);
    return output;
}

float3 emergence(float h)
{
    float3 colorB;
    if (h < 0.45)
        colorB = float3(0.86, 0.95, 0.84);
    else if (h < 0.8)
        colorB = float3(0.76, 0.85, 0.8);
    else
        colorB = float3(0.72, 0.69, 0.72);
    return colorB;
}

PixelOutput mainEmergence(PixelInput input)
{
    float2 at = resolution * input.UVMapping.xy + offset;
    float main = noise(at + float2(96, 16) * time, 1024.);
    float side = noise(at + float2(72, -4) * (time + 5.), 192.);

    float ret = main - side * .2;
    float3 color = emergence(0.5 + ret * 1.1);
    PixelOutput output;
    output.Color = float4(color, 1.0);
    return output;
}

float3 entanglement(float h)
{
    float3 colorB;
    if (h < 0.3)
        colorB = float3(0.36, 0.71, 0.88);
    else if (h < 0.4)
        colorB = float3(0.36, 0.5, 0.69);
    else if (h < 0.6)
        colorB = float3(0.65, 0.91, 0.98);
    else if (h < 0.7)
        colorB = float3(1., 1., 1.);
    else
        colorB = float3(0.98, 0.79, 0.91);
    return colorB;
}

PixelOutput mainEntanglement(PixelInput input)
{
    float2 at = resolution * input.UVMapping.xy + offset;
    at += noise2(at + float2(50, 50) * time, 64.) * 0.13 * 32;
    float main = noise(at + float2(-80, 0) * time, 256.);
    float side = noise(at + float2(-30, 10) * (time + 5.), 1024.);

    float ret = main - side * 0.5;
    float3 color = entanglement(0.5 + ret * 1.1);
    PixelOutput output;
    output.Color = float4(color, 1.0);
    return output;
}

float3 overflow(float h)
{
    float3 colorB;
    if (h < 0.3)
        colorB = float3(0.99, 0.69, 0.84);
    else if (h < 0.33)
        colorB = float3(0.72, 0.24, 0.56);
    else if (h < 0.67)
        colorB = float3(0.99, 0.47, 0.76);
    else if (h < 0.7)
        colorB = float3(0.66, 0.24, 0.59);
    else
        colorB = float3(0.72, 0.38, 0.81);
    return colorB;
}

PixelOutput mainOverflow(PixelInput input)
{
    float d = -10;
    float2 cur = Rotate2D(input.UVMapping + offset * float2(1, -1) / resolution, d) * resolution;
    float at = (cur.x + cos(cur.y / 64.0) * 384.0) / 96.0 + time / 1.875;

    float3 color = overflow(0.5 + 0.5 * cos(at) + sin(time / 2.75) * 0.17);
    PixelOutput output;
    output.Color = float4(color, 1.0);
    return output;
}

technique Parallel
{
    pass Pass1
    {
        VertexShader = compile CURRENT_VS(paddingMapper);
        PixelShader = compile CURRENT_PS(mainParallel);
    }
}
technique Interference
{
    pass Pass1
    {
        VertexShader = compile CURRENT_VS(paddingMapper);
        PixelShader = compile CURRENT_PS(mainInterference);
    }
}
technique Observation
{
    pass Pass1
    {
        VertexShader = compile CURRENT_VS(VSObservationCircle);
        PixelShader = compile CURRENT_PS(mainObservationCircle);
    }
    pass Pass2
    {
        VertexShader = compile CURRENT_VS(paddingMapper);
        PixelShader = compile CURRENT_PS(mainObservation);
    }
}
technique Emergence
{
    pass Pass1
    {
        VertexShader = compile CURRENT_VS(paddingMapper);
        PixelShader = compile CURRENT_PS(mainEmergence);
    }
}
technique Entanglement
{
    pass Pass1
    {
        VertexShader = compile CURRENT_VS(paddingMapper);
        PixelShader = compile CURRENT_PS(mainEntanglement);
    }
}
technique Overflow
{
    pass Pass1
    {
        VertexShader = compile CURRENT_VS(paddingMapper);
        PixelShader = compile CURRENT_PS(mainOverflow);
    }
}
