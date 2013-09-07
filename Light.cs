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
using Microsoft.Xna.Framework;

namespace Q2BSP
{
    public class CLight
    {
        public float[] s_blocklights;

        private Vector3 pointcolor;
        private CShared.SCPlane lightplane; // used as shadow plane
        private Vector3 lightspot;

        public CLight()
        {
            s_blocklights = new float[34 * 34 * 3];
        }

        public void SetCacheState(ref CModel.SMSurface surf)
        {
            surf.cached_light = new float[CQ2BSP.MAXLIGHTMAPS];

            for(int maps = 0; maps < CQ2BSP.MAXLIGHTMAPS && surf.styles[maps] != 255; maps++)
            {
                surf.cached_light[maps] = CClient.cl.RefDef.lightstyles[surf.styles[maps]].white;
            }
        }

        /// <summary>
        /// R_BuildLightMap
        /// ---------------
        /// Combine and scale multiple lightmaps into the floating format in blocklights
        /// </summary>
        public void BuildLightMap(CModel.SModel _SModel, ref CModel.SMSurface surf, int dest_pos, ref byte[] dest, int stride)
        {
            int smax, tmax;
            int r, g, b, a, max;
            int size;
            int lightmap;
            float[] scale;
            int nummaps;
            float[] bl;
            CLocal.SLightStyle style;

            int bl_pos;
            float modulate = 1.0f;

            if (
                (_SModel.texinfo[surf.texinfo].flags & CQ2BSP.ESurface.SURF_SKY) == CQ2BSP.ESurface.SURF_SKY
                | (_SModel.texinfo[surf.texinfo].flags & CQ2BSP.ESurface.SURF_TRANS33) == CQ2BSP.ESurface.SURF_TRANS33
                | (_SModel.texinfo[surf.texinfo].flags & CQ2BSP.ESurface.SURF_TRANS66) == CQ2BSP.ESurface.SURF_TRANS66
                | (_SModel.texinfo[surf.texinfo].flags & CQ2BSP.ESurface.SURF_WARP) == CQ2BSP.ESurface.SURF_WARP
                )
            {
                CMain.Error(CMain.EErrorParm.ERR_WARNING, "BuildLightMap called for non-lit surface");
            }

            smax = (surf.extents[0] >> 4) + 1;
            tmax = (surf.extents[1] >> 4) + 1;
            size = smax * tmax;

            if (size > ((sizeof(float) * s_blocklights.Length) >> 4))
                CMain.Error(CMain.EErrorParm.ERR_WARNING, "Bad s_blocklights size");

            // set to full bright if no light data
            if (surf.samples == -1)
            {
                for (int i = 0; i < size * 3; i++)
                {
                    s_blocklights[i] = 255;
                }

                for (int maps = 0; maps < CQ2BSP.MAXLIGHTMAPS && surf.styles[maps] != 255; maps++)
                {
                    style = CClient.cl.RefDef.lightstyles[surf.styles[maps]];
                }

                goto store;
            }

            // count the # of maps
            for (nummaps = 0; nummaps < CQ2BSP.MAXLIGHTMAPS && surf.styles[nummaps] != 255; nummaps++) ;

            lightmap = surf.samples;

            // add all the lightmaps
            if (nummaps == 1)
            {
                scale = new float[3];

                for (int maps = 0; maps < CQ2BSP.MAXLIGHTMAPS && surf.styles[maps] != 255; maps++)
                {
                    bl = s_blocklights;
                    bl_pos = 0;

                    for (int i = 0; i < 3; i++)
                    {
                        scale[i] = modulate * CClient.cl.RefDef.lightstyles[surf.styles[maps]].rgb[i];
                    }

                    if (scale[0] == 1.0f && scale[1] == 1.0f && scale[2] == 1.0f)
                    {
                        for (int i = 0; i < size; i++, bl_pos += 3)
                        {
                            bl[bl_pos + 0] = _SModel.lightdata[lightmap + (i * 3) + 0];
                            bl[bl_pos + 1] = _SModel.lightdata[lightmap + (i * 3) + 1];
                            bl[bl_pos + 2] = _SModel.lightdata[lightmap + (i * 3) + 2];
                        }
                    }
                    else
                    {
                        for (int i = 0; i < size; i++, bl_pos += 3)
                        {
                            bl[bl_pos + 0] = _SModel.lightdata[lightmap + (i * 3) + 0] * scale[0];
                            bl[bl_pos + 1] = _SModel.lightdata[lightmap + (i * 3) + 1] * scale[1];
                            bl[bl_pos + 2] = _SModel.lightdata[lightmap + (i * 3) + 2] * scale[2];
                        }
                    }

                    // skip to next lightmap
                    lightmap += size * 3;
                }
            }
            else
            {
                scale = new float[3];

                for (int i = 0; i < (sizeof(float) * size * 3); i++)
                {
                    s_blocklights[i] = 0;
                }

                for (int maps = 0; maps < CQ2BSP.MAXLIGHTMAPS && surf.styles[maps] != 255; maps++)
                {
                    bl = s_blocklights;
                    bl_pos = 0;

                    for (int i = 0; i < 3; i++)
                    {
                        scale[i] = modulate * CClient.cl.RefDef.lightstyles[surf.styles[maps]].rgb[i];
                    }

                    if (scale[0] == 1.0f && scale[1] == 1.0f && scale[2] == 1.0f)
                    {
                        for (int i = 0; i < size; i++, bl_pos += 3)
                        {
                            bl[bl_pos + 0] += _SModel.lightdata[lightmap + (i * 3) + 0];
                            bl[bl_pos + 1] += _SModel.lightdata[lightmap + (i * 3) + 1];
                            bl[bl_pos + 2] += _SModel.lightdata[lightmap + (i * 3) + 2];
                        }
                    }
                    else
                    {
                        for (int i = 0; i < size; i++, bl_pos += 3)
                        {
                            bl[bl_pos + 0] += _SModel.lightdata[lightmap + (i * 3) + 0] * scale[0];
                            bl[bl_pos + 1] += _SModel.lightdata[lightmap + (i * 3) + 1] * scale[1];
                            bl[bl_pos + 2] += _SModel.lightdata[lightmap + (i * 3) + 2] * scale[2];
                        }
                    }

                    // skip to next lightmap
                    lightmap += size * 3;
                }
            }

            // add all the dynamic lights
            //if (surf.dlightframe == CMain.r_framecount)
            //    R_AddDynamicLights(surf);

            // put into texture format
            store:
            stride -= (smax << 2);
            bl = s_blocklights;
            bl_pos = 0;

            for (int i = 0; i < tmax; i++, dest_pos += stride)
            {
                for (int j = 0; j < smax; j++)
                {
                    r = Convert.ToInt32(bl[bl_pos + 0]);
                    g = Convert.ToInt32(bl[bl_pos + 1]);
                    b = Convert.ToInt32(bl[bl_pos + 2]);

                    // catch negative lights
                    if (r < 0)
                        r = 0;
                    if (g < 0)
                        g = 0;
                    if (b < 0)
                        b = 0;

                    // determine the brightest of the three color components
                    if (r > g)
                        max = r;
                    else
                        max = g;
                    if (b > max)
                        max = b;

                    // alpha is ONLY used for the mono lightmap case.  For this reason
                    // we set it to the brightest of the color components so that
                    // things don't get too dim.
                    a = max;

                    // rescale all the color components if the intensity of the greatest
                    // channel exceeds 1.0
                    if (max > 255)
                    {
                        float t = 255.0f / max;

                        r = (int)(r * t);
                        g = (int)(g * t);
                        b = (int)(b * t);
                        a = (int)(a * t);
                    }

                    dest[dest_pos + 0] = (byte)r;
                    dest[dest_pos + 1] = (byte)g;
                    dest[dest_pos + 2] = (byte)b;
                    dest[dest_pos + 3] = (byte)a;

                    bl_pos += 3;
                    dest_pos += 4;
                }
            }
        }

        public void LightmapAnimation(int surf)
        {
            int map;
            bool check_dynamic;
            bool is_dynamic;
            CModel.SMSurface surface;

            check_dynamic = false;
            is_dynamic = false;


            surface = CQ2BSP.SWorldData.surfaces[surf];
            for (map = 0; map < CQ2BSP.MAXLIGHTMAPS && surface.styles[map] != 255; map++)
            {
                if (surface.cached_light == null) // jkrige ??
                    break;

                if (CClient.cl.RefDef.lightstyles[surface.styles[map]].white != surface.cached_light[map])
                {
                    check_dynamic = true;
                    break;
                }
            }

            // dynamic this frame or dynamic previously
            if (surface.dlightframe == CMain.r_framecount | check_dynamic == true)
            {
                if (
                    (CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_WARP) != CQ2BSP.ESurface.SURF_WARP
                    && (CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_TRANS33) != CQ2BSP.ESurface.SURF_TRANS33
                    && (CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_TRANS66) != CQ2BSP.ESurface.SURF_TRANS66
                    && (CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_SKY) != CQ2BSP.ESurface.SURF_SKY
                    )
                {
                    is_dynamic = true;
                }
            }

            if (is_dynamic == true)
            {
                //System.Diagnostics.Debug.WriteLine("DYNAMIC!");
            }

            // gl_surf.c
            // void R_RenderBrushPoly (msurface_t *fa)
            // void GL_RenderLightmappedPoly( msurface_t *surf )
        }

        public int RecursiveLightPoint(int idx_node, Vector3 start, Vector3 end)
        {
            float front;
            float back;
            float frac;
            int side;
            CShared.SCPlane plane;
            Vector3 mid;
            int surf;
            int s, t;
            int ds, dt;
            CModel.SMTexInfo tex;
            int lightmap;
            int r;
            CModel.SMNode node;

            node = CQ2BSP.SWorldData.nodes[idx_node];

            if (node.contents != -1)
                return -1; // didn't hit anything

            // calculate mid point
            // FIXME: optimize for axial
            plane = CQ2BSP.SWorldData.planes[node.plane];
            front = Vector3.Dot(start, plane.normal) - plane.dist;
            back = Vector3.Dot(end, plane.normal) - plane.dist;
            side = Convert.ToInt32(front < 0);

            if (Convert.ToInt32(back < 0) == side)
                return RecursiveLightPoint(node.children[side], start, end);

            frac = front / (front - back);
            mid.X = start.X + (end.X - start.X) * frac;
            mid.Y = start.Y + (end.Y - start.Y) * frac;
            mid.Z = start.Z + (end.Z - start.Z) * frac;


            // go down front side
            r = RecursiveLightPoint(node.children[side], start, mid);

            if (r >= 0)
                return r; // hit something

            if (Convert.ToInt32(back < 0) == side)
                return -1; // didn't hit anything


            // check for impact on this node
            lightspot.X = mid.X;
            lightspot.Y = mid.Y;
            lightspot.Z = mid.Z;
            lightplane = plane;

            surf = node.firstsurface;
            for (int i = 0; i < node.numsurfaces; i++, surf++)
            {
                Vector3 vecs;

                if ((CQ2BSP.SWorldData.surfaces[surf].flags & CModel.EMSurface.SURF_DRAWTURB) == CModel.EMSurface.SURF_DRAWTURB)
                    continue; // no lightmaps

                if ((CQ2BSP.SWorldData.surfaces[surf].flags & CModel.EMSurface.SURF_DRAWSKY) == CModel.EMSurface.SURF_DRAWSKY)
                    continue; // no lightmaps


                tex = CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo];

                vecs.X = tex.vecs[0].X;
                vecs.Y = tex.vecs[0].Y;
                vecs.Z = tex.vecs[0].Z;
                s = (int)(Vector3.Dot(mid, vecs) + tex.vecs[0].W);

                vecs.X = tex.vecs[1].X;
                vecs.Y = tex.vecs[1].Y;
                vecs.Z = tex.vecs[1].Z;
                t = (int)(Vector3.Dot(mid, vecs) + tex.vecs[1].W);


                if (s < CQ2BSP.SWorldData.surfaces[surf].texturemins[0] || t < CQ2BSP.SWorldData.surfaces[surf].texturemins[1])
                    continue;

                ds = s - CQ2BSP.SWorldData.surfaces[surf].texturemins[0];
                dt = t - CQ2BSP.SWorldData.surfaces[surf].texturemins[1];

                if (ds > CQ2BSP.SWorldData.surfaces[surf].extents[0] || dt > CQ2BSP.SWorldData.surfaces[surf].extents[1])
                    continue;

                if (CQ2BSP.SWorldData.surfaces[surf].samples == -1)
                    return 0;

                ds >>= 4;
                dt >>= 4;

                lightmap = CQ2BSP.SWorldData.surfaces[surf].samples;
                pointcolor.X = CShared.vec3_origin.X;
                pointcolor.Y = CShared.vec3_origin.Y;
                pointcolor.Z = CShared.vec3_origin.Z;

                if (lightmap != -1)
                {
                    Vector3 scale;

                    lightmap += 3 * (dt * ((CQ2BSP.SWorldData.surfaces[surf].extents[0] >> 4) + 1) + ds);

                    for (int maps = 0; maps < CQ2BSP.MAXLIGHTMAPS && CQ2BSP.SWorldData.surfaces[surf].styles[maps] != 255; maps++)
                    {
                        scale = Vector3.Zero;

                        for (i = 0; i < 3; i++)
                        {
                            // jkrige ??
                            //scale.X = CLocal.gl_modulate * CClient.cl.RefDef.lightstyles[CQ2BSP.SWorldData.surfaces[surf].styles[maps]].rgb[0];
                            //scale.Y = CLocal.gl_modulate * CClient.cl.RefDef.lightstyles[CQ2BSP.SWorldData.surfaces[surf].styles[maps]].rgb[1];
                            //scale.Z = CLocal.gl_modulate * CClient.cl.RefDef.lightstyles[CQ2BSP.SWorldData.surfaces[surf].styles[maps]].rgb[2];
                            scale.X = 1.0f;
                            scale.Y = 1.0f;
                            scale.Z = 1.0f;
                            // jkrige ??
                        }

                        pointcolor.X += CQ2BSP.SWorldData.lightdata[lightmap + 0] * scale.X * (1.0f / 255);
                        pointcolor.Y += CQ2BSP.SWorldData.lightdata[lightmap + 1] * scale.Y * (1.0f / 255);
                        pointcolor.Z += CQ2BSP.SWorldData.lightdata[lightmap + 2] * scale.Z * (1.0f / 255);

                        lightmap += 3 * ((CQ2BSP.SWorldData.surfaces[surf].extents[0] >> 4) + 1) * ((CQ2BSP.SWorldData.surfaces[surf].extents[1] >> 4) + 1);
                    }
                }

                return 1;
            }

            // go down back side
            return RecursiveLightPoint(node.children[Convert.ToInt32(side == 0)], mid, end);
        }

        public void R_LightPoint(Vector3 p, ref Color inColor)
        {
            Vector3 color;
            Vector3 end;
            float r;
            //int lnum;
            //dlight_t* dl;
            //float light;
            //vec3_t dist;
            //float add;


            if (CQ2BSP.SWorldData.lightdata == null | CQ2BSP.SWorldData.lightdata.Length == 0)
            {
                inColor.R = inColor.G = inColor.B = inColor.A = 1;

                return;
            }

            end.X = p.X;
            end.Y = p.Y;
            end.Z = p.Z - 2048;

            r = RecursiveLightPoint(0, p, end);

            if (r == -1)
            {
                color.X = CShared.vec3_origin.X;
                color.Y = CShared.vec3_origin.Y;
                color.Z = CShared.vec3_origin.Z;
            }
            else
            {
                color.X = pointcolor.X;
                color.Y = pointcolor.Y;
                color.Z = pointcolor.Z;
            }


            //
            // add dynamic lights
            //
            /*light = 0;
            dl = r_newrefdef.dlights;
            for (lnum = 0; lnum < r_newrefdef.num_dlights; lnum++, dl++)
            {
                VectorSubtract(currententity->origin,
                                dl->origin,
                                dist);
                add = dl->intensity - VectorLength(dist);
                add *= (1.0 / 256);
                if (add > 0)
                {
                    VectorMA(color, add, dl->color, color);
                }
            }

            VectorScale(color, gl_modulate->value, color);*/


            color.X = color.X * CLocal.gl_modulate;
            color.Y = color.Y * CLocal.gl_modulate;
            color.Z = color.Z * CLocal.gl_modulate;

            inColor.R = (byte)(color.X * 255);
            inColor.G = (byte)(color.Y * 255);
            inColor.B = (byte)(color.Z * 255);
            inColor.A = 255;
        }

    }
}
