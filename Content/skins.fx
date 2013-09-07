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

//-------- Technique: TexturedSkin --------
VertexShaderSkinOutput TexturedSkinVS(float4 inPos : POSITION, float3 inNormal: NORMAL, float2 inTexCoords: TEXCOORD0)
{
	VertexShaderSkinOutput Output = (VertexShaderSkinOutput)0;
	
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
	Output.ambientLightColor = float4(1.0, 1.0, 1.0, 1.0);
	
	return Output;
}

float4 TexturedSkinPS(PixelShaderSkinInput Input) : COLOR
{
	float4 diffuseColor = tex2D(TextureSamplerSkin, Input.TextureCoords);

	// gamma correction
	diffuseColor = CalculateGamma(diffuseColor * xLightModel);
	diffuseColor.a = 1.0;
	
	return diffuseColor;
}
