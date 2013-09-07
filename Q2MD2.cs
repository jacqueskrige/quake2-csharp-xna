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
    public class CQ2MD2
    {
        private void BuildAliasModelBuffer(ref CModel.SModelMD2 _SModelMD2)
        {
            List<CModel.ModelVertexFormat> vertbuffer;

            if (_SModelMD2.vbModel != null)
                return;

            CProgram.gQ2Game.gCMain.ModelVertexDeclaration = new VertexDeclaration(CModel.ModelVertexFormat.VertexElements);

            _SModelMD2.vbModel = new VertexBuffer[_SModelMD2.md2.num_frames];

            // builds a vertexbuffer for each animation frame
            for (int i = 0; i < _SModelMD2.md2.num_frames; i++)
            {
                vertbuffer = new List<CModel.ModelVertexFormat>();

                for (int j = 0; j < _SModelMD2.md2.num_tris; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        CModel.ModelVertexFormat _vert;

                        _vert.Position.X = ((float)_SModelMD2.aliasframes[i].verts[_SModelMD2.triangles[j].index_xyz[k]].v[0]) * _SModelMD2.aliasframes[i].scale[0];
                        _vert.Position.Y = ((float)_SModelMD2.aliasframes[i].verts[_SModelMD2.triangles[j].index_xyz[k]].v[1]) * _SModelMD2.aliasframes[i].scale[1];
                        _vert.Position.Z = ((float)_SModelMD2.aliasframes[i].verts[_SModelMD2.triangles[j].index_xyz[k]].v[2]) * _SModelMD2.aliasframes[i].scale[2];
                        _vert.Position.X += _SModelMD2.aliasframes[i].translate[0];
                        _vert.Position.Y += _SModelMD2.aliasframes[i].translate[1];
                        _vert.Position.Z += _SModelMD2.aliasframes[i].translate[2];

                        _vert.TextureCoordinate.X = (float)(((double)_SModelMD2.st[_SModelMD2.triangles[j].index_st[k]].s + 0.5) / (double)_SModelMD2.md2.skinwidth);
                        _vert.TextureCoordinate.Y = (float)(((double)_SModelMD2.st[_SModelMD2.triangles[j].index_st[k]].t + 0.5) / (double)_SModelMD2.md2.skinheight);

                        _vert.Normal.X = 0.0f;
                        _vert.Normal.Y = 0.0f;
                        _vert.Normal.Z = 0.0f;

                        vertbuffer.Add(_vert);
                    }
                }

                // set up model frame vertex buffer
                _SModelMD2.vbModel[i] = new VertexBuffer(CProgram.gQ2Game.gGraphicsDevice, CProgram.gQ2Game.gCMain.ModelVertexDeclaration, vertbuffer.Count, BufferUsage.WriteOnly);
                _SModelMD2.vbModel[i].SetData(vertbuffer.ToArray());

                vertbuffer.Clear();
                vertbuffer = null;
            }
        }

        public void Mod_LoadAliasModel(ref CModel.SModel _SModel, MemoryStream ms)
        {
            BinaryReader br;
            List<string> skinnames;
            List<CQ2MD2.SSTVert> st;
            List<CQ2MD2.SDTriangle> triangles;
            List<CQ2MD2.SAliasFrameDesc> aliasframes;

            // HACK - prevent model loading if in Heretic II mode
            if (CProgram.gQ2Game.gCMain.r_htic2 == true)
                return;

            ms.Seek(0, System.IO.SeekOrigin.Begin);
            br = new BinaryReader(ms);


            // identity (header)
            _SModel.ModelMD2.md2.identification = br.ReadBytes(4);
            if (_SModel.ModelMD2.md2.identification[0] != 'I'
                || _SModel.ModelMD2.md2.identification[1] != 'D'
                || _SModel.ModelMD2.md2.identification[2] != 'P'
                || _SModel.ModelMD2.md2.identification[3] != '2')
            {
                System.Diagnostics.Debug.WriteLine("MDL file " + _SModel.name + " doesn't have IDP2 id");

                br.Close();
                return;
            }


            // model version
            _SModel.ModelMD2.md2.version = br.ReadInt32();
            if (_SModel.ModelMD2.md2.version != ALIAS_VERSION)
            {
                System.Diagnostics.Debug.WriteLine(_SModel.name + " has wrong version number (" + _SModel.ModelMD2.md2.version + " should be " + ALIAS_VERSION + ")");
                
                br.Close();
                return;
            }


            // skin size
            _SModel.ModelMD2.md2.skinwidth = br.ReadInt32();
            _SModel.ModelMD2.md2.skinheight = br.ReadInt32();
            if (_SModel.ModelMD2.md2.skinheight > CLocal.MAX_LBM_HEIGHT)
            {
                System.Diagnostics.Debug.WriteLine("model " + _SModel.name + " has a skin taller than " + CLocal.MAX_LBM_HEIGHT + ".");

                br.Close();
                return;
            }


            // frame size
            _SModel.ModelMD2.md2.framesize = br.ReadInt32();


            // number of skins
            _SModel.ModelMD2.md2.num_skins = br.ReadInt32();


            // number of vertices
            _SModel.ModelMD2.md2.num_xyz = br.ReadInt32();
            if (_SModel.ModelMD2.md2.num_xyz > MAX_VERTS)
            {
                System.Diagnostics.Debug.WriteLine("model " + _SModel.name + " has too many vertices");

                br.Close();
                return;
            }


            // number of st vertices
            _SModel.ModelMD2.md2.num_st = br.ReadInt32();
            if (_SModel.ModelMD2.md2.num_st <= 0)
            {
                System.Diagnostics.Debug.WriteLine("model " + _SModel.name + " has no st vertices");

                br.Close();
                return;
            }


            // number of triangles
            _SModel.ModelMD2.md2.num_tris = br.ReadInt32();
            if (_SModel.ModelMD2.md2.num_tris <= 0)
            {
                System.Diagnostics.Debug.WriteLine("model " + _SModel.name + " has no triangles");

                br.Close();
                return;
            }


            // number of gl commands
            _SModel.ModelMD2.md2.num_glcmds = br.ReadInt32();


            // number of frames
            _SModel.ModelMD2.md2.num_frames = br.ReadInt32();
            if (_SModel.ModelMD2.md2.num_frames <= 0)
            {
                System.Diagnostics.Debug.WriteLine("model " + _SModel.name + " has no frames");

                br.Close();
                return;
            }


            // load offsets
            _SModel.ModelMD2.md2.ofs_skins = br.ReadInt32();  // each skin is a MAX_SKINNAME string
            _SModel.ModelMD2.md2.ofs_st = br.ReadInt32();     // byte offset from start for stverts
            _SModel.ModelMD2.md2.ofs_tris = br.ReadInt32();   // offset for dtriangles
            _SModel.ModelMD2.md2.ofs_frames = br.ReadInt32(); // offset for first frame
            _SModel.ModelMD2.md2.ofs_glcmds = br.ReadInt32(); // offset for strip/fan command list
            _SModel.ModelMD2.md2.ofs_end = br.ReadInt32();    // end of file


            //
            // load the skin names
            //
            skinnames = new List<string>();
            for (int i = 0; i < _SModel.ModelMD2.md2.num_skins; i++)
            {
                skinnames.Add(CShared.Com_ToString(br.ReadChars(MAX_SKINNAME)));
            }

            if (skinnames.Count != 0)
            {
                _SModel.ModelMD2.skinnames = skinnames.ToArray();
                skinnames.Clear();
                skinnames = null;
            }

            
            //
            // load base s and t vertices (not used in gl version)
            //
            st = new List<SSTVert>();
            for (int i = 0; i < _SModel.ModelMD2.md2.num_st; i++)
            {
                SSTVert _SSTVert;

                _SSTVert.s = br.ReadInt16();
                _SSTVert.t = br.ReadInt16();

                st.Add(_SSTVert);
            }

            if (st.Count != 0)
            {
                _SModel.ModelMD2.st = st.ToArray();
                st.Clear();
                st = null;
            }


            //
            // load the triangles
            //
            triangles = new List<SDTriangle>();
            for (int i = 0; i < _SModel.ModelMD2.md2.num_tris; i++)
            {
                SDTriangle _SDTriangle;

                _SDTriangle.index_xyz = new short[3];
                _SDTriangle.index_xyz[0] = br.ReadInt16();
                _SDTriangle.index_xyz[1] = br.ReadInt16();
                _SDTriangle.index_xyz[2] = br.ReadInt16();

                _SDTriangle.index_st = new short[3];
                _SDTriangle.index_st[0] = br.ReadInt16();
                _SDTriangle.index_st[1] = br.ReadInt16();
                _SDTriangle.index_st[2] = br.ReadInt16();

                triangles.Add(_SDTriangle);
            }

            if (triangles.Count != 0)
            {
                _SModel.ModelMD2.triangles = triangles.ToArray();
                triangles.Clear();
                triangles = null;
            }


            //
            // load the frames
            //
            aliasframes = new List<SAliasFrameDesc>();
            for (int i = 0; i < _SModel.ModelMD2.md2.num_frames; i++)
            {
                SAliasFrameDesc _SAliasFrameDesc;

                _SAliasFrameDesc.scale = new float[3];
                _SAliasFrameDesc.scale[0] = br.ReadSingle();
                _SAliasFrameDesc.scale[1] = br.ReadSingle();
                _SAliasFrameDesc.scale[2] = br.ReadSingle();

                _SAliasFrameDesc.translate = new float[3];
                _SAliasFrameDesc.translate[0] = br.ReadSingle();
                _SAliasFrameDesc.translate[1] = br.ReadSingle();
                _SAliasFrameDesc.translate[2] = br.ReadSingle();

                _SAliasFrameDesc.name = br.ReadChars(16);

                _SAliasFrameDesc.verts = new STrivertx[_SModel.ModelMD2.md2.num_xyz];
                for (int j = 0; j < _SModel.ModelMD2.md2.num_xyz; j++)
                {
                    _SAliasFrameDesc.verts[j].v = new byte[3];
                    _SAliasFrameDesc.verts[j].v[0] = br.ReadByte();
                    _SAliasFrameDesc.verts[j].v[1] = br.ReadByte();
                    _SAliasFrameDesc.verts[j].v[2] = br.ReadByte();

                    _SAliasFrameDesc.verts[j].lightnormalindex = br.ReadByte();
                }

                aliasframes.Add(_SAliasFrameDesc);
            }

            if (aliasframes.Count != 0)
            {
                _SModel.ModelMD2.aliasframes = aliasframes.ToArray();
                aliasframes.Clear();
                aliasframes = null;
            }


            //
            // load the gl commands
            //
            _SModel.ModelMD2.glcmds = br.ReadBytes(_SModel.ModelMD2.md2.num_glcmds * sizeof(int));



            // set the model type
            _SModel.ModType = CModel.EModType.MOD_ALIAS;


            // register all skins
            _SModel.ModelMD2.skins = new Microsoft.Xna.Framework.Graphics.Texture2D[_SModel.ModelMD2.md2.num_skins];
            for (int i = 0; i < _SModel.ModelMD2.md2.num_skins; i++)
            {
                _SModel.ModelMD2.skins[i] = CProgram.gQ2Game.gCMain.gCImage.LoadSkin(_SModel.ModelMD2.skinnames[i]);
            }

            // set default mins/maxs
            _SModel.mins.X = -32;
            _SModel.mins.Y = -32;
            _SModel.mins.Z = -32;
            _SModel.maxs.X = 32;
            _SModel.maxs.Y = 32;
            _SModel.maxs.Z = 32;
            
            // close the binary reader
            br.Close();
            br = null;

            // close the memory stream
            ms.Close();
            ms = null;

            BuildAliasModelBuffer(ref _SModel.ModelMD2);
        }

        public void R_DrawAliasModel(CModel.SEntities _CurrentEntity)
        {
            Matrix wMatrix;
            Color shadelight;

            if (_CurrentEntity.Model.ModelMD2.vbModel == null)
                return;
            
            // set vertex buffer
            CProgram.gQ2Game.gGraphicsDevice.SetVertexBuffer(_CurrentEntity.Model.ModelMD2.vbModel[_CurrentEntity.ModelFrameSeqStart + _CurrentEntity.ModelFrameCurrent]);

            // create model rotation and translation matrix
            wMatrix = Matrix.CreateFromYawPitchRoll(_CurrentEntity.Angles.Y, _CurrentEntity.Angles.X, _CurrentEntity.Angles.Z);
            wMatrix *= Matrix.CreateTranslation(_CurrentEntity.Origin);

            // calculate the model light color
            shadelight = new Color();
            CProgram.gQ2Game.gCMain.gCLight.R_LightPoint(_CurrentEntity.Origin, ref shadelight);

            // update HLSL variables
            CProgram.gQ2Game.gEffect.Parameters["xWorld"].SetValue(wMatrix);
            CProgram.gQ2Game.gEffect.Parameters["xLightModel"].SetValue(shadelight.ToVector4());


            CProgram.gQ2Game.gEffect.CurrentTechnique = CProgram.gQ2Game.gEffect.Techniques["TexturedSkin"];


            // bind new texture
            CProgram.gQ2Game.gEffect.Parameters["xTextureSkin"].SetValue(_CurrentEntity.Model.ModelMD2.skins[_CurrentEntity.ModelCurrentSkin]);

            foreach (EffectPass pass in CProgram.gQ2Game.gEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                CProgram.gQ2Game.gGraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _CurrentEntity.Model.ModelMD2.md2.num_tris);
            }

            CMain.c_brush_polys += _CurrentEntity.Model.ModelMD2.md2.num_tris;
        }



        // md2 model renderfx flags
        [Flags]
        public enum EModelFlags : int
        {
            RF_MINLIGHT = 1,       // allways have some light (viewmodel)
            RF_VIEWERMODEL = 2,    // don't draw through eyes, only mirrors
            RF_WEAPONMODEL = 4,    // only draw through eyes
            RF_FULLBRIGHT = 8,     // allways draw full intensity
            RF_DEPTHHACK = 16,     // for view weapon Z crunching
            RF_TRANSLUCENT = 32,
            RF_FRAMELERP = 64,
            RF_BEAM = 128,
            RF_CUSTOMSKIN = 256,   // skin is an index in image_precache
            RF_GLOW = 512,         // pulse lighting for bonus items
            RF_SHELL_RED = 1024,
            RF_SHELL_GREEN = 2048,
            RF_SHELL_BLUE = 4096
        }


        // ================================================================
        // 
        // MD2 MODELS
        // 
        // ================================================================

        public const int IDALIASHEADER = (('2' << 24) + ('P' << 16) + ('D' << 8) + 'I');
        private const int ALIAS_VERSION = 8;
        private const int MAX_TRIANGLES = 4096;
        private const int MAX_VERTS = 2048;
        private const int MAX_FRAMES = 512;
        private const int MAX_MD2SKINS = 32;
        private const int MAX_GLCMDS = 16384;

        private const int MAX_FRAMENAME = 16;
        private const int MAX_SKINNAME = 64;

        public struct SSTVert
        {
            public short s;
            public short t;
        }

        public struct SDTriangle
        {
            public short[] index_xyz;   // size 3
            public short[] index_st;    // size 3
        }

        public struct STrivertx
        {
            public byte[] v; // size 3
            public byte lightnormalindex;
        }

        public struct SAliasFrameDesc
        {
            public float[] scale;       // size 3 (multiply byte verts by this)
            public float[] translate;   // size 3 (then add this)
            public char[] name;         // size 16 (frame name from grabbing)
            public STrivertx[] verts;
        }

        // the glcmd format:
        // a positive integer starts a tristrip command, followed by that many
        // vertex structures.
        // a negative integer starts a trifan command, followed by -x vertexes
        // a zero indicates the end of the command list.
        // a vertex consists of a floating point s, a floating point t,
        // and an integer vertex index.

        public struct SMd2
        {
            public byte[] identification;   // size 4 (should be IDP2)
            public int version;             // version: must be 8

            public int skinwidth;           // texture width
            public int skinheight;          // texture height

            public int framesize;           // byte size of each frame

            public int num_skins;           // number of skins
            public int num_xyz;             // number of vertices
            public int num_st;              // number of texture coordinates (greater than num_xyz for seams)
            public int num_tris;            // number of triangles
            public int num_glcmds;          // dwords in strip/fan command list
            public int num_frames;          // number of frames

            public int ofs_skins;           // each skin is a MAX_SKINNAME string
            public int ofs_st;              // byte offset from start for stverts
            public int ofs_tris;            // offset for dtriangles
            public int ofs_frames;          // offset for first frame
            public int ofs_glcmds;          // offset for strip/fan command list
            public int ofs_end;             // end of file
        }

    }
}
