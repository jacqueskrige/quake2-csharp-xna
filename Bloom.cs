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
    public class CBloom
    {
        public int BlurAmount;
        private SpriteBatch spriteBatch;

        private RenderTarget2D RenderTargetScene;
        private RenderTarget2D RenderTarget1;
        private RenderTarget2D RenderTarget2;

        /// <summary>
        /// Setup
        /// -----
        /// Create the render targets used by the bloom postprocess
        /// </summary>
        public void Setup()
        {
            int Width;
            int Height;
            SurfaceFormat Format;

            // get the resolution and format of our main backbuffer
            PresentationParameters pp = CProgram.gQ2Game.gGraphicsDevice.PresentationParameters;

            Width = pp.BackBufferWidth;
            Height = pp.BackBufferHeight;
            Format = pp.BackBufferFormat;

            // create a texture for rendering the main scene, prior to applying bloom.
            RenderTargetScene = new RenderTarget2D(CProgram.gQ2Game.gGraphicsDevice, Width, Height, false, Format, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);

            // create two render targets for bloom processing half the size of the backbuffer
            Width /= 2;
            Height /= 2;
            RenderTarget1 = new RenderTarget2D(CProgram.gQ2Game.gGraphicsDevice, Width, Height);
            RenderTarget2 = new RenderTarget2D(CProgram.gQ2Game.gGraphicsDevice, Width, Height);

            BlurAmount = 4;
            spriteBatch = new SpriteBatch(CProgram.gQ2Game.gGraphicsDevice);
        }

        /// <summary>
        /// DrawBloom
        /// ---------
        /// This should be called at the very start of the scene rendering. The bloom component uses it to redirect drawing into its custom rendertarget,
        /// so it can capture the scene image in preparation for applying the bloom filter.
        /// </summary>
        public void DrawBloom()
        {
            if (CProgram.gQ2Game.gCMain.r_bloom == false)
                return;

            if (CProgram.gQ2Game.gCMain.r_wireframe == true)
                return;

            CProgram.gQ2Game.gGraphicsDevice.SetRenderTarget(RenderTargetScene);
        }

        /// <summary>
        /// Draw
        /// ----
        /// Process and draw the bloom postprocess
        /// </summary>
        public void Draw()
        {
            if (CProgram.gQ2Game.gCMain.r_bloom == false)
                return;

            if (CProgram.gQ2Game.gCMain.r_wireframe == true)
                return;

            CProgram.gQ2Game.gGraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;


            // pass 1
            // draw the scene into render target 1, using a shader that extracts only the brightest pixels
            DrawQuad(RenderTargetScene, RenderTarget1, "BloomExtract", IntermediateBuffer.PreBloom);


            // pass 2
            // draw from render target 1 into render target 2, applying a horizontal gaussian blur filter
            SetBlurEffectParameters(1.0f / (float)RenderTarget1.Width, 0.0f);
            DrawQuad(RenderTarget1, RenderTarget2, "GaussianBlur", IntermediateBuffer.BlurredHorizontally);


            // pass 3
            // draw from render target 2 back into render target 1, applying a vertical gaussian blur filter
            SetBlurEffectParameters(0.0f, 1.0f / (float)RenderTarget1.Height);
            DrawQuad(RenderTarget2, RenderTarget1, "GaussianBlur", IntermediateBuffer.BlurredBothWays);


            // pass 4
            // draw both render target 1 and the original scene texture back into the backbuffer
            // using a shader that combines them to produce the final bloomed result
            CProgram.gQ2Game.gGraphicsDevice.SetRenderTarget(null);
            CProgram.gQ2Game.gGraphicsDevice.Textures[1] = RenderTargetScene;


            CProgram.gQ2Game.gGraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            DrawQuad(RenderTarget1, CProgram.gQ2Game.gGraphicsDevice.Viewport.Width, CProgram.gQ2Game.gGraphicsDevice.Viewport.Height, "BloomCombine", IntermediateBuffer.FinalResult);
        }

        private void DrawQuad(Texture2D texture, RenderTarget2D RenderTarget, string Technique, IntermediateBuffer currentBuffer)
        {
            CProgram.gQ2Game.gGraphicsDevice.SetRenderTarget(RenderTarget);
            DrawQuad(texture, RenderTarget.Width, RenderTarget.Height, Technique, currentBuffer);
            CProgram.gQ2Game.gGraphicsDevice.SetRenderTarget(null);
        }

        private void DrawQuad(Texture2D texture, int Width, int Height, string Technique, IntermediateBuffer currentBuffer)
        {
            // if the user has selected one of the show intermediate buffer options,  we still draw the quad to make sure the image will end up on the screen,
            // but might need to skip applying the custom pixel shader.
            if (IntermediateBuffer.FinalResult < currentBuffer)
                CProgram.gQ2Game.gEffect = null;
            else
                CProgram.gQ2Game.gEffect.CurrentTechnique = CProgram.gQ2Game.gEffect.Techniques[Technique];

            spriteBatch.Begin(0, BlendState.Opaque, null, null, null, CProgram.gQ2Game.gEffect);
            spriteBatch.Draw(texture, new Rectangle(0, 0, Width, Height), Color.White);
            spriteBatch.End();
        }

        /// <summary>
        /// SetBlurEffectParameters
        /// -----------------------
        /// Computes sample weightings and texture coordinate offsets
        /// for one pass of a separable gaussian blur filter
        /// </summary>
        private void SetBlurEffectParameters(float dx, float dy)
        {
            EffectParameter weightsParameter;
            EffectParameter offsetsParameter;
            int sampleCount;
            float[] sampleWeights;
            Vector2[] sampleOffsets;
            float totalWeights;

            // get the sample weight and offset effect parameters
            weightsParameter = CProgram.gQ2Game.gEffect.Parameters["SampleWeights"];
            offsetsParameter = CProgram.gQ2Game.gEffect.Parameters["SampleOffsets"];

            // get the amount of samples our gaussian blur effect supports
            sampleCount = weightsParameter.Elements.Count;

            // create temporary arrays for computing our filter settings
            sampleWeights = new float[sampleCount];
            sampleOffsets = new Vector2[sampleCount];

            // the first sample always has a zero offset
            sampleWeights[0] = ComputeGaussian(0, BlurAmount);
            sampleOffsets[0] = new Vector2(0);

            // maintain a sum of all the weighting values
            totalWeights = sampleWeights[0];

            // add pairs of additional sample taps, positioned along a line in both directions from the center
            for (int i = 0; i < sampleCount / 2; i++)
            {
                float weight;
                float sampleOffset;
                Vector2 delta;

                // store weights for the positive and negative taps
                weight = ComputeGaussian(i + 1, BlurAmount);
                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;
                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of pixel shader samples,
                // we take advantage of the bilinear filtering hardware inside the texture fetch unit.
                // If we position our texture coordinates exactly halfway between two texels, the filtering
                // unit will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather than just one at a time.
                // The 1.5 offset kicks things off by positioning us nicely in between two texels.
                sampleOffset = i * 2 + 1.5f;

                delta = new Vector2(dx, dy) * sampleOffset;

                // store texture coordinate offsets for the positive and negative taps
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // normalize the list of sample weightings, so they will always sum to one
            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            // tell the effect about our new filter settings
            weightsParameter.SetValue(sampleWeights);
            offsetsParameter.SetValue(sampleOffsets);
        }

        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        private float ComputeGaussian(float n, float theta)
        {
            return (float)((1.0d / Math.Sqrt(2 * Math.PI * theta)) * Math.Exp(-(n * n) / (2 * theta * theta)));
        }

        /// <summary>
        /// Release
        /// -------
        /// De-allocate the render targets used by the bloom postprocess
        /// </summary>
        public void Release()
        {
            if (RenderTargetScene != null)
                RenderTargetScene.Dispose();

            if (RenderTarget1 != null)
                RenderTarget1.Dispose();

            if (RenderTarget2 != null)
                RenderTarget2.Dispose();
        }


        public enum IntermediateBuffer
        {
            PreBloom,
            BlurredHorizontally,
            BlurredBothWays,
            FinalResult,
        }

    }
}
