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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace Q2BSP
{
    public class CQ2BSP
    {
        public static CModel.SModel SWorldData;

        public int floodvalid;
        public bool[] portalopen;

        public bool worldMapLoaded;

        public CQ2BSP()
        {
            worldMapLoaded = false;
        }

        private void Mod_LoadVertexes(SDHeader header, ref CModel.SModel _SModel, ref System.IO.BinaryReader br)
        {
            List<CModel.SMVertex> MVertex = new List<CModel.SMVertex>();

            br.BaseStream.Seek(header.lumps[LUMP_VERTEXES].fileofs, System.IO.SeekOrigin.Begin);
            while (br.BaseStream.Position < (header.lumps[LUMP_VERTEXES].fileofs + header.lumps[LUMP_VERTEXES].filelen))
            {
                CModel.SMVertex _MVertex;

                _MVertex.Position.X = br.ReadSingle();
                _MVertex.Position.Y = br.ReadSingle();
                _MVertex.Position.Z = br.ReadSingle();

                MVertex.Add(_MVertex);
            }

            _SModel.numvertexes = MVertex.Count;
            _SModel.vertexes = MVertex.ToArray();
        }

        private void Mod_LoadEdges(SDHeader header, ref CModel.SModel _SModel, ref System.IO.BinaryReader br)
        {
            List<CModel.SMEdge> MEdge = new List<CModel.SMEdge>();

            br.BaseStream.Seek(header.lumps[LUMP_EDGES].fileofs, System.IO.SeekOrigin.Begin);
            while (br.BaseStream.Position < (header.lumps[LUMP_EDGES].fileofs + header.lumps[LUMP_EDGES].filelen))
            {
                CModel.SMEdge _MEdge;

                _MEdge.v = new ushort[2];
                _MEdge.v[0] = br.ReadUInt16();
                _MEdge.v[1] = br.ReadUInt16();

                _MEdge.cachededgeoffset = 0;

                MEdge.Add(_MEdge);
            }

            _SModel.numedges = MEdge.Count;
            _SModel.edges = MEdge.ToArray();
        }

        private void Mod_LoadSurfedges(SDHeader header, ref CModel.SModel _SModel, ref System.IO.BinaryReader br)
        {
            List<int> DSurfEdge = new List<int>();

            br.BaseStream.Seek(header.lumps[LUMP_SURFEDGES].fileofs, System.IO.SeekOrigin.Begin);
            while (br.BaseStream.Position < (header.lumps[LUMP_SURFEDGES].fileofs + header.lumps[LUMP_SURFEDGES].filelen))
            {
                DSurfEdge.Add(br.ReadInt32());
            }

            _SModel.numsurfedges = DSurfEdge.Count;
            _SModel.surfedges = DSurfEdge.ToArray();
        }

        private void Mod_LoadPlanes(SDHeader header, ref CModel.SModel _SModel, ref System.IO.BinaryReader br)
        {
            int bits;
            float[] normal = new float[3];
            List<CShared.SCPlane> CPlane = new List<CShared.SCPlane>();

            br.BaseStream.Seek(header.lumps[LUMP_PLANES].fileofs, System.IO.SeekOrigin.Begin);
            while (br.BaseStream.Position < (header.lumps[LUMP_PLANES].fileofs + header.lumps[LUMP_PLANES].filelen))
            {
                CShared.SCPlane _CPlane;

                normal[0] = 0.0f;
                normal[1] = 0.0f;
                normal[2] = 0.0f;

                bits = 0;

                for (int j = 0; j < 3; j++)
                {
                    normal[j] = br.ReadSingle();

                    if (normal[j] < 0)
                        bits |= 1 << j;
                }

                _CPlane.normal.X = normal[0];
                _CPlane.normal.Y = normal[1];
                _CPlane.normal.Z = normal[2];

                _CPlane.dist = br.ReadSingle();
                _CPlane.type = (byte)br.ReadInt32();
                _CPlane.signbits = (byte)bits;

                _CPlane.pad = new byte[2];

                CPlane.Add(_CPlane);
            }

            _SModel.planes = CPlane.ToArray();
            _SModel.numplanes = _SModel.planes.Length;
        }

        private void Mod_LoadTexinfo(SDHeader header, ref CModel.SModel _SModel, ref System.IO.BinaryReader br)
        {
            List<CModel.SMTexInfo> MTexInfo = new List<CModel.SMTexInfo>();

            br.BaseStream.Seek(header.lumps[LUMP_TEXINFO].fileofs, System.IO.SeekOrigin.Begin);
            while (br.BaseStream.Position < (header.lumps[LUMP_TEXINFO].fileofs + header.lumps[LUMP_TEXINFO].filelen))
            {
                CModel.SMTexInfo _MTexInfo;
                string texture;
                int next;

                _MTexInfo.vecs = new Microsoft.Xna.Framework.Vector4[2];
                for (int i = 0; i < 2; i++)
                {
                    _MTexInfo.vecs[i].X = br.ReadSingle();
                    _MTexInfo.vecs[i].Y = br.ReadSingle();
                    _MTexInfo.vecs[i].Z = br.ReadSingle();
                    _MTexInfo.vecs[i].W = br.ReadSingle();
                }

                _MTexInfo.flags = (ESurface)br.ReadInt32();
                br.ReadInt32(); // value
                texture = CShared.Com_ToString(br.ReadChars(32));
                texture = "textures/" + texture;

                next = br.ReadInt32();
                if (next > 0)
                    _MTexInfo.next = next;
                else
                    _MTexInfo.next = 0;

                _MTexInfo.image = CProgram.gQ2Game.gCMain.gCImage.FindImage(texture, out _MTexInfo.Width, out _MTexInfo.Height, CImage.EImageType.IT_WALL);

                // TODO
                //out->image = GL_FindImage (name, it_wall);
		        //if (!out->image)
		        //{
			    //    ri.Con_Printf (PRINT_ALL, "Couldn't load %s\n", name);
			    //    out->image = r_notexture;
		        //}

                _MTexInfo.numframes = 0;

                MTexInfo.Add(_MTexInfo);
            }

            // count animation frames
            for (int i = 0; i < MTexInfo.Count; i++)
            {
                CModel.SMTexInfo _MTexInfo = MTexInfo[i];
                _MTexInfo.numframes = 1;

                for (int step = _MTexInfo.next; step != 0 && step != i; step = MTexInfo[step].next)
                {
                    _MTexInfo.numframes++;
                }

                MTexInfo[i] = _MTexInfo;
            }

            _SModel.numtexinfo = MTexInfo.Count;
            _SModel.texinfo = MTexInfo.ToArray();
        }

        private void Mod_LoadFaces(SDHeader header, ref CModel.SModel _SModel, ref System.IO.BinaryReader br)
        {
            List<SDFace> DFace = new List<SDFace>();
            List<CModel.SMSurface> MSurface = new List<CModel.SMSurface>();

            br.BaseStream.Seek(header.lumps[LUMP_FACES].fileofs, System.IO.SeekOrigin.Begin);
            while (br.BaseStream.Position < (header.lumps[LUMP_FACES].fileofs + header.lumps[LUMP_FACES].filelen))
            {
                SDFace _DFace;

                _DFace.planenum = br.ReadUInt16();
                _DFace.side = br.ReadInt16();

                _DFace.firstedge = br.ReadInt32();
                _DFace.numedges = br.ReadInt16();
                _DFace.texinfo = br.ReadInt16();

                _DFace.styles = br.ReadBytes(MAXLIGHTMAPS);
                _DFace.lightofs = br.ReadInt32();

                DFace.Add(_DFace);
            }

            CProgram.gQ2Game.gCMain.gCSurface.BeginBuildingLightmaps(ref _SModel);

            for (int i = 0; i < DFace.Count; i++)
            {
                CModel.SMSurface _MSurface;
                int side;
                short ti;

                _MSurface.firstedge = DFace[i].firstedge;
                _MSurface.numedges = DFace[i].numedges;
                _MSurface.flags = 0;
                _MSurface.polys = null;
                _MSurface.ibSurface = null;
                _MSurface.ibData = null;


                side = DFace[i].side;
                if (side != 0)
                    _MSurface.flags |= CModel.EMSurface.SURF_PLANEBACK;

                _MSurface.plane = DFace[i].planenum;

                _MSurface.plane2 = new Microsoft.Xna.Framework.Plane();

                ti = DFace[i].texinfo;
                if (ti < 0 || ti >= _SModel.numtexinfo)
                    CMain.Error(CMain.EErrorParm.ERR_WARNING, "MOD_LoadBmodel: bad texinfo number");
                _MSurface.texinfo = ti;

                _MSurface.bounds = CLocal.ClearBounds();

                _MSurface.boundsMid.X = 0.0f;
                _MSurface.boundsMid.Y = 0.0f;
                _MSurface.boundsMid.Z = 0.0f;

                _MSurface.lightIndex = null;
                _MSurface.lightLength = null;

                _MSurface.texturechain = 0;
                _MSurface.visframe = 0;

                // lighting info
                _MSurface.dlightframe = 0;
                _MSurface.dlightbits = 0;
                _MSurface.lightmaptexturenum = 0;
                _MSurface.styles = new byte[MAXLIGHTMAPS];
                for (int j = 0; j < MAXLIGHTMAPS; j++)
                {
                    _MSurface.styles[j] = DFace[i].styles[j];
                }

                _MSurface.cached_light = null;
                _MSurface.samples = DFace[i].lightofs;

                _MSurface.light_s = 0;
                _MSurface.light_t = 0;
                _MSurface.dlight_s = 0;
                _MSurface.dlight_t = 0;

                _MSurface.extents = null;
                _MSurface.texturemins = null;
                CModel.CalcSurfaceExtents(ref _SModel, ref _MSurface);

                // set the drawing flags
                if ((_SModel.texinfo[_MSurface.texinfo].flags & ESurface.SURF_WARP) == ESurface.SURF_WARP)
                {
                    _MSurface.flags |= CModel.EMSurface.SURF_DRAWTURB;

                    for (int j = 0; j < 2; j++)
                    {
                        _MSurface.extents[j] = 16384;
                        _MSurface.texturemins[j] = -8192;
                    }

                    // TODO
                    // cut up polygon for warps
                    //SubdivideSurface(i, ref _WorldModel.mfaces[i]);
                    //SubdivideSurface (out);
                }

                // create lightmaps and polygons
                if (
                    (_SModel.texinfo[_MSurface.texinfo].flags & ESurface.SURF_SKY) != ESurface.SURF_SKY
                    && (_SModel.texinfo[_MSurface.texinfo].flags & ESurface.SURF_TRANS33) != ESurface.SURF_TRANS33
                    && (_SModel.texinfo[_MSurface.texinfo].flags & ESurface.SURF_TRANS66) != ESurface.SURF_TRANS66
                    && (_SModel.texinfo[_MSurface.texinfo].flags & ESurface.SURF_WARP) != ESurface.SURF_WARP
                    )
                {
                    CProgram.gQ2Game.gCMain.gCSurface.CreateSurfaceLightmap(_SModel, ref _MSurface);
                }

                if ((_SModel.texinfo[_MSurface.texinfo].flags & ESurface.SURF_WARP) == ESurface.SURF_WARP)
                    CProgram.gQ2Game.gCMain.gCSurface.BuildPolygonFromSurface(ref _MSurface);
                
                MSurface.Add(_MSurface);
            }

            CProgram.gQ2Game.gCMain.gCSurface.EndBuildingLightmaps(ref _SModel);

            _SModel.numsurfaces = MSurface.Count;
            _SModel.surfaces = MSurface.ToArray();
        }

        private void Mod_LoadMarksurfaces(SDHeader header, ref CModel.SModel _SModel, ref System.IO.BinaryReader br)
        {
            List<int> MarkSurface = new List<int>();

            br.BaseStream.Seek(header.lumps[LUMP_LEAFFACES].fileofs, System.IO.SeekOrigin.Begin);
            while (br.BaseStream.Position < (header.lumps[LUMP_LEAFFACES].fileofs + header.lumps[LUMP_LEAFFACES].filelen))
            {
                int msurf = br.ReadInt16();

                if (msurf < 0 || msurf >= _SModel.numsurfaces)
                    CMain.Error(CMain.EErrorParm.ERR_FATAL, "Mod_ParseMarksurfaces: bad surface number");

                MarkSurface.Add(msurf);
            }

            _SModel.nummarksurfaces = MarkSurface.Count;
            _SModel.marksurfaces = MarkSurface.ToArray();
        }

        private void Mod_LoadLighting(SDHeader header, ref CModel.SModel _SModel, ref System.IO.BinaryReader br)
        {
            if (header.lumps[LUMP_LIGHTING].filelen == 0)
            {
                _SModel.lightdata = null;
                return;
            }

            br.BaseStream.Seek(header.lumps[LUMP_LIGHTING].fileofs, System.IO.SeekOrigin.Begin);
            _SModel.lightdata = br.ReadBytes(header.lumps[LUMP_LIGHTING].filelen);
        }

        private void Mod_LoadVisibility(SDHeader header, ref CModel.SModel _SModel, ref System.IO.BinaryReader br)
        {
            int pos_visdata = 0;
            br.BaseStream.Seek(header.lumps[LUMP_VISIBILITY].fileofs, System.IO.SeekOrigin.Begin);

            if (header.lumps[LUMP_VISIBILITY].filelen == 0)
            {
                _SModel.vis.numclusters = 0;
                _SModel.vis.bitofs = null;
                _SModel.visdata = null;

                return;
            }

            _SModel.vis.numclusters = br.ReadInt32();
            _SModel.vis.bitofs = new int[_SModel.vis.numclusters, 2];

            for (int i = 0; i < _SModel.vis.numclusters; i++)
            {
                _SModel.vis.bitofs[i, DVIS_PVS] = br.ReadInt32();
                _SModel.vis.bitofs[i, DVIS_PHS] = br.ReadInt32();

                // decrement the offsets, because the data is stored in a seperate byte array
                _SModel.vis.bitofs[i, DVIS_PVS] -= 4 + ((4 * _SModel.vis.numclusters * 2));
                _SModel.vis.bitofs[i, DVIS_PHS] -= 4 + ((4 * _SModel.vis.numclusters * 2));
            }

            _SModel.visdata = new byte[(header.lumps[LUMP_VISIBILITY].fileofs + header.lumps[LUMP_VISIBILITY].filelen) - br.BaseStream.Position];
            while (br.BaseStream.Position < (header.lumps[LUMP_VISIBILITY].fileofs + header.lumps[LUMP_VISIBILITY].filelen))
            {
                _SModel.visdata[pos_visdata++] = br.ReadByte();
            }
        }

        private void Mod_SetParent(ref CModel.SModel _SModel, int idx_node, int idx_parent)
        {
            _SModel.nodes[idx_node].parent = idx_parent;

            if (_SModel.nodes[idx_node].contents != -1)
                return;

            Mod_SetParent(ref _SModel, _SModel.nodes[idx_node].children[0], idx_node);
            Mod_SetParent(ref _SModel, _SModel.nodes[idx_node].children[1], idx_node);
        }

        private void Mod_LoadNodesAndLeafs(SDHeader header, ref CModel.SModel _SModel, ref System.IO.BinaryReader br)
        {
            List<SDNode> DNode = new List<SDNode>();
            List<SDLeaf> DLeaf = new List<SDLeaf>();
            List<CModel.SMNode> MNode = new List<CModel.SMNode>();


            // load nodes
            br.BaseStream.Seek(header.lumps[LUMP_NODES].fileofs, System.IO.SeekOrigin.Begin);
            while (br.BaseStream.Position < (header.lumps[LUMP_NODES].fileofs + header.lumps[LUMP_NODES].filelen))
            {
                SDNode _DNode;

                _DNode.planenum = br.ReadInt32();

                _DNode.children = new int[2];
                _DNode.children[0] = br.ReadInt32();
                _DNode.children[1] = br.ReadInt32();

                _DNode.mins = new short[3];
                _DNode.mins[0] = br.ReadInt16();
                _DNode.mins[1] = br.ReadInt16();
                _DNode.mins[2] = br.ReadInt16();

                _DNode.maxs = new short[3];
                _DNode.maxs[0] = br.ReadInt16();
                _DNode.maxs[1] = br.ReadInt16();
                _DNode.maxs[2] = br.ReadInt16();

                _DNode.firstface = br.ReadUInt16();
                _DNode.numfaces = br.ReadUInt16();

                DNode.Add(_DNode);
            }

            for (int i = 0; i < DNode.Count; i++)
            {
                CModel.SMNode _MNode;
                int p;

                _MNode.bounds.Min.X = DNode[i].mins[0];
                _MNode.bounds.Min.Y = DNode[i].mins[1];
                _MNode.bounds.Min.Z = DNode[i].mins[2];
                _MNode.bounds.Max.X = DNode[i].maxs[0];
                _MNode.bounds.Max.Y = DNode[i].maxs[1];
                _MNode.bounds.Max.Z = DNode[i].maxs[2];

                _MNode.plane = DNode[i].planenum;

                _MNode.firstsurface = DNode[i].firstface;
                _MNode.numsurfaces = DNode[i].numfaces;
                _MNode.contents = -1; // differentiate from leafs

                _MNode.children = new int[2];
                for (int j = 0; j < 2; j++)
                {
                    p = DNode[i].children[j];

                    if (p >= 0)
                        _MNode.children[j] = p;
                    else
                        _MNode.children[j] = DNode.Count + (-1 - p);
                }

                _MNode.parent = 0;
                _MNode.visframe = 0;

                // leaf specific
                _MNode.cluster = 0;
                _MNode.area = 0;
                _MNode.firstmarksurface = 0;
                _MNode.nummarksurfaces = 0;

                MNode.Add(_MNode);
            }


            // load leafs
            br.BaseStream.Seek(header.lumps[LUMP_LEAFS].fileofs, System.IO.SeekOrigin.Begin);
            while (br.BaseStream.Position < (header.lumps[LUMP_LEAFS].fileofs + header.lumps[LUMP_LEAFS].filelen))
            {
                SDLeaf _DLeaf;

                _DLeaf.contents = br.ReadInt32();

                _DLeaf.cluster = br.ReadInt16();
                _DLeaf.area = br.ReadInt16();

                _DLeaf.mins = new short[3];
                _DLeaf.mins[0] = br.ReadInt16();
                _DLeaf.mins[1] = br.ReadInt16();
                _DLeaf.mins[2] = br.ReadInt16();

                _DLeaf.maxs = new short[3];
                _DLeaf.maxs[0] = br.ReadInt16();
                _DLeaf.maxs[1] = br.ReadInt16();
                _DLeaf.maxs[2] = br.ReadInt16();

                _DLeaf.firstleafface = br.ReadUInt16();
                _DLeaf.numleaffaces = br.ReadUInt16();

                _DLeaf.firstleafbrush = br.ReadUInt16();
                _DLeaf.numleafbrushes = br.ReadUInt16();

                DLeaf.Add(_DLeaf);
            }

            for (int i = 0; i < DLeaf.Count; i++)
            {
                CModel.SMNode _MNode;

                _MNode.bounds.Min.X = DLeaf[i].mins[0];
                _MNode.bounds.Min.Y = DLeaf[i].mins[1];
                _MNode.bounds.Min.Z = DLeaf[i].mins[2];
                _MNode.bounds.Max.X = DLeaf[i].maxs[0];
                _MNode.bounds.Max.Y = DLeaf[i].maxs[1];
                _MNode.bounds.Max.Z = DLeaf[i].maxs[2];

                _MNode.contents = DLeaf[i].contents;

                _MNode.cluster = DLeaf[i].cluster;
                _MNode.area = DLeaf[i].area;

                _MNode.firstmarksurface = DLeaf[i].firstleafface;
                _MNode.nummarksurfaces = DLeaf[i].numleaffaces;

                _MNode.parent = 0;
                _MNode.visframe = 0;

                // node specific
                _MNode.plane = 0;
                _MNode.firstsurface = 0;
                _MNode.numsurfaces = 0;
                _MNode.children = null;

                MNode.Add(_MNode);
            }

            _SModel.numnodes = DNode.Count + DLeaf.Count;
            _SModel.numDecisionNodes = DNode.Count;
            _SModel.numleafs = DLeaf.Count;
            _SModel.nodes = MNode.ToArray();

            // chain decendants
            Mod_SetParent(ref _SModel, 0, -1);
        }

        private float RadiusFromBounds(Microsoft.Xna.Framework.BoundingBox bounds)
        {
            Microsoft.Xna.Framework.Vector3 corner;

            corner.X = Math.Abs(bounds.Min.X) > Math.Abs(bounds.Max.X) ? Math.Abs(bounds.Min.X) : Math.Abs(bounds.Max.X);
            corner.Y = Math.Abs(bounds.Min.Y) > Math.Abs(bounds.Max.Y) ? Math.Abs(bounds.Min.Y) : Math.Abs(bounds.Max.Y);
            corner.Z = Math.Abs(bounds.Min.Z) > Math.Abs(bounds.Max.Z) ? Math.Abs(bounds.Min.Z) : Math.Abs(bounds.Max.Z);

            return corner.Length();
        }

        private void Mod_LoadSubmodels(SDHeader header, ref CModel.SModel _SModel, ref System.IO.BinaryReader br)
        {
            List<SDModel> DModel = new List<SDModel>();
            List<CModel.SMModel> MModel = new List<CModel.SMModel>();

            br.BaseStream.Seek(header.lumps[LUMP_MODELS].fileofs, System.IO.SeekOrigin.Begin);
            while (br.BaseStream.Position < (header.lumps[LUMP_MODELS].fileofs + header.lumps[LUMP_MODELS].filelen))
            {
                SDModel _DModel;

                _DModel.mins = new float[3];
                _DModel.mins[0] = br.ReadSingle();
                _DModel.mins[1] = br.ReadSingle();
                _DModel.mins[2] = br.ReadSingle();

                _DModel.maxs = new float[3];
                _DModel.maxs[0] = br.ReadSingle();
                _DModel.maxs[1] = br.ReadSingle();
                _DModel.maxs[2] = br.ReadSingle();

                _DModel.origin = new float[3];
                _DModel.origin[0] = br.ReadSingle();
                _DModel.origin[1] = br.ReadSingle();
                _DModel.origin[2] = br.ReadSingle();

                _DModel.headnode = br.ReadInt32();

                _DModel.firstface = br.ReadInt32();
                _DModel.numfaces = br.ReadInt32();

                DModel.Add(_DModel);
            }

            for (int i = 0; i < DModel.Count; i++)
            {
                CModel.SMModel _MModel;

                _MModel.bounds.Min.X = DModel[i].mins[0] - 1;
                _MModel.bounds.Min.Y = DModel[i].mins[1] - 1;
                _MModel.bounds.Min.Z = DModel[i].mins[2] - 1;
                _MModel.bounds.Max.X = DModel[i].maxs[0] + 1;
                _MModel.bounds.Max.Y = DModel[i].maxs[1] + 1;
                _MModel.bounds.Max.Z = DModel[i].maxs[2] + 1;

                _MModel.origin.X = DModel[i].origin[0];
                _MModel.origin.Y = DModel[i].origin[1];
                _MModel.origin.Z = DModel[i].origin[2];

                _MModel.radius = RadiusFromBounds(_MModel.bounds);

                _MModel.headnode = DModel[i].headnode;
                _MModel.firstface = DModel[i].firstface;
                _MModel.numfaces = DModel[i].numfaces;

                _MModel.visleafs = 0;

                MModel.Add(_MModel);
            }

            _SModel.numsubmodels = MModel.Count;
            _SModel.submodels = MModel.ToArray();
        }

        private void Mod_LoadAreas(SDHeader header, ref CModel.SModel _SModel, ref System.IO.BinaryReader br)
        {
            List<SDArea> DArea = new List<SDArea>();
            List<CModel.SMArea> MArea = new List<CModel.SMArea>();

            br.BaseStream.Seek(header.lumps[LUMP_AREAS].fileofs, System.IO.SeekOrigin.Begin);
            while (br.BaseStream.Position < (header.lumps[LUMP_AREAS].fileofs + header.lumps[LUMP_AREAS].filelen))
            {
                SDArea _DArea;

                _DArea.numareaportals = br.ReadInt32();
                _DArea.firstareaportal = br.ReadInt32();

                DArea.Add(_DArea);
            }

            for (int i = 0; i < DArea.Count; i++)
            {
                CModel.SMArea _MArea;

                _MArea.numareaportals = DArea[i].numareaportals;
                _MArea.firstareaportal = DArea[i].firstareaportal;
                _MArea.floodvalid = 0;
                _MArea.floodnum = 0;

                MArea.Add(_MArea);
            }

            _SModel.numareas = MArea.Count;
            _SModel.areas = MArea.ToArray();
        }

        private void Mod_LoadAreaPortals(SDHeader header, ref CModel.SModel _SModel, ref System.IO.BinaryReader br)
        {
            List<SDAreaPortal> DAreaPortal = new List<SDAreaPortal>();
            List<CModel.SMAreaPortal> MAreaPortal = new List<CModel.SMAreaPortal>();

            br.BaseStream.Seek(header.lumps[LUMP_AREAPORTALS].fileofs, System.IO.SeekOrigin.Begin);
            while (br.BaseStream.Position < (header.lumps[LUMP_AREAPORTALS].fileofs + header.lumps[LUMP_AREAPORTALS].filelen))
            {
                SDAreaPortal _DAreaPortal;

                _DAreaPortal.portalnum = br.ReadInt32();
                _DAreaPortal.otherarea = br.ReadInt32();

                DAreaPortal.Add(_DAreaPortal);
            }

            for (int i = 0; i < DAreaPortal.Count; i++)
            {
                CModel.SMAreaPortal _MAreaPortal;

                _MAreaPortal.portalnum = DAreaPortal[i].portalnum;
                _MAreaPortal.otherarea = DAreaPortal[i].otherarea;

                MAreaPortal.Add(_MAreaPortal);
            }

            _SModel.numareaportals = MAreaPortal.Count;
            _SModel.areaportals = MAreaPortal.ToArray();
        }

        private void Mod_LoadEntityString(SDHeader header, ref CModel.SModel _SModel, ref System.IO.BinaryReader br)
        {
            if (header.lumps[LUMP_ENTITIES].filelen > MAX_MAP_ENTSTRING)
            {
                // entity string exceeds maximum
                _SModel.map_entitystring = null;
                return;
            }

            br.BaseStream.Seek(header.lumps[LUMP_ENTITIES].fileofs, System.IO.SeekOrigin.Begin);
            if (br.BaseStream.Position < (header.lumps[LUMP_ENTITIES].fileofs + header.lumps[LUMP_ENTITIES].filelen))
            {
                _SModel.map_entitystring = CShared.Com_ToString(br.ReadChars(header.lumps[LUMP_ENTITIES].filelen));
                _SModel.map_entitystring = _SModel.map_entitystring.Replace("\r", "").Replace("\n", "\r\n");
            }
            else
            {
                _SModel.map_entitystring = null;
            }
        }

        public void Mod_LoadBrushModel(ref CModel.SModel _SModel, MemoryStream ms)
        {
            SDHeader header;
            List<SLump> lumps;
            BinaryReader br;

            ms.Seek(0, System.IO.SeekOrigin.Begin);
            br = new BinaryReader(ms);

            if (worldMapLoaded == true)
            {
                CMain.Error(CMain.EErrorParm.ERR_FATAL, "ERROR: attempted to redundantly load world map");
                return;
            }

            worldMapLoaded = true;

            _SModel.ModType = CModel.EModType.MOD_BRUSH;

            header.ident = br.ReadInt32();
            if (header.ident != IDBSPHEADER)
            {
                CMain.Error(CMain.EErrorParm.ERR_WARNING, "RE_LoadWorldMap: " + _SModel.name + " has wrong identity.");
                return;
            }

            header.version = br.ReadInt32();
            if (header.version != BSP_VERSION)
            {
                CMain.Error(CMain.EErrorParm.ERR_FATAL, "RE_LoadWorldMap: " + _SModel.name + " has wrong version number (" + header.version.ToString() + " should be " + BSP_VERSION.ToString() + ")");
                return;
            }

            lumps = new List<SLump>();
            for (int i = 0; i < HEADER_LUMPS; i++)
            {
                SLump _lump;
                _lump.fileofs = br.ReadInt32();
                _lump.filelen = br.ReadInt32();

                lumps.Add(_lump);
            }
            header.lumps = lumps.ToArray();

            CProgram.gQ2Game.gCMain.gCImage.StartWAL(ref _SModel);

            // load into heap
            Mod_LoadVertexes(header, ref _SModel, ref br);
            Mod_LoadEdges(header, ref _SModel, ref br);
            Mod_LoadSurfedges(header, ref _SModel, ref br);
            Mod_LoadLighting(header, ref _SModel, ref br);
            Mod_LoadPlanes(header, ref _SModel, ref br);
            Mod_LoadTexinfo(header, ref _SModel, ref br);
            Mod_LoadFaces(header, ref _SModel, ref br);
            Mod_LoadMarksurfaces(header, ref _SModel, ref br);
            Mod_LoadVisibility(header, ref _SModel, ref br);
            Mod_LoadNodesAndLeafs(header, ref _SModel, ref br);
            Mod_LoadSubmodels(header, ref _SModel, ref br);
            Mod_LoadAreas(header, ref _SModel, ref br);
            Mod_LoadAreaPortals(header, ref _SModel, ref br);
            Mod_LoadEntityString(header, ref _SModel, ref br);
            _SModel.numframes = 2;


            // set up the submodels
            for (int i = 0; i < _SModel.numsubmodels; i++)
            {
                CModel.SMModel bm;
                CModel.SModel starmod;

                bm = _SModel.submodels[i];
                starmod = CProgram.gQ2Game.gCMain.gCModel.mod_inline[i];

                starmod.ModType = CModel.EModType.MOD_BRUSH;
                starmod.firstmodelsurface = bm.firstface;
                starmod.nummodelsurfaces = bm.numfaces;
                starmod.firstnode = bm.headnode;

                if (starmod.firstnode >= _SModel.numnodes)
                    System.Diagnostics.Debug.WriteLine("Inline model " + i.ToString() + " has bad firstnode");

                starmod.maxs = bm.bounds.Max;
                starmod.mins = bm.bounds.Min;
                starmod.radius = bm.radius;
                starmod.numleafs = bm.visleafs;

                CProgram.gQ2Game.gCMain.gCModel.mod_inline[i] = starmod;
            }

            CProgram.gQ2Game.gCMain.gCImage.FinalizeWAL(ref _SModel);

            portalopen = new bool[MAX_MAP_AREAPORTALS];
            FloodAreaConnections(ref _SModel);
        }

        public void R_DrawBrushModel(CModel.SEntities _CurrentEntity)
        {
            Matrix wMatrix;
            //EffectParameter EP;
            Vector3 mins;
            Vector3 maxs;
            bool rotated;

            // update HLSL variables
            CProgram.gQ2Game.gCMain.UpdateHLSL(-1);

            // sort textures and surfaces
            if (CProgram.gQ2Game.gCMain.MarkSurfaceSetupStatic(ref _CurrentEntity.Model) == false)
                return;

            // set vertex buffer
            CProgram.gQ2Game.gGraphicsDevice.SetVertexBuffer(_CurrentEntity.Model.vbWorldSolid);


            if (_CurrentEntity.Angles.X != 0.0f || _CurrentEntity.Angles.Y != 0.0f || _CurrentEntity.Angles.Z != 0.0f)
            {
                rotated = true;

                mins.X = _CurrentEntity.Origin.X - _CurrentEntity.Model.radius;
                mins.Y = _CurrentEntity.Origin.Y - _CurrentEntity.Model.radius;
                mins.Z = _CurrentEntity.Origin.Z - _CurrentEntity.Model.radius;

                maxs.X = _CurrentEntity.Origin.X + _CurrentEntity.Model.radius;
                maxs.Y = _CurrentEntity.Origin.Y + _CurrentEntity.Model.radius;
                maxs.Z = _CurrentEntity.Origin.Z + _CurrentEntity.Model.radius;
            }
            else
            {
                rotated = false;

                mins.X = _CurrentEntity.Origin.X + _CurrentEntity.Model.mins.X;
                mins.Y = _CurrentEntity.Origin.Y + _CurrentEntity.Model.mins.Y;
                mins.Z = _CurrentEntity.Origin.Z + _CurrentEntity.Model.mins.Z;

                maxs.X = _CurrentEntity.Origin.X + _CurrentEntity.Model.maxs.X;
                maxs.Y = _CurrentEntity.Origin.Y + _CurrentEntity.Model.maxs.Y;
                maxs.Z = _CurrentEntity.Origin.Z + _CurrentEntity.Model.maxs.Z;
            }

            //if (R_CullBox(mins, maxs))
            //    return;

            CMain.ModelOrigin.X = CClient.cl.RefDef.ViewOrigin.X - _CurrentEntity.Origin.X;
            CMain.ModelOrigin.Y = CClient.cl.RefDef.ViewOrigin.Y - _CurrentEntity.Origin.Y;
            CMain.ModelOrigin.Z = CClient.cl.RefDef.ViewOrigin.Z - _CurrentEntity.Origin.Z;

            if (rotated == true)
            {
                Vector3 temp;
                Vector3 forward, right, up;

                temp.X = CMain.ModelOrigin.X;
                temp.Y = CMain.ModelOrigin.Y;
                temp.Z = CMain.ModelOrigin.Z;

                forward = Vector3.Zero;
                right = Vector3.Zero;
                up = Vector3.Zero;

                CShared.AngleVectors(_CurrentEntity.Angles, ref forward, ref right, ref up);

                CMain.ModelOrigin.X = Vector3.Dot(temp, forward);
                CMain.ModelOrigin.Y = -Vector3.Dot(temp, right);
                CMain.ModelOrigin.Z = Vector3.Dot(temp, up);
            }

            if (rotated == false)
            {
                wMatrix = Matrix.CreateFromYawPitchRoll(_CurrentEntity.Angles.Y, _CurrentEntity.Angles.X, _CurrentEntity.Angles.Z);
                wMatrix *= Matrix.CreateTranslation(_CurrentEntity.Origin);
            }
            else
            {
                wMatrix = Matrix.CreateTranslation(_CurrentEntity.Origin);
            }


            // update HLSL variables
            CProgram.gQ2Game.gEffect.Parameters["xWorld"].SetValue(wMatrix);


            // set a rendering technique
            if ((_CurrentEntity.Model.Flags & CQ2MD2.EModelFlags.RF_TRANSLUCENT) == CQ2MD2.EModelFlags.RF_TRANSLUCENT)
                CProgram.gQ2Game.gEffect.CurrentTechnique = CProgram.gQ2Game.gEffect.Techniques["TexturedTranslucent"];
            else
            {
                if (CProgram.gQ2Game.gCMain.r_hardwarelight == false)
                    CProgram.gQ2Game.gEffect.CurrentTechnique = CProgram.gQ2Game.gEffect.Techniques["TexturedLightmap"];
                else
                    CProgram.gQ2Game.gEffect.CurrentTechnique = CProgram.gQ2Game.gEffect.Techniques["TexturedLight"];
            }


            // jkrige TODO - probably need to update this since most brushmodels can translate/rotate
            // save effect parameter collection shortcut
            //EP = CProgram.gQ2Game.gEffect.Parameters["lights"];

            for (int i = 0; i < _CurrentEntity.Model.nummodelsurfaces; i++)
            {
                int surf = _CurrentEntity.Model.firstmodelsurface + i;

                // update surface HLSL variables
                CProgram.gQ2Game.gCMain.UpdateHLSL(surf);

                // set lights for current surface
                //for (int k = 0; k < CProgram.gQ2Game.gCMain.r_maxLights; k++)
                //{
                //    _CurrentEntity.Model.lights[CQ2BSP.SWorldData.surfaces[surf].lightIndex[k]].SetLight(EP.Elements[k]);
                //}
            }


            for (int i = 0; i < _CurrentEntity.Model.lSChainLightmap.Count; i++)
            {
                // bind new lightmap
                int lightmaptexturenum = _CurrentEntity.Model.lSChainLightmap[i].lightmaptexturenum;
                CProgram.gQ2Game.gEffect.Parameters["xTextureLightmap"].SetValue(CQ2BSP.SWorldData.WorldLightmaps[lightmaptexturenum]);

                for (int j = 0; j < _CurrentEntity.Model.lSChainLightmap[i].TexInfo.Count; j++)
                {
                    // bind new texture
                    int texanim = CProgram.gQ2Game.gCMain.TextureAnimation(_CurrentEntity.Model.lSChainLightmap[i].TexInfo[j].texinfo);
                    int texinfo = _CurrentEntity.Model.lSChainLightmap[i].TexInfo[j].texinfo;
                    CProgram.gQ2Game.gEffect.Parameters["xTextureDiffuse"].SetValue(CQ2BSP.SWorldData.WorldTextures[CQ2BSP.SWorldData.texinfo[texanim].image].Tex2D);

                    // set the indices
                    CProgram.gQ2Game.gGraphicsDevice.Indices = _CurrentEntity.Model.ibWorldSolid[lightmaptexturenum, texinfo].ibBuffer;

                    foreach (EffectPass pass in CProgram.gQ2Game.gEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        CProgram.gQ2Game.gGraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            0,
                            0,
                            _CurrentEntity.Model.vbWorldSolid.VertexCount,
                            0,
                            _CurrentEntity.Model.ibWorldSolid[lightmaptexturenum, texinfo].PrimitiveCount);
                    }
                    CMain.c_brush_polys += _CurrentEntity.Model.ibWorldSolid[lightmaptexturenum, texinfo].PrimitiveCount;
                }
            }
        }


        // ================================================================
        // 
        // AREAPORTALS
        // 
        // ================================================================

        public void FloodArea_r(CModel.SModel _SModel, ref CModel.SMArea area, int floodnum)
        {
            if (area.floodvalid == floodvalid)
            {
                if (area.floodnum == floodnum)
                    return;

                System.Diagnostics.Debug.WriteLine("FloodArea_r: reflooded");
                return;
            }

            area.floodnum = floodnum;
            area.floodvalid = floodvalid;

            for (int i = 0; i < area.numareaportals; i++)
            {
                if (portalopen[_SModel.areaportals[area.firstareaportal + i].portalnum] == true)
                    FloodArea_r(_SModel, ref _SModel.areas[_SModel.areaportals[area.firstareaportal + i].otherarea], floodnum);
            }
        }

        public void FloodAreaConnections(ref CModel.SModel _SModel)
        {
            int floodnum;

            // all current floods are now invalid
            floodvalid++;
            floodnum = 0;

            // area 0 is not used
            for (int i = 1; i < _SModel.numareas; i++)
            {
                if (_SModel.areas[i].floodvalid == floodvalid)
                    continue; // already flooded into

                floodnum++;
                FloodArea_r(_SModel, ref _SModel.areas[i], floodnum);
            }
        }




        // surface flags
        [Flags]
        public enum ESurface : int
        {
            SURF_LIGHT = 0x1,       // value will hold the light strength
            SURF_SLICK = 0x2,       // effects game physics
            SURF_SKY = 0x4,         // don't draw, but add to sky
            SURF_WARP = 0x8,        // turbulent water warp
            SURF_TRANS33 = 0x10,    // translucency at 33 percent
            SURF_TRANS66 = 0x20,    // translucency at 66 percent
            SURF_FLOWING = 0x40,    // scroll towards angle
            SURF_NODRAW = 0x80,     // don't bother referencing the texture
            SURF_HINT = 0x100,      // make a primary bsp splitter
            SURF_SKIP = 0x200       // completely ignore, allowing non-closed brushes
        }


        // ================================================================
        // 
        // BSP FILE FORMAT
        // 
        // ================================================================
        public const int IDBSPHEADER = (('P' << 24) + ('S' << 16) + ('B' << 8) + 'I'); // little-endian "IBSP"
        private const int BSP_VERSION = 38;

        // upper design bounds
        // leaffaces, leafbrushes, planes, and verts are still bounded by 16 bit short limits
        public const int MAX_MAP_MODELS = 1024;
        public const int MAX_MAP_BRUSHES = 8192;
        public const int MAX_MAP_ENTITIES = 2048;
        public const int MAX_MAP_ENTSTRING = 0x40000;
        public const int MAX_MAP_TEXINFO = 8192;

        public const int MAX_MAP_AREAS = 256;
        public const int MAX_MAP_AREAPORTALS = 1024;
        public const int MAX_MAP_PLANES = 65536;
        public const int MAX_MAP_NODES = 65536;
        public const int MAX_MAP_BRUSHSIDES = 65536;
        public const int MAX_MAP_LEAFS = 65536;
        public const int MAX_MAP_VERTS = 65536;
        public const int MAX_MAP_FACES = 65536;
        public const int MAX_MAP_LEAFFACES = 65536;
        public const int MAX_MAP_LEAFBRUSHES = 65536;
        public const int MAX_MAP_PORTALS = 65536;
        public const int MAX_MAP_EDGES = 128000;
        public const int MAX_MAP_SURFEDGES = 256000;
        public const int MAX_MAP_LIGHTING = 0x200000;
        public const int MAX_MAP_VISIBILITY = 0x100000;

        // key / value pair sizes
        private const int MAX_KEY = 32;
        private const int MAX_VALUE = 1024;

        // ================================================================

        public struct SLump
        {
            public int fileofs;
            public int filelen;
        }

        private const int LUMP_ENTITIES = 0;
        private const int LUMP_PLANES = 1;
        private const int LUMP_VERTEXES = 2;
        private const int LUMP_VISIBILITY = 3;
        private const int LUMP_NODES = 4;
        private const int LUMP_TEXINFO = 5;
        private const int LUMP_FACES = 6;
        private const int LUMP_LIGHTING = 7;
        private const int LUMP_LEAFS = 8;
        private const int LUMP_LEAFFACES = 9;
        private const int LUMP_LEAFBRUSHES = 10;
        private const int LUMP_EDGES = 11;
        private const int LUMP_SURFEDGES = 12;
        private const int LUMP_MODELS = 13;
        private const int LUMP_BRUSHES = 14;
        private const int LUMP_BRUSHSIDES = 15;
        private const int LUMP_POP = 16;
        private const int LUMP_AREAS = 17;
        private const int LUMP_AREAPORTALS = 18;
        private const int HEADER_LUMPS = 19;

        public struct SDHeader
        {
            public int ident;
            public int version;
            public SLump[] lumps; // size: HEADER_LUMPS
        }

        public struct SDModel
        {
            public float[] mins; // size: 3
            public float[] maxs; // size: 3
            public float[] origin; // size: 3 (for sounds or lights)
            public int headnode;

            // submodels just draw faces without walking the bsp tree
            public int firstface;
            public int numfaces;
        }

        public struct SDVertex
        {
            public float[] point; // size: 3
        }

        // 0-2 are axial planes
        public const int PLANE_X = 0;
        public const int PLANE_Y = 1;
        public const int PLANE_Z = 2;
        
        // 3-5 are non-axial planes snapped to the nearest
        public const int PLANE_ANYX = 3;
        public const int PLANE_ANYY = 4;
        public const int PLANE_ANYZ = 5;
        
        // planes (x&~1) and (x&~1)+1 are allways opposites

        public struct SDPlane
        {
            public float[] normal; // size: 3
            public float dist;
            public int type; // PLANE_X - PLANE_ANYZ ?remove? trivial to regenerate
        }

        // contents flags are seperate bits
        // a given brush can contribute multiple content bits
        // multiple brushes can be in a single leaf

        // lower bits are stronger, and will eat weaker brushes completely
        public const int CONTENTS_SOLID = 1;   // an eye is never valid in a solid
        public const int CONTENTS_WINDOW = 2;  // translucent, but not watery
        public const int CONTENTS_AUX = 4;
        public const int CONTENTS_LAVA = 8;
        public const int CONTENTS_SLIME = 16;
        public const int CONTENTS_WATER = 32;
        public const int CONTENTS_MIST = 64;
        public const int LAST_VISIBLE_CONTENTS = 64;

        // remaining contents are non-visible, and don't eat brushes

        private const int CONTENTS_AREAPORTAL = 0x8000;

        private const int CONTENTS_PLAYERCLIP = 0x10000;
        private const int CONTENTS_MONSTERCLIP = 0x20000;
        
        // currents can be added to any other contents, and may be mixed
        private const int CONTENTS_CURRENT_0 = 0x40000;
        private const int CONTENTS_CURRENT_90 = 0x80000;
        private const int CONTENTS_CURRENT_180 = 0x100000;
        private const int CONTENTS_CURRENT_270 = 0x200000;
        private const int CONTENTS_CURRENT_UP = 0x400000;
        private const int CONTENTS_CURRENT_DOWN = 0x800000;

        private const int CONTENTS_ORIGIN = 0x1000000;          // removed before bsping an entity

        private const int CONTENTS_MONSTER = 0x2000000;         // should never be on a brush, only in game
        private const int CONTENTS_DEADMONSTER = 0x4000000;
        private const int CONTENTS_DETAIL = 0x8000000;          // brushes to be added after vis leafs
        private const int CONTENTS_TRANSLUCENT = 0x10000000;    // auto set if any surface has trans
        private const int CONTENTS_LADDER = 0x20000000;


        private const int SURF_LIGHT = 0x1;     // value will hold the light strength

        private const int SURF_SLICK = 0x2;     // effects game physics

        private const int SURF_SKY = 0x4;       // don't draw, but add to sky
        private const int SURF_WARP = 0x8;      // turbulent water warp
        private const int SURF_TRANS33 = 0x10;
        private const int SURF_TRANS66 = 0x20;
        private const int SURF_FLOWING = 0x40;  // scroll towards angle
        private const int SURF_NODRAW = 0x80;   // don't bother referencing the texture

        private const int SURF_HINT = 0x100;    // make a primary bsp splitter
        private const int SURF_SKIP = 0x200;    // completely ignore, allowing non-closed brushes

        public struct SDNode
        {
            public int planenum;
            public int[] children;      // size: 2 (negative numbers are -(leafs+1), not nodes)
            public short[] mins;        // size: 3 (for frustom culling)
            public short[] maxs;        // size: 3 (for frustom culling)
            public ushort firstface;
            public ushort numfaces;     // counting both sides
        }

        public struct STexInfo
        {
            public float[,] vecs;   // size: [2,4] ([s/t][xyz offset])
            public ESurface flags;  // miptex flags + overrides
            public int value;       // light emission, etc
            public string texture;  // size: 32 (texture name (textures/*.wal))
            public int nexttexinfo; // for animations, -1 = end of chain
        }

        // note that edge 0 is never used, because negative edge nums are used for
        // counterclockwise use of the edge in a face
        public struct SDEdge
        {
            public ushort[] v; // size: 2 (vertex numbers)
        }

        public const int MAXLIGHTMAPS = 4;

        public struct SDFace
        {
            public ushort planenum;
            public short side;

            public int firstedge;   // we must support > 64k edges
            public short numedges;
            public short texinfo;

            // lighting info
            public byte[] styles;   // size: MAXLIGHTMAPS
            public int lightofs;    // start of [numstyles*surfsize] samples
        }

        public struct SDLeaf
        {
            public int contents; // OR of all brushes (not needed?)

            public short cluster;
            public short area;

            public short[] mins; // size: 3 (for frustum culling)
            public short[] maxs; // size: 3 (for frustum culling)

            public ushort firstleafface;
            public ushort numleaffaces;

            public ushort firstleafbrush;
            public ushort numleafbrushes;
        }

        public struct SDBrushSide
        {
            public ushort planenum; // facing out of the leaf
            public short texinfo;
        }

        public struct SDBrush
        {
            public int firstside;
            public int numsides;
            public int contents;
        }

        private const int ANGLE_UP = -1;
        private const int ANGLE_DOWN = -2;

        // the visibility lump consists of a header with a count, then
        // byte offsets for the PVS and PHS of each cluster, then the raw
        // compressed bit vectors
        public const int DVIS_PVS = 0;
        public const int DVIS_PHS = 1;

        public struct SDVis
        {
            public int numclusters;
            public int[,] bitofs; // size: [8,2] (bitofs[numclusters][2])
        }

        // each area has a list of portals that lead into other areas
        // when portals are closed, other areas may not be visible or
        // hearable even if the vis info says that it should be
        public struct SDAreaPortal
        {
            public int portalnum;
            public int otherarea;
        }

        public struct SDArea
        {
            public int numareaportals;
            public int firstareaportal;
        }


    }
}
