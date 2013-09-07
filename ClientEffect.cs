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
    public class CClientEffect
    {
        // ==============================================================
        // 
        // LIGHT STYLE MANAGEMENT
        // 
        // ==============================================================
        public SCLightStyle[] cl_lightstyle;
        public int lastofs;

        public CClientEffect()
        {
            cl_lightstyle = new SCLightStyle[CLocal.MAX_LIGHTSTYLES];

            for (int i = 0; i < CLocal.MAX_LIGHTSTYLES; i++)
            {
                cl_lightstyle[i].length = 0;
                cl_lightstyle[i].value = new float[3];
                cl_lightstyle[i].map = new float[CShared.MAX_QPATH];
            }
        }

        public void CL_ClearLightStyles()
        {
            for (int i = 0; i < cl_lightstyle.Length; i++)
            {
                cl_lightstyle[i].length = 0;

                for (int j = 0; j < cl_lightstyle[i].value.Length; j++)
                {
                    cl_lightstyle[i].value[j] = 0.0f;
                }

                for (int j = 0; j < cl_lightstyle[i].map.Length; j++)
                {
                    cl_lightstyle[i].map[j] = 0.0f;
                }
            }

            lastofs = -1;
        }

        public void CL_RunLightStyles()
        {
            int ofs;

            ofs = (int)(CProgram.gQ2Game.gCMain.gTimeGame.TotalGameTime.TotalMilliseconds / 1000.0f);
            if (ofs == lastofs)
                return;

            lastofs = ofs;

            for (int i = 0; i < CLocal.MAX_LIGHTSTYLES; i++)
            {
                if (cl_lightstyle[i].length == 0)
                {
                    cl_lightstyle[i].value[0] = cl_lightstyle[i].value[1] = cl_lightstyle[i].value[2] = 1.0f;
                    continue;
                }

                if (cl_lightstyle[i].length == 1)
                    cl_lightstyle[i].value[0] = cl_lightstyle[i].value[1] = cl_lightstyle[i].value[2] = cl_lightstyle[i].map[0];
                else
                    cl_lightstyle[i].value[0] = cl_lightstyle[i].value[1] = cl_lightstyle[i].value[2] = cl_lightstyle[i].map[ofs % cl_lightstyle[i].length];
            }
        }

        public void CL_SetLightstyle(int i)
        {
            int len;
            string s;

            s = CClient.cl.configstrings[CShared.CS_LIGHTS + i];
            len = s.Length;

            if (len >= CShared.MAX_QPATH)
                CMain.Error(CMain.EErrorParm.ERR_WARNING, "svc_lightstyle length=" + len.ToString());

            cl_lightstyle[i].length = len;

            for (int j = 0; j < len; j++)
            {
                cl_lightstyle[i].map[j] = (float)(s[j] - 'a') / (float)('m' - 'a');
            }
        }

        public void CL_AddLightStyles()
        {
            for (int i = 0; i < CLocal.MAX_LIGHTSTYLES; i++)
            {
                CProgram.gQ2Game.gCCommon.gCClientMain.gCClientView.V_AddLightStyle(i, cl_lightstyle[i].value[0], cl_lightstyle[i].value[1], cl_lightstyle[i].value[2]);
            }
        }

        public void CL_ClearEffects()
        {
            CL_ClearLightStyles();
        }

        public struct SCLightStyle
        {
            public int length;
            public float[] value; // size: 3
            public float[] map; // size: MAX_QPATH
        }

    }
}
