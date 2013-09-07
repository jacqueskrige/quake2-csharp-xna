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

namespace Q2BSP
{
    public class CFiles
    {
        // ========================================================================
        // PAK FILE LOADING
        // 
        // The .pak files are just a linear collapse of a directory tree
        // ========================================================================
        public const int IDPAKHEADER = (('K' << 24) + ('C' << 16) + ('A' << 8) + 'P');
        public const int PACK_MAX_FILES = 8192; // bumped from 4096 to support loading Heretic2
        public const int PACK_MAX_FILENAME_LENGTH = 56;

        // total number of files inside pak
        private int fs_packFiles;

        // the loaded pack information
        private SPack? qPack;

        public void FS_LoadPak(string PakFile, string BaseName)
        {
            if (CProgram.gQ2Game.gCMain.r_usepak == false)
                return;

            qPack = FS_LoadPakFile(PakFile, BaseName);
        }

        public void FS_ClosePak()
        {
            if (CProgram.gQ2Game.gCMain.r_usepak == false)
                return;

            if (qPack.HasValue == false)
                return;

            qPack.Value.handle.Close();
            qPack = null;
        }

        /// <summary>
        /// FS_ReadFile
        /// -----------
        /// filename are relative to the quake search path
        /// </summary>
        public void FS_ReadFile(string qpath, out byte[] qdata)
        {
            qdata = FS_ReadFile2(qpath);
        }

        public void FS_ReadFile(string qpath, out MemoryStream qdata)
        {
            byte[] data = FS_ReadFile2(qpath);

            if (data != null)
                qdata = new MemoryStream(data);
            else
                qdata = null;
        }

        private byte[] FS_ReadFile2(string qpath)
        {
            int i;
            bool found = false;

            if (qPack.HasValue == false)
                return null;

            for (i = 0; i < qPack.Value.buildBuffer.Count; i++)
            {
                if (qpath == qPack.Value.buildBuffer[i].Name)
                {
                    found = true;
                    break;
                }

            }

            if (found == true)
            {
                BinaryReader r = new BinaryReader(qPack.Value.handle);

                r.BaseStream.Seek(qPack.Value.buildBuffer[i].Position, System.IO.SeekOrigin.Begin);
                return r.ReadBytes(qPack.Value.buildBuffer[i].Size);
            }

            return null;
        }

        private SPack? FS_LoadPakFile(string PakFile, string BaseName)
        {
            SPack Pack;
            BinaryReader r;

            PakFile = CProgram.gQ2Game.Content.RootDirectory + "\\" + PakFile;

            if (File.Exists(PakFile) == false)
            {
                CMain.Error(CMain.EErrorParm.ERR_FATAL, "PAK file not found.");
                return null;
            }

            Pack.handle = File.OpenRead(PakFile);

            if (Pack.handle == null)
                return null;

            r = new BinaryReader(Pack.handle);
            PakFile = PakFile.ToLower();
            BaseName = BaseName.ToLower();

            if (r.ReadInt32() != IDPAKHEADER)
            {
                Pack.handle.Close();
                Pack.handle = null;

                CMain.Error(CMain.EErrorParm.ERR_FATAL, PakFile + " is not a packfile");
                return null;
            }

            Pack.packDirOffset = r.ReadInt32();
            Pack.packDirLength = r.ReadInt32();

            // if the directory offset is beyond the EOF then we assume its htic2-0.pak
            // Raven Software probably did this so unaware PAK readers fails to read the Heretic2 content
            if (CProgram.gQ2Game.gCMain.r_htic2 == true)
            {
                if (Pack.packDirOffset > r.BaseStream.Length)
                {
                    Pack.packDirOffset = 215695973; // 0x0cdb4265 (215 695 973 bytes)
                    Pack.packDirLength = 264256;    // EOF - Pack.packDirOffset (EOF: 0x0cdf4aa5 | 215 960 229 bytes)
                }
            }

            // PACK_MAX_FILENAME_LENGTH + FilePosition + FileLength
            Pack.numfiles = Pack.packDirLength / (PACK_MAX_FILENAME_LENGTH + sizeof(int) + sizeof(int));

            if (Pack.numfiles > PACK_MAX_FILES)
                CMain.Error(CMain.EErrorParm.ERR_FATAL, PakFile + " has " + Pack.numfiles + " files");

            Pack.buildBuffer = new List<SFileInPack>();

            fs_packFiles += Pack.numfiles;
            Pack.pakFilename = PakFile;
            Pack.pakBasename = BaseName.Replace(".pak", "");

            r.BaseStream.Seek(Pack.packDirOffset, SeekOrigin.Begin);

            for (int i = 0; i < Pack.numfiles; i++)
            {
                SFileInPack _FileInPack;

                _FileInPack.Name = CShared.Com_ToString(r.ReadChars(PACK_MAX_FILENAME_LENGTH)).ToLower();
                _FileInPack.Position = r.ReadInt32();
                _FileInPack.Size = r.ReadInt32();

                Pack.buildBuffer.Add(_FileInPack);
            }

            //for (int i = 0; i < Pack.buildBuffer.Count; i++)
            //{
            //    r.BaseStream.Seek(Pack.buildBuffer[i].Position, System.IO.SeekOrigin.Begin);
            //    fs_headerLongs[fs_numHeaderLongs++] = CCrc32.GetMemoryCRC32(r.ReadBytes(Pack.buildBuffer[i].Size));
            //}

            //Pack.checksum = CProgram.vCCommon.Com_BlockChecksum(fs_headerLongs);
            //Pack.pure_checksum = CProgram.vCCommon.Com_BlockChecksumKey(fs_headerLongs, fs_checksumFeed);

            // As of yet unassigned
            //Pack.hashSize = 0;
            Pack.pakGamename = null;

            return Pack;
        }


        public struct SPack
        {
            public string pakFilename;              // c:\quake2\baseq2\pak0.pak
            public string pakBasename;              // pak0
            public string pakGamename;              // baseq2
            public FileStream handle;               // handle to pack file
            //public long checksum;                 // regular checksum
            //public long pure_checksum;            // checksum for pure
            public int numfiles;                    // number of files in pak
            public List<SFileInPack> buildBuffer;   // file entry

            public int packDirOffset;
            public int packDirLength;
        }

        public struct SFileInPack
        {
            public string Name;     // name of the file
            public long Position;   // file info position in pak
            public int Size;        // file info size in pak
        }


        // ========================================================================
        // PCX FILE LOADING
        // 
        // Used for as many images as possible
        // ========================================================================
        public struct SPCX
        {
            public byte manufacturer;
            public byte version;
            public byte encoding;
            public byte bits_per_pixel;
            public ushort xmin;
            public ushort ymin;
            public ushort xmax;
            public ushort ymax;
            public ushort hres;
            public ushort vres;
            public byte[] palette; // size: 48 (unsigned char ??)
            public byte reserved;
            public byte color_planes;
            public ushort bytes_per_line;
            public ushort palette_type;
            public byte[] filler; // size: 58
            public byte data; // unsigned char ??
        }

        /*typedef struct
        {
            char	manufacturer;
            char	version;
            char	encoding;
            char	bits_per_pixel;
            unsigned short	xmin,ymin,xmax,ymax;
            unsigned short	hres,vres;
            unsigned char	palette[48];
            char	reserved;
            char	color_planes;
            unsigned short	bytes_per_line;
            unsigned short	palette_type;
            char	filler[58];
            unsigned char	data;			// unbounded
        } pcx_t;*/

    }
}
