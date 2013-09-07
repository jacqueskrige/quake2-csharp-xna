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


// Applies a one dimensional gaussian blur filter
#define SAMPLE_COUNT 15

float2 SampleOffsets[SAMPLE_COUNT];
float SampleWeights[SAMPLE_COUNT];

float4 GaussianBlurPS(float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 Color = float4(0.0, 0.0, 0.0, 0.0);
	
	// combine a number of weighted image filter taps
	for (int i = 0; i < SAMPLE_COUNT; i++)
	{
		Color += tex2D(TextureSampler, texCoord + SampleOffsets[i]) * SampleWeights[i];
	}
	
	return Color;
}
