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

#define LIGHT_COUNT 8

#include "generic.fx"
#include "skins.fx"
#include "textures.fx"
#include "bloom.fx"
#include "blur.fx"


// SKINS.FX
technique TexturedSkin
{
	pass Pass0
	{   
		VertexShader = compile vs_3_0 TexturedSkinVS();
		PixelShader  = compile ps_3_0 TexturedSkinPS();
	}
}


// TEXTURES.FX
technique Textured
{
	pass Pass0
	{   
		VertexShader = compile vs_3_0 TexturedVS();
		PixelShader  = compile ps_3_0 TexturedPS();
	}
}

technique TexturedLightmap
{
	pass Pass0
	{   
		VertexShader = compile vs_3_0 TexturedVS();
		PixelShader  = compile ps_3_0 TexturedLightmapPS();
	}
}

technique TexturedWarped
{
	pass Pass0
	{   
		VertexShader = compile vs_3_0 TexturedWarpedVS();
		PixelShader  = compile ps_3_0 TexturedPS();
	}
}

technique TexturedTranslucent
{
	pass Pass0
	{
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		VertexShader = compile vs_3_0 TexturedVS();
		PixelShader  = compile ps_3_0 TexturedTranslucentPS();
	}
}

technique TexturedWarpedTranslucent
{
	pass Pass0
	{
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		VertexShader = compile vs_3_0 TexturedWarpedVS();
		PixelShader  = compile ps_3_0 TexturedTranslucentPS();
	}
}

technique TexturedSkybox
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 TexturedSkyboxVS();
		PixelShader  = compile ps_3_0 TexturedSkyboxPS();
	}
}

technique TexturedLight
{
	pass Pass0
	{   
		VertexShader = compile vs_3_0 TexturedLightVS();
		PixelShader  = compile ps_3_0 TexturedLightPS();
	}
}


// BLOOM.FX
technique BloomExtract
{
	pass Pass0
	{
		PixelShader  = compile ps_3_0 BloomExtractPS();
	}
}

technique BloomCombine
{
	pass Pass0
	{
		PixelShader  = compile ps_3_0 BloomCombinePS();
	}
}


// BLUR.FX (this is used twice by the bloom postprocess, blurring horizontally, then vertically)
technique GaussianBlur
{
	pass Pass0
	{
		PixelShader  = compile ps_3_0 GaussianBlurPS();
	}
}
