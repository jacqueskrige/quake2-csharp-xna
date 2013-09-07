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
    public class CServer
    {
        // max recipients for heartbeat packets
        public const int MAX_MASTERS = 8;

        //server_static_t svs;				// persistant server info
        public static SServer sv; // local server


        public struct SServer
        {
            public EServerState state; // precache commands are only valid during load

            public bool attractloop; // running cinematics and demos for the local system only
            public bool loadgame; // client begins should reuse existing entity

            public uint time; // always sv.framenum * 100 msec
            public int framenum;

            public string name; // size: MAX_QPATH (map name, or cinematic name)
            //struct cmodel_s		*models[MAX_MODELS];

            public string[] configstrings;  // size: [MAX_CONFIGSTRINGS, MAX_QPATH]
            public EClientState[] baselines; // size: MAX_EDICTS

            // the multicast buffer is used to send a message to a set of clients
            // it is only used to marshall data until SV_Multicast is called
            public CCommon.SSizeBuf multicast;
            public byte[] multicast_buf; // size: MAX_MSGLEN

            // demo server information
            //FILE		*demofile;
            public bool timedemo; // don't time sync
        }




        /*typedef struct
{
	server_state_t	state;			// precache commands are only valid during load

	qboolean	attractloop;		// running cinematics and demos for the local system only
	qboolean	loadgame;			// client begins should reuse existing entity

	unsigned	time;				// always sv.framenum * 100 msec
	int			framenum;

	char		name[MAX_QPATH];			// map name, or cinematic name
	struct cmodel_s		*models[MAX_MODELS];

	char		configstrings[MAX_CONFIGSTRINGS][MAX_QPATH];
	entity_state_t	baselines[MAX_EDICTS];

	// the multicast buffer is used to send a message to a set of clients
	// it is only used to marshall data until SV_Multicast is called
	sizebuf_t	multicast;
	byte		multicast_buf[MAX_MSGLEN];

	// demo server information
	FILE		*demofile;
	qboolean	timedemo;		// don't time sync
} server_t;*/





        public enum EServerState
        {
            ss_dead,        // no map loaded
            ss_loading,     // spawning level edicts
            ss_game,        // actively running
            ss_cinematic,
            ss_demo,
            ss_pic
        }
        // some qc commands are only valid before the server has finished
        // initializing (precache commands, static sounds / objects, etc)

        public enum EClientState
        {
            cs_free,        // can be reused for a new connection
            cs_zombie,      // client has been disconnected, but don't reuse connection for a couple seconds
            cs_connected,   // has been assigned to a client_t, but not in game yet
            cs_spawned      // client is fully in game
        }


        

        

    }
}
