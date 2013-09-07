/*
===========================================================================
Copyright (C) 2000-2011 Korvin Korax
Author: Jacques Krige
http://www.sagamedev.com
http://www.korvinkorax.com

This file is part of Quake2 BSP XNA Renderer source code.
Parts of the source code is copyright (C) Id Software, Inc.

Quake2 BSP XNA Renderer source code is free software; you can redistribute it
and/or modify it under the terms of the GNU General Public License as
published by the Free Software Foundation; either version 2 of the License,
or (at your option) any later version.

Quake2 BSP XNA Renderer source code is distributed in the hope that it will be
useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Foobar; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
===========================================================================
*/

struct Light
{
    float4 color;
    float4 position;
    float falloff;
    float range;
};

struct VertexShaderOutput
{
     float4 Position			: POSITION;
     float2 TextureCoords		: TEXCOORD0;
     float2 LightmapCoords		: TEXCOORD1;
     float3 WorldNormal			: TEXCOORD2;
     float3 WorldPosition		: TEXCOORD3;
     float4 ambientLightColor	: COLOR0;
};

struct PixelShaderInput
{
     float2 TextureCoords		: TEXCOORD0;
     float2 LightmapCoords		: TEXCOORD1;
     float3 WorldNormal			: TEXCOORD2;
     float3 WorldPosition		: TEXCOORD3;
     float4 ambientLightColor	: COLOR0;
};


struct VertexShaderSkinOutput
{
     float4 Position			: POSITION;
     float2 TextureCoords		: TEXCOORD0;
     float3 WorldNormal			: TEXCOORD2;
     float3 WorldPosition		: TEXCOORD3;
     float4 ambientLightColor	: COLOR0;
};

struct PixelShaderSkinInput
{
     float2 TextureCoords		: TEXCOORD0;
     float3 WorldNormal			: TEXCOORD2;
     float3 WorldPosition		: TEXCOORD3;
     float4 ambientLightColor	: COLOR0;
};


struct VertexShaderOutput_Skybox
{
     float4 Position		: POSITION;
     float2 TextureCoords	: TEXCOORD0;
     float3 WorldPosition	: TEXCOORD1;
};

struct PixelShaderInput_Skybox
{
     float2 TextureCoords	: TEXCOORD0;
     float3 WorldPosition	: TEXCOORD1;
};


// -------- XNA to HLSL variables --------
float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;
float4 xLightModel;
float4 xLightAmbient;
float xLightPower;
float xTextureAlpha;
float xGamma;
float xRealTime;
float xBloomThreshold;
float xBaseIntensity;
float xBloomIntensity;
float xBaseSaturation;
float xBloomSaturation;
bool xPointLights;
bool xFlow;


// 7:  GaussianQuad  - A 4-sample Gaussian filter used as a texture magnification or minification filter. 
// 6:  PyramidalQuad - A 4-sample tent filter used as a texture magnification or minification filter. 
// 3:  Anisotropic   - Anisotropic texture filtering used as a texture magnification or minification filter. This type of filtering compensates for distortion caused by the difference in angle between the texture polygon and the plane of the screen. 
// 2:  Linear        - Bilinear interpolation filtering used as a texture magnification or minification filter. A weighted average of a 2x2 area of texels surrounding the desired pixel is used. The texture filter used between mipmap levels is trilinear mipmap interpolation, in which the rasterizer performs linear interpolation on pixel color, using the texels of the two nearest mipmap textures.  
// 1:  Point         - Point filtering used as a texture magnification or minification filter. The texel with coordinates nearest to the desired pixel value is used. The texture filter used between mipmap levels is based on the nearest point; that is, the rasterizer uses the color from the texel of the nearest mipmap texture. 
// 0:  None          - Mipmapping disabled. The rasterizer uses the magnification filter instead. 


// -------- Texture Samplers --------
Texture xTextureDiffuse;
sampler TextureSamplerDiffuse = sampler_state {
		texture = <xTextureDiffuse>;
		MagFilter = Anisotropic;
		MinFilter = Anisotropic;
		MipFilter = Linear;
		MaxAnisotropy = 8;
		AddressU = Wrap;
		AddressV = Wrap;
};

Texture xTextureLightmap;
sampler TextureSamplerLightmap = sampler_state {
		texture = <xTextureLightmap>;
		MagFilter = Anisotropic;
		MinFilter = Anisotropic;
		MipFilter = LINEAR;
		MaxAnisotropy = 8;
		AddressU = Wrap;
		AddressV = Wrap;
};

Texture xTextureSkin;
sampler TextureSamplerSkin = sampler_state {
		texture = <xTextureSkin>;
		MagFilter = Anisotropic;
		MinFilter = Anisotropic;
		MipFilter = Linear;
		MaxAnisotropy = 8;
		AddressU = Clamp;
		AddressV = Clamp;
};

sampler TextureSampler : register(s0);


float4 CalculateSingleLightmap(Light light, float3 worldPosition, float3 worldNormal, float4 LightmapColor)
{
	float3 lightVector = light.position - worldPosition;
    float lightDist = length(lightVector);
    float3 directionToLight = normalize(lightVector);
    
    // calculate the intensity of the light with exponential falloff
	float baseIntensity = pow(saturate((light.range - lightDist) / light.range), light.falloff);
	float diffuseIntensity = saturate(dot(directionToLight, worldNormal));
	diffuseIntensity *= xLightPower;
	
	// modify the lightmap pixels
	float4 diffuse = diffuseIntensity * light.color;
	float blendfactor = min(1, lightDist / light.range);
	float4 lightmapPixel = (1 - blendfactor) * diffuse + blendfactor * LightmapColor;
	
	
	return lightmapPixel * baseIntensity;
}

float4 CalculateSingleLight(Light light, float3 worldPosition, float3 worldNormal, float2 TextureCoords, float4 diffuseColor, float4 specularColor)
{
	float3 lightVector = light.position - worldPosition;
    float lightDist = length(lightVector);
    float3 directionToLight = normalize(lightVector);
    
    //calculate the intensity of the light with exponential falloff
	float baseIntensity = pow(saturate((light.range - lightDist) / light.range), light.falloff);
	float diffuseIntensity = saturate(dot(directionToLight, worldNormal));
	diffuseIntensity *= xLightPower;
	
	float4 diffuse = diffuseIntensity * light.color * diffuseColor;
	
	//calculate phong components per-pixel
	//float3 reflectionVector = normalize(reflect(directionToLight, worldNormal));
	//float3 directionToCamera = normalize(cameraPosition - worldPosition);
	
	//calculate specular component
	//float4 specular = saturate(light.color * specularColor * specularIntensity * pow(saturate(dot(reflectionVector, directionToCamera)), specularPower));
	//return (diffuse + specular) * baseIntensity;
	
	return diffuse * baseIntensity;
}

float4 CalculateGamma(float4 color)
{
	return log(1 + color) * xGamma;
}

float2 CalculateWarp(float2 TexCoords)
{
	float2 TC;
	float TurbScale = (256.0f / (2 * 3.141592));
	float Flowing = 0.0f;
	
	if (xFlow == true)
		Flowing = -64.0f * ((xRealTime * 0.5f) - (int)(xRealTime * 0.5f));
	
	// muliplying the result by 3.0 to make the warping more dramatic
	TC.x = TexCoords.x + sin(radians((TexCoords.y * 0.125f + xRealTime) * TurbScale)) * 3.0f;
	TC.x += Flowing;
	TC.x *= (1.0f / 64.0f);
	TC.y = TexCoords.y + sin(radians((TexCoords.x * 0.125f + xRealTime) * TurbScale)) * 3.0f;
	TC.y *= (1.0f / 64.0f);
	
	return TC;
}
