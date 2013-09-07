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

namespace Q2BSP
{
    public class CMain
    {
        public const string GameTitle = "Quake2 BSP XNA Renderer - Korvin Korax";
        public const int FieldOfView = 65;

        // game timing and frame rate
        public GameTime gTimeGame;
        private DateTime gTimeStart;
        public double gTimeRealTotal;
        public SFrameRate FrameRate;

        public VertexDeclaration WorldVertexDeclaration;
        public VertexDeclaration ModelVertexDeclaration;

        // render states
        public RasterizerState gRasterizerState;
        public BlendState gBlendState;
        public DepthStencilState gDepthStencilState;

        // game speeds
        public float SpeedGame;
        public float SpeedMouse;
        public float SpeedRotation;
        public float SpeedForward;
        public float SpeedSide;
        public float SpeedUp;
        public int MoveForward;
        public int MoveSide;
        public int MoveUp;

        // bsp rendering variables
        public static int r_viewcluster, r_viewcluster2, r_oldviewcluster, r_oldviewcluster2;
        public int r_visframecount;     // bumped when going to a new PVS
        public static int r_framecount; // used for dlight push checking
        public bool r_novis;            // CVAR candidate
        public bool r_nocull;           // CVAR candidate
        public bool r_lockpvs;          // CVAR candidate
        public bool r_bloom;            // CVAR candidate
        public int r_maxLights;
        public bool r_wireframe;
        public bool r_hardwarelight;
        public bool r_usepak;
        public bool r_htic2;
        public bool r_controls;
        public bool r_writelightmap;
        public bool r_drawentities;

        public static Vector3 ModelOrigin;
        public static Vector3 ModelAngles;

        public Random Rand;

        // polycounters
        public static int c_brush_polys;
        public static int c_alias_polys;

        // input processing
        private CInput gCInput;

        // image loading
        public CImage gCImage;

        // quake2 pak loading
        public CFiles gCFiles;

        // quake2 bsp loading
        public CQ2BSP gCQ2BSP;

        // quake2 md2 loading
        public CQ2MD2 gCQ2MD2;

        // quake2 skybox loading
        public CSky gCSky;

        // quake2 bsp in-memory management
        public CModel gCModel;

        // quake2 bsp lightmap management
        public CLight gCLight;

        // quake2 bsp surface management
        public CSurface gCSurface;

        // bloom effect
        public CBloom gCBloom;

        // game globals
        public CLocal.SGlobal gSGlobal;


        public CMain()
        {
            gCInput = new CInput();
            gCImage = new CImage();
            gCFiles = new CFiles();
            gCQ2BSP = new CQ2BSP();
            gCQ2MD2 = new CQ2MD2();
            gCSky = new CSky();
            gCModel = new CModel();
            gCLight = new CLight();
            gCSurface = new CSurface();
            gCBloom = new CBloom();

            gRasterizerState = new RasterizerState();
            gBlendState = new BlendState();
            gDepthStencilState = new DepthStencilState();

            SpeedGame = 1.0f;

            r_novis = CConfig.GetConfigBOOL("No Vis");
            r_nocull = true;
            r_lockpvs = false;
            r_bloom = true;
            r_wireframe = false;
            r_hardwarelight = false;
            r_usepak = CConfig.GetConfigBOOL("Load From Pak");
            r_htic2 = CConfig.GetConfigBOOL("Heretic2");
            r_controls = false;
            r_writelightmap = false;
            r_drawentities = true;

            Rand = new Random();


            // update OldHLSL to values that will force an initial update
            gSGlobal.OldHLSL.xLightModel.R = 1;
            gSGlobal.OldHLSL.xLightModel.G = 1;
            gSGlobal.OldHLSL.xLightModel.B = 1;
            gSGlobal.OldHLSL.xLightModel.A = 1;
            gSGlobal.OldHLSL.xLightAmbient.R = 10;
            gSGlobal.OldHLSL.xLightAmbient.G = 10;
            gSGlobal.OldHLSL.xLightAmbient.B = 10;
            gSGlobal.OldHLSL.xLightAmbient.A = 10;
            gSGlobal.OldHLSL.xLightPower = -1.0f;
            gSGlobal.OldHLSL.xTextureAlpha = -1.0f;
            gSGlobal.OldHLSL.xGamma = -1.0f;
            gSGlobal.OldHLSL.xRealTime = -1.0f;
            gSGlobal.OldHLSL.xBloomThreshold = -1.0f;
            gSGlobal.OldHLSL.xBaseIntensity = -1.0f;
            gSGlobal.OldHLSL.xBloomIntensity = -1.0f;
            gSGlobal.OldHLSL.xBaseSaturation = -1.0f;
            gSGlobal.OldHLSL.xBloomSaturation = -1.0f;
            gSGlobal.OldHLSL.xPointLights = true;
            gSGlobal.OldHLSL.xFlow = true;

            // default values for HLSL variables
            gSGlobal.HLSL.xLightModel.R = 0;
            gSGlobal.HLSL.xLightModel.G = 0;
            gSGlobal.HLSL.xLightModel.B = 0;
            gSGlobal.HLSL.xLightModel.A = 0;
            gSGlobal.HLSL.xLightAmbient.R = 20;
            gSGlobal.HLSL.xLightAmbient.G = 20;
            gSGlobal.HLSL.xLightAmbient.B = 20;
            gSGlobal.HLSL.xLightAmbient.A = 255;
            gSGlobal.HLSL.xLightPower = 1.5f;
            gSGlobal.HLSL.xPointLights = false;


            // default
            //gCBloom.BlurAmount = 4;
            //gSGlobal.HLSL.xBloomThreshold = 0.25f;
            //gSGlobal.HLSL.xBaseIntensity = 1.0f;
            //gSGlobal.HLSL.xBloomIntensity = 1.25f;
            //gSGlobal.HLSL.xBaseSaturation = 1.0f;
            //gSGlobal.HLSL.xBloomSaturation = 1.0f;

            // soft
            if (r_htic2 == true)
            {
                gCBloom.BlurAmount = 3;
                gSGlobal.HLSL.xBloomThreshold = 0.0f;
                gSGlobal.HLSL.xBaseIntensity = 1.0f;
                gSGlobal.HLSL.xBloomIntensity = 1.0f;
                gSGlobal.HLSL.xBaseSaturation = 1.0f;
                gSGlobal.HLSL.xBloomSaturation = 1.0f;
            }

            // desaturated
            //gCBloom.BlurAmount = 8;
            //gSGlobal.HLSL.xBloomThreshold = 0.5f;
            //gSGlobal.HLSL.xBaseIntensity = 1.0f;
            //gSGlobal.HLSL.xBloomIntensity = 2.0f;
            //gSGlobal.HLSL.xBaseSaturation = 1.0f;
            //gSGlobal.HLSL.xBloomSaturation = 0.0f;

            // saturated
            //gCBloom.BlurAmount = 4;
            //gSGlobal.HLSL.xBloomThreshold = 0.25f;
            //gSGlobal.HLSL.xBaseIntensity = 1.0f;
            //gSGlobal.HLSL.xBloomIntensity = 2.0f;
            //gSGlobal.HLSL.xBaseSaturation = 0.0f;
            //gSGlobal.HLSL.xBloomSaturation = 2.0f;

            // blurry
            //gCBloom.BlurAmount = 2;
            //gSGlobal.HLSL.xBloomThreshold = 0.0f;
            //gSGlobal.HLSL.xBaseIntensity = 0.1f;
            //gSGlobal.HLSL.xBloomIntensity = 1.0f;
            //gSGlobal.HLSL.xBaseSaturation = 1.0f;
            //gSGlobal.HLSL.xBloomSaturation = 1.0f;

            // subtle
            if (r_htic2 == false)
            {
                gCBloom.BlurAmount = 2;
                gSGlobal.HLSL.xBloomThreshold = 0.5f;
                gSGlobal.HLSL.xBaseIntensity = 1.0f;
                gSGlobal.HLSL.xBloomIntensity = 1.0f;
                gSGlobal.HLSL.xBaseSaturation = 1.0f;
                gSGlobal.HLSL.xBloomSaturation = 1.0f;
            }


            // original gamedir
            if (System.IO.Directory.Exists(CProgram.gQ2Game.Content.RootDirectory + "\\baseq2") == false)
                System.IO.Directory.CreateDirectory(CProgram.gQ2Game.Content.RootDirectory + "\\baseq2");

            // mission pack 1 gamedir
            if (System.IO.Directory.Exists(CProgram.gQ2Game.Content.RootDirectory + "\\xatrix") == false)
                System.IO.Directory.CreateDirectory(CProgram.gQ2Game.Content.RootDirectory + "\\xatrix");

            // mission pack 2 gamedir
            if (System.IO.Directory.Exists(CProgram.gQ2Game.Content.RootDirectory + "\\rogue") == false)
                System.IO.Directory.CreateDirectory(CProgram.gQ2Game.Content.RootDirectory + "\\rogue");


            // current gamedir
            if (System.IO.Directory.Exists(CProgram.gQ2Game.Content.RootDirectory + "\\" + CConfig.GetConfigSTRING("Base Game") + "\\env") == false)
                System.IO.Directory.CreateDirectory(CProgram.gQ2Game.Content.RootDirectory + "\\" + CConfig.GetConfigSTRING("Base Game") + "\\env");

            if (System.IO.Directory.Exists(CProgram.gQ2Game.Content.RootDirectory + "\\" + CConfig.GetConfigSTRING("Base Game") + "\\maps") == false)
                System.IO.Directory.CreateDirectory(CProgram.gQ2Game.Content.RootDirectory + "\\" + CConfig.GetConfigSTRING("Base Game") + "\\maps");

            if (System.IO.Directory.Exists(CProgram.gQ2Game.Content.RootDirectory + "\\" + CConfig.GetConfigSTRING("Base Game") + "\\scrnshot") == false)
                System.IO.Directory.CreateDirectory(CProgram.gQ2Game.Content.RootDirectory + "\\" + CConfig.GetConfigSTRING("Base Game") + "\\scrnshot");

            if (System.IO.Directory.Exists(CProgram.gQ2Game.Content.RootDirectory + "\\" + CConfig.GetConfigSTRING("Base Game") + "\\textures") == false)
                System.IO.Directory.CreateDirectory(CProgram.gQ2Game.Content.RootDirectory + "\\" + CConfig.GetConfigSTRING("Base Game") + "\\textures");
        }

        private void RealTimeInit()
        {
            gTimeStart = DateTime.Now;

            FrameRate.TimeElapsed = TimeSpan.Zero;
            FrameRate.FrameCount = 0;
            FrameRate.FrameRate = 0;

            gTimeRealTotal = 0.0d;
        }

        private void RealTimeUpdate()
        {
            TimeSpan TS;

            FrameRate.TimeElapsed += CProgram.gQ2Game.gCMain.gTimeGame.ElapsedGameTime;

            TS = DateTime.Now - gTimeStart;
            gTimeRealTotal = TS.TotalMilliseconds;
        }

        public void FrameUpdate()
        {
            SpeedMouse = (float)gTimeGame.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            SpeedMouse *= 0.25f * SpeedGame;

            SpeedRotation = (float)gTimeGame.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            SpeedRotation *= 1.3f * SpeedGame;

            SpeedForward = (float)gTimeGame.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            SpeedForward *= (260.0f * MoveForward) * SpeedGame;

            SpeedSide = (float)gTimeGame.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            SpeedSide *= (195.0f * MoveSide) * SpeedGame;

            SpeedUp = (float)gTimeGame.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            SpeedUp *= (130.0f * MoveUp) * SpeedGame;

            // set player pointlight position
            Vector4 PlayerLight;
            PlayerLight.X = CClient.cl.RefDef.ViewOrigin.X;
            PlayerLight.Y = CClient.cl.RefDef.ViewOrigin.Y;
            PlayerLight.Z = CClient.cl.RefDef.ViewOrigin.Z;
            PlayerLight.W = 1.0f;
            CQ2BSP.SWorldData.lights[1].gPosition = PlayerLight;

            gCInput.ProcessMouse();
            gCInput.ProcessKeyboard();

            WorldViewUpdate();
        }

        public void WorldViewInit()
        {
            Vector3 YawPitchRoll;

            YawPitchRoll.X = Microsoft.Xna.Framework.MathHelper.ToRadians(CClient.cl.RefDef.ViewAngles.X);
            YawPitchRoll.Y = Microsoft.Xna.Framework.MathHelper.ToRadians(CClient.cl.RefDef.ViewAngles.Y);
            YawPitchRoll.Z = Microsoft.Xna.Framework.MathHelper.ToRadians(CClient.cl.RefDef.ViewAngles.Z + 90.0f); // adding 90 degrees (evaluate this)

            CClient.cl.RefDef.TargetOrigin = new Vector3(CClient.cl.RefDef.ViewOrigin.X, CClient.cl.RefDef.ViewOrigin.Y, CClient.cl.RefDef.ViewOrigin.Z);
            CClient.cl.RefDef.TargetAngles = Quaternion.CreateFromYawPitchRoll(YawPitchRoll.X, YawPitchRoll.Y, YawPitchRoll.Z);

            RealTimeInit();
        }

        public void WorldViewUpdate()
        {
            Vector3 CameraOrigin;

            CameraOrigin = new Vector3(0.0f, 1.0f, 0.0f);
            CameraOrigin = Vector3.Transform(CameraOrigin, Matrix.CreateFromQuaternion(CClient.cl.RefDef.TargetAngles));
            CameraOrigin += CClient.cl.RefDef.TargetOrigin;

            CClient.cl.RefDef.ViewUp = new Vector3(0, 0, 1);
            CClient.cl.RefDef.ViewUp = Vector3.Transform(CClient.cl.RefDef.ViewUp, Matrix.CreateFromQuaternion(CClient.cl.RefDef.TargetAngles));

            gSGlobal.HLSL.xViewMatrix = Matrix.CreateLookAt(CameraOrigin, CClient.cl.RefDef.TargetOrigin, CClient.cl.RefDef.ViewUp);
            gSGlobal.HLSL.xProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(Microsoft.Xna.Framework.MathHelper.ToRadians(FieldOfView), CProgram.gQ2Game.gGraphicsDevice.Viewport.AspectRatio, 0.1f, 4096.0f);

            RealTimeUpdate();
        }

        private void CalculateFrameRate()
        {
            if (FrameRate.TimeElapsed > TimeSpan.FromSeconds(1))
            {
                FrameRate.TimeElapsed -= TimeSpan.FromSeconds(1);
                FrameRate.FrameRate = FrameRate.FrameCount;
                FrameRate.FrameCount = 0;
            }

            FrameRate.FrameCount++;
        }

        public static void Error(EErrorParm Code, string Msg)
        {
            //if (Code == EErrorParm.ERR_WARNING)
            //    MessageBox.Show(Msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //else if (Code == EErrorParm.ERR_FATAL)
            //    MessageBox.Show(Msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //else
            //    MessageBox.Show(Msg);

            //if (Code == EErrorParm.ERR_FATAL)
            //    Application.Exit();

            System.Diagnostics.Debug.WriteLine(Msg);
            CProgram.gQ2Game.Exit();

        }

        public static void Printf(string Msg)
        {
            //ConsoleText += Msg.Replace("\r\n", "\n").Replace("\n\r", "\n").Replace("\n", "\r\n");
            //cProgram.uM.textBoxConsole.Text = ConsoleText;

            System.Diagnostics.Debug.WriteLine(Msg);
        }


        // =====================================================================
        //
        // WORLD BUILDING FUNCTIONS
        // 
        // =====================================================================

        #region World Building
        public void BuildWorldModel(string BSPFileName)
        {
            List<CModel.WorldVertexFormat> vertbuffer;

            if (string.IsNullOrEmpty(BSPFileName) == true)
                return;

            gCFiles.FS_LoadPak(CConfig.GetConfigSTRING("Base Game") + "\\" + CConfig.GetConfigSTRING("Pak Name"), System.IO.Path.GetFileNameWithoutExtension(CConfig.GetConfigSTRING("Pak Name")));

            vertbuffer = new List<CModel.WorldVertexFormat>();
            WorldVertexDeclaration = new VertexDeclaration(CModel.WorldVertexFormat.VertexElements);

            gCModel.R_BeginRegistration(BSPFileName);
            gCModel.R_EndRegistration();

            for (int i = 0; i < CQ2BSP.SWorldData.numsurfaces; i++)
            {
                List<CModel.WorldVertexFormat> verts = new List<CModel.WorldVertexFormat>();
                CQ2BSP.SWorldData.surfaces[i].polys = new CModel.SGLPoly[1]; // multiple if warped

                for (int j = 0; j < CQ2BSP.SWorldData.surfaces[i].numedges; j++)
                {
                    CModel.WorldVertexFormat _verts;
                    Vector3 vec0;
                    Vector3 vec1;

                    // select edge index
                    int edge = CQ2BSP.SWorldData.surfedges[CQ2BSP.SWorldData.surfaces[i].firstedge + j];


                    // find vertex index to be used from edge list
                    if (edge >= 0)
                        _verts.Position = CQ2BSP.SWorldData.vertexes[CQ2BSP.SWorldData.edges[edge].v[0]].Position;
                    else
                        _verts.Position = CQ2BSP.SWorldData.vertexes[CQ2BSP.SWorldData.edges[-edge].v[1]].Position;


                    // initialize normals to zero
                    _verts.Normal.X = 0;
                    _verts.Normal.Y = 0;
                    _verts.Normal.Z = 0;


                    vec0.X = CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[i].texinfo].vecs[0].X;
                    vec0.Y = CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[i].texinfo].vecs[0].Y;
                    vec0.Z = CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[i].texinfo].vecs[0].Z;
                    vec1.X = CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[i].texinfo].vecs[1].X;
                    vec1.Y = CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[i].texinfo].vecs[1].Y;
                    vec1.Z = CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[i].texinfo].vecs[1].Z;


                    // set texture coordinates
                    _verts.TextureCoordinate.X = Vector3.Dot(_verts.Position, vec0) + CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[i].texinfo].vecs[0].W;
                    _verts.TextureCoordinate.X /= CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[i].texinfo].Width;
                    _verts.TextureCoordinate.Y = Vector3.Dot(_verts.Position, vec1) + CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[i].texinfo].vecs[1].W;
                    _verts.TextureCoordinate.Y /= CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[i].texinfo].Height;


                    // set lightmap coordinates
                    _verts.LightmapCoordinate.X = Vector3.Dot(_verts.Position, vec0) + CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[i].texinfo].vecs[0].W;
                    _verts.LightmapCoordinate.X -= CQ2BSP.SWorldData.surfaces[i].texturemins[0];
                    _verts.LightmapCoordinate.X += CQ2BSP.SWorldData.surfaces[i].light_s * 16;
                    _verts.LightmapCoordinate.X += 8;
                    _verts.LightmapCoordinate.X /= CSurface.BLOCK_WIDTH * 16;
                    _verts.LightmapCoordinate.Y = Vector3.Dot(_verts.Position, vec1) + CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[i].texinfo].vecs[1].W;
                    _verts.LightmapCoordinate.Y -= CQ2BSP.SWorldData.surfaces[i].texturemins[1];
                    _verts.LightmapCoordinate.Y += CQ2BSP.SWorldData.surfaces[i].light_t * 16;
                    _verts.LightmapCoordinate.Y += 8;
                    _verts.LightmapCoordinate.Y /= CSurface.BLOCK_HEIGHT * 16;


                    verts.Add(_verts);
                }


                // save surface polygon
                CQ2BSP.SWorldData.surfaces[i].polys[0].next = 0;
                CQ2BSP.SWorldData.surfaces[i].polys[0].chain = 0;
                CQ2BSP.SWorldData.surfaces[i].polys[0].numverts = verts.Count;
                CQ2BSP.SWorldData.surfaces[i].polys[0].verts = new CModel.SPolyVerts[verts.Count];
                for (int j = 0; j < verts.Count; j++)
                {
                    CQ2BSP.SWorldData.surfaces[i].polys[0].verts[j].offset = 0;
                    CQ2BSP.SWorldData.surfaces[i].polys[0].verts[j].vertex = verts[j];
                }


                // cut up surface polygon for warps
                gCSurface.SubdivideSurface(ref CQ2BSP.SWorldData.surfaces[i]);


                // bounds & normals
                CQ2BSP.SWorldData.surfaces[i].bounds = CLocal.ClearBounds();
                for (int j = 0; j < CQ2BSP.SWorldData.surfaces[i].polys.Length; j++)
                {
                    for (int k = 0; k < CQ2BSP.SWorldData.surfaces[i].polys[j].numverts; k++)
                    {
                        // calculate vertex normals
                        CModel.CalculateSurfaceNormal(ref CQ2BSP.SWorldData.surfaces[i].polys[j].verts);

                        // surface bounds for frustum culling
                        CLocal.AddPointToBounds(CQ2BSP.SWorldData.surfaces[i].polys[j].verts[k].vertex.Position, ref CQ2BSP.SWorldData.surfaces[i].bounds);
                    }
                }


                // surface bounds middle for light distance calculations
                Vector3 vmid = Vector3.Subtract(CQ2BSP.SWorldData.surfaces[i].bounds.Max, CQ2BSP.SWorldData.surfaces[i].bounds.Min);
                CQ2BSP.SWorldData.surfaces[i].boundsMid.X = CQ2BSP.SWorldData.surfaces[i].bounds.Min.X + (Math.Abs(vmid.X) / 2);
                CQ2BSP.SWorldData.surfaces[i].boundsMid.Y = CQ2BSP.SWorldData.surfaces[i].bounds.Min.Y + (Math.Abs(vmid.Y) / 2);
                CQ2BSP.SWorldData.surfaces[i].boundsMid.Z = CQ2BSP.SWorldData.surfaces[i].bounds.Min.Z + (Math.Abs(vmid.Z) / 2);


                // calculate surface plane
                CQ2BSP.SWorldData.surfaces[i].plane2 = new Plane(verts[0].Normal, Vector3.Dot(verts[0].Normal, verts[0].Position));
                CQ2BSP.SWorldData.surfaces[i].plane2.Normalize();


                // add surface polys to vertex buffer
                for (int j = 0; j < CQ2BSP.SWorldData.surfaces[i].polys.Length; j++)
                {
                    for (int k = 0; k < CQ2BSP.SWorldData.surfaces[i].polys[j].numverts; k++)
                    {
                        CQ2BSP.SWorldData.surfaces[i].polys[j].verts[k].offset = vertbuffer.Count;
                        vertbuffer.Add(CQ2BSP.SWorldData.surfaces[i].polys[j].verts[k].vertex);
                    }
                }

                // build surface index buffer for quicker warps
                gCSurface.BuildSurfaceIndex(ref CQ2BSP.SWorldData.surfaces[i]);
            }


            // set up world vertex buffer
            CQ2BSP.SWorldData.vbWorldSolid = new VertexBuffer(CProgram.gQ2Game.gGraphicsDevice, WorldVertexDeclaration, vertbuffer.Count, BufferUsage.WriteOnly);
            CQ2BSP.SWorldData.vbWorldSolid.SetData(vertbuffer.ToArray());


            // set up world index buffers
            CQ2BSP.SWorldData.ibWorldSolid = new CModel.SMIndexBuffer[CQ2BSP.SWorldData.WorldLightmaps.Length, CQ2BSP.SWorldData.texinfo.Length];
            for (int i = 0; i < CQ2BSP.SWorldData.ibWorldSolid.GetLength(0); i++)
            {
                for (int j = 0; j < CQ2BSP.SWorldData.ibWorldSolid.GetLength(1); j++)
                {
                    CQ2BSP.SWorldData.ibWorldSolid[i, j].ibIndices = null;

                    if (CProgram.gQ2Game.gGraphicsDevice.GraphicsProfile == GraphicsProfile.HiDef)
                        CQ2BSP.SWorldData.ibWorldSolid[i, j].ibBuffer = new IndexBuffer(CProgram.gQ2Game.gGraphicsDevice, IndexElementSize.ThirtyTwoBits, 4096, BufferUsage.WriteOnly);
                    else
                        CQ2BSP.SWorldData.ibWorldSolid[i, j].ibBuffer = new IndexBuffer(CProgram.gQ2Game.gGraphicsDevice, IndexElementSize.SixteenBits, 4096, BufferUsage.WriteOnly);
                }
            }

            BuildBrushModels();

            gCModel.BuildEntities();
            gCSky.BuildSkybox();

            for (int i = 0; i < CQ2BSP.SWorldData.numsurfaces; i++)
            {
                MarkSurfaceLights(i);
            }

            gCFiles.FS_ClosePak();
        }

        private void BuildBrushModels()
        {
            // index 0 reserved for worldmodel
            for (int i = 1; i < gCModel.mod_inline.Length; i++)
            {
                List<CModel.WorldVertexFormat> vertbuffer;

                if (gCModel.mod_inline[i].ModType != CModel.EModType.MOD_BRUSH)
                    continue;

                // skip brushmodel if theres no surfaces
                if (gCModel.mod_inline[i].nummodelsurfaces == 0)
                    continue;

                vertbuffer = new List<CModel.WorldVertexFormat>();

                for (int j = 0; j < gCModel.mod_inline[i].nummodelsurfaces; j++)
                {
                    int surf = gCModel.mod_inline[i].firstmodelsurface + j;
                    List<CModel.WorldVertexFormat> verts = new List<CModel.WorldVertexFormat>();
                    CQ2BSP.SWorldData.surfaces[surf].polys = new CModel.SGLPoly[1]; // multiple if warped

                    // jkrige TODO: submodel flag settings should actually be set by the gamecode instead of based on the surfaces like seen below
                    if ((CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_TRANS33) == CQ2BSP.ESurface.SURF_TRANS33
                        | (CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_TRANS66) == CQ2BSP.ESurface.SURF_TRANS66
                        )
                    {
                        if ((gCModel.mod_inline[i].Flags & CQ2MD2.EModelFlags.RF_TRANSLUCENT) != CQ2MD2.EModelFlags.RF_TRANSLUCENT)
                            gCModel.mod_inline[i].Flags |= CQ2MD2.EModelFlags.RF_TRANSLUCENT;
                    }

                    // dont bother to build surfaces with SURF_NODRAW flags set
                    if ((CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_NODRAW) == CQ2BSP.ESurface.SURF_NODRAW)
                        continue;

                    for (int k = 0; k < CQ2BSP.SWorldData.surfaces[surf].numedges; k++)
                    {
                        CModel.WorldVertexFormat _verts;
                        Vector3 vec0;
                        Vector3 vec1;


                        // select edge index
                        int edge = CQ2BSP.SWorldData.surfedges[CQ2BSP.SWorldData.surfaces[surf].firstedge + k];


                        // find vertex index to be used from edge list
                        if (edge >= 0)
                            _verts.Position = CQ2BSP.SWorldData.vertexes[CQ2BSP.SWorldData.edges[edge].v[0]].Position;
                        else
                            _verts.Position = CQ2BSP.SWorldData.vertexes[CQ2BSP.SWorldData.edges[-edge].v[1]].Position;


                        // initialize normals to zero
                        _verts.Normal.X = 0;
                        _verts.Normal.Y = 0;
                        _verts.Normal.Z = 0;


                        vec0.X = CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].vecs[0].X;
                        vec0.Y = CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].vecs[0].Y;
                        vec0.Z = CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].vecs[0].Z;
                        vec1.X = CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].vecs[1].X;
                        vec1.Y = CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].vecs[1].Y;
                        vec1.Z = CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].vecs[1].Z;


                        // set texture coordinates
                        _verts.TextureCoordinate.X = Vector3.Dot(_verts.Position, vec0) + CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].vecs[0].W;
                        _verts.TextureCoordinate.X /= CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].Width;
                        _verts.TextureCoordinate.Y = Vector3.Dot(_verts.Position, vec1) + CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].vecs[1].W;
                        _verts.TextureCoordinate.Y /= CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].Height;


                        // set lightmap coordinates
                        _verts.LightmapCoordinate.X = Vector3.Dot(_verts.Position, vec0) + CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].vecs[0].W;
                        _verts.LightmapCoordinate.X -= CQ2BSP.SWorldData.surfaces[surf].texturemins[0];
                        _verts.LightmapCoordinate.X += CQ2BSP.SWorldData.surfaces[surf].light_s * 16;
                        _verts.LightmapCoordinate.X += 8;
                        _verts.LightmapCoordinate.X /= CSurface.BLOCK_WIDTH * 16;
                        _verts.LightmapCoordinate.Y = Vector3.Dot(_verts.Position, vec1) + CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].vecs[1].W;
                        _verts.LightmapCoordinate.Y -= CQ2BSP.SWorldData.surfaces[surf].texturemins[1];
                        _verts.LightmapCoordinate.Y += CQ2BSP.SWorldData.surfaces[surf].light_t * 16;
                        _verts.LightmapCoordinate.Y += 8;
                        _verts.LightmapCoordinate.Y /= CSurface.BLOCK_HEIGHT * 16;


                        verts.Add(_verts);
                    }


                    // skip brushmodel if theres not enough vertices
                    if (verts.Count < 3)
                        continue;

                    // save surface polygon
                    CQ2BSP.SWorldData.surfaces[surf].polys[0].next = 0;
                    CQ2BSP.SWorldData.surfaces[surf].polys[0].chain = 0;
                    CQ2BSP.SWorldData.surfaces[surf].polys[0].numverts = verts.Count;
                    CQ2BSP.SWorldData.surfaces[surf].polys[0].verts = new CModel.SPolyVerts[verts.Count];
                    for (int k = 0; k < verts.Count; k++)
                    {
                        CQ2BSP.SWorldData.surfaces[surf].polys[0].verts[k].offset = 0;
                        CQ2BSP.SWorldData.surfaces[surf].polys[0].verts[k].vertex = verts[k];
                    }


                    // cut up surface polygon for warps
                    gCSurface.SubdivideSurface(ref CQ2BSP.SWorldData.surfaces[surf]);


                    // bounds & normals
                    CQ2BSP.SWorldData.surfaces[surf].bounds = CLocal.ClearBounds();
                    for (int k = 0; k < CQ2BSP.SWorldData.surfaces[surf].polys.Length; k++)
                    {
                        for (int l = 0; l < CQ2BSP.SWorldData.surfaces[surf].polys[k].numverts; l++)
                        {
                            // calculate vertex normals
                            CModel.CalculateSurfaceNormal(ref CQ2BSP.SWorldData.surfaces[surf].polys[k].verts);

                            // surface bounds for frustum culling
                            CLocal.AddPointToBounds(CQ2BSP.SWorldData.surfaces[surf].polys[k].verts[l].vertex.Position, ref CQ2BSP.SWorldData.surfaces[surf].bounds);
                        }
                    }


                    // surface bounds middle for light distance calculations
                    Vector3 vmid = Vector3.Subtract(CQ2BSP.SWorldData.surfaces[surf].bounds.Max, CQ2BSP.SWorldData.surfaces[surf].bounds.Min);
                    CQ2BSP.SWorldData.surfaces[surf].boundsMid.X = CQ2BSP.SWorldData.surfaces[surf].bounds.Min.X + (Math.Abs(vmid.X) / 2);
                    CQ2BSP.SWorldData.surfaces[surf].boundsMid.Y = CQ2BSP.SWorldData.surfaces[surf].bounds.Min.Y + (Math.Abs(vmid.Y) / 2);
                    CQ2BSP.SWorldData.surfaces[surf].boundsMid.Z = CQ2BSP.SWorldData.surfaces[surf].bounds.Min.Z + (Math.Abs(vmid.Z) / 2);


                    // calculate surface plane
                    CQ2BSP.SWorldData.surfaces[surf].plane2 = new Plane(verts[0].Normal, Vector3.Dot(verts[0].Normal, verts[0].Position));
                    CQ2BSP.SWorldData.surfaces[surf].plane2.Normalize();


                    // add surface polys to vertex buffer
                    for (int k = 0; k < CQ2BSP.SWorldData.surfaces[surf].polys.Length; k++)
                    {
                        for (int l = 0; l < CQ2BSP.SWorldData.surfaces[surf].polys[k].numverts; l++)
                        {
                            CQ2BSP.SWorldData.surfaces[surf].polys[k].verts[l].offset = vertbuffer.Count;
                            vertbuffer.Add(CQ2BSP.SWorldData.surfaces[surf].polys[k].verts[l].vertex);
                        }
                    }

                    // build surface index buffer for quicker warps
                    gCSurface.BuildSurfaceIndex(ref CQ2BSP.SWorldData.surfaces[surf]);

                    MarkSurfaceStatic(ref gCModel.mod_inline[i], surf);
                }


                // skip brushmodel if theres not enough vertices
                if (vertbuffer.Count < 3)
                    continue;


                // set up brush model vertex buffer
                gCModel.mod_inline[i].vbWorldSolid = new VertexBuffer(CProgram.gQ2Game.gGraphicsDevice, WorldVertexDeclaration, vertbuffer.Count, BufferUsage.WriteOnly);
                gCModel.mod_inline[i].vbWorldSolid.SetData(vertbuffer.ToArray());


                // set up brush model index buffers
                gCModel.mod_inline[i].ibWorldSolid = new CModel.SMIndexBuffer[CQ2BSP.SWorldData.WorldLightmaps.Length, CQ2BSP.SWorldData.texinfo.Length];
                for (int j = 0; j < gCModel.mod_inline[i].ibWorldSolid.GetLength(0); j++)
                {
                    for (int k = 0; k < gCModel.mod_inline[i].ibWorldSolid.GetLength(1); k++)
                    {
                        gCModel.mod_inline[i].ibWorldSolid[j, k].ibIndices = null;

                        if (CProgram.gQ2Game.gGraphicsDevice.GraphicsProfile == GraphicsProfile.HiDef)
                            gCModel.mod_inline[i].ibWorldSolid[j, k].ibBuffer = new IndexBuffer(CProgram.gQ2Game.gGraphicsDevice, IndexElementSize.ThirtyTwoBits, 2048, BufferUsage.WriteOnly);
                        else
                            gCModel.mod_inline[i].ibWorldSolid[j, k].ibBuffer = new IndexBuffer(CProgram.gQ2Game.gGraphicsDevice, IndexElementSize.SixteenBits, 2048, BufferUsage.WriteOnly);
                    }
                }
            }
        }

        private void MarkSurfaceLights(int surface)
        {
            Vector3 lightpos;
            float vlen;
            bool Exists;
            int ExistsIndex;
            int SelectedLights;

            List<int> LightIndex = new List<int>();
            List<double> LightLength = new List<double>();

            for (int i = 0; i < CQ2BSP.SWorldData.lights.Length; i++)
            {
                lightpos.X = CQ2BSP.SWorldData.lights[i].gPosition.X;
                lightpos.Y = CQ2BSP.SWorldData.lights[i].gPosition.Y;
                lightpos.Z = CQ2BSP.SWorldData.lights[i].gPosition.Z;

                // find which side of the surface we are on
                if ((Microsoft.Xna.Framework.Vector3.Dot(lightpos, CQ2BSP.SWorldData.surfaces[surface].plane2.Normal) - CQ2BSP.SWorldData.surfaces[surface].plane2.D) < 0)
                    continue;

                vlen = Vector3.Subtract(CQ2BSP.SWorldData.surfaces[surface].boundsMid, lightpos).Length();

                for (int j = 0; j < r_maxLights; j++)
                {
                    Exists = false;
                    ExistsIndex = 0;

                    for (ExistsIndex = 0; ExistsIndex < LightIndex.Count; ExistsIndex++)
                    {
                        if (LightIndex[ExistsIndex] == i)
                        {
                            Exists = true;
                            break;
                        }
                    }


                    if (j < LightLength.Count)
                    {
                        if (vlen < LightLength[j] && Exists == false)
                        {
                            if (LightLength.Count >= r_maxLights)
                            {
                                float newlight = vlen - CQ2BSP.SWorldData.lights[i].gRange;

                                if (newlight < 0.0d)
                                {
                                    LightIndex.RemoveAt(LightIndex.Count - 1);
                                    LightLength.RemoveAt(LightLength.Count - 1);
                                }
                            }

                            if (j < LightLength.Count)
                            {
                                LightIndex.Insert(j, i);
                                LightLength.Insert(j, vlen);
                            }
                            else
                            {
                                LightIndex.Add(i);
                                LightLength.Add(vlen);
                            }
                        }
                    }
                    else
                    {
                        if (Exists == false)
                        {
                            LightIndex.Add(i);
                            LightLength.Add(vlen);
                        }
                    }
                }
            }

            SelectedLights = LightIndex.Count;
            if (SelectedLights < r_maxLights)
            {
                lightpos.X = CQ2BSP.SWorldData.lights[0].gPosition.X;
                lightpos.Y = CQ2BSP.SWorldData.lights[0].gPosition.Y;
                lightpos.Z = CQ2BSP.SWorldData.lights[0].gPosition.Z;

                vlen = Vector3.Subtract(CQ2BSP.SWorldData.surfaces[surface].boundsMid, lightpos).Length();

                for (int i = 0; i < r_maxLights - SelectedLights; i++)
                {
                    LightIndex.Add(0);
                    LightLength.Add(vlen);
                }
            }

            CQ2BSP.SWorldData.surfaces[surface].lightIndex = LightIndex.ToArray();
            CQ2BSP.SWorldData.surfaces[surface].lightLength = LightLength.ToArray();
        }
        #endregion


        // =====================================================================
        //
        // WORLD RENDERING FUNCTIONS
        // 
        // =====================================================================

        #region World Rendering
        public void SetupFrame()
        {
            int leaf;

            r_framecount++;

            // current viewcluster
            r_oldviewcluster = r_viewcluster;
            r_oldviewcluster2 = r_viewcluster2;
            leaf = gCModel.Mod_PointInLeaf(CClient.cl.RefDef.ViewOrigin);
            r_viewcluster = r_viewcluster2 = CQ2BSP.SWorldData.nodes[leaf].cluster;


            // check above and below so crossing solid water doesn't draw wrong
            if (CQ2BSP.SWorldData.nodes[leaf].contents == 0)
            {
                // look down a bit
                Microsoft.Xna.Framework.Vector3 temp;

                temp = CClient.cl.RefDef.ViewOrigin;
                temp.Z -= 16.0f;

                leaf = gCModel.Mod_PointInLeaf(temp);
            }
            else
            {
                // look up a bit
                Microsoft.Xna.Framework.Vector3 temp;

                temp = CClient.cl.RefDef.ViewOrigin;
                temp.Z += 16.0f;

                leaf = gCModel.Mod_PointInLeaf(temp);
            }

            if ((CQ2BSP.SWorldData.nodes[leaf].contents & CQ2BSP.CONTENTS_SOLID) == 0
                && CQ2BSP.SWorldData.nodes[leaf].cluster != r_viewcluster2)
            {
                r_viewcluster2 = CQ2BSP.SWorldData.nodes[leaf].cluster;
            }
            // current viewcluster

            c_brush_polys = 0;
            c_alias_polys = 0;
        }

        public void SetFrustum()
        {
            CClient.cl.RefDef.FrustumBounds = new BoundingFrustum(gSGlobal.HLSL.xViewMatrix * gSGlobal.HLSL.xProjectionMatrix);
        }

        public void MarkLeaves()
        {
            byte[] vis;
            byte[] fatvis;
            int c;
            int leaf;
            int cluster;
            int idx_parent;

            if (r_oldviewcluster == r_viewcluster && r_oldviewcluster2 == r_viewcluster2 && r_novis == false && r_viewcluster != -1)
                return;

            // development aid to let you run around and see exactly where the pvs ends
            if (r_lockpvs == true)
                return;

            r_visframecount++;
            r_oldviewcluster = r_viewcluster;
            r_oldviewcluster2 = r_viewcluster2;

            if (r_novis == true || r_viewcluster == -1 || CQ2BSP.SWorldData.visdata == null)
            {
                // mark everything
                for (int i = 0; i < CQ2BSP.SWorldData.numleafs; i++)
                {
                    CQ2BSP.SWorldData.nodes[CQ2BSP.SWorldData.numDecisionNodes + i].visframe = r_visframecount;
                }

                for (int i = 0; i < CQ2BSP.SWorldData.numDecisionNodes; i++)
                {
                    CQ2BSP.SWorldData.nodes[i].visframe = r_visframecount;
                }

                return;
            }

            vis = gCModel.Mod_ClusterPVS(r_viewcluster);


            // may have to combine two clusters because of solid water boundaries
            if (r_viewcluster2 != r_viewcluster)
            {
                fatvis = new byte[CQ2BSP.MAX_MAP_LEAFS / 8];

                for (int i = 0; i < (CQ2BSP.SWorldData.numleafs + 7) / 8; i++)
                {
                    fatvis[i] = vis[i];
                }

                vis = gCModel.Mod_ClusterPVS(r_viewcluster2);
                c = (CQ2BSP.SWorldData.numleafs + 31) / 32;

                for (int i = 0; i < c; i++)
                {
                    int ifatvis;
                    byte[] bfatvis;

                    ifatvis = BitConverter.ToInt32(fatvis, i * 4);
                    ifatvis |= BitConverter.ToInt32(vis, i * 4);
                    bfatvis = BitConverter.GetBytes(ifatvis);

                    fatvis[(i * 4) + 0] = bfatvis[0];
                    fatvis[(i * 4) + 1] = bfatvis[1];
                    fatvis[(i * 4) + 2] = bfatvis[2];
                    fatvis[(i * 4) + 3] = bfatvis[3];
                }

                // jkrige - is this correct?
                for (int i = 0; i < (CQ2BSP.SWorldData.numleafs + 7) / 8; i++)
                {
                    vis[i] = fatvis[i];
                }
                // jkrige - is this correct?
            }

            for (int i = 0; i < CQ2BSP.SWorldData.numleafs; i++)
            {
                leaf = CQ2BSP.SWorldData.numDecisionNodes + i;
                cluster = CQ2BSP.SWorldData.nodes[leaf].cluster;

                if (cluster < 0 || cluster >= CQ2BSP.SWorldData.vis.numclusters)
                    continue;

                // check general pvs
                if ((vis[cluster >> 3] & (1 << (cluster & 7))) == 0)
                    continue;

                // check for door connection
                // jkrige - quake3
                //if ((tr.refdef.areamask[leaf->area >> 3] & (1 << (leaf->area & 7))))
                //{
                //    continue;		// not visible
                //}
                // jkrige - quake3

                idx_parent = leaf;
                do
                {
                    if (CQ2BSP.SWorldData.nodes[idx_parent].visframe == r_visframecount)
                        break;

                    CQ2BSP.SWorldData.nodes[idx_parent].visframe = r_visframecount;
                    idx_parent = CQ2BSP.SWorldData.nodes[idx_parent].parent;

                } while (idx_parent >= 0);
            }
        }

        private bool R_CullBox(Microsoft.Xna.Framework.BoundingBox bounds)
        {
            bool cull = false;

            if (r_nocull == true)
                return false;

            switch (CClient.cl.RefDef.FrustumBounds.Contains(bounds))
            {
                case ContainmentType.Disjoint:
                    cull = true;
                    break;

                case ContainmentType.Intersects:
                    cull = false;
                    break;

                case ContainmentType.Contains:
                    cull = false;
                    break;
            }

            return cull;
        }

        private void MarkSurfaceStatic(ref CModel.SModel _SModel, int FaceIndex)
        {
            STextureStatic _STextureStatic;

            if (_SModel.MarkSurfaceListStatic == null)
                _SModel.MarkSurfaceListStatic = new List<STextureStatic>();

            _STextureStatic.lightmaptexturenum = CQ2BSP.SWorldData.surfaces[FaceIndex].lightmaptexturenum;
            _STextureStatic.texinfo = CQ2BSP.SWorldData.surfaces[FaceIndex].texinfo;
            _STextureStatic.surf = FaceIndex;

            _SModel.MarkSurfaceListStatic.Add(_STextureStatic);
        }

        private void MarkSurfaceDynamic(ref CModel.SModel _SModel, int FaceIndex)
        {
            STextureDynamic _STextureDynamic;

            if (_SModel.MarkSurfaceListDynamic == null)
                _SModel.MarkSurfaceListDynamic = new List<STextureDynamic>();

            _STextureDynamic.texinfo = CQ2BSP.SWorldData.surfaces[FaceIndex].texinfo;
            _STextureDynamic.surf = FaceIndex;

            _SModel.MarkSurfaceListDynamic.Add(_STextureDynamic);
        }

        private void RecursiveWorldNode(int idx_node)
        {
            int c;
            int side;
            CModel.EMSurface sidebit;
            int surf;
            CModel.SMNode node;
            CShared.SCPlane plane;

            node = CQ2BSP.SWorldData.nodes[idx_node];

            if (node.contents == CQ2BSP.CONTENTS_SOLID)
                return; // solid

            if (node.visframe != r_visframecount)
                return;

            if (R_CullBox(node.bounds) == true)
                return;


            // if a leaf node, draw stuff
            if (node.contents != -1)
            {
                // check for door connected areas
                // jkrige - quake2
                //if (r_newrefdef.areabits)
                //{
                //    if (!(r_newrefdef.areabits[pleaf->area >> 3] & (1 << (pleaf->area & 7))))
                //        return;		// not visible
                //}
                // jkrige - quake2

                c = node.nummarksurfaces;

                if (c != 0)
                {
                    int mark = 0;

                    do
                    {
                        CQ2BSP.SWorldData.surfaces[CQ2BSP.SWorldData.marksurfaces[node.firstmarksurface + mark]].visframe = r_framecount;
                        mark++;
                    } while (--c != 0);
                }

                return;
            }


            // 
            // node is just a decision point, so go down the apropriate sides
            // 


            // find which side of the node we are on
            plane = CQ2BSP.SWorldData.planes[node.plane];
            if ((Microsoft.Xna.Framework.Vector3.Dot(ModelOrigin, plane.normal) - plane.dist) >= 0)
            {
                side = 0;
                sidebit = 0;
            }
            else
            {
                side = 1;
                sidebit = CModel.EMSurface.SURF_PLANEBACK;
            }


            // recurse down the children, front side first
            RecursiveWorldNode(node.children[side]);


            // draw stuff
            for (c = node.numsurfaces, surf = node.firstsurface; c != 0; c--, surf++)
            {
                if (CQ2BSP.SWorldData.surfaces[surf].visframe != r_framecount)
                    continue;

                if ((CQ2BSP.SWorldData.surfaces[surf].flags & CModel.EMSurface.SURF_PLANEBACK) != sidebit)
                    continue; // wrong side

                /*if (
                    (CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_TRANS33) == CQ2BSP.ESurface.SURF_TRANS33
                    | (CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_TRANS66) == CQ2BSP.ESurface.SURF_TRANS66
                    | (CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_WARP) == CQ2BSP.ESurface.SURF_WARP
                    )
                {
                    MarkSurfaceTransWarp(surf);
                }
                else
                {
                    if ((CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_SKY) != CQ2BSP.ESurface.SURF_SKY)
                        MarkSurfaceStatic(surf);
                }*/

                if (
                    (CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_WARP) == CQ2BSP.ESurface.SURF_WARP
                    | (CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_TRANS33) == CQ2BSP.ESurface.SURF_TRANS33
                    | (CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_TRANS66) == CQ2BSP.ESurface.SURF_TRANS66
                    | (CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_FLOWING) == CQ2BSP.ESurface.SURF_FLOWING
                    )
                {
                    MarkSurfaceDynamic(ref CQ2BSP.SWorldData, surf);
                }
                else
                {
                    if ((CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_SKY) != CQ2BSP.ESurface.SURF_SKY)
                        MarkSurfaceStatic(ref CQ2BSP.SWorldData, surf);
                }
            }

            // recurse down the back side
            RecursiveWorldNode(node.children[Convert.ToInt32(side == 0)]);
        }

        /// <summary>
        /// MarkSurfaceSetup
        /// ----------------
        /// Sort surfaces according to texture
        /// </summary>
        public bool MarkSurfaceSetupStatic(ref CModel.SModel _SModel)
        {
            List<int> indices;

            if (_SModel.MarkSurfaceListStatic == null)
                return false;

            if (_SModel.MarkSurfaceListStatic.Count == 0)
                return false;


            // build lightmap/texture/surface chains
            _SModel.lSChainLightmap = new List<SChainLightmap>();
            for (int i = 0; i < _SModel.MarkSurfaceListStatic.Count; i++)
            {
                int idxLightmap = -1;
                int idxTexture = -1;
                int idxSurface = -1;


                // check for lightmap modification
                gCLight.LightmapAnimation(_SModel.MarkSurfaceListStatic[i].surf);


                // generate lightmap list
                for (int j = 0; j < _SModel.lSChainLightmap.Count; j++)
                {
                    if (_SModel.lSChainLightmap[j].lightmaptexturenum == _SModel.MarkSurfaceListStatic[i].lightmaptexturenum)
                    {
                        idxLightmap = j;
                        break;
                    }
                }

                if (idxLightmap == -1)
                {
                    SChainLightmap _SCLM;
                    _SCLM.lightmaptexturenum = _SModel.MarkSurfaceListStatic[i].lightmaptexturenum;
                    _SCLM.TexInfo = new List<SChainTexture>();

                    _SModel.lSChainLightmap.Add(_SCLM);
                    idxLightmap = _SModel.lSChainLightmap.Count - 1;
                }


                // generate texinfo list
                SChainLightmap _SChainLightmap = _SModel.lSChainLightmap[idxLightmap];
                for (int j = 0; j < _SChainLightmap.TexInfo.Count; j++)
                {
                    if (_SChainLightmap.TexInfo[j].texinfo == _SModel.MarkSurfaceListStatic[i].texinfo)
                    {
                        idxTexture = j;
                        break;
                    }
                }

                if (idxTexture == -1)
                {
                    SChainTexture _SCT;
                    _SCT.texinfo = _SModel.MarkSurfaceListStatic[i].texinfo;
                    _SCT.surf = new List<int>();

                    _SChainLightmap.TexInfo.Add(_SCT);
                    idxTexture = _SChainLightmap.TexInfo.Count - 1;
                }
                _SModel.lSChainLightmap[idxLightmap] = _SChainLightmap;


                // generate surface list
                SChainTexture _SChainTexture = _SModel.lSChainLightmap[idxLightmap].TexInfo[idxTexture];
                for (int j = 0; j < _SChainTexture.surf.Count; j++)
                {
                    if (_SChainTexture.surf[j] == _SModel.MarkSurfaceListStatic[i].surf)
                    {
                        idxSurface = j;
                        break;
                    }
                }

                if (idxSurface == -1)
                {
                    _SChainTexture.surf.Add(_SModel.MarkSurfaceListStatic[i].surf);
                    idxSurface = _SChainTexture.surf.Count - 1;
                }

                _SModel.lSChainLightmap[idxLightmap].TexInfo[idxTexture] = _SChainTexture;
            }


            // build cluster-based index buffer
            indices = new List<int>();
            for (int i = 0; i < _SModel.lSChainLightmap.Count; i++)
            {
                for (int j = 0; j < _SModel.lSChainLightmap[i].TexInfo.Count; j++)
                {
                    int[] idx1;
                    int[] idx2;
                    int lm = _SModel.lSChainLightmap[i].lightmaptexturenum;
                    int tx = _SModel.lSChainLightmap[i].TexInfo[j].texinfo;
                    bool isEqual = true;

                    indices.Clear();

                    for (int k = 0; k < _SModel.lSChainLightmap[i].TexInfo[j].surf.Count; k++)
                    {
                        int[] idx = gCSurface.BuildSurfaceIndex(CQ2BSP.SWorldData.surfaces[_SModel.lSChainLightmap[i].TexInfo[j].surf[k]]);

                        if (idx == null)
                            continue;

                        for (int l = 0; l < idx.Length; l++)
                        {
                            indices.Add(idx[l]);
                        }
                    }

                    idx1 = indices.ToArray();
                    idx2 = _SModel.ibWorldSolid[lm, tx].ibIndices;

                    if (idx2 != null)
                    {
                        if (idx1.Length == idx2.Length)
                        {
                            for (int k = 0; k < idx1.Length; k++)
                            {
                                if (idx1[k] != idx2[k])
                                {
                                    isEqual = false;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            isEqual = false;
                        }
                    }
                    else
                    {
                        isEqual = false;
                    }


                    // re-build the index buffer the indices doesn't match
                    if(isEqual == false)
                    {
                        _SModel.ibWorldSolid[lm, tx].ibIndices = new int[idx1.Length];
                        for (int k = 0; k < idx1.Length; k++)
                        {
                            _SModel.ibWorldSolid[lm, tx].ibIndices[k] = idx1[k];
                        }
                        _SModel.ibWorldSolid[lm, tx].PrimitiveCount = _SModel.ibWorldSolid[lm, tx].ibIndices.Length / 3;

                        if (_SModel.ibWorldSolid[lm, tx].ibBuffer.IndexCount < idx1.Length)
                        {
                            _SModel.ibWorldSolid[lm, tx].ibBuffer.Dispose();
                            _SModel.ibWorldSolid[lm, tx].ibBuffer = null;

                            if (CProgram.gQ2Game.gGraphicsDevice.GraphicsProfile == GraphicsProfile.HiDef)
                                _SModel.ibWorldSolid[lm, tx].ibBuffer = new IndexBuffer(CProgram.gQ2Game.gGraphicsDevice, IndexElementSize.ThirtyTwoBits, idx1.Length, BufferUsage.WriteOnly);
                            else
                                _SModel.ibWorldSolid[lm, tx].ibBuffer = new IndexBuffer(CProgram.gQ2Game.gGraphicsDevice, IndexElementSize.SixteenBits, idx1.Length, BufferUsage.WriteOnly);
                        }

                        if (CProgram.gQ2Game.gGraphicsDevice.GraphicsProfile == GraphicsProfile.HiDef)
                        {
                            _SModel.ibWorldSolid[lm, tx].ibBuffer.SetData(_SModel.ibWorldSolid[lm, tx].ibIndices);
                        }
                        else
                        {
                            short[] ibData16 = new short[_SModel.ibWorldSolid[lm, tx].ibIndices.Length];

                            for (int k = 0; k < _SModel.ibWorldSolid[lm, tx].ibIndices.Length; k++)
                                ibData16[k] = (short)_SModel.ibWorldSolid[lm, tx].ibIndices[k];

                            _SModel.ibWorldSolid[lm, tx].ibBuffer.SetData(ibData16);
                        }
                    }
                }
            }

            indices.Clear();
            indices = null;

            return true;
        }

        public bool MarkSurfaceSetupDynamic(ref CModel.SModel _SModel)
        {
            if (_SModel.MarkSurfaceListDynamic == null)
                return false;

            if (_SModel.MarkSurfaceListDynamic.Count == 0)
                return false;

            return true;
        }

        public int TextureAnimation(int TexInfo)
        {
            int c;

            if (CQ2BSP.SWorldData.texinfo[TexInfo].next == 0)
                return TexInfo;

            c = (int)((gTimeRealTotal / 200.0d) % (double)CQ2BSP.SWorldData.texinfo[TexInfo].numframes);
            while (c != 0)
            {
                TexInfo = CQ2BSP.SWorldData.texinfo[TexInfo].next;
                c--;
            }

            return TexInfo;
        }

        public void UpdateHLSL(int surf)
        {
            if (surf < 0)
            {
                // update global HLSL variables
                CProgram.gQ2Game.gEffect.Parameters["xWorld"].SetValue(Matrix.Identity);

                if (gSGlobal.HLSL.xViewMatrix != gSGlobal.OldHLSL.xViewMatrix)
                {
                    CProgram.gQ2Game.gEffect.Parameters["xView"].SetValue(gSGlobal.HLSL.xViewMatrix);
                    gSGlobal.OldHLSL.xViewMatrix = gSGlobal.HLSL.xViewMatrix;
                }

                if (gSGlobal.HLSL.xProjectionMatrix != gSGlobal.OldHLSL.xProjectionMatrix)
                {
                    CProgram.gQ2Game.gEffect.Parameters["xProjection"].SetValue(gSGlobal.HLSL.xProjectionMatrix);
                    gSGlobal.OldHLSL.xProjectionMatrix = gSGlobal.HLSL.xProjectionMatrix;
                }

                if (gSGlobal.HLSL.xLightModel != gSGlobal.OldHLSL.xLightModel)
                {
                    CProgram.gQ2Game.gEffect.Parameters["xLightModel"].SetValue(gSGlobal.HLSL.xLightModel.ToVector4());
                    gSGlobal.OldHLSL.xLightModel = gSGlobal.HLSL.xLightModel;
                }

                if (gSGlobal.HLSL.xLightAmbient != gSGlobal.OldHLSL.xLightAmbient)
                {
                    CProgram.gQ2Game.gEffect.Parameters["xLightAmbient"].SetValue(gSGlobal.HLSL.xLightAmbient.ToVector4());
                    gSGlobal.OldHLSL.xLightAmbient = gSGlobal.HLSL.xLightAmbient;
                }

                if (gSGlobal.HLSL.xLightPower != gSGlobal.OldHLSL.xLightPower)
                {
                    CProgram.gQ2Game.gEffect.Parameters["xLightPower"].SetValue(gSGlobal.HLSL.xLightPower);
                    gSGlobal.OldHLSL.xLightPower = gSGlobal.HLSL.xLightPower;
                }

                if (gSGlobal.HLSL.xGamma != gSGlobal.OldHLSL.xGamma)
                {
                    CProgram.gQ2Game.gEffect.Parameters["xGamma"].SetValue(gSGlobal.HLSL.xGamma);
                    gSGlobal.OldHLSL.xGamma = gSGlobal.HLSL.xGamma;
                }

                CProgram.gQ2Game.gEffect.Parameters["xRealTime"].SetValue((float)gTimeGame.TotalGameTime.TotalMilliseconds / 1000.0f);

                if (gSGlobal.HLSL.xBloomThreshold != gSGlobal.OldHLSL.xBloomThreshold)
                {
                    CProgram.gQ2Game.gEffect.Parameters["xBloomThreshold"].SetValue(gSGlobal.HLSL.xBloomThreshold);
                    gSGlobal.OldHLSL.xBloomThreshold = gSGlobal.HLSL.xBloomThreshold;
                }

                if (gSGlobal.HLSL.xBaseIntensity != gSGlobal.OldHLSL.xBaseIntensity)
                {
                    CProgram.gQ2Game.gEffect.Parameters["xBaseIntensity"].SetValue(gSGlobal.HLSL.xBaseIntensity);
                    gSGlobal.OldHLSL.xBaseIntensity = gSGlobal.HLSL.xBaseIntensity;
                }

                if (gSGlobal.HLSL.xBloomIntensity != gSGlobal.OldHLSL.xBloomIntensity)
                {
                    CProgram.gQ2Game.gEffect.Parameters["xBloomIntensity"].SetValue(gSGlobal.HLSL.xBloomIntensity);
                    gSGlobal.OldHLSL.xBloomIntensity = gSGlobal.HLSL.xBloomIntensity;
                }

                if (gSGlobal.HLSL.xBaseSaturation != gSGlobal.OldHLSL.xBaseSaturation)
                {
                    CProgram.gQ2Game.gEffect.Parameters["xBaseSaturation"].SetValue(gSGlobal.HLSL.xBaseSaturation);
                    gSGlobal.OldHLSL.xBaseSaturation = gSGlobal.HLSL.xBaseSaturation;
                }

                if (gSGlobal.HLSL.xBloomSaturation != gSGlobal.OldHLSL.xBloomSaturation)
                {
                    CProgram.gQ2Game.gEffect.Parameters["xBloomSaturation"].SetValue(gSGlobal.HLSL.xBloomSaturation);
                    gSGlobal.OldHLSL.xBloomSaturation = gSGlobal.HLSL.xBloomSaturation;
                }

                if (gSGlobal.HLSL.xPointLights != gSGlobal.OldHLSL.xPointLights)
                {
                    CProgram.gQ2Game.gEffect.Parameters["xPointLights"].SetValue(gSGlobal.HLSL.xPointLights);
                    gSGlobal.OldHLSL.xPointLights = gSGlobal.HLSL.xPointLights;
                }
            }
            else
            {
                // update surface HLSL variables

                // translucency
                if ((CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_TRANS33) == CQ2BSP.ESurface.SURF_TRANS33)
                    gSGlobal.HLSL.xTextureAlpha = 0.33f;
                else if ((CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_TRANS66) == CQ2BSP.ESurface.SURF_TRANS66)
                    gSGlobal.HLSL.xTextureAlpha = 0.66f;
                else
                    gSGlobal.HLSL.xTextureAlpha = 1.0f;

                if (gSGlobal.HLSL.xTextureAlpha != gSGlobal.OldHLSL.xTextureAlpha)
                {
                    CProgram.gQ2Game.gEffect.Parameters["xTextureAlpha"].SetValue(gSGlobal.HLSL.xTextureAlpha);
                    gSGlobal.OldHLSL.xTextureAlpha = gSGlobal.HLSL.xTextureAlpha;
                }

                // flowing
                if ((CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_FLOWING) == CQ2BSP.ESurface.SURF_FLOWING)
                    gSGlobal.HLSL.xFlow = true;
                else
                    gSGlobal.HLSL.xFlow = false;

                if (gSGlobal.HLSL.xFlow != gSGlobal.OldHLSL.xFlow)
                {
                    CProgram.gQ2Game.gEffect.Parameters["xFlow"].SetValue(gSGlobal.HLSL.xFlow);
                    gSGlobal.OldHLSL.xFlow = gSGlobal.HLSL.xFlow;
                }
            }
        }

        /// <summary>
        /// DrawMarkedSurfaces
        /// ------------------
        /// Lightmapped surfaces always have only one set of polys per surface
        /// </summary>
        private void DrawMarkedSurfacesStatic()
        {
            EffectParameter EP;

            // update HLSL variables
            UpdateHLSL(-1);

            // sort textures and surfaces
            if (MarkSurfaceSetupStatic(ref CQ2BSP.SWorldData) == false)
                return;

            // set vertex buffer
            CProgram.gQ2Game.gGraphicsDevice.SetVertexBuffer(CQ2BSP.SWorldData.vbWorldSolid);

            // set a rendering technique
            if (CProgram.gQ2Game.gCMain.r_hardwarelight == false)
                CProgram.gQ2Game.gEffect.CurrentTechnique = CProgram.gQ2Game.gEffect.Techniques["TexturedLightmap"];
            else
                CProgram.gQ2Game.gEffect.CurrentTechnique = CProgram.gQ2Game.gEffect.Techniques["TexturedLight"];

            // save effect parameter collection shortcut
            EP = CProgram.gQ2Game.gEffect.Parameters["lights"];

            for (int i = 0; i < CQ2BSP.SWorldData.MarkSurfaceListStatic.Count; i++)
            {
                // update surface HLSL variables
                UpdateHLSL(CQ2BSP.SWorldData.MarkSurfaceListStatic[i].surf);

                // set lights for current surface
                for (int k = 0; k < r_maxLights; k++)
                {
                    CQ2BSP.SWorldData.lights[CQ2BSP.SWorldData.surfaces[CQ2BSP.SWorldData.MarkSurfaceListStatic[i].surf].lightIndex[k]].SetLight(EP.Elements[k]);
                }
            }

            for (int i = 0; i < CQ2BSP.SWorldData.lSChainLightmap.Count; i++)
            {
                // bind new lightmap
                int lightmaptexturenum = CQ2BSP.SWorldData.lSChainLightmap[i].lightmaptexturenum;
                CProgram.gQ2Game.gEffect.Parameters["xTextureLightmap"].SetValue(CQ2BSP.SWorldData.WorldLightmaps[lightmaptexturenum]);

                for (int j = 0; j < CQ2BSP.SWorldData.lSChainLightmap[i].TexInfo.Count; j++)
                {
                    // bind new texture
                    int texanim = TextureAnimation(CQ2BSP.SWorldData.lSChainLightmap[i].TexInfo[j].texinfo);
                    int texinfo = CQ2BSP.SWorldData.lSChainLightmap[i].TexInfo[j].texinfo;
                    CProgram.gQ2Game.gEffect.Parameters["xTextureDiffuse"].SetValue(CQ2BSP.SWorldData.WorldTextures[CQ2BSP.SWorldData.texinfo[texanim].image].Tex2D);

                    // set the indices
                    CProgram.gQ2Game.gGraphicsDevice.Indices = CQ2BSP.SWorldData.ibWorldSolid[lightmaptexturenum, texinfo].ibBuffer;

                    foreach (EffectPass pass in CProgram.gQ2Game.gEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        CProgram.gQ2Game.gGraphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            0,
                            0,
                            CQ2BSP.SWorldData.vbWorldSolid.VertexCount,
                            0,
                            CQ2BSP.SWorldData.ibWorldSolid[lightmaptexturenum, texinfo].PrimitiveCount);
                    }
                    c_brush_polys += CQ2BSP.SWorldData.ibWorldSolid[lightmaptexturenum, texinfo].PrimitiveCount;
                }
            }
        }

        /// <summary>
        /// DrawMarkedSurfacesTransWarp
        /// ---------------------------
        /// Translucent (non-warp) surfaces always have only one set of polys per surface
        /// Warped (liquid) surfaces usually have multiple sets of polys per surface
        /// </summary>
        private void DrawMarkedSurfacesDynamic()
        {
            EffectTechnique CurrentTechnique = null;
            int lightmaptexturenum = -1;
            int texinfo = -1;

            // update HLSL variables (only if we have drawn entities because we updated the matrix)
            if (CProgram.gQ2Game.gCMain.r_drawentities == true)
                UpdateHLSL(-1);

            if (MarkSurfaceSetupDynamic(ref CQ2BSP.SWorldData) == false)
                return;

            // set vertex buffer
            CProgram.gQ2Game.gGraphicsDevice.SetVertexBuffer(CQ2BSP.SWorldData.vbWorldSolid);

            for (int i = 0; i < CQ2BSP.SWorldData.MarkSurfaceListDynamic.Count; i++)
            {
                EffectTechnique CurrentTechnique2;
                int PrimitiveCount;
                int texinfo2;
                int lightmaptexturenum2;
                int surf = CQ2BSP.SWorldData.MarkSurfaceListDynamic[i].surf;

                if (CQ2BSP.SWorldData.surfaces[surf].ibData == null)
                    continue;

                if (CQ2BSP.SWorldData.surfaces[surf].ibData.Length < 3)
                    continue;

                // update surface HLSL variables
                UpdateHLSL(surf);

                PrimitiveCount = CQ2BSP.SWorldData.surfaces[surf].ibData.Length / 3;

                // set a rendering technique
                if (
                    (CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_TRANS33) == CQ2BSP.ESurface.SURF_TRANS33
                    | (CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_TRANS66) == CQ2BSP.ESurface.SURF_TRANS66
                    )
                {
                    if ((CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_WARP) == CQ2BSP.ESurface.SURF_WARP)
                        CurrentTechnique2 = CProgram.gQ2Game.gEffect.Techniques["TexturedWarpedTranslucent"];
                    else
                        CurrentTechnique2 = CProgram.gQ2Game.gEffect.Techniques["TexturedTranslucent"];
                }
                else
                {
                    if ((CQ2BSP.SWorldData.texinfo[CQ2BSP.SWorldData.surfaces[surf].texinfo].flags & CQ2BSP.ESurface.SURF_WARP) == CQ2BSP.ESurface.SURF_WARP)
                    {
                        CurrentTechnique2 = CProgram.gQ2Game.gEffect.Techniques["TexturedWarped"];
                    }
                    else
                    {
                        if (CProgram.gQ2Game.gCMain.r_hardwarelight == false)
                            CurrentTechnique2 = CProgram.gQ2Game.gEffect.Techniques["TexturedLightmap"];
                        else
                            CurrentTechnique2 = CProgram.gQ2Game.gEffect.Techniques["TexturedLight"];
                    }
                }

                if (CurrentTechnique != CurrentTechnique2)
                {
                    CProgram.gQ2Game.gEffect.CurrentTechnique = CurrentTechnique2;
                    CurrentTechnique = CurrentTechnique2;
                }

                // bind new texture
                texinfo2 = TextureAnimation(CQ2BSP.SWorldData.MarkSurfaceListDynamic[i].texinfo);
                if (texinfo != texinfo2)
                {
                    CProgram.gQ2Game.gEffect.Parameters["xTextureDiffuse"].SetValue(CQ2BSP.SWorldData.WorldTextures[CQ2BSP.SWorldData.texinfo[texinfo2].image].Tex2D);
                    texinfo = texinfo2;
                }

                // bind new lightmap
                lightmaptexturenum2 = CQ2BSP.SWorldData.surfaces[surf].lightmaptexturenum;
                if (lightmaptexturenum != lightmaptexturenum2)
                {
                    CProgram.gQ2Game.gEffect.Parameters["xTextureLightmap"].SetValue(CQ2BSP.SWorldData.WorldLightmaps[lightmaptexturenum2]);
                    lightmaptexturenum = lightmaptexturenum2;
                }


                // set the indices
                CProgram.gQ2Game.gGraphicsDevice.Indices = CQ2BSP.SWorldData.surfaces[surf].ibSurface;

                foreach (EffectPass pass in CProgram.gQ2Game.gEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    CProgram.gQ2Game.gGraphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        0,
                        0,
                        CQ2BSP.SWorldData.vbWorldSolid.VertexCount,
                        0,
                        PrimitiveCount);
                }
                c_brush_polys += PrimitiveCount;
            }
        }

        private void DrawEntitiesOnList()
        {
            CModel.SEntities _CurrentEntity;

            if (r_drawentities == false)
                return;

            if (CQ2BSP.SWorldData.entities == null)
                return;

            if (CQ2BSP.SWorldData.entities.Length == 0)
                return;
            
            
            // draw non-translucent first
            for (int i = 0; i < CQ2BSP.SWorldData.entities.Length; i++)
            {

                // HACK - Added some animation for demo purposes
                if (CQ2BSP.SWorldData.entities[i].ModelFrameSeqStart != CQ2BSP.SWorldData.entities[i].ModelFrameSeqEnd)
                {
                    if (CQ2BSP.SWorldData.entities[i].Model.ModType == CModel.EModType.MOD_ALIAS)
                    {
                        int numframes = CQ2BSP.SWorldData.entities[i].ModelFrameSeqEnd - CQ2BSP.SWorldData.entities[i].ModelFrameSeqStart;
                        int curframe;

                        if (numframes <= 0)
                            continue;

                        curframe = (int)((gTimeRealTotal / 100.0d) % numframes);

                        if (CQ2BSP.SWorldData.entities[i].ModelFrameOffset + curframe > numframes)
                        {
                            int newframe = (CQ2BSP.SWorldData.entities[i].ModelFrameOffset + curframe) - numframes;
                            CQ2BSP.SWorldData.entities[i].ModelFrameCurrent = newframe;
                        }
                        else
                        {
                            CQ2BSP.SWorldData.entities[i].ModelFrameCurrent = CQ2BSP.SWorldData.entities[i].ModelFrameOffset + curframe;
                        }
                    }
                }
                // HACK - Added some animation for demo purposes


                _CurrentEntity = CQ2BSP.SWorldData.entities[i];

                if ((_CurrentEntity.Model.Flags & CQ2MD2.EModelFlags.RF_TRANSLUCENT) == CQ2MD2.EModelFlags.RF_TRANSLUCENT)
                    continue; // translucent

                //if (!currentmodel)
                //{
                //    R_DrawNullModel();
                //    continue;
                //}

                switch (_CurrentEntity.Model.ModType)
                {
                    case CModel.EModType.MOD_ALIAS:
                        gCQ2MD2.R_DrawAliasModel(_CurrentEntity);
                        break;
                    case CModel.EModType.MOD_BRUSH:
                        gCQ2BSP.R_DrawBrushModel(_CurrentEntity);
                        break;
                    case CModel.EModType.MOD_SPRITE:
                        //R_DrawSpriteModel(currententity);
                        break;

                    //default:
                    //    System.Diagnostics.Debug.WriteLine("bad model type.");
                    //    break;
                }
            }


            // draw transparent entities
            // we could sort these if it ever becomes a problem...
            //qglDepthMask(0); // no z writes
            for (int i = 0; i < CQ2BSP.SWorldData.entities.Length; i++)
            {
                _CurrentEntity = CQ2BSP.SWorldData.entities[i];

                if ((_CurrentEntity.Model.Flags & CQ2MD2.EModelFlags.RF_TRANSLUCENT) != CQ2MD2.EModelFlags.RF_TRANSLUCENT)
                    continue; // solid

                //if (!currentmodel)
                //{
                //    R_DrawNullModel();
                //    continue;
                //}

                switch (_CurrentEntity.Model.ModType)
                {
                    case CModel.EModType.MOD_ALIAS:
                        gCQ2MD2.R_DrawAliasModel(_CurrentEntity);
                        break;
                    case CModel.EModType.MOD_BRUSH:
                        gCQ2BSP.R_DrawBrushModel(_CurrentEntity);
                        break;
                    case CModel.EModType.MOD_SPRITE:
                        //R_DrawSpriteModel(currententity);
                        break;

                    //default:
                    //    System.Diagnostics.Debug.WriteLine("bad model type.");
                    //    break;
                }
            }
            //qglDepthMask (1); // back to writing
        }

        private void DrawWorld()
        {
            if (CQ2BSP.SWorldData.MarkSurfaceListStatic != null)
                CQ2BSP.SWorldData.MarkSurfaceListStatic.Clear();

            if (CQ2BSP.SWorldData.MarkSurfaceListDynamic != null)
                CQ2BSP.SWorldData.MarkSurfaceListDynamic.Clear();

            ModelOrigin = CClient.cl.RefDef.ViewOrigin;
            ModelAngles = CClient.cl.RefDef.ViewAngles;
            RecursiveWorldNode(0);

            gCSky.DrawSkyBox();

            DrawMarkedSurfacesStatic();

            DrawEntitiesOnList();

            DrawMarkedSurfacesDynamic();
        }

        public void FrameRenderView()
        {
            SetupFrame();
            SetFrustum();
            MarkLeaves();
            DrawWorld();

            CalculateFrameRate();
        }
        #endregion




        // parameters to the main Error routine
        public enum EErrorParm
        {
            ERR_FATAL,					// exit the entire application with a popup window
            ERR_WARNING				    // print to console and notify
        }



        public struct SFrameRate
        {
            public TimeSpan TimeElapsed;
            public int FrameCount;
            public int FrameRate;
        }


        // ================================================================
        // 
        // TEXTURE CHAINING
        // 
        // ================================================================

        public struct SChainLightmap
        {
            public int lightmaptexturenum;
            public List<SChainTexture> TexInfo;
        }

        public struct SChainTexture
        {
            public int texinfo;
            public List<int> surf;
        }

        public struct STextureStatic
        {
            public int lightmaptexturenum;
            public int texinfo;
            public int surf;
        }

        public struct STextureDynamic
        {
            public int texinfo;
            public int surf;
        }

    }
}
