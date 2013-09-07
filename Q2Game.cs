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
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Q2BSP
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class CQ2Game : Microsoft.Xna.Framework.Game
    {
        public GraphicsDeviceManager gGraphicsDeviceManager;
        public GraphicsDevice gGraphicsDevice;
        public Effect gEffect;
        public SpriteBatch spriteBatch;

        public CMain gCMain;
        public CCommon gCCommon;
        public CGamma gCGamma;

        // temporary text drawing
        SpriteFont Font1;
        Vector2 FontPos;

        /// <summary>
        /// CQ2Game
        /// ----------
        /// The game constructor
        /// </summary>
        public CQ2Game()
        {
            gGraphicsDeviceManager = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
        }

        private void DrawOverlayText(Vector2 Origin, string Text, Color TextColor)
        {
            spriteBatch.Begin();

            Origin.X -= 1;
            Origin.Y -= 1;
            spriteBatch.DrawString(Font1, Text, FontPos, Color.Black, 0, Origin, 1.0f, SpriteEffects.None, 0.5f);

            Origin.X += 1;
            Origin.Y += 1;
            spriteBatch.DrawString(Font1, Text, FontPos, TextColor, 0, Origin, 1.0f, SpriteEffects.None, 0.5f);

            spriteBatch.End();
        }


        // =====================================================================
        //
        // XNA SPECIFIC FUNCTIONS
        // 
        // =====================================================================

        /// <summary>
        /// Initialize
        /// ----------
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Window.Title = CMain.GameTitle;
            gGraphicsDeviceManager.PreferMultiSampling = true;
            gGraphicsDeviceManager.IsFullScreen = CConfig.GetConfigBOOL("Fullscreen");

            if (gGraphicsDeviceManager.IsFullScreen == true)
            {
                int Width;
                int Height;

                try
                {
                    Width = Convert.ToInt32(CConfig.GetConfigSTRING("Width"));
                    Height = Convert.ToInt32(CConfig.GetConfigSTRING("Height"));
                }
                catch
                {
                    Width = 1024;
                    Height = 768;
                }

                gGraphicsDeviceManager.PreferredBackBufferWidth = Width;
                gGraphicsDeviceManager.PreferredBackBufferHeight = Height;
            }
            else
            {
                gGraphicsDeviceManager.PreferredBackBufferWidth = 1024;
                gGraphicsDeviceManager.PreferredBackBufferHeight = 768;
            }

            if (CConfig.GetConfigBOOL("Vertical Sync") == true)
            {
                gGraphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
                gGraphicsDeviceManager.GraphicsDevice.PresentationParameters.PresentationInterval = PresentInterval.Default;
            }
            else
            {
                gGraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
                gGraphicsDeviceManager.GraphicsDevice.PresentationParameters.PresentationInterval = PresentInterval.Immediate;

                TargetElapsedTime = TimeSpan.FromMilliseconds(1);
            }
            IsFixedTimeStep = true;

            gGraphicsDeviceManager.ApplyChanges();
            gGraphicsDevice = gGraphicsDeviceManager.GraphicsDevice;

            gCMain = new CMain();
            gCCommon = new CCommon();
            gCGamma = new CGamma();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent
        /// -----------
        /// Will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            if (CProgram.gQ2Game.gGraphicsDevice.GraphicsProfile == GraphicsProfile.HiDef)
            {
                //gCMain.r_maxLights = 8;
                //gEffect = Content.Load<Effect>("effects30");

                // "Cannot mix shader model 3.0 with earlier shader models. If either the vertex shader or pixel shader is compiled as 3.0, they must both be."
                // FIXME: Not sure how to get tis fixed... using SM2 instead
                gCMain.r_maxLights = 2;
                gEffect = Content.Load<Effect>("effects20");
            }
            else
            {
                gCMain.r_maxLights = 2;
                gEffect = Content.Load<Effect>("effects20");
            }

            Font1 = Content.Load<SpriteFont>("SpriteFont1");
            FontPos = new Vector2(5, 5);

            gCMain.gCBloom.Setup();

            gCMain.BuildWorldModel(CConfig.GetConfigSTRING("Map Name"));
            gCMain.WorldViewInit();
        }

        /// <summary>
        /// UnloadContent
        /// -------------
        /// Will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            gCMain.gCBloom.Release();
        }

        /// <summary>
        /// Update
        /// ------
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            gCMain.gTimeGame = gameTime;

            gCMain.FrameUpdate();
            gCCommon.Qcommon_Frame(1);

            base.Update(gameTime);
        }

        /// <summary>
        /// Draw
        /// ----
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Vector2 FontOrigin;
            gCMain.gTimeGame = gameTime;

            gCMain.gCBloom.DrawBloom();

            gGraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);


            // reset and set the rasterizer states
            CProgram.gQ2Game.gCMain.gRasterizerState = new RasterizerState();
            CProgram.gQ2Game.gCMain.gRasterizerState.CullMode = CullMode.CullCounterClockwiseFace;

            if (CProgram.gQ2Game.gCMain.r_wireframe == false)
                CProgram.gQ2Game.gCMain.gRasterizerState.FillMode = FillMode.Solid;
            else
                CProgram.gQ2Game.gCMain.gRasterizerState.FillMode = FillMode.WireFrame;

            gGraphicsDevice.RasterizerState = CProgram.gQ2Game.gCMain.gRasterizerState;
            
            
            // reset the blend and depth states
            gGraphicsDevice.BlendState = BlendState.Opaque;


            // reset and set the depthstencil states
            CProgram.gQ2Game.gCMain.gDepthStencilState = new DepthStencilState();
            CProgram.gQ2Game.gCMain.gDepthStencilState.DepthBufferEnable = true;
            gGraphicsDevice.DepthStencilState = CProgram.gQ2Game.gCMain.gDepthStencilState;


            gCMain.FrameRenderView();
            gCMain.gCBloom.Draw();


            // we are drawing the overlay text here, because we don't want it to be affected by postprocessing

            // reset and set the rasterizer states
            CProgram.gQ2Game.gCMain.gRasterizerState = new RasterizerState();
            CProgram.gQ2Game.gCMain.gRasterizerState.FillMode = FillMode.Solid;
            gGraphicsDevice.RasterizerState = CProgram.gQ2Game.gCMain.gRasterizerState;


            FontOrigin.X = 0;
            FontOrigin.Y = 0;
            DrawOverlayText(FontOrigin, CClient.cl.RefDef.MapName, Color.White);

            FontOrigin.X = 0;
            FontOrigin.Y = -40;
            DrawOverlayText(FontOrigin, "FPS Rate: " + gCMain.FrameRate.FrameRate.ToString(), Color.Silver);

            FontOrigin.X = 0;
            FontOrigin.Y = -60;
            DrawOverlayText(FontOrigin, "PVS Cluster: " + CMain.r_viewcluster.ToString(), Color.Silver);

            FontOrigin.X = 0;
            FontOrigin.Y = -80;
            DrawOverlayText(FontOrigin, "PVS Locked: " + gCMain.r_lockpvs.ToString(), Color.Silver);

            FontOrigin.X = 0;
            FontOrigin.Y = -100;
            DrawOverlayText(FontOrigin, "Bloom: " + gCMain.r_bloom.ToString(), Color.Silver);

            FontOrigin.X = 0;
            FontOrigin.Y = -120;
            DrawOverlayText(FontOrigin, "Primitives: " + CMain.c_brush_polys.ToString(), Color.Silver);

            FontOrigin.X = 0;
            FontOrigin.Y = -140;
            DrawOverlayText(FontOrigin, "Origin: (XYZ) " + Convert.ToInt64(CClient.cl.RefDef.ViewOrigin.X).ToString() + ", " + Convert.ToInt64(CClient.cl.RefDef.ViewOrigin.Y).ToString() + ", " + Convert.ToInt64(CClient.cl.RefDef.ViewOrigin.Z).ToString(), Color.Silver);

            FontOrigin.X = 0;
            FontOrigin.Y = -160;
            DrawOverlayText(FontOrigin, "View: (XYZ) " + Convert.ToInt64(CClient.cl.RefDef.ViewAngles.X).ToString() + ", " + Convert.ToInt64(CClient.cl.RefDef.ViewAngles.Y).ToString() + ", " + Convert.ToInt64(CClient.cl.RefDef.ViewAngles.Z).ToString(), Color.Silver);

            FontOrigin.X = 0;
            FontOrigin.Y = -180;
            DrawOverlayText(FontOrigin, "Gamma: " + gCMain.gSGlobal.HLSL.xGamma.ToString(), Color.Silver);

            FontOrigin.X = 0;
            FontOrigin.Y = -220;
            DrawOverlayText(FontOrigin, "[SPACE] show/hide controls", Color.Gray);

            if (CProgram.gQ2Game.gCMain.r_controls == true)
            {
                FontOrigin.X = 0;
                FontOrigin.Y = -240;
                DrawOverlayText(FontOrigin, "[ARROWS] forward/back, roll", Color.Gray);

                FontOrigin.X = 0;
                FontOrigin.Y = -260;
                DrawOverlayText(FontOrigin, "[Q]/[A] up/down", Color.Gray);

                FontOrigin.X = 0;
                FontOrigin.Y = -280;
                DrawOverlayText(FontOrigin, "[Z]/[X] left/right", Color.Gray);

                FontOrigin.X = 0;
                FontOrigin.Y = -300;
                DrawOverlayText(FontOrigin, "[MOUSE] pitch/yaw", Color.Gray);

                FontOrigin.X = 0;
                FontOrigin.Y = -320;
                DrawOverlayText(FontOrigin, "[O] fill mode", Color.Gray);

                FontOrigin.X = 0;
                FontOrigin.Y = -340;
                DrawOverlayText(FontOrigin, "[+]/[-] gamma", Color.Gray);

                FontOrigin.X = 0;
                FontOrigin.Y = -360;
                DrawOverlayText(FontOrigin, "[P] pointlights on/off", Color.Gray);

                FontOrigin.X = 0;
                FontOrigin.Y = -380;
                DrawOverlayText(FontOrigin, "[H] lightmaps on/off", Color.Gray);

                FontOrigin.X = 0;
                FontOrigin.Y = -400;
                DrawOverlayText(FontOrigin, "[L] pvs lock on/off", Color.Gray);

                FontOrigin.X = 0;
                FontOrigin.Y = -420;
                DrawOverlayText(FontOrigin, "[B] bloom on/off", Color.Gray);

                FontOrigin.X = 0;
                FontOrigin.Y = -440;
                DrawOverlayText(FontOrigin, "[E] entities on/off", Color.Gray);
            }

            base.Draw(gameTime);
        }

    }
}
