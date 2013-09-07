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


// The initial phase of implementing bloom postprocess is to extract the brighter areas of a given image
float4 BloomExtractPS(float2 texCoord : TEXCOORD0) : COLOR0
{
	// get the original texture pixel color
	float4 Color = tex2D(TextureSampler, texCoord);
	
	// only retain the values that are brighter than the specified threshold
	return saturate((Color - xBloomThreshold) / (1 - xBloomThreshold));
}


// The final phase of implementing bloom postprocess is to combine the bloom image with the original scene
sampler BaseSampler : register(s1);
sampler BloomSampler : register(s0);

float4 AdjustSaturation(float4 Color, float Saturation)
{
	// the constants 0.3, 0.59 and 0.11 are chosen because the human eye
	// is more sensitive to green light and less to blue light.
	float Gray = dot(Color, float3(0.3, 0.59, 0.11));
	
	return lerp(Gray, Color, Saturation);
}

float4 BloomCombinePS(float2 texCoord : TEXCOORD0) : COLOR0
{
	// get the original base texture pixel color as well as the bloom pixel color
	float4 base = tex2D(BaseSampler, texCoord);
	float4 bloom = tex2D(BloomSampler, texCoord);
	
	// adjust pixel color saturation and intensity
	base = AdjustSaturation(base, xBaseSaturation) * xBaseIntensity;
	bloom = AdjustSaturation(bloom, xBloomSaturation) * xBloomIntensity;
	
	// darken the base image in areas where there is a lot of bloom
	// to prevent things looking excessively burned-out.
	base *= (1 - saturate(bloom));
	
	return base + bloom;
}
