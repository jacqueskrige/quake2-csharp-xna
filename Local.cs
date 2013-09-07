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

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Q2BSP
{
    public class CLocal
    {
        public const int MAX_LIGHTSTYLES = 256;
        public const int TEXNUM_LIGHTMAPS = 1024;
        public const int MAX_LBM_HEIGHT = 480;

        public const float gl_modulate = 1.0f;

        public static BoundingBox ClearBounds()
        {
            BoundingBox bounds;

            bounds.Min.X = bounds.Min.Y = bounds.Min.Z = 99999;
            bounds.Max.X = bounds.Max.Y = bounds.Max.Z = -99999;

            return bounds;
        }

        public static void AddPointToBounds(Vector3 v, ref BoundingBox bounds)
        {
            if (v.X < bounds.Min.X)
                bounds.Min.X = v.X;

            if (v.X > bounds.Max.X)
                bounds.Max.X = v.X;


            if (v.Y < bounds.Min.Y)
                bounds.Min.Y = v.Y;

            if (v.Y > bounds.Max.Y)
                bounds.Max.Y = v.Y;


            if (v.Z < bounds.Min.Z)
                bounds.Min.Z = v.Z;

            if (v.Z > bounds.Max.Z)
                bounds.Max.Z = v.Z;
        }



        public struct SLightStyle
        {
            public float[] rgb; // size: 3 (0.0 - 2.0)
            public float white; // highest of rgb
        }

        public struct SRefDef
        {
            public Vector3 ViewOrigin;
            public Vector3 ViewAngles;
            public Vector3 ViewUp;

            public Vector3 TargetOrigin;
            public Quaternion TargetAngles;

            public BoundingFrustum FrustumBounds;

            public int EntityIndex;

            public string MapName;

            public SLightStyle[] lightstyles; // size: MAX_LIGHTSTYLES
        }

        public struct SHLSL
        {
            public Matrix xViewMatrix;
            public Matrix xProjectionMatrix;
            public Matrix xWorld;
            public Color xLightModel;
            public Color xLightAmbient;
            public float xLightPower;
            public float xTextureAlpha;
            public float xGamma;
            public float xRealTime;
            public float xBloomThreshold;
            public float xBaseIntensity;
            public float xBloomIntensity;
            public float xBaseSaturation;
            public float xBloomSaturation;
            public bool xPointLights;
            public bool xFlow;
        }

        public struct SGlobal
        {
            public SHLSL HLSL;
            public SHLSL OldHLSL;
        }

    }
}
