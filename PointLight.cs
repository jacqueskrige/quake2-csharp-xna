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
using System.Text;

namespace Q2BSP
{
    public class CPointLight
    {
        //////////////////////////////////////////////////////////////
        // The only parameter that will be changing for every light //
        // each frame is the postion parameter.  For the sake of    //
        // reducing string look-ups, the position parameter is      //
        // stored as a parameter instance.  The other parameters    //
        // are updated much less frequently, so a tradeoff has been //
        // made for clarity.                                        //
        //////////////////////////////////////////////////////////////

        private EffectParameter positionParameter;
        private EffectParameter instanceParameter;

        private float LightRange = 300.0f;
        private float LightFalloff = 1.1f;
        private Vector4 LightPosition;
        private Color LightColor = Color.White;

        public CPointLight(Vector4 initialPosition)
        {
            gPosition = initialPosition;
        }

        public CPointLight(Vector4 initialPosition, float initialRange)
        {
            gPosition = initialPosition;
            gRange = initialRange;
        }

        public CPointLight(Vector4 initialPosition, float initialRange, float Red, float Green, float Blue)
        {
            gPosition = initialPosition;
            gRange = initialRange;
            LightColor.R = (byte)(255 * Red);
            LightColor.G = (byte)(255 * Green);
            LightColor.B = (byte)(255 * Blue);
            LightColor.A = (byte)255;
            gColor = LightColor;
        }

        public CPointLight(Vector4 initialPosition, EffectParameter lightParameter)
        {
            //////////////////////////////////////////////////////////////
            // An instance of a light is bound to an instance of a      //
            // Light structure defined in the effect.  The              //
            // "StructureMembers" property of a parameter is used to    //
            // access the individual fields of a structure.             //
            //////////////////////////////////////////////////////////////
            instanceParameter = lightParameter;
            positionParameter = instanceParameter.StructureMembers["position"];
            gPosition = initialPosition;
            instanceParameter.StructureMembers["range"].SetValue(LightRange);
            instanceParameter.StructureMembers["falloff"].SetValue(LightFalloff);
            instanceParameter.StructureMembers["color"].SetValue(LightColor.ToVector4());
        }

        public void SetLight(EffectParameter lightParameter)
        {
            instanceParameter = lightParameter;

            //if (CProgram.gQ2Game.gCMain.r_hardwarelight == false)
            //    return;

            instanceParameter.StructureMembers["position"].SetValue(LightPosition);
            instanceParameter.StructureMembers["range"].SetValue(LightRange);
            instanceParameter.StructureMembers["falloff"].SetValue(LightFalloff);
            instanceParameter.StructureMembers["color"].SetValue(LightColor.ToVector4());

            // generic.fx
            //struct Light
            //{
            //    float4 color;    // 0
            //    float4 position; // 1
            //    float falloff;   // 2
            //    float range;     // 3
            //};
            //instanceParameter.StructureMembers[0].SetValue(LightColor.ToVector4()); // "color"
            //instanceParameter.StructureMembers[1].SetValue(LightPosition);          // "position"
            //instanceParameter.StructureMembers[2].SetValue(LightFalloff);           // "falloff"
            //instanceParameter.StructureMembers[3].SetValue(LightRange);             // "range"
        }

        #region Light Properties
        public Vector4 gPosition
        {
            set
            {
                LightPosition = value;

                if (positionParameter != null)
                    positionParameter.SetValue(LightPosition);
            }
            get
            {
                return LightPosition;
            }
        }


        public Color gColor
        {
            set
            {
                LightColor = value;

                if (instanceParameter != null)
                    instanceParameter.StructureMembers["color"].SetValue(LightColor.ToVector4());
            }
            get
            {
                return LightColor;
            }
        }

        public float gRange
        {
            set
            {
                LightRange = value;

                if (instanceParameter != null)
                    instanceParameter.StructureMembers["range"].SetValue(LightRange);
            }
            get
            {
                return LightRange;
            }
        }


        public float gFalloff
        {
            set
            {
                LightFalloff = value;

                if (instanceParameter != null)
                    instanceParameter.StructureMembers["falloff"].SetValue(LightFalloff);
            }
            get
            {
                return LightFalloff;
            }
        }
        #endregion
    }
}
