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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace Q2BSP
{
    public class CImage
    {
        private const int MIPLEVELS_QUAKE2 = 4;
        private const int MIPLEVELS_HERETIC2 = 16;

        private byte[] Palette;
        private List<STextureWAL> Textures;
        private List<Texture2D> Lightmaps;
        private List<Texture2D> Skies;
        private List<Texture2D> Pictures;

        public CImage()
        {
            Textures = new List<STextureWAL>();
            Lightmaps = new List<Texture2D>();
            Skies = new List<Texture2D>();
            Pictures = new List<Texture2D>();

            Palette = LoadPalette(EPalette.Quake2);
        }

        public int current_lightmap_texture
        {
            get
            {
                return Lightmaps.Count;
            }
        }

        /// <summary>
        /// LoadPalette
        /// -----------
        /// Loads the palette from a palette file
        /// The filesize should be 768 bytes (768 / 3 = 256 entries)
        /// </summary>
        private byte[] LoadPalette(string FileName)
        {
            byte[] palette;

            using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read))
            {
                BinaryReader r = new BinaryReader(fs);
                palette = r.ReadBytes(Convert.ToInt32(r.BaseStream.Length));

                r.Close();
            }

            //System.Diagnostics.Debug.WriteLine("-------------------------------------------");
            //for (int i = 0; i < 768; i += 3)
            //{
            //    System.Diagnostics.Debug.WriteLine(palette[i + 0] + ", " + palette[i + 1] + ", " + palette[i + 2] + ",");
            //}
            //System.Diagnostics.Debug.WriteLine("-------------------------------------------");

            return palette;
        }

        /// <summary>
        /// LoadPalette
        /// -----------
        /// Loads the palette from preset palette
        /// </summary>
        private byte[] LoadPalette(EPalette palette)
        {
            byte[] q1palette;
            byte[] q2palette;
            byte[] h2palette;

            #region Quake1 Palette
            q1palette = new byte[768]
                {
                    000, 000, 000,
                    015, 015, 015,
                    031, 031, 031,
                    047, 047, 047,
                    063, 063, 063,
                    075, 075, 075,
                    091, 091, 091,
                    107, 107, 107,
                    123, 123, 123,
                    139, 139, 139,
                    155, 155, 155,
                    171, 171, 171,
                    187, 187, 187,
                    203, 203, 203,
                    219, 219, 219,
                    235, 235, 235,
                    015, 011, 007,
                    023, 015, 011,
                    031, 023, 011,
                    039, 027, 015,
                    047, 035, 019,
                    055, 043, 023,
                    063, 047, 023,
                    075, 055, 027,
                    083, 059, 027,
                    091, 067, 031,
                    099, 075, 031,
                    107, 083, 031,
                    115, 087, 031,
                    123, 095, 035,
                    131, 103, 035,
                    143, 111, 035,
                    011, 011, 015,
                    019, 019, 027,
                    027, 027, 039,
                    039, 039, 051,
                    047, 047, 063,
                    055, 055, 075,
                    063, 063, 087,
                    071, 071, 103,
                    079, 079, 115,
                    091, 091, 127,
                    099, 099, 139,
                    107, 107, 151,
                    115, 115, 163,
                    123, 123, 175,
                    131, 131, 187,
                    139, 139, 203,
                    000, 000, 000,
                    007, 007, 000,
                    011, 011, 000,
                    019, 019, 000,
                    027, 027, 000,
                    035, 035, 000,
                    043, 043, 007,
                    047, 047, 007,
                    055, 055, 007,
                    063, 063, 007,
                    071, 071, 007,
                    075, 075, 011,
                    083, 083, 011,
                    091, 091, 011,
                    099, 099, 011,
                    107, 107, 015,
                    007, 000, 000,
                    015, 000, 000,
                    023, 000, 000,
                    031, 000, 000,
                    039, 000, 000,
                    047, 000, 000,
                    055, 000, 000,
                    063, 000, 000,
                    071, 000, 000,
                    079, 000, 000,
                    087, 000, 000,
                    095, 000, 000,
                    103, 000, 000,
                    111, 000, 000,
                    119, 000, 000,
                    127, 000, 000,
                    019, 019, 000,
                    027, 027, 000,
                    035, 035, 000,
                    047, 043, 000,
                    055, 047, 000,
                    067, 055, 000,
                    075, 059, 007,
                    087, 067, 007,
                    095, 071, 007,
                    107, 075, 011,
                    119, 083, 015,
                    131, 087, 019,
                    139, 091, 019,
                    151, 095, 027,
                    163, 099, 031,
                    175, 103, 035,
                    035, 019, 007,
                    047, 023, 011,
                    059, 031, 015,
                    075, 035, 019,
                    087, 043, 023,
                    099, 047, 031,
                    115, 055, 035,
                    127, 059, 043,
                    143, 067, 051,
                    159, 079, 051,
                    175, 099, 047,
                    191, 119, 047,
                    207, 143, 043,
                    223, 171, 039,
                    239, 203, 031,
                    255, 243, 027,
                    011, 007, 000,
                    027, 019, 000,
                    043, 035, 015,
                    055, 043, 019,
                    071, 051, 027,
                    083, 055, 035,
                    099, 063, 043,
                    111, 071, 051,
                    127, 083, 063,
                    139, 095, 071,
                    155, 107, 083,
                    167, 123, 095,
                    183, 135, 107,
                    195, 147, 123,
                    211, 163, 139,
                    227, 179, 151,
                    171, 139, 163,
                    159, 127, 151,
                    147, 115, 135,
                    139, 103, 123,
                    127, 091, 111,
                    119, 083, 099,
                    107, 075, 087,
                    095, 063, 075,
                    087, 055, 067,
                    075, 047, 055,
                    067, 039, 047,
                    055, 031, 035,
                    043, 023, 027,
                    035, 019, 019,
                    023, 011, 011,
                    015, 007, 007,
                    187, 115, 159,
                    175, 107, 143,
                    163, 095, 131,
                    151, 087, 119,
                    139, 079, 107,
                    127, 075, 095,
                    115, 067, 083,
                    107, 059, 075,
                    095, 051, 063,
                    083, 043, 055,
                    071, 035, 043,
                    059, 031, 035,
                    047, 023, 027,
                    035, 019, 019,
                    023, 011, 011,
                    015, 007, 007,
                    219, 195, 187,
                    203, 179, 167,
                    191, 163, 155,
                    175, 151, 139,
                    163, 135, 123,
                    151, 123, 111,
                    135, 111, 095,
                    123, 099, 083,
                    107, 087, 071,
                    095, 075, 059,
                    083, 063, 051,
                    067, 051, 039,
                    055, 043, 031,
                    039, 031, 023,
                    027, 019, 015,
                    015, 011, 007,
                    111, 131, 123,
                    103, 123, 111,
                    095, 115, 103,
                    087, 107, 095,
                    079, 099, 087,
                    071, 091, 079,
                    063, 083, 071,
                    055, 075, 063,
                    047, 067, 055,
                    043, 059, 047,
                    035, 051, 039,
                    031, 043, 031,
                    023, 035, 023,
                    015, 027, 019,
                    011, 019, 011,
                    007, 011, 007,
                    255, 243, 027,
                    239, 223, 023,
                    219, 203, 019,
                    203, 183, 015,
                    187, 167, 015,
                    171, 151, 011,
                    155, 131, 007,
                    139, 115, 007,
                    123, 099, 007,
                    107, 083, 000,
                    091, 071, 000,
                    075, 055, 000,
                    059, 043, 000,
                    043, 031, 000,
                    027, 015, 000,
                    011, 007, 000,
                    000, 000, 255,
                    011, 011, 239,
                    019, 019, 223,
                    027, 027, 207,
                    035, 035, 191,
                    043, 043, 175,
                    047, 047, 159,
                    047, 047, 143,
                    047, 047, 127,
                    047, 047, 111,
                    047, 047, 095,
                    043, 043, 079,
                    035, 035, 063,
                    027, 027, 047,
                    019, 019, 031,
                    011, 011, 015,
                    043, 000, 000,
                    059, 000, 000,
                    075, 007, 000,
                    095, 007, 000,
                    111, 015, 000,
                    127, 023, 007,
                    147, 031, 007,
                    163, 039, 011,
                    183, 051, 015,
                    195, 075, 027,
                    207, 099, 043,
                    219, 127, 059,
                    227, 151, 079,
                    231, 171, 095,
                    239, 191, 119,
                    247, 211, 139,
                    167, 123, 059,
                    183, 155, 055,
                    199, 195, 055,
                    231, 227, 087,
                    127, 191, 255,
                    171, 231, 255,
                    215, 255, 255,
                    103, 000, 000,
                    139, 000, 000,
                    179, 000, 000,
                    215, 000, 000,
                    255, 000, 000,
                    255, 243, 147,
                    255, 247, 199,
                    255, 255, 255,
                    159, 091, 083
                };
            #endregion

            #region Quake2 Palette
            q2palette = new byte[768]
                {
                    000, 000, 000,
                    015, 015, 015,
                    031, 031, 031,
                    047, 047, 047,
                    063, 063, 063,
                    075, 075, 075,
                    091, 091, 091,
                    107, 107, 107,
                    123, 123, 123,
                    139, 139, 139,
                    155, 155, 155,
                    171, 171, 171,
                    187, 187, 187,
                    203, 203, 203,
                    219, 219, 219,
                    235, 235, 235,
                    099, 075, 035,
                    091, 067, 031,
                    083, 063, 031,
                    079, 059, 027,
                    071, 055, 027,
                    063, 047, 023,
                    059, 043, 023,
                    051, 039, 019,
                    047, 035, 019,
                    043, 031, 019,
                    039, 027, 015,
                    035, 023, 015,
                    027, 019, 011,
                    023, 015, 011,
                    019, 015, 007,
                    015, 011, 007,
                    095, 095, 111,
                    091, 091, 103,
                    091, 083, 095,
                    087, 079, 091,
                    083, 075, 083,
                    079, 071, 075,
                    071, 063, 067,
                    063, 059, 059,
                    059, 055, 055,
                    051, 047, 047,
                    047, 043, 043,
                    039, 039, 039,
                    035, 035, 035,
                    027, 027, 027,
                    023, 023, 023,
                    019, 019, 019,
                    143, 119, 083,
                    123, 099, 067,
                    115, 091, 059,
                    103, 079, 047,
                    207, 151, 075,
                    167, 123, 059,
                    139, 103, 047,
                    111, 083, 039,
                    235, 159, 039,
                    203, 139, 035,
                    175, 119, 031,
                    147, 099, 027,
                    119, 079, 023,
                    091, 059, 015,
                    063, 039, 011,
                    035, 023, 007,
                    167, 059, 043,
                    159, 047, 035,
                    151, 043, 027,
                    139, 039, 019,
                    127, 031, 015,
                    115, 023, 011,
                    103, 023, 007,
                    087, 019, 000,
                    075, 015, 000,
                    067, 015, 000,
                    059, 015, 000,
                    051, 011, 000,
                    043, 011, 000,
                    035, 011, 000,
                    027, 007, 000,
                    019, 007, 000,
                    123, 095, 075,
                    115, 087, 067,
                    107, 083, 063,
                    103, 079, 059,
                    095, 071, 055,
                    087, 067, 051,
                    083, 063, 047,
                    075, 055, 043,
                    067, 051, 039,
                    063, 047, 035,
                    055, 039, 027,
                    047, 035, 023,
                    039, 027, 019,
                    031, 023, 015,
                    023, 015, 011,
                    015, 011, 007,
                    111, 059, 023,
                    095, 055, 023,
                    083, 047, 023,
                    067, 043, 023,
                    055, 035, 019,
                    039, 027, 015,
                    027, 019, 011,
                    015, 011, 007,
                    179, 091, 079,
                    191, 123, 111,
                    203, 155, 147,
                    215, 187, 183,
                    203, 215, 223,
                    179, 199, 211,
                    159, 183, 195,
                    135, 167, 183,
                    115, 151, 167,
                    091, 135, 155,
                    071, 119, 139,
                    047, 103, 127,
                    023, 083, 111,
                    019, 075, 103,
                    015, 067, 091,
                    011, 063, 083,
                    007, 055, 075,
                    007, 047, 063,
                    007, 039, 051,
                    000, 031, 043,
                    000, 023, 031,
                    000, 015, 019,
                    000, 007, 011,
                    000, 000, 000,
                    139, 087, 087,
                    131, 079, 079,
                    123, 071, 071,
                    115, 067, 067,
                    107, 059, 059,
                    099, 051, 051,
                    091, 047, 047,
                    087, 043, 043,
                    075, 035, 035,
                    063, 031, 031,
                    051, 027, 027,
                    043, 019, 019,
                    031, 015, 015,
                    019, 011, 011,
                    011, 007, 007,
                    000, 000, 000,
                    151, 159, 123,
                    143, 151, 115,
                    135, 139, 107,
                    127, 131, 099,
                    119, 123, 095,
                    115, 115, 087,
                    107, 107, 079,
                    099, 099, 071,
                    091, 091, 067,
                    079, 079, 059,
                    067, 067, 051,
                    055, 055, 043,
                    047, 047, 035,
                    035, 035, 027,
                    023, 023, 019,
                    015, 015, 011,
                    159, 075, 063,
                    147, 067, 055,
                    139, 059, 047,
                    127, 055, 039,
                    119, 047, 035,
                    107, 043, 027,
                    099, 035, 023,
                    087, 031, 019,
                    079, 027, 015,
                    067, 023, 011,
                    055, 019, 011,
                    043, 015, 007,
                    031, 011, 007,
                    023, 007, 000,
                    011, 000, 000,
                    000, 000, 000,
                    119, 123, 207,
                    111, 115, 195,
                    103, 107, 183,
                    099, 099, 167,
                    091, 091, 155,
                    083, 087, 143,
                    075, 079, 127,
                    071, 071, 115,
                    063, 063, 103,
                    055, 055, 087,
                    047, 047, 075,
                    039, 039, 063,
                    035, 031, 047,
                    027, 023, 035,
                    019, 015, 023,
                    011, 007, 007,
                    155, 171, 123,
                    143, 159, 111,
                    135, 151, 099,
                    123, 139, 087,
                    115, 131, 075,
                    103, 119, 067,
                    095, 111, 059,
                    087, 103, 051,
                    075, 091, 039,
                    063, 079, 027,
                    055, 067, 019,
                    047, 059, 011,
                    035, 047, 007,
                    027, 035, 000,
                    019, 023, 000,
                    011, 015, 000,
                    000, 255, 000,
                    035, 231, 015,
                    063, 211, 027,
                    083, 187, 039,
                    095, 167, 047,
                    095, 143, 051,
                    095, 123, 051,
                    255, 255, 255,
                    255, 255, 211,
                    255, 255, 167,
                    255, 255, 127,
                    255, 255, 083,
                    255, 255, 039,
                    255, 235, 031,
                    255, 215, 023,
                    255, 191, 015,
                    255, 171, 007,
                    255, 147, 000,
                    239, 127, 000,
                    227, 107, 000,
                    211, 087, 000,
                    199, 071, 000,
                    183, 059, 000,
                    171, 043, 000,
                    155, 031, 000,
                    143, 023, 000,
                    127, 015, 000,
                    115, 007, 000,
                    095, 000, 000,
                    071, 000, 000,
                    047, 000, 000,
                    027, 000, 000,
                    239, 000, 000,
                    055, 055, 255,
                    255, 000, 000,
                    000, 000, 255,
                    043, 043, 035,
                    027, 027, 023,
                    019, 019, 015,
                    235, 151, 127,
                    195, 115, 083,
                    159, 087, 051,
                    123, 063, 027,
                    235, 211, 199,
                    199, 171, 155,
                    167, 139, 119,
                    135, 107, 087,
                    159, 091, 083
                };
            #endregion

            #region Hexen2 Palette
            h2palette = new byte[768]
                {
                    000, 000, 000,
                    000, 000, 000,
                    008, 008, 008,
                    016, 016, 016,
                    024, 024, 024,
                    032, 032, 032,
                    040, 040, 040,
                    048, 048, 048,
                    056, 056, 056,
                    064, 064, 064,
                    072, 072, 072,
                    080, 080, 080,
                    084, 084, 084,
                    088, 088, 088,
                    096, 096, 096,
                    104, 104, 104,
                    112, 112, 112,
                    120, 120, 120,
                    128, 128, 128,
                    136, 136, 136,
                    148, 148, 148,
                    156, 156, 156,
                    168, 168, 168,
                    180, 180, 180,
                    184, 184, 184,
                    196, 196, 196,
                    204, 204, 204,
                    212, 212, 212,
                    224, 224, 224,
                    232, 232, 232,
                    240, 240, 240,
                    252, 252, 252,
                    008, 008, 012,
                    016, 016, 020,
                    024, 024, 028,
                    028, 032, 036,
                    036, 036, 044,
                    044, 044, 052,
                    048, 052, 060,
                    056, 056, 068,
                    064, 064, 072,
                    076, 076, 088,
                    092, 092, 104,
                    108, 112, 128,
                    128, 132, 152,
                    152, 156, 176,
                    168, 172, 196,
                    188, 196, 220,
                    032, 024, 020,
                    040, 032, 028,
                    048, 036, 032,
                    052, 044, 040,
                    060, 052, 044,
                    068, 056, 052,
                    076, 064, 056,
                    084, 072, 064,
                    092, 076, 072,
                    100, 084, 076,
                    108, 092, 084,
                    112, 096, 088,
                    120, 104, 096,
                    128, 112, 100,
                    136, 116, 108,
                    144, 124, 112,
                    020, 024, 020,
                    028, 032, 028,
                    032, 036, 032,
                    040, 044, 040,
                    044, 048, 044,
                    048, 056, 048,
                    056, 064, 056,
                    064, 068, 064,
                    068, 076, 068,
                    084, 092, 084,
                    104, 112, 104,
                    120, 128, 120,
                    140, 148, 136,
                    156, 164, 152,
                    172, 180, 168,
                    188, 196, 184,
                    048, 032, 008,
                    060, 040, 008,
                    072, 048, 016,
                    084, 056, 020,
                    092, 064, 028,
                    100, 072, 036,
                    108, 080, 044,
                    120, 092, 052,
                    136, 104, 060,
                    148, 116, 072,
                    160, 128, 084,
                    168, 136, 092,
                    180, 144, 100,
                    188, 152, 108,
                    196, 160, 116,
                    204, 168, 124,
                    016, 020, 016,
                    020, 028, 020,
                    024, 032, 024,
                    028, 036, 028,
                    032, 044, 032,
                    036, 048, 036,
                    040, 056, 040,
                    044, 060, 044,
                    048, 068, 048,
                    052, 076, 052,
                    060, 084, 060,
                    068, 092, 064,
                    076, 100, 072,
                    084, 108, 076,
                    092, 116, 084,
                    100, 128, 092,
                    024, 012, 008,
                    032, 016, 008,
                    040, 020, 008,
                    052, 024, 012,
                    060, 028, 012,
                    068, 032, 012,
                    076, 036, 016,
                    084, 044, 020,
                    092, 048, 024,
                    100, 056, 028,
                    112, 064, 032,
                    120, 072, 036,
                    128, 080, 044,
                    144, 092, 056,
                    168, 112, 072,
                    192, 132, 088,
                    024, 004, 004,
                    036, 004, 004,
                    048, 000, 000,
                    060, 000, 000,
                    068, 000, 000,
                    080, 000, 000,
                    088, 000, 000,
                    100, 000, 000,
                    112, 000, 000,
                    132, 000, 000,
                    152, 000, 000,
                    172, 000, 000,
                    192, 000, 000,
                    212, 000, 000,
                    232, 000, 000,
                    252, 000, 000,
                    016, 012, 032,
                    028, 020, 048,
                    032, 028, 056,
                    040, 036, 068,
                    052, 044, 080,
                    060, 056, 092,
                    068, 064, 104,
                    080, 072, 116,
                    088, 084, 128,
                    100, 096, 140,
                    108, 108, 152,
                    120, 116, 164,
                    132, 132, 176,
                    144, 144, 188,
                    156, 156, 200,
                    172, 172, 212,
                    036, 020, 004,
                    052, 024, 004,
                    068, 032, 004,
                    080, 040, 000,
                    100, 048, 004,
                    124, 060, 004,
                    140, 072, 004,
                    156, 088, 008,
                    172, 100, 008,
                    188, 116, 012,
                    204, 128, 012,
                    220, 144, 016,
                    236, 160, 020,
                    252, 184, 056,
                    248, 200, 080,
                    248, 220, 120,
                    020, 016, 004,
                    028, 024, 008,
                    036, 032, 008,
                    044, 040, 012,
                    052, 048, 016,
                    056, 056, 016,
                    064, 064, 020,
                    068, 072, 024,
                    072, 080, 028,
                    080, 092, 032,
                    084, 104, 040,
                    088, 116, 044,
                    092, 128, 052,
                    092, 140, 052,
                    092, 148, 056,
                    096, 160, 064,
                    060, 016, 016,
                    072, 024, 024,
                    084, 028, 028,
                    100, 036, 036,
                    112, 044, 044,
                    124, 052, 048,
                    140, 064, 056,
                    152, 076, 064,
                    044, 020, 008,
                    056, 028, 012,
                    072, 032, 016,
                    084, 040, 020,
                    096, 044, 028,
                    112, 052, 032,
                    124, 056, 040,
                    140, 064, 048,
                    024, 020, 016,
                    036, 028, 020,
                    044, 036, 028,
                    056, 044, 032,
                    064, 052, 036,
                    072, 060, 044,
                    080, 068, 048,
                    092, 076, 052,
                    100, 084, 060,
                    112, 092, 068,
                    120, 100, 072,
                    132, 112, 080,
                    144, 120, 088,
                    152, 128, 096,
                    160, 136, 104,
                    168, 148, 112,
                    036, 024, 012,
                    044, 032, 016,
                    052, 040, 020,
                    060, 044, 020,
                    072, 052, 024,
                    080, 060, 028,
                    088, 068, 028,
                    104, 076, 032,
                    148, 096, 056,
                    160, 108, 064,
                    172, 116, 072,
                    180, 124, 080,
                    192, 132, 088,
                    204, 140, 092,
                    216, 156, 108,
                    060, 020, 092,
                    100, 036, 116,
                    168, 072, 164,
                    204, 108, 192,
                    004, 084, 004,
                    004, 132, 004,
                    000, 180, 000,
                    000, 216, 000,
                    004, 004, 144,
                    016, 068, 204,
                    036, 132, 224,
                    088, 168, 232,
                    216, 004, 004,
                    244, 072, 000,
                    252, 128, 000,
                    252, 172, 024,
                    252, 252, 252
                };
            #endregion

            if (palette == EPalette.Quake1)
                return q1palette;
            else if (palette == EPalette.Quake2)
                return q2palette;
            else if (palette == EPalette.Hexen2)
                return h2palette;

            return null;
        }

        /// <summary>
        /// CreateLightmap
        /// --------------
        /// Creates a new lightmap texture from the allocated blocklights
        /// </summary>
        public void CreateLightmap(byte[] lmBuffer)
        {
            System.IO.MemoryStream MemStream = new System.IO.MemoryStream();
            Bitmap vBitmap = new Bitmap(CSurface.BLOCK_WIDTH, CSurface.BLOCK_HEIGHT, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int y = 0; y < CSurface.BLOCK_HEIGHT; y++)
            {
                for (int x = 0; x < CSurface.BLOCK_WIDTH; x++)
                {
                    int ColR, ColG, ColB;
                    int Pos = ((y * CSurface.BLOCK_WIDTH) + x) * 4;

                    // colored lightmaps
                    ColR = lmBuffer[Pos + 0];
                    ColG = lmBuffer[Pos + 1];
                    ColB = lmBuffer[Pos + 2];

                    // mono lightmaps
                    //ColR = lmBuffer[Pos + 3];
                    //ColG = lmBuffer[Pos + 3];
                    //ColB = lmBuffer[Pos + 3];

                    vBitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(255, ColR, ColG, ColB));
                }
            }

            vBitmap.Save(MemStream, System.Drawing.Imaging.ImageFormat.Png);
            MemStream.Seek(0, System.IO.SeekOrigin.Begin);

            Lightmaps.Add(Texture2D.FromStream(CProgram.gQ2Game.gGraphicsDevice, MemStream));

            if (CProgram.gQ2Game.gCMain.r_writelightmap == true)
            {
                using (Stream strm = File.OpenWrite("lightmap" + (Lightmaps.Count - 1) + ".png"))
                {
                    Lightmaps[Lightmaps.Count - 1].SaveAsPng(strm, vBitmap.Width, vBitmap.Height);
                }

                //Lightmaps[Lightmaps.Count - 1].Save("lightmap" + (Lightmaps.Count - 1) + ".bmp", ImageFileFormat.Bmp);
            }
        }

        public void StartLightmaps(ref CModel.SModel _SModel)
        {
            if (_SModel.WorldLightmaps != null)
            {
                for (int i = 0; i < _SModel.WorldLightmaps.Length; i++)
                {
                    _SModel.WorldLightmaps[i].Dispose();
                    _SModel.WorldLightmaps[i] = null;
                }
            }
            _SModel.WorldLightmaps = null;

            for (int i = 0; i < Lightmaps.Count; i++)
            {
                Lightmaps[i].Dispose();
                Lightmaps[i] = null;
            }
            Lightmaps.Clear();
        }

        public void FinalizeLightmaps(ref CModel.SModel _SModel)
        {
            _SModel.WorldLightmaps = Lightmaps.ToArray();

            for (int i = 0; i < _SModel.WorldLightmaps.Length; i++)
            {
                _SModel.WorldLightmaps[i] = Lightmaps[i];
            }
        }

        public int FindImage(string FileName, out int Width, out int Height, EImageType imgtype)
        {
            int index;

            if (imgtype == EImageType.IT_WALL)
            {
                STextureWAL TexWAL;
                
                if (CProgram.gQ2Game.gCMain.r_htic2 == false)
                {
                    // load Quake2 WAL
                    TexWAL = LoadWAL(FileName, out Width, out Height);
                }
                else
                {
                    // load Heretic2 M8
                    TexWAL = LoadM8(FileName, out Width, out Height);
                }

                if (TexWAL.Tex2D != null)
                    Textures.Add(TexWAL);

                return Textures.Count - 1;
            }
            else if (imgtype == EImageType.IT_SKY)
            {
                if (CProgram.gQ2Game.gCMain.r_htic2 == false)
                {
                    Texture2D Tex2D;

                    // load general image
                    Tex2D = LoadSky("env\\" + FileName + ".tga", out Width, out Height);

                    if (Tex2D != null)
                        Skies.Add(Tex2D);
                }
                else
                {
                    STextureWAL TexWAL;

                    // load Heretic2 M8
                    TexWAL = LoadM8("pics\\skies\\" + FileName, out Width, out Height);

                    if (TexWAL.Tex2D != null)
                        Skies.Add(TexWAL.Tex2D);
                }

                return Skies.Count - 1;
            }
            else if (imgtype == EImageType.IT_PIC)
            {
                Texture2D Tex2D;

                Tex2D = LoadPic(FileName, out Width, out Height);
                Pictures.Add(Tex2D);
                return Pictures.Count - 1;
            }
            else
            {
                index = -1;
                Width = 0;
                Height = 0;
            }

            return index;
        }

        private MemoryStream LoadAsPNG(Stream strm, out int Width, out int Height)
        {
            MemoryStream ms = null;

            try
            {
                // attempt loading the image stream with standard .NET

                strm.Seek(0, System.IO.SeekOrigin.Begin);

                Bitmap vBitmap = new Bitmap(strm);
                Width = vBitmap.Width;
                Height = vBitmap.Height;

                ms = new MemoryStream();
                vBitmap.Save(ms, ImageFormat.Png);
            }
            catch
            {
                // incase of an exception the image stream may not be a validly .NET supported format
                // lets try Targa (TGA), maybe we'll get lucky

                strm.Seek(0, System.IO.SeekOrigin.Begin);

                Paloma.TargaImage vTargaImage = new Paloma.TargaImage(strm);

                Width = vTargaImage.Image.Width;
                Height = vTargaImage.Image.Height;

                ms = new MemoryStream();
                vTargaImage.Image.Save(ms, ImageFormat.Png);
            }

            return ms;
        }

        private Texture2D LoadPCX(string FileName, out int Width, out int Height)
        {
            MemoryStream ms;
            Texture2D Tex2D;
            Bitmap vBitmap;
            int ScaledWidth;
            int ScaledHeight;

            CFiles.SPCX _SPCX;
            BinaryReader br;
            byte dataByte;
            byte[,] pix;
            byte[] raw32;
            int i;
            int runLength;

            if (Path.GetExtension(FileName) != ".pcx" && Path.GetExtension(FileName) != "")
                FileName = Path.GetDirectoryName(FileName) + "\\" + Path.GetFileNameWithoutExtension(FileName) + ".pcx";

            FileName = FileName.Replace("\\", "/").ToLower();
            ms = null;

            if (CProgram.gQ2Game.gCMain.r_usepak == true)
            {
                CProgram.gQ2Game.gCMain.gCFiles.FS_ReadFile(FileName, out ms);
            }
            else
            {
                string FilePath = CProgram.gQ2Game.Content.RootDirectory + "\\" + CConfig.GetConfigSTRING("Base Game") + "\\" + FileName;
                FileStream _fs = null;
                byte[] buf;

                if (File.Exists(FilePath) == false)
                {
                    Width = 0;
                    Height = 0;

                    return null;
                }

                _fs = new System.IO.FileStream(FilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                buf = new byte[_fs.Length];

                _fs.Read(buf, 0, (int)_fs.Length);
                ms = new MemoryStream(buf);

                if (_fs != null)
                    _fs.Close();
            }

            ms.Seek(0, System.IO.SeekOrigin.Begin);
            br = new BinaryReader(ms);

            _SPCX.manufacturer = br.ReadByte();
            _SPCX.version = br.ReadByte();
            _SPCX.encoding = br.ReadByte();
            _SPCX.bits_per_pixel = br.ReadByte();

            _SPCX.xmin = br.ReadUInt16();
            _SPCX.ymin = br.ReadUInt16();
            _SPCX.xmax = br.ReadUInt16();
            _SPCX.ymax = br.ReadUInt16();
            _SPCX.hres = br.ReadUInt16();
            _SPCX.vres = br.ReadUInt16();

            _SPCX.palette = br.ReadBytes(48);
            _SPCX.reserved = br.ReadByte();
            _SPCX.color_planes = br.ReadByte();
            _SPCX.bytes_per_line = br.ReadUInt16();
            _SPCX.palette_type = br.ReadUInt16();
            _SPCX.filler = br.ReadBytes(58);

            if (_SPCX.manufacturer != 0x0a
                || _SPCX.version != 5
                || _SPCX.encoding != 1
                || _SPCX.bits_per_pixel != 8
                || _SPCX.xmax >= 640
                || _SPCX.ymax >= 480)
            {
                System.Diagnostics.Debug.WriteLine("Bad PCX file.");

                Width = 0;
                Height = 0;

                return null;
            }

            ScaledWidth = _SPCX.xmax + 1;
            ScaledHeight = _SPCX.ymax + 1;

            pix = new byte[ScaledWidth, ScaledHeight];

            for (int y = 0; y <= _SPCX.ymax; y++)
            {
                for (int x = 0; x <= _SPCX.xmax; )
                {
                    dataByte = br.ReadByte();

                    if ((dataByte & 0xC0) == 0xC0)
                    {
                        runLength = dataByte & 0x3F;
                        dataByte = br.ReadByte();
                    }
                    else
                        runLength = 1;

                    while (runLength-- > 0)
                        pix[x++, y] = dataByte;
                }
            }

            br.Close();
            br = null;

            ms.Close();
            ms = null;


            // convert 8-bit PCX texture to 32-bit
            raw32 = new byte[ScaledWidth * ScaledHeight * 4];
            i = 0;

            for (int y = 0; y < ScaledHeight; y++)
            {
                for (int x = 0; x < ScaledWidth; x++)
                {
                    byte r = Palette[pix[x, y] * 3 + 0];
                    byte g = Palette[pix[x, y] * 3 + 1];
                    byte b = Palette[pix[x, y] * 3 + 2];

                    raw32[i + 0] = r;
                    raw32[i + 1] = g;
                    raw32[i + 2] = b;
                    raw32[i + 3] = 255;
                    i += 4;
                }
            }


            // use the PCX data to generate an in-memory bitmap
            ms = new System.IO.MemoryStream();
            vBitmap = new Bitmap(ScaledWidth, ScaledHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int y = 0; y < ScaledHeight; y++)
            {
                for (int x = 0; x < ScaledWidth; x++)
                {
                    int Pos = ((y * ScaledWidth) + x) * 4;

                    int ColR = raw32[Pos + 0];
                    int ColG = raw32[Pos + 1];
                    int ColB = raw32[Pos + 2];
                    int ColA = raw32[Pos + 3];

                    vBitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(ColA, ColR, ColG, ColB));
                }
            }

            vBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Seek(0, System.IO.SeekOrigin.Begin);
            
            Width = ScaledWidth;
            Height = ScaledHeight;
            
            
            // check texture if its pow2
            for (ScaledWidth = 1; ScaledWidth < Width; ScaledWidth <<= 1) ;
            for (ScaledHeight = 1; ScaledHeight < Height; ScaledHeight <<= 1) ;

            if (ScaledWidth > 2048)
                ScaledWidth = 2048;

            if (ScaledHeight > 2048)
                ScaledHeight = 2048;
            
            
            // load texture from stream
            ms.Seek(0, System.IO.SeekOrigin.Begin);
            Tex2D = Texture2D.FromStream(CProgram.gQ2Game.gGraphicsDevice, ms, /*Scaled*/Width, /*Scaled*/Height, false);
            ms.Close();
            Tex2D.Name = FileName;

            return Tex2D;
        }

        public Texture2D LoadSkin(string FileName, out int Width, out int Height)
        {
            Texture2D Tex2D = LoadPCX(FileName, out Width, out Height);

            return Tex2D;
        }

        public Texture2D LoadSkin(string FileName)
        {
            int Width;
            int Height;

            return LoadSkin(FileName, out Width, out Height);
        }

        private STextureWAL LoadWAL(string FileName, out int Width, out int Height)
        {
            FileStream fs;
            MemoryStream ms;

            BinaryReader br;
            STextureWAL Wal;
            byte[] Wal8;
            byte[] Wal32;

            MemoryStream MemStream;
            Bitmap vBitmap;
            int ScaledWidth;
            int ScaledHeight;

            FileName = Path.GetDirectoryName(FileName) + "\\" + Path.GetFileNameWithoutExtension(FileName) + ".wal";

            Wal.Name = null;
            Wal.OriginalWidth = 0;
            Wal.OriginalHeight = 0;
            Wal.ScaledWidth = 0;
            Wal.ScaledHeight = 0;
            Wal.Offsets = null;
            Wal.AnimName = null;
            Wal.Flags = 0;
            Wal.Contents = 0;
            Wal.Value = 0;
            Wal.Tex2D = null;


            // search for a loaded texture
            for (int i = 0; i < Textures.Count; i++)
            {
                if (Path.GetFileNameWithoutExtension(Textures[i].Name) == Path.GetFileNameWithoutExtension(FileName))
                {
                    // returning the unscaled (original) texture size
                    Width = Textures[i].OriginalWidth;
                    Height = Textures[i].OriginalHeight;

                    return Textures[i];
                }
            }


            // load the WAL file
            if (CProgram.gQ2Game.gCMain.r_usepak == true)
            {
                FileName = FileName.Replace("\\", "/").ToLower();
                CProgram.gQ2Game.gCMain.gCFiles.FS_ReadFile(FileName, out ms);

                if (ms == null)
                {
                    Width = 0;
                    Height = 0;

                    return Wal;
                }

                br = new System.IO.BinaryReader(ms);
            }
            else
            {
                string FullPath;

                FileName = FileName.ToLower();
                FullPath = CProgram.gQ2Game.Content.RootDirectory + "\\" + CConfig.GetConfigSTRING("Base Game") + "\\" + FileName;

                if (File.Exists(FullPath) == true)
                {
                    fs = new System.IO.FileStream(FullPath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                }
                else
                {
                    fs = null;
                    Width = 0;
                    Height = 0;

                    return Wal;
                }

                br = new System.IO.BinaryReader(fs);
            }

            Wal.Name = CShared.Com_ToString(br.ReadChars(32));
            Wal.OriginalWidth = br.ReadInt32();
            Wal.OriginalHeight = br.ReadInt32();


            // read the mipmap texture offsets
            Wal.Offsets = new int[MIPLEVELS_QUAKE2];
            for (int i = 0; i < MIPLEVELS_QUAKE2; i++)
            {
                Wal.Offsets[i] = br.ReadInt32();
            }

            Wal.AnimName = CShared.Com_ToString(br.ReadChars(32));
            Wal.Flags = br.ReadInt32();
            Wal.Contents = br.ReadInt32();
            Wal.Value = br.ReadInt32();

            // read the first mipmap texture (we are only interested in the first mipmap)
            br.BaseStream.Seek(Wal.Offsets[0], System.IO.SeekOrigin.Begin);
            Wal8 = br.ReadBytes(Wal.OriginalWidth * Wal.OriginalHeight);
            br.Close();

            Wal32 = new byte[Wal.OriginalWidth * Wal.OriginalHeight * 4];


            // convert 8-bit WAL texture to 32-bit
            for (long i = 0, j = 0; i < (Wal.OriginalWidth * Wal.OriginalHeight); i++, j += 4)
            {
                byte r = Palette[Wal8[i] * 3 + 0];
                byte g = Palette[Wal8[i] * 3 + 1];
                byte b = Palette[Wal8[i] * 3 + 2];

                // the idTech2 engine doubles the intensity of textures and when rendering translucent surfaces
                // it halves the intensity which brings translucent textures back to its original intensity
                // with our renderer we did not double the intensity of textures, to get the same effect we just
                // halve the intensity of translucent textures
                if (
                    ((CQ2BSP.ESurface)Wal.Flags & CQ2BSP.ESurface.SURF_TRANS33) == CQ2BSP.ESurface.SURF_TRANS33
                    | ((CQ2BSP.ESurface)Wal.Flags & CQ2BSP.ESurface.SURF_TRANS66) == CQ2BSP.ESurface.SURF_TRANS66
                    )
                {
                    r = (byte)Math.Floor((float)r / 2.0f);
                    g = (byte)Math.Floor((float)g / 2.0f);
                    b = (byte)Math.Floor((float)b / 2.0f);
                }

                Wal32[j + 0] = r;
                Wal32[j + 1] = g;
                Wal32[j + 2] = b;
                Wal32[j + 3] = 255;
            }


            // use the WAL data to generate an in-memory bitmap
            MemStream = new System.IO.MemoryStream();
            vBitmap = new Bitmap(Wal.OriginalWidth, Wal.OriginalHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int y = 0; y < Wal.OriginalHeight; y++)
            {
                for (int x = 0; x < Wal.OriginalWidth; x++)
                {
                    int Pos = ((y * Wal.OriginalWidth) + x) * 4;

                    int ColR = Wal32[Pos + 0];
                    int ColG = Wal32[Pos + 1];
                    int ColB = Wal32[Pos + 2];
                    int ColA = Wal32[Pos + 3];

                    vBitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(ColA, ColR, ColG, ColB));
                }
            }

            vBitmap.Save(MemStream, System.Drawing.Imaging.ImageFormat.Png);
            MemStream.Seek(0, System.IO.SeekOrigin.Begin);

            Width = ScaledWidth = Wal.OriginalWidth;
            Height = ScaledHeight = Wal.OriginalHeight;

            
            // check texture if its pow2
            for (ScaledWidth = 1; ScaledWidth < Width; ScaledWidth <<= 1) ;
            for (ScaledHeight = 1; ScaledHeight < Height; ScaledHeight <<= 1) ;

            if (ScaledWidth > 2048)
                ScaledWidth = 2048;

            if (ScaledHeight > 2048)
                ScaledHeight = 2048;

            Wal.ScaledWidth = ScaledWidth;
            Wal.ScaledHeight = ScaledHeight;

            MemStream.Seek(0, System.IO.SeekOrigin.Begin);


            // load texture from memory
            Wal.Tex2D = Texture2D.FromStream(CProgram.gQ2Game.gGraphicsDevice, MemStream, Wal.ScaledWidth, Wal.ScaledHeight, true);
            Wal.Tex2D.Name = FileName;

            return Wal;
        }

        private STextureWAL LoadM8(string FileName, out int Width, out int Height)
        {
            FileStream fs;
            MemoryStream ms;

            BinaryReader br;
            STextureWAL Wal;
            int FileVersion;
            byte[] PaletteHtic2;
            byte[] M8;
            byte[] Wal32;

            MemoryStream MemStream;
            Bitmap vBitmap;
            int ScaledWidth;
            int ScaledHeight;

            FileName = Path.GetDirectoryName(FileName) + "\\" + Path.GetFileNameWithoutExtension(FileName) + ".m8";

            Wal.Name = null;
            Wal.OriginalWidth = 0;
            Wal.OriginalHeight = 0;
            Wal.ScaledWidth = 0;
            Wal.ScaledHeight = 0;
            Wal.Offsets = null;
            Wal.AnimName = null;
            Wal.Flags = 0;
            Wal.Contents = 0;
            Wal.Value = 0;
            Wal.Tex2D = null;


            // search for a loaded texture
            for (int i = 0; i < Textures.Count; i++)
            {
                if (Path.GetFileNameWithoutExtension(Textures[i].Name) == Path.GetFileNameWithoutExtension(FileName))
                {
                    // returning the unscaled (original) texture size
                    Width = Textures[i].OriginalWidth;
                    Height = Textures[i].OriginalHeight;

                    return Textures[i];
                }
            }


            // load the M8 file
            if (CProgram.gQ2Game.gCMain.r_usepak == true)
            {
                FileName = FileName.Replace("\\", "/").ToLower();
                CProgram.gQ2Game.gCMain.gCFiles.FS_ReadFile(FileName, out ms);

                if (ms == null)
                {
                    Width = 0;
                    Height = 0;

                    return Wal;
                }

                br = new System.IO.BinaryReader(ms);
            }
            else
            {
                string FullPath;

                FileName = FileName.ToLower();
                FullPath = CProgram.gQ2Game.Content.RootDirectory + "\\" + CConfig.GetConfigSTRING("Base Game") + "\\" + FileName;

                if (File.Exists(FullPath) == true)
                {
                    fs = new System.IO.FileStream(FullPath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                }
                else
                {
                    fs = null;
                    Width = 0;
                    Height = 0;

                    return Wal;
                }

                br = new System.IO.BinaryReader(fs);
            }


            // read the M8 file version
            // the file version must be 2 for Heretic II textures
            // we will just ignore that fact for now
            FileVersion = br.ReadInt32();
            Wal.Name = CShared.Com_ToString(br.ReadChars(32));

            // read the first of 16 mipmap width entries and skip the rest
            Wal.OriginalWidth = br.ReadInt32();
            br.BaseStream.Seek(br.BaseStream.Position + ((MIPLEVELS_HERETIC2 - 1) * sizeof(int)), SeekOrigin.Begin);

            // read the first of 16 mipmap height entries and skip the rest
            Wal.OriginalHeight = br.ReadInt32();
            br.BaseStream.Seek(br.BaseStream.Position + ((MIPLEVELS_HERETIC2 - 1) * sizeof(int)), SeekOrigin.Begin);

            // read the mipmap texture offsets
            Wal.Offsets = new int[MIPLEVELS_HERETIC2];
            for (int i = 0; i < MIPLEVELS_HERETIC2; i++)
            {
                Wal.Offsets[i] = br.ReadInt32();
            }

            Wal.AnimName = CShared.Com_ToString(br.ReadChars(32));

            PaletteHtic2 = br.ReadBytes(768);

            Wal.Flags = br.ReadInt32();
            Wal.Contents = br.ReadInt32();
            Wal.Value = br.ReadInt32();

            // read the first mipmap texture (we are only interested in the first mipmap)
            br.BaseStream.Seek(Wal.Offsets[0], System.IO.SeekOrigin.Begin);
            M8 = br.ReadBytes(Wal.OriginalWidth * Wal.OriginalHeight);
            br.Close();

            Wal32 = new byte[Wal.OriginalWidth * Wal.OriginalHeight * 4];


            // convert 8-bit M8 texture to 32-bit
            for (long i = 0, j = 0; i < (Wal.OriginalWidth * Wal.OriginalHeight); i++, j += 4)
            {
                byte r = PaletteHtic2[M8[i] * 3 + 0];
                byte g = PaletteHtic2[M8[i] * 3 + 1];
                byte b = PaletteHtic2[M8[i] * 3 + 2];

                // the idTech2 engine doubles the intensity of textures and when rendering translucent surfaces
                // it halves the intensity which brings translucent textures back to its original intensity
                // with our renderer we did not double the intensity of textures, to get the same effect we just
                // halve the intensity of translucent textures
                if (
                    ((CQ2BSP.ESurface)Wal.Flags & CQ2BSP.ESurface.SURF_TRANS33) == CQ2BSP.ESurface.SURF_TRANS33
                    | ((CQ2BSP.ESurface)Wal.Flags & CQ2BSP.ESurface.SURF_TRANS66) == CQ2BSP.ESurface.SURF_TRANS66
                    )
                {
                    r = (byte)Math.Floor((float)r / 2.0f);
                    g = (byte)Math.Floor((float)g / 2.0f);
                    b = (byte)Math.Floor((float)b / 2.0f);
                }

                Wal32[j + 0] = r;
                Wal32[j + 1] = g;
                Wal32[j + 2] = b;
                Wal32[j + 3] = 255;
            }


            // use the M8 data to generate an in-memory bitmap
            MemStream = new System.IO.MemoryStream();
            vBitmap = new Bitmap(Wal.OriginalWidth, Wal.OriginalHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int y = 0; y < Wal.OriginalHeight; y++)
            {
                for (int x = 0; x < Wal.OriginalWidth; x++)
                {
                    int Pos = ((y * Wal.OriginalWidth) + x) * 4;

                    int ColR = Wal32[Pos + 0];
                    int ColG = Wal32[Pos + 1];
                    int ColB = Wal32[Pos + 2];
                    int ColA = Wal32[Pos + 3];

                    vBitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(ColA, ColR, ColG, ColB));
                }
            }

            vBitmap.Save(MemStream, System.Drawing.Imaging.ImageFormat.Png);
            MemStream.Seek(0, System.IO.SeekOrigin.Begin);

            Width = ScaledWidth = Wal.OriginalWidth;
            Height = ScaledHeight = Wal.OriginalHeight;


            // check texture if its pow2
            for (ScaledWidth = 1; ScaledWidth < Width; ScaledWidth <<= 1) ;
            for (ScaledHeight = 1; ScaledHeight < Height; ScaledHeight <<= 1) ;

            if (ScaledWidth > 2048)
                ScaledWidth = 2048;

            if (ScaledHeight > 2048)
                ScaledHeight = 2048;

            Wal.ScaledWidth = ScaledWidth;
            Wal.ScaledHeight = ScaledHeight;

            MemStream.Seek(0, System.IO.SeekOrigin.Begin);


            // load texture from memory
            Wal.Tex2D = Texture2D.FromStream(CProgram.gQ2Game.gGraphicsDevice, MemStream, Wal.ScaledWidth, Wal.ScaledHeight, true);
            Wal.Tex2D.Name = FileName;

            return Wal;
        }

        private Texture2D LoadPic(string FileName, out int Width, out int Height)
        {
            MemoryStream ms;
            Texture2D Tex2D;
            int ScaledWidth;
            int ScaledHeight;

            if (Path.GetExtension(FileName) != ".png" && Path.GetExtension(FileName) != ".tga" && Path.GetExtension(FileName) != ".pcx" && Path.GetExtension(FileName) != "")
                FileName = Path.GetDirectoryName(FileName) + "\\" + Path.GetFileNameWithoutExtension(FileName) + ".png";

            FileName = FileName.Replace("\\", "/").ToLower();
            ms = null;

            if (CProgram.gQ2Game.gCMain.r_usepak == true)
            {
                MemoryStream _ms = null;

                CProgram.gQ2Game.gCMain.gCFiles.FS_ReadFile(FileName, out _ms);
                ms = LoadAsPNG(_ms, out ScaledWidth, out ScaledHeight);

                if (_ms != null)
                    _ms.Close();
            }
            else
            {
                string FilePath = CProgram.gQ2Game.Content.RootDirectory + "\\" + CConfig.GetConfigSTRING("Base Game") + "\\" + FileName;
                FileStream _fs = null;

                if (File.Exists(FilePath) == false)
                    FilePath = Path.ChangeExtension(FilePath, ".png");

                _fs = new System.IO.FileStream(FilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                ms = LoadAsPNG(_fs, out ScaledWidth, out ScaledHeight);

                if (_fs != null)
                    _fs.Close();
            }

            Width = ScaledWidth;
            Height = ScaledHeight;


            // search for a loaded texture
            for (int i = 0; i < Pictures.Count; i++)
            {
                if (Path.GetFileNameWithoutExtension(Pictures[i].Name) == Path.GetFileNameWithoutExtension(FileName))
                {
                    return Pictures[i];
                }
            }


            // check texture if its pow2
            for (ScaledWidth = 1; ScaledWidth < Width; ScaledWidth <<= 1) ;
            for (ScaledHeight = 1; ScaledHeight < Height; ScaledHeight <<= 1) ;

            if (ScaledWidth > 2048)
                ScaledWidth = 2048;

            if (ScaledHeight > 2048)
                ScaledHeight = 2048;


            // load texture from stream
            ms.Seek(0, System.IO.SeekOrigin.Begin);
            Tex2D = Texture2D.FromStream(CProgram.gQ2Game.gGraphicsDevice, ms, ScaledWidth, ScaledHeight, true);
            ms.Close();
            Tex2D.Name = FileName;

            return Tex2D;
        }

        private Texture2D LoadSky(string FileName, out int Width, out int Height)
        {
            MemoryStream ms;
            Texture2D Tex2D;
            int ScaledWidth;
            int ScaledHeight;

            if (Path.GetExtension(FileName) != ".png" && Path.GetExtension(FileName) != ".tga" && Path.GetExtension(FileName) != ".pcx" && Path.GetExtension(FileName) != "")
                FileName = Path.GetDirectoryName(FileName) + "\\" + Path.GetFileNameWithoutExtension(FileName) + ".png";

            FileName = FileName.Replace("\\", "/").ToLower();
            ms = null;

            if (CProgram.gQ2Game.gCMain.r_usepak == true)
            {
                MemoryStream _ms = null;

                CProgram.gQ2Game.gCMain.gCFiles.FS_ReadFile(FileName, out _ms);
                ms = LoadAsPNG(_ms, out ScaledWidth, out ScaledHeight);

                if (_ms != null)
                    _ms.Close();
            }
            else
            {
                string FilePath = CProgram.gQ2Game.Content.RootDirectory + "\\" + CConfig.GetConfigSTRING("Base Game") + "\\" + FileName;
                FileStream _fs = null;

                if (File.Exists(FilePath) == false)
                    FilePath = Path.ChangeExtension(FilePath, ".png");

                _fs = new System.IO.FileStream(FilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                ms = LoadAsPNG(_fs, out ScaledWidth, out ScaledHeight);

                if (_fs != null)
                    _fs.Close();
            }

            Width = ScaledWidth;
            Height = ScaledHeight;


            // search for a loaded texture
            for (int i = 0; i < Skies.Count; i++)
            {
                if (Path.GetFileNameWithoutExtension(Skies[i].Name) == Path.GetFileNameWithoutExtension(FileName))
                {
                    if (ms != null)
                        ms.Close();

                    return Skies[i];
                }
            }


            // check texture if its pow2
            for (ScaledWidth = 1; ScaledWidth < Width; ScaledWidth <<= 1) ;
            for (ScaledHeight = 1; ScaledHeight < Height; ScaledHeight <<= 1) ;

            if (ScaledWidth > 2048)
                ScaledWidth = 2048;

            if (ScaledHeight > 2048)
                ScaledHeight = 2048;


            // load texture from stream
            ms.Seek(0, System.IO.SeekOrigin.Begin);
            Tex2D = Texture2D.FromStream(CProgram.gQ2Game.gGraphicsDevice, ms, ScaledWidth, ScaledHeight, true);
            ms.Close();
            Tex2D.Name = FileName;

            return Tex2D;
        }

        public void StartWAL(ref CModel.SModel _SModel)
        {
            // clear worldmodel texture array
            if (_SModel.WorldTextures != null)
            {
                for (int i = 0; i < _SModel.WorldTextures.Length; i++)
                {
                    if (_SModel.WorldTextures[i].Tex2D != null)
                    {
                        _SModel.WorldTextures[i].Tex2D.Dispose();
                        _SModel.WorldTextures[i].Tex2D = null;
                    }
                }
            }
            _SModel.WorldTextures = null;

            // clear texture loading list
            for (int i = 0; i < Textures.Count; i++)
            {
                STextureWAL _Texture = Textures[i];

                if (_Texture.Tex2D != null)
                {
                    _Texture.Tex2D.Dispose();
                    _Texture.Tex2D = null;
                }

                Textures[i] = _Texture;
            }
            Textures.Clear();
        }

        public void FinalizeWAL(ref CModel.SModel _SModel)
        {
            _SModel.WorldTextures = new STextureWAL[Textures.Count];

            for (int i = 0; i < Textures.Count; i++)
            {
                _SModel.WorldTextures[i] = Textures[i];
            }
        }

        public void StartSky(ref CModel.SModel _SModel)
        {
            if (_SModel.WorldSkies != null)
            {
                for (int i = 0; i < _SModel.WorldSkies.Length; i++)
                {
                    _SModel.WorldSkies[i].Dispose();
                    _SModel.WorldSkies[i] = null;
                }
            }
            _SModel.WorldSkies = null;

            for (int i = 0; i < Skies.Count; i++)
            {
                Skies[i].Dispose();
                Skies[i] = null;
            }
            Skies.Clear();
        }

        public void FinalizeSky(ref CModel.SModel _SModel)
        {
            _SModel.WorldSkies = Skies.ToArray();
        }


        public enum EImageType
        {
            IT_SKIN,
            IT_SPRITE,
            IT_WALL,
            IT_SKY,
            IT_PIC
        }


        // =====================================================================
        // 
        // .WAL TEXTURE FORMAT
        // 
        // =====================================================================

        public struct STextureWAL
        {
            public string Name;         // size: 32
            public int OriginalWidth;   // original WAL width
            public int OriginalHeight;  // original WAL height
            public int ScaledWidth;     // scaled pow2 width
            public int ScaledHeight;    // scaled pow2 height
            public int[] Offsets;       // size: MIPLEVELS
            public string AnimName;     // size: 32 (next in the animation sequence)
            public int Flags;           // texture (surface) flags
            public int Contents;        // texture content flags
            public int Value;           // value attached to texture (light) / content (lava damage) flags

            public Texture2D Tex2D;
        }

        private enum EPalette
        {
            Quake1,
            Quake2,
            Hexen2
        }

    }
}
