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
    public class CCommon
    {
        public CClientMain gCClientMain;

        // IMPORTANT NOTE: #1
        // main.cs will probably be desolved eventually as common.cs will likely be the main entry point
        // either that or main.cs will be used via xna draws while common.cs will be used via xna updates
        // ...something like that

        // IMPORTANT NOTE: #2
        // all the client source code from the common.cs class downwards is not referenced at this time.
        // its a guideline for continued development.

        public CCommon()
        {
            gCClientMain = new CClientMain();
        }

        public void Qcommon_Frame(int msec)
        {
            //if (host_speeds->value)
            //    time_before = Sys_Milliseconds();

            //SV_Frame(msec);

            //if (host_speeds->value)
            //    time_between = Sys_Milliseconds();

            gCClientMain.CL_Frame(msec);

            gCClientMain.gCClientView.V_RenderView();
        }


        public struct SSizeBuf
        {
            public bool allowoverflow; // if false, do a Com_Error
            public bool overflowed; // set to true if the buffer size failed
            public byte[] data;
            public int maxsize;
            public int cursize;
            public int readcount;
        }


        // ==============================================================
        // 
        // PROTOCOL
        // 
        // ==============================================================
        public const int PROTOCOL_VERSION = 34;

        public const int PORT_MASTER = 27900;
        public const int PORT_CLIENT = 27901;
        public const int PORT_SERVER = 27910;

        public const int UPDATE_BACKUP = 16; // copies of entity_state_t to keep buffered, must be power of two
        public const int UPDATE_MASK = UPDATE_BACKUP - 1;

        // the svc_strings[] array in ClientParse.cs should mirror this.

        // server to client
        public enum ESVC_Ops
        {
            svc_bad,
            
            // these ops are known to the game dll
            svc_muzzleflash,
            svc_muzzleflash2,
            svc_temp_entity,
            svc_layout,
            svc_inventory,

            // the rest are private to the client and server
            svc_nop,
            svc_disconnect,
            svc_reconnect,
            svc_sound,                  // <see code>
            svc_print,                  // [byte] id [string] null terminated string
            svc_stufftext,              // [string] stuffed into client's console buffer, should be \n terminated
            svc_serverdata,             // [long] protocol ...
            svc_configstring,           // [short] [string]
            svc_spawnbaseline,
            svc_centerprint,            // [string] to put in center of the screen
            svc_download,               // [short] size [size bytes]
            svc_playerinfo,             // variable
            svc_packetentities,         // [...]
            svc_deltapacketentities,    // [...]
            svc_frame
        }

        // client to server
        public enum ECLC_Ops
        {
            clc_bad,
            clc_nop,
            clc_move,           // [[usercmd_t]
            clc_userinfo,       // [[userinfo string]
            clc_stringcmd       // [string] message
        };

    }
}
