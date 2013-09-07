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

float numLights = LIGHT_COUNT;
shared Light lights[LIGHT_COUNT];


//-------- Technique: Textured --------
VertexShaderOutput TexturedVS(float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0, float2 inLightCoords: TEXCOORD1)
{
	VertexShaderOutput Output = (VertexShaderOutput)0;
	
	// generate the viewprojection and worldviewprojection
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	
	// transform the input position to the output
	Output.Position = mul(inPos, preWorldViewProjection);
	
	Output.WorldNormal = normalize(mul(inNormal, (float3x3)xWorld));
	Output.WorldPosition = mul(inPos, xWorld);
	
	// copy the tex coords to the interpolator
	Output.TextureCoords = inTexCoords;
	
	// copy the lightmap coords to the interpolator
	Output.LightmapCoords = inLightCoords;
	
	// copy the ambient lighting
	Output.ambientLightColor = float4(1.0, 1.0, 1.0, 1.0);
	
	return Output;
}

float4 TexturedPS(PixelShaderInput Input) : COLOR
{
	float4 diffuseColor = tex2D(TextureSamplerDiffuse, Input.TextureCoords);

	// gamma correction
	diffuseColor = CalculateGamma(diffuseColor);
	diffuseColor.a = 1.0;
	
	return diffuseColor;
}


//-------- Technique: TexturedWarped --------
VertexShaderOutput TexturedWarpedVS(float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0, float2 inLightCoords: TEXCOORD1)
{
	VertexShaderOutput Output = (VertexShaderOutput)0;
	
	// generate the viewprojection and worldviewprojection
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	
	// transform the input position to the output
	Output.Position = mul(inPos, preWorldViewProjection);
	
	Output.WorldNormal = normalize(mul(inNormal, (float3x3)xWorld));
	Output.WorldPosition = mul(inPos, xWorld);
	
	// copy the tex coords to the interpolator
	Output.TextureCoords = CalculateWarp(inTexCoords);
	
	// copy the lightmap coords to the interpolator
	Output.LightmapCoords = inLightCoords;
	
	// copy the ambient lighting
	Output.ambientLightColor = float4(1.0, 1.0, 1.0, 1.0);
	
	return Output;
}


//-------- Technique: TexturedLightmap --------
float4 TexturedLightmapPS(PixelShaderInput Input) : COLOR
{
	float4 Color;
	float4 TextureColor = tex2D(TextureSamplerDiffuse, Input.TextureCoords);
	float4 LightmapColor = tex2D(TextureSamplerLightmap, Input.LightmapCoords);
	
	//all color components are summed in the pixel shader
	if(xPointLights == true)
	{
		for(int i = 0; i < LIGHT_COUNT; i++)
		{
			LightmapColor += CalculateSingleLightmap(lights[i], Input.WorldPosition, Input.WorldNormal, LightmapColor); // * (i < numLights);
		}
	}
	
	// gamma correction
	Color = CalculateGamma(TextureColor * LightmapColor);
	Color.a = 1.0;
	
	return Color;
}


//-------- Technique: TexturedTranslucent --------
float4 TexturedTranslucentPS(PixelShaderInput Input) : COLOR
{
	float4 diffuseColor = tex2D(TextureSamplerDiffuse, Input.TextureCoords);

	// gamma correction
	diffuseColor = CalculateGamma(diffuseColor);
	diffuseColor.a = xTextureAlpha;
	
	return diffuseColor;
}


//-------- Technique: TexturedSkybox --------
VertexShaderOutput_Skybox TexturedSkyboxVS(float4 inPos : POSITION, float2 inTexCoords: TEXCOORD0)
{
	VertexShaderOutput_Skybox Output = (VertexShaderOutput_Skybox)0;
	
	// generate the viewprojection and worldviewprojection
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	
	// transform the input position to the output
	Output.Position = mul(inPos, preWorldViewProjection);
	Output.WorldPosition = mul(inPos, xWorld);
	
	// copy the tex coords to the interpolator
	Output.TextureCoords = inTexCoords;
	
	return Output;
}

float4 TexturedSkyboxPS(PixelShaderInput_Skybox Input) : COLOR
{
	float4 diffuseColor = tex2D(TextureSamplerDiffuse, Input.TextureCoords);
	
	// gamma correction
	diffuseColor = CalculateGamma(diffuseColor);
	diffuseColor.a = 1.0;
	
	return diffuseColor;
}


//-------- Technique: TexturedLight --------
VertexShaderOutput TexturedLightVS(float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0)
{
	VertexShaderOutput Output = (VertexShaderOutput)0;
	
	// generate the viewprojection and worldviewprojection
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	
	// transform the input position to the output
	Output.Position = mul(inPos, preWorldViewProjection);
	
	Output.WorldNormal = normalize(mul(inNormal, (float3x3)xWorld));
	Output.WorldPosition = mul(inPos, xWorld);
	
	// copy the tex coords to the interpolator
	Output.TextureCoords = inTexCoords;
	
	// copy the ambient lighting
	Output.ambientLightColor = xLightAmbient;
	
	return Output;
}

float4 TexturedLightPS(PixelShaderInput Input) : COLOR
{
	float4 diffuseColor = tex2D(TextureSamplerDiffuse, Input.TextureCoords);
	float4 specularColor = float4(0.15, 0.15, 0.15, 0.15);
	float4 Color = Input.ambientLightColor * diffuseColor;
	
	//all color components are summed in the pixel shader
	if(xPointLights == true)
	{
		for(int i = 0; i < LIGHT_COUNT; i++)
		{
			Color += CalculateSingleLight(lights[i], Input.WorldPosition, Input.WorldNormal, Input.TextureCoords, diffuseColor, specularColor); // * (i < numLights);
		}
	}
	
	// gamma correction
	Color = CalculateGamma(Color);
	Color.a = 1.0;
	
	return Color;
}
