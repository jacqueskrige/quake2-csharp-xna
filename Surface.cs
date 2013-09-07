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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace Q2BSP
{
    public class CSurface
    {
        // original values for BLOCK_WIDTH and BLOCK_HEIGHT is 128
        // original values for MAX_LIGHTMAPS is 128

        private const int SUBDIVIDE_SIZE = 64;

        public const int BLOCK_WIDTH = 512;
        public const int BLOCK_HEIGHT = 512;

        public const int LIGHTMAP_BYTES = 4;
        public const int MAX_LIGHTMAPS = 16;

        private SLightmapState lms;

        public CSurface()
        {
            lms.lightmap_surfaces = new int[MAX_LIGHTMAPS];
            lms.allocated = new int[BLOCK_WIDTH];
            lms.lightmap_buffer = new byte[BLOCK_WIDTH * BLOCK_HEIGHT * LIGHTMAP_BYTES];
        }


        /// <summary>
        /// BoundPoly
        /// ---------
        /// Bind mins and maxs
        /// </summary>
        private void BoundPoly(int numverts, float[] verts, ref float[] mins, ref float[] maxs)
        {
            mins[0] = mins[1] = mins[2] = 9999;
            maxs[0] = maxs[1] = maxs[2] = -9999;

            for (int i = 0; i < numverts; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    mins[j] = Math.Min(mins[j], verts[(i * 3) + j]);
                    maxs[j] = Math.Max(maxs[j], verts[(i * 3) + j]);
                }
            }
        }

        /// <summary>
        /// SubdividePolygon
        /// ----------------
        /// Breaks a polygon up along axial 64 unit boundaries
        /// so that turbulent and sky warps can be done reasonably.
        /// </summary>
        public void SubdividePolygon(CModel.SMSurface surf, ref List<CModel.SGLPoly> polys, int numverts, float[] verts)
        {
            float[] mins, maxs, dist;
            float m;
            float frac;
            int i, j, k, v;
            float[] Front, Back;
            int f, b;
            CModel.SGLPoly poly;
            Vector3 total;
            float total_s;
            float total_t;

            mins = new float[3];
            maxs = new float[3];
            dist = new float[64];

            Front = new float[64 * 3];
            Back = new float[64 * 3];

            if (numverts > 60)
                CMain.Error(CMain.EErrorParm.ERR_WARNING, "(error) numverts = " + numverts);

            // Bind mins and maxs
            BoundPoly(numverts, verts, ref mins, ref maxs);

            for (i = 0; i < 3; i++)
            {
                m = (mins[i] + maxs[i]) * 0.5f;
                m = SUBDIVIDE_SIZE * (float)Math.Floor(m / (float)SUBDIVIDE_SIZE + 0.5f);

                if (maxs[i] - m < 8)
                    continue;
                if (m - mins[i] < 8)
                    continue;

                // cut it
                v = i;
                for (j = 0; j < numverts; j++, v += 3)
                {
                    dist[j] = verts[v] - m;
                }

                // wrap cases
                dist[j] = dist[0];
                v -= i;
                verts[v + 0] = verts[0 + 0];
                verts[v + 1] = verts[0 + 1];
                verts[v + 2] = verts[0 + 2];

                f = 0;
                b = 0;
                v = 0;
                for (j = 0; j < numverts; j++, v += 3)
                {
                    if (dist[j] >= 0)
                    {
                        Front[(f * 3) + 0] = verts[v + 0];
                        Front[(f * 3) + 1] = verts[v + 1];
                        Front[(f * 3) + 2] = verts[v + 2];
                        f++;
                    }
                    if (dist[j] <= 0)
                    {
                        Back[(b * 3) + 0] = verts[v + 0];
                        Back[(b * 3) + 1] = verts[v + 1];
                        Back[(b * 3) + 2] = verts[v + 2];
                        b++;
                    }

                    if (dist[j] == 0 || dist[j + 1] == 0)
                        continue;

                    if ((dist[j] > 0) != (dist[j + 1] > 0))
                    {
                        // clip point
                        frac = dist[j] / (dist[j] - dist[j + 1]);

                        for (k = 0; k < 3; k++)
                        {
                            Front[(f * 3) + k] = Back[(b * 3) + k] = verts[v + k] + frac * (verts[3 + v + k] - verts[v + k]);
                        }
                        f++;
                        b++;
                    }
                }

                SubdividePolygon(surf, ref polys, f, Front);
                SubdividePolygon(surf, ref polys, b, Back);

                return;
            }


            // add a point in the center to help keep warp valid
            poly.next = 0;
            poly.chain = 0;
            poly.numverts = numverts + 2;
            poly.verts = new CModel.SPolyVerts[poly.numverts];

            total = Vector3.Zero;
            total_s = 0.0f;
            total_t = 0.0f;

            v = 0;
            for (i = 0; i < numverts; i++, v += 3)
            {
                float s;
                float t;
                Vector3 vec0;
                Vector3 vec1;

                poly.verts[i + 1].vertex.Position.X = verts[v + 0];
                poly.verts[i + 1].vertex.Position.Y = verts[v + 1];
                poly.verts[i + 1].vertex.Position.Z = verts[v + 2];

                vec0.X = CQ2BSP.SWorldData.texinfo[surf.texinfo].vecs[0].X;
                vec0.Y = CQ2BSP.SWorldData.texinfo[surf.texinfo].vecs[0].Y;
                vec0.Z = CQ2BSP.SWorldData.texinfo[surf.texinfo].vecs[0].Z;
                vec1.X = CQ2BSP.SWorldData.texinfo[surf.texinfo].vecs[1].X;
                vec1.Y = CQ2BSP.SWorldData.texinfo[surf.texinfo].vecs[1].Y;
                vec1.Z = CQ2BSP.SWorldData.texinfo[surf.texinfo].vecs[1].Z;

                s = Vector3.Dot(poly.verts[i + 1].vertex.Position, vec0);
                t = Vector3.Dot(poly.verts[i + 1].vertex.Position, vec1);

                total_s += s;
                total_t += t;
                total.X += verts[v + 0];
                total.Y += verts[v + 1];
                total.Z += verts[v + 2];

                poly.verts[i + 1].vertex.TextureCoordinate.X = s;
                poly.verts[i + 1].vertex.TextureCoordinate.Y = t;

                poly.verts[i + 1].vertex.LightmapCoordinate.X = 0.0f;
                poly.verts[i + 1].vertex.LightmapCoordinate.Y = 0.0f;

                poly.verts[i + 1].vertex.Normal.X = 0.0f;
                poly.verts[i + 1].vertex.Normal.Y = 0.0f;
                poly.verts[i + 1].vertex.Normal.Z = 0.0f;
            }

            CShared.VectorScale(total, (1.0f / numverts), ref poly.verts[0].vertex.Position);
            poly.verts[0].vertex.TextureCoordinate.X = total_s / numverts;
            poly.verts[0].vertex.TextureCoordinate.Y = total_t / numverts;

            // copy first vertex to last
            poly.verts[i + 1] = poly.verts[1];

            // insert centered point at first index
            polys.Insert(0, poly);
        }

        /// <summary>
        /// SubdivideSurface
        /// ----------------
        /// Breaks a polygon up along axial 64 unit boundaries
        /// so that turbulent and sky warps can be done reasonably.
        /// </summary>
        public void SubdivideSurface(ref CModel.SMSurface surf)
        {
            float[] verts;
            int numverts;
            List<CModel.SGLPoly> polys;

            if ((surf.flags & CModel.EMSurface.SURF_DRAWTURB) != CModel.EMSurface.SURF_DRAWTURB)
                return;

            verts = new float[64 * 3];
            numverts = surf.polys[0].numverts;

            for (int i = 0; i < surf.polys[0].numverts; i++)
            {
                verts[(i * 3) + 0] = surf.polys[0].verts[i].vertex.Position.X;
                verts[(i * 3) + 1] = surf.polys[0].verts[i].vertex.Position.Y;
                verts[(i * 3) + 2] = surf.polys[0].verts[i].vertex.Position.Z;
            }

            surf.polys = null;
            polys = new List<CModel.SGLPoly>();

            SubdividePolygon(surf, ref polys, numverts, verts);

            surf.polys = polys.ToArray();
            polys.Clear();
        }

        /// <summary>
        /// BuildSurfaceIndex
        /// ------------------
        /// Convert the surface's vertex format from triangle fan to triangle list
        /// This is used to speed up surface warping by using an index buffer
        /// </summary>
        public void BuildSurfaceIndex(ref CModel.SMSurface surf)
        {
            List<int> ib;

            //if ((surf.flags & CModel.EMSurface.SURF_DRAWTURB) != CModel.EMSurface.SURF_DRAWTURB)
            //    return;

            ib = new List<int>();
            ib.Clear();

            for (int i = 0; i < surf.polys.Length; i++)
            {
                for (int j = 2; j < surf.polys[i].verts.Length; j++)
                {
                    ib.Add(surf.polys[i].verts[0].offset);
                    ib.Add(surf.polys[i].verts[j - 1].offset);
                    ib.Add(surf.polys[i].verts[j].offset);
                }
            }

            surf.ibData = ib.ToArray();

            // setup the index buffer
            if (CProgram.gQ2Game.gGraphicsDevice.GraphicsProfile == GraphicsProfile.HiDef)
            {
                surf.ibSurface = new IndexBuffer(CProgram.gQ2Game.gGraphicsDevice, IndexElementSize.ThirtyTwoBits /*typeof(int)*/, surf.ibData.Length, BufferUsage.WriteOnly);
                surf.ibSurface.SetData(surf.ibData);
            }
            else
            {
                short[] ibData16 = new short[surf.ibData.Length];

                for (int i = 0; i < surf.ibData.Length; i++)
                    ibData16[i] = (short)surf.ibData[i];

                surf.ibSurface = new IndexBuffer(CProgram.gQ2Game.gGraphicsDevice, IndexElementSize.SixteenBits, ibData16.Length, BufferUsage.WriteOnly);
                surf.ibSurface.SetData(ibData16);
            }

            //surf.ibSurface.SetData(surf.ibData);
        }

        /// <summary>
        /// BuildSurfaceIndex
        /// ------------------
        /// Convert the surface's vertex format from triangle fan to triangle list
        /// This is used to speed up surface warping by using an index buffer
        /// </summary>
        public int[] BuildSurfaceIndex(CModel.SMSurface surf)
        {
            List<int> ib = new List<int>();
            ib.Clear();

            for (int i = 0; i < surf.polys.Length; i++)
            {
                for (int j = 2; j < surf.polys[i].verts.Length; j++)
                {
                    ib.Add(surf.polys[i].verts[0].offset);
                    ib.Add(surf.polys[i].verts[j - 1].offset);
                    ib.Add(surf.polys[i].verts[j].offset);
                }
            }

            return ib.ToArray();
        }

        /// <summary>
        /// BeginBuildingLightmaps
        /// ----------------------
        /// Initializes the lightmap building process
        /// </summary>
        public void BeginBuildingLightmaps(ref CModel.SModel _SModel)
        {
            CLocal.SLightStyle[] _LightStyle;

            // no dlightcache
            CMain.r_framecount = 1;

            // setup the base lightstyles so the lightmaps won't have to be regenerated
            // the first time they're seen
            _LightStyle = new CLocal.SLightStyle[CLocal.MAX_LIGHTSTYLES];
            for (int i = 0; i < CLocal.MAX_LIGHTSTYLES; i++)
            {
                _LightStyle[i].rgb = new float[3];
                _LightStyle[i].rgb[0] = 1.0f;
                _LightStyle[i].rgb[1] = 1.0f;
                _LightStyle[i].rgb[2] = 1.0f;
                _LightStyle[i].white = 3.0f;
            }

            CClient.cl.RefDef.lightstyles = _LightStyle;

            // jkrige ??
            //if (!gl_state.lightmap_textures)
            //{
            //    gl_state.lightmap_textures = TEXNUM_LIGHTMAPS;
            //}
            // jkrige ??

            CProgram.gQ2Game.gCMain.gCImage.StartLightmaps(ref _SModel);
            LM_InitBlock();
        }

        /// <summary>
        /// LM_InitBlock
        /// ------------
        /// Initializes a new lightmap texture and clears the pixel data
        /// </summary>
        public void LM_InitBlock()
        {
            for (int i = 0; i < lms.allocated.Length; i++)
            {
                lms.allocated[i] = 0;
            }

            for (int i = 0; i < (BLOCK_WIDTH * BLOCK_HEIGHT * LIGHTMAP_BYTES); i++)
            {
                lms.lightmap_buffer[i] = 0;
            }
        }
        

        /// <summary>
        /// LM_UploadBlock
        /// --------------
        /// Creates a new lightmap texture
        /// </summary>
        public void LM_UploadBlock(bool dynamic)
        {
            int height = 0;

            //if (dynamic == true)
            //    texture = 0;
            //else
            //    texture = lms.current_lightmap_texture;

            //GL_Bind(gl_state.lightmap_textures + texture);
            //qglTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
            //qglTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

            if (dynamic == true)
            {
                for (int i = 0; i < BLOCK_WIDTH; i++)
                {
                    if (lms.allocated[i] > height)
                        height = lms.allocated[i];
                }

                /*qglTexSubImage2D(GL_TEXTURE_2D,
                          0,
                          0, 0,
                          BLOCK_WIDTH, height,
                          GL_LIGHTMAP_FORMAT,
                          GL_UNSIGNED_BYTE,
                          lms.lightmap_buffer);*/
            }
            else
            {
                CProgram.gQ2Game.gCMain.gCImage.CreateLightmap(lms.lightmap_buffer);

                /*qglTexImage2D(GL_TEXTURE_2D,
                       0,
                       lms.internal_format,
                       BLOCK_WIDTH, BLOCK_HEIGHT,
                       0,
                       GL_LIGHTMAP_FORMAT,
                       GL_UNSIGNED_BYTE,
                       lms.lightmap_buffer);*/

                if (CProgram.gQ2Game.gCMain.gCImage.current_lightmap_texture == MAX_LIGHTMAPS)
                    CMain.Error(CMain.EErrorParm.ERR_WARNING, "LM_UploadBlock() - MAX_LIGHTMAPS exceeded\n");
            }
        }

        /// <summary>
        /// LM_AllocBlock
        /// -------------
        /// returns a texture number and the position inside it
        /// </summary>
        public bool LM_AllocBlock(int w, int h, ref int x, ref int y)
        {
            int best;
            int best2;

            best = BLOCK_HEIGHT;

            for (int i = 0; i < BLOCK_WIDTH - w; i++)
            {
                int j;
                best2 = 0;

                for (j = 0; j < w; j++)
                {
                    if (lms.allocated[i + j] >= best)
                        break;

                    if (lms.allocated[i + j] > best2)
                        best2 = lms.allocated[i + j];
                }

                if (j == w)
                {
                    // this is a valid spot
                    x = i;
                    y = best = best2;
                }
            }

            if (best + h > BLOCK_HEIGHT)
                return false;

            for (int i = 0; i < w; i++)
            {
                lms.allocated[x + i] = best + h;
            }

            return true;
        }

        /// <summary>
        /// CreateSurfaceLightmap
        /// ---------------------
        /// Creates a lightmap surface
        /// </summary>
        public void CreateSurfaceLightmap(CModel.SModel _SModel, ref CModel.SMSurface surf)
        {
            int smax;
            int tmax;
            int bytepos;

            if (
                (surf.flags & CModel.EMSurface.SURF_DRAWSKY) == CModel.EMSurface.SURF_DRAWSKY
                | (surf.flags & CModel.EMSurface.SURF_DRAWTURB) == CModel.EMSurface.SURF_DRAWTURB
                )
            {
                return;
            }

            smax = (surf.extents[0] >> 4) + 1;
            tmax = (surf.extents[1] >> 4) + 1;

            if (LM_AllocBlock(smax, tmax, ref surf.light_s, ref surf.light_t) == false)
            {
                LM_UploadBlock(false);
                LM_InitBlock();

                if (LM_AllocBlock(smax, tmax, ref surf.light_s, ref surf.light_t) == false)
                    CMain.Error(CMain.EErrorParm.ERR_FATAL, "Consecutive calls to LM_AllocBlock(" + smax + "," + tmax + ") failed\n");
            }

            surf.lightmaptexturenum = CProgram.gQ2Game.gCMain.gCImage.current_lightmap_texture;

            bytepos = (surf.light_t * BLOCK_WIDTH + surf.light_s) * LIGHTMAP_BYTES;

            CProgram.gQ2Game.gCMain.gCLight.SetCacheState(ref surf);
            CProgram.gQ2Game.gCMain.gCLight.BuildLightMap(_SModel, ref surf, bytepos, ref lms.lightmap_buffer, BLOCK_WIDTH * LIGHTMAP_BYTES);
        }

        /// <summary>
        /// EndBuildingLightmaps
        /// --------------------
        /// Finalizes the processed lightmaps
        /// </summary>
        public void EndBuildingLightmaps(ref CModel.SModel _SModel)
        {
            LM_UploadBlock(false);
            CProgram.gQ2Game.gCMain.gCImage.FinalizeLightmaps(ref _SModel);
        }

        public void BuildPolygonFromSurface(ref CModel.SMSurface surf)
        {
            // most parts of the BuildWorld() function in CMain could be done here
            // if so, the polygons needs to be subdivided first most likely
            // see the function calling this function
        }

        public struct SLightmapState
        {
            //public int current_lightmap_texture;
            public int[] lightmap_surfaces; // size: MAX_LIGHTMAPS

            public int[] allocated; // size: BLOCK_WIDTH

            // the lightmap texture data needs to be kept in
            // main memory so texsubimage can update properly
            public byte[] lightmap_buffer; //size: (BLOCK_WIDTH * BLOCK_HEIGHT * LIGHTMAP_BYTES)
        }

    }
}
