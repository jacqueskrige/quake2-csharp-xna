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
    public class CClientMain
    {
        public CClientParse gCClientParse;
        public CClientEffect gCClientEffect;
        public CClientView gCClientView;
        public CClientEntity gCClientEntity;

        public CClientMain()
        {
            gCClientParse = new CClientParse();
            gCClientEffect = new CClientEffect();
            gCClientView = new CClientView();
            gCClientEntity = new CClientEntity();



            // TEMPORARY! (SHOULD BE IN SP_WORLDSPAWN)
            if (CClient.cl.configstrings == null)
                CClient.cl.configstrings = new string[CShared.MAX_CONFIGSTRINGS];

            // setup light animation tables. 'a' is total darkness, 'z' is doublebright.

            // 0 normal
            CClient.cl.configstrings[CShared.CS_LIGHTS + 0] = "m";

            // 1 FLICKER (first variety)
            CClient.cl.configstrings[CShared.CS_LIGHTS + 1] = "mmnmmommommnonmmonqnmmo";

            // 2 SLOW STRONG PULSE
            CClient.cl.configstrings[CShared.CS_LIGHTS + 2] = "abcdefghijklmnopqrstuvwxyzyxwvutsrqponmlkjihgfedcba";

            // 3 CANDLE (first variety)
            CClient.cl.configstrings[CShared.CS_LIGHTS + 3] = "mmmmmaaaaammmmmaaaaaabcdefgabcdefg";

            // 4 FAST STROBE
            CClient.cl.configstrings[CShared.CS_LIGHTS + 4] = "mamamamamama";

            // 5 GENTLE PULSE 1
            CClient.cl.configstrings[CShared.CS_LIGHTS + 5] = "jklmnopqrstuvwxyzyxwvutsrqponmlkj";

            // 6 FLICKER (second variety)
            CClient.cl.configstrings[CShared.CS_LIGHTS + 6] = "nmonqnmomnmomomno";

            // 7 CANDLE (second variety)
            CClient.cl.configstrings[CShared.CS_LIGHTS + 7] = "mmmaaaabcdefgmmmmaaaammmaamm";

            // 8 CANDLE (third variety)
            CClient.cl.configstrings[CShared.CS_LIGHTS + 8] = "mmmaaammmaaammmabcdefaaaammmmabcdefmmmaaaa";

            // 9 SLOW STROBE (fourth variety)
            CClient.cl.configstrings[CShared.CS_LIGHTS + 9] = "aaaaaaaazzzzzzzz";

            // 10 FLUORESCENT FLICKER
            CClient.cl.configstrings[CShared.CS_LIGHTS + 10] = "mmamammmmammamamaaamammma";

            // 11 SLOW PULSE NOT FADE TO BLACK
            CClient.cl.configstrings[CShared.CS_LIGHTS + 11] = "abcdefghijklmnopqrrqponmlkjihgfedcba";

            // styles 32-62 are assigned by the light program for switchable lights

            // 63 testing
            CClient.cl.configstrings[CShared.CS_LIGHTS + 63] = "a";
        }

        private void CL_ReadPackets()
        {
            gCClientParse.CL_ParseServerMessage();
        }

        public void CL_Frame(int msec)
        {
            CL_ReadPackets();

            gCClientEffect.CL_RunLightStyles();
        }

        public void CL_ClearState()
        {
            gCClientEffect.CL_ClearEffects();
        }

        public void CL_Disconnect()
        {
            CL_ClearState();
        }

    }
}
