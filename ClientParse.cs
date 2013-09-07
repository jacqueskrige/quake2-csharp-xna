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
    public class CClientParse
    {
        public string[] svc_strings =
        {
            "svc_bad",

            "svc_muzzleflash",
            "svc_muzzlflash2",
            "svc_temp_entity",
            "svc_layout",
            "svc_inventory",
            
            "svc_nop",
            "svc_disconnect",
            "svc_reconnect",
            "svc_sound",
            "svc_print",
            "svc_stufftext",
            "svc_serverdata",
            "svc_configstring",
            "svc_spawnbaseline",
            "svc_centerprint",
            "svc_download",
            "svc_playerinfo",
            "svc_packetentities",
            "svc_deltapacketentities",
            "svc_frame"
        };


        public void CL_ParseServerData()
        {
            CProgram.gQ2Game.gCCommon.gCClientMain.CL_ClearState();
        }

        private void CL_ParseConfigString()
        {
            int i;
            string s;

            //i = MSG_ReadShort(&net_message);
            //if (i < 0 || i >= MAX_CONFIGSTRINGS)
            //    Com_Error(ERR_DROP, "configstring > MAX_CONFIGSTRINGS");
            //s = MSG_ReadString(&net_message);

            //strncpy(olds, cl.configstrings[i], sizeof(olds));
            //olds[sizeof(olds) - 1] = 0;


            // jkrige - just temporary setting a fake network message
            i = CShared.CS_LIGHTS;
            s = "gjrbdwed";
            //if (CClient.cl.configstrings == null)
            //    CClient.cl.configstrings = new string[CShared.MAX_CONFIGSTRINGS];
            // jkrige - just temporary setting a fake network message

            CClient.cl.configstrings[i] = s;

            // do something apropriate

            if (i >= CShared.CS_LIGHTS && i < CShared.CS_LIGHTS + CShared.MAX_LIGHTSTYLES)
                CProgram.gQ2Game.gCCommon.gCClientMain.gCClientEffect.CL_SetLightstyle(i - CShared.CS_LIGHTS);
        }

        public void CL_ParseServerMessage()
        {
            CL_ParseServerData();

            CL_ParseConfigString();
        }

    }
}
