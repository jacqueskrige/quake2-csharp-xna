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
using System.Text;

namespace Q2BSP
{
    public class CClientView
    {
        private CLocal.SLightStyle[] r_lightstyles;

        public CClientView()
        {
            r_lightstyles = new CLocal.SLightStyle[CLocal.MAX_LIGHTSTYLES];

            for (int i = 0; i < CLocal.MAX_LIGHTSTYLES; i++)
            {
                r_lightstyles[i].white = 0;
                r_lightstyles[i].rgb = new float[3];
            }
        }

        public void V_AddLightStyle(int style, float r, float g, float b)
        {
            if (style < 0 || style > CShared.MAX_LIGHTSTYLES)
                CMain.Error(CMain.EErrorParm.ERR_WARNING, "Bad light style " + style.ToString());

            r_lightstyles[style].white = r + g + b;
            r_lightstyles[style].rgb[0] = r;
            r_lightstyles[style].rgb[1] = g;
            r_lightstyles[style].rgb[2] = b;
        }

        public void V_RenderView()
        {
            CProgram.gQ2Game.gCCommon.gCClientMain.gCClientEntity.CL_AddEntities();

            CClient.cl.RefDef.lightstyles = r_lightstyles;
        }

    }
}
