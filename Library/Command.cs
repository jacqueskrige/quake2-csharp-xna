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
using System.IO;
using System.Text;

namespace Q2BSP.Library
{
    public class CCommand
    {
        public const string TextReturn = "\r\n";

        public static bool WriteFile(string FilePath, string FileData)
        {
            FileStream FS;
            BinaryWriter BW;

            if (string.IsNullOrEmpty(FilePath) == true)
                return false;

            if (string.IsNullOrEmpty(FileData) == true)
                return false;

            FS = System.IO.File.Open(FilePath, FileMode.Create, FileAccess.Write);

            if (FS == null)
                return false;

            BW = new BinaryWriter(FS);
            for (int i = 0; i < FileData.Length; i++)
            {
                BW.Write(FileData[i]);
            }

            BW.Close();
            BW = null;

            FS.Close();
            FS.Dispose();
            FS = null;

            return true;
        }

        public static string LoadFile(string FileName)
        {
            string MapData;
            char[] ch;
            FileStream FS;
            BinaryReader BR;
            StringBuilder SB;

            if (string.IsNullOrEmpty(FileName) == true)
                return null;

            FS = System.IO.File.OpenRead(FileName);

            if (FS == null)
                return null;

            BR = new BinaryReader(FS);
            ch = BR.ReadChars((int)FS.Length);

            BR.Close();
            BR = null;

            FS.Close();
            FS.Dispose();
            FS = null;

            SB = new StringBuilder(ch.Length);
            SB.Append(ch);

            MapData = SB.ToString();

            if (string.IsNullOrEmpty(MapData) == true)
                return null;

            // fix line ends
            MapData = MapData.Replace("\r", "").Replace("\n", "\r\n");

            return MapData;
        }

    }
}
