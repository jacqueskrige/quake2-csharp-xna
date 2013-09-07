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
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace Q2BSP
{
    public class CGamma
    {
        private float GammaValue;

        public CGamma()
        {
            GammaValue = 2.1f;

            CProgram.gQ2Game.gCMain.gSGlobal.HLSL.xGamma = GammaValue;
        }

        public void GammaLighten()
        {
            if (GammaValue > 5.0f)
                GammaValue = 5.0f;

            if (GammaValue == 5.0f)
                return;

            GammaValue += 0.02f;
            GammaValue = (float)Math.Round(GammaValue, 2);

            CProgram.gQ2Game.gCMain.gSGlobal.HLSL.xGamma = GammaValue;
            
            //SetGamma(GammaValue, GammaValue, GammaValue);
        }

        public void GammaDarken()
        {
            if (GammaValue < 1.0f)
                GammaValue = 1.0f;

            if (GammaValue <= 1.0f)
                return;

            GammaValue -= 0.02f;
            GammaValue = (float)Math.Round(GammaValue, 2);

            CProgram.gQ2Game.gCMain.gSGlobal.HLSL.xGamma = GammaValue;

            //SetGamma(GammaValue, GammaValue, GammaValue);
        }

        /*private void SetGamma(float Red, float Green, float Blue)
        {
            // map the float values (0.0 min, 1.0 max) to byte values (0 min, 255 max)
            //byte redOffset = (byte)(Red * 255);
            //byte greenOffset = (byte)(Green * 255);
            //byte blueOffset = (byte)(Blue * 255);
            short redOffset = (short)(Red * 255);
            short greenOffset = (short)(Green * 255);
            short blueOffset = (short)(Blue * 255);

            // get the gamma ramp
            GammaRamp ramp = CProgram.gQ2Game.gGraphicsDevice.GetGammaRamp();
            short[] r = ramp.GetRed();
            short[] g = ramp.GetGreen();
            short[] b = ramp.GetBlue();

            // set the gamma values
            // they are stored as shorts, but are treated as ushorts by the hardware
            // if the value is over short.MaxValue, subtract ushort.MaxValue from it
            for (short i = 0; i < 256; i++)
            {
                r[i] *= redOffset;
                if (r[i] > short.MaxValue)
                {
                    r[i] -= (short)(r[i] - ushort.MaxValue * 2);
                    System.Diagnostics.Debug.WriteLine("over red");
                }
                //if (r[i] > 2550)
                //    r[i] = 2550;

                g[i] *= greenOffset;
                if (g[i] > short.MaxValue)
                {
                    g[i] -= (short)(g[i] - ushort.MaxValue * 2);
                    System.Diagnostics.Debug.WriteLine("over green");
                }
                //if (g[i] > 2550)
                //    g[i] = 2550;

                b[i] *= blueOffset;
                if (b[i] > short.MaxValue)
                {
                    b[i] -= (short)(b[i] - ushort.MaxValue * 2);
                    System.Diagnostics.Debug.WriteLine("over blue");
                }
                //if (b[i] > 2550)
                //    b[i] = 2550;
            }

            // set the gamma values
            ramp.SetRed(r);
            ramp.SetGreen(g);
            ramp.SetBlue(b);
            CProgram.gQ2Game.gGraphicsDevice.SetGammaRamp(true, ramp);
        }*/

        public float Gamma
        {
            get
            {
                return GammaValue;
            }
        }

    }
}
