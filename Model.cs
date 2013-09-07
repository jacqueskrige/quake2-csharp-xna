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
using System.IO;
using System.Text;

namespace Q2BSP
{
    public class CModel
    {
        private const float COLINEAR_EPSILON = 0.001f;
        private const int MAX_KEY = 32;
        private const int MAX_VALUE = 1024;

        private Q2BSP.Library.CScript vScript;
        private byte[] mod_novis; // size: [MAX_MAP_LEAFS/8]


        public const int MAX_MOD_KNOWN = 512;
        public SModel[] mod_known; // MAX_MOD_KNOWN
        public int mod_numknown;

        // the inline * models from the current map are kept seperate
        public SModel[] mod_inline; // MAX_MOD_KNOWN

        public SModel NullModel;
        public int registration_sequence;
        
        public List<SEntities> Entities;

        public CModel()
        {
            mod_novis = new byte[CQ2BSP.MAX_MAP_LEAFS / 8];

            for (int i = 0; i < (CQ2BSP.MAX_MAP_LEAFS / 8); i++)
            {
                mod_novis[i] = 0xff;
            }

            mod_numknown = 1; // slot 0 is always reserved for the worldmodel
            mod_known = new SModel[MAX_MOD_KNOWN];
            mod_inline = new SModel[MAX_MOD_KNOWN];

            for (int i = 0; i < MAX_MOD_KNOWN; i++)
            {
                mod_known[i] = new SModel();
                mod_inline[i] = new SModel();
            }

            NullModel = new SModel();
            CQ2BSP.SWorldData = new SModel();
        }

        public static void CalcSurfaceExtents(ref SModel _SModel, ref SMSurface mface)
        {
            float[] mins;
            float[] maxs;
            int[] bmins;
            int[] bmaxs;
            float val;
            SMVertex v;
            SMTexInfo tex;

            mface.texturemins = new short[2];
            mface.extents = new short[2];

            mins = new float[2];
            maxs = new float[2];

            bmins = new int[2];
            bmaxs = new int[2];

            mins[0] = mins[1] = 999999;
            maxs[0] = maxs[1] = -999999;

            tex = _SModel.texinfo[mface.texinfo];

            for (int i = 0; i < mface.numedges; i++)
            {
                int e = _SModel.surfedges[mface.firstedge + i];

                if (e >= 0)
                    v = _SModel.vertexes[_SModel.edges[e].v[0]];
                else
                    v = _SModel.vertexes[_SModel.edges[-e].v[1]];

                for (int j = 0; j < 2; j++)
                {
                    Vector3 vec;
                    vec.X = tex.vecs[j].X;
                    vec.Y = tex.vecs[j].Y;
                    vec.Z = tex.vecs[j].Z;

                    val = Vector3.Dot(v.Position, vec) + tex.vecs[j].W;
                    
                    if (val < mins[j])
                        mins[j] = val;
                    if (val > maxs[j])
                        maxs[j] = val;
                }
            }

            for (int i = 0; i < 2; i++)
            {
                bmins[i] = (int)Math.Floor(mins[i] / 16);
                bmaxs[i] = (int)Math.Ceiling(maxs[i] / 16);

                mface.texturemins[i] = (short)(bmins[i] * 16);
                mface.extents[i] = (short)((bmaxs[i] - bmins[i]) * 16);
            }
        }

        private static WorldVertexFormat[] SkipCoLinearPoints(List<WorldVertexFormat> verts)
        {
            int numverts;
            List<WorldVertexFormat> _verts;

            if (verts == null)
                return null;

            if (verts.Count < 3)
                return null;

            numverts = verts.Count;

            // skip co-linear points
            for (int i = 0; i < numverts; ++i)
            {
                Microsoft.Xna.Framework.Vector3 v_prev;
                Microsoft.Xna.Framework.Vector3 v_current;
                Microsoft.Xna.Framework.Vector3 v_next;

                Microsoft.Xna.Framework.Vector3 v1;
                Microsoft.Xna.Framework.Vector3 v2;

                v_prev = verts[(i + numverts - 1) % numverts].Position;
                v_current = verts[i].Position;
                v_next = verts[(i + 1) % numverts].Position;

                Microsoft.Xna.Framework.Vector3.Subtract(ref v_current, ref v_prev, out v1);
                v1 = Microsoft.Xna.Framework.Vector3.Normalize(v1);
                Microsoft.Xna.Framework.Vector3.Subtract(ref v_next, ref v_prev, out v2);
                v2 = Microsoft.Xna.Framework.Vector3.Normalize(v2);

                // skip co-linear point
                if (Math.Abs(v1.X - v2.X) <= COLINEAR_EPSILON && Math.Abs(v1.Y - v2.Y) <= COLINEAR_EPSILON && Math.Abs(v1.Z - v2.Z) <= COLINEAR_EPSILON)
                {
                    for (int j = i + 1; j < numverts; ++j)
                    {
                        verts[j - 1] = verts[j];
                    }

                    --numverts;

                    // retry next vertex next time, which is now current vertex
                    --i;
                }
            }

            // remove co-linear points
            _verts = new List<WorldVertexFormat>();
            for (int i = 0; i < numverts; i++)
            {
                _verts.Add(verts[i]);
            }

            return _verts.ToArray();
        }

        public static void CalculateSurfaceNormal(ref SPolyVerts[] PolyVerts)
        {
            List<WorldVertexFormat> verts;
            WorldVertexFormat[] _verts;
            Microsoft.Xna.Framework.Vector3 side1;
            Microsoft.Xna.Framework.Vector3 side2;
            Microsoft.Xna.Framework.Vector3 Norm;

            if (PolyVerts == null)
                return;

            if (PolyVerts.Length < 3)
                return;

            // generate vertex list
            verts = new List<WorldVertexFormat>();
            for (int i = 0; i < PolyVerts.Length; i++)
            {
                verts.Add(PolyVerts[i].vertex);
            }

            // remove co-linear points and calculate the normal
            _verts = SkipCoLinearPoints(verts);
            if (_verts.Length > 2)
            {
                side1 = _verts[0].Position - _verts[1].Position;
                side2 = _verts[2].Position - _verts[1].Position;
                Norm = Vector3.Cross(side1, side2);
                Norm = Vector3.Normalize(Norm);
            }
            else
            {
                Norm = Vector3.Zero;
            }

            // assign the normal to each vertex of the surface
            for (int j = 0; j < PolyVerts.Length; j++)
            {
                PolyVerts[j].vertex.Normal = Norm;
            }
        }

        public int Mod_PointInLeaf(Vector3 p)
        {
            int node;
            int plane;
            float d;

            //if (!model || !model->nodes)
            //    ri.Sys_Error(ERR_DROP, "Mod_PointInLeaf: bad model");

            node = 0;
            while (true)
            {
                if (CQ2BSP.SWorldData.nodes[node].contents != -1)
                    break;

                plane = CQ2BSP.SWorldData.nodes[node].plane;
                d = Microsoft.Xna.Framework.Vector3.Dot(p, CQ2BSP.SWorldData.planes[plane].normal) - CQ2BSP.SWorldData.planes[plane].dist;

                if (d > 0)
                    node = CQ2BSP.SWorldData.nodes[node].children[0];
                else
                    node = CQ2BSP.SWorldData.nodes[node].children[1];
            }

            return node;
        }

        private byte[] Mod_DecompressVis(int bitofs)
        {
            int c;
            int row;
            int pos_in;
            int pos_out = 0;
            byte[] decompressed = new byte[CQ2BSP.MAX_MAP_LEAFS / 8];

            row = (CQ2BSP.SWorldData.vis.numclusters + 7) >> 3;

            if (CQ2BSP.SWorldData.visdata == null)
            {
                // no vis info, so make all visible
                while (row != 0)
                {
                    decompressed[pos_out++] = 0xff;
                    row--;
                }

                return decompressed;
            }

            pos_in = bitofs;

            do
            {
                if (CQ2BSP.SWorldData.visdata[pos_in] != 0)
                {
                    decompressed[pos_out++] = CQ2BSP.SWorldData.visdata[pos_in++];
                    continue;
                }

                c = CQ2BSP.SWorldData.visdata[pos_in + 1];
                pos_in += 2;

                while (c != 0)
                {
                    decompressed[pos_out++] = 0x00;
                    c--;
                }
            } while (pos_out < row);

            return decompressed;
        }

        public byte[] Mod_ClusterPVS(int cluster)
        {
            if (CQ2BSP.SWorldData.visdata == null || cluster < 0 || cluster >= CQ2BSP.SWorldData.vis.numclusters)
                return mod_novis;

            return Mod_DecompressVis(CQ2BSP.SWorldData.vis.bitofs[cluster, CQ2BSP.DVIS_PVS]);
        }

        private void ParseEpair(ref SEpair _Epair)
        {
            if (vScript.Token.Length >= MAX_KEY - 1)
            {
                CMain.Error(CMain.EErrorParm.ERR_FATAL, "ParseEpair: token too long\nKey: " + vScript.Token);
            }
            _Epair.Key = vScript.Token.ToLower();

            vScript.GetToken(false);

            if (vScript.Token.Length >= MAX_VALUE - 1)
            {
                CMain.Error(CMain.EErrorParm.ERR_FATAL, "ParseEpair: token too long\nValue: " + vScript.Token);
            }
            _Epair.Value = vScript.Token;

            // strip trailing spaces that sometimes get accidentally added in the editor
            _Epair.Key = _Epair.Key.Trim();
            _Epair.Value = _Epair.Value.Trim();
        }

        public string ValueForKey(SEntities _Entity, string Key)
        {
            for (int i = 0; i < _Entity.Epairs.Length; i++)
            {
                if (_Entity.Epairs[i].Key == Key)
                    return _Entity.Epairs[i].Value;
            }

            return "";
        }

        public string ValueForKey(List<SEpair> _Epairs, string Key)
        {
            for (int i = 0; i < _Epairs.Count; i++)
            {
                if (_Epairs[i].Key == Key)
                    return _Epairs[i].Value;
            }

            return "";
        }

        public string ValueForKey(SEpair[] _Epairs, string Key)
        {
            for (int i = 0; i < _Epairs.Length; i++)
            {
                if (_Epairs[i].Key == Key)
                    return _Epairs[i].Value;
            }

            return "";
        }

        private bool ParseMapEntity()
        {
            SEntities _Entity;
            List<SEpair> Epairs;
            string ModelPathTris;
            string ModelPathSkin;

            if (vScript == null)
            {
                CMain.Error(CMain.EErrorParm.ERR_WARNING, "Source MAP file not loaded.");
                return false;
            }

            //if (vScript.BufferAvailable() == false)
            //    return false;

            if (vScript.GetToken(true) == false)
                return false;

            if (vScript.Token != "{")
            {
                CMain.Error(CMain.EErrorParm.ERR_FATAL, "ParseEntity: { not found, found " + vScript.Token + " on line " + vScript.ScriptLine + ".");
                return false;
                //Error("ParseEntity: { not found, found %s on line %d - last entity was at: <%4.2f, %4.2f, %4.2f>...", token, scriptline, entities[num_entities].origin[0], entities[num_entities].origin[1], entities[num_entities].origin[2]);
            }

            if (Entities.Count == CQ2BSP.MAX_MAP_ENTITIES)
                CMain.Error(CMain.EErrorParm.ERR_FATAL, "num_entities == MAX_MAP_ENTITIES");

            //_Entity.Brushes = new List<SBspBrush>();
            //_Entity.Epairs = new List<SEpair>();

            Epairs = new List<SEpair>();
            _Entity.Origin.X = 0.0f;
            _Entity.Origin.Y = 0.0f;
            _Entity.Origin.Z = 0.0f;
            _Entity.Angles.X = 0.0f;
            _Entity.Angles.Y = 0.0f;
            _Entity.Angles.Z = 0.0f;
            _Entity.Model = NullModel;
            _Entity.ModelCurrentSkin = 0;
            _Entity.ModelFrameOffset = 0;
            _Entity.ModelFrameCurrent = 0;
            _Entity.ModelFrameSeqStart = 0;
            _Entity.ModelFrameSeqEnd = 0;

            do
            {
                if (vScript.GetToken(true) == false)
                    CMain.Error(CMain.EErrorParm.ERR_FATAL, "ParseEntity: EOF without closing brace");

                if (vScript.Token == "}")
                    break;

                if (vScript.Token == "{")
                {
                    // parse the brush
                    /*SBspBrush _Brush;

                    if (vScript.GetToken(true) == false)
                        break;

                    vScript.UnGetToken();

                    _Brush.sides = new List<SSide>();

                    ParseRawBrush(ref _Brush);
                    _Entity.Brushes.Add(_Brush);*/
                }
                else
                {
                    SEpair _Epair;

                    _Epair.Key = "";
                    _Epair.Value = "";

                    // parse a key / value pair
                    ParseEpair(ref _Epair);
                    Epairs.Add(_Epair);
                }
            } while (true);


            // set the mesh and skin
            SetupModelMesh(ref Epairs);

            _Entity.Epairs = Epairs.ToArray();
            
            ModelPathTris = ValueForKey(_Entity.Epairs, "modelpathtris");
            ModelPathSkin = ValueForKey(_Entity.Epairs, "modelpathskin");

            // set or load the model
            if (string.IsNullOrWhiteSpace(ModelPathTris) == false)
            {
                _Entity.Model = R_RegisterModel(ModelPathTris);
            }
            else
            {
                string BrushModel = ValueForKey(_Entity.Epairs, "model");

                if (string.IsNullOrWhiteSpace(BrushModel) == false)
                {
                    _Entity.Model = R_RegisterModel(BrushModel);
                }
            }

            // set the skin index
            for (int i = 0; i < _Entity.Model.ModelMD2.md2.num_skins; i++)
            {
                if (ModelPathSkin == _Entity.Model.ModelMD2.skinnames[i])
                {
                    _Entity.ModelCurrentSkin = i;
                    break;
                }
            }


            // set the mesh animation
            SetupModelMeshAnimation(ref _Entity);


            if (_Entity.Epairs.Length != 0)
            {
                string Origin;
                string Angles;


                // set the origin
                Origin = ValueForKey(_Entity, "origin");
                if (string.IsNullOrEmpty(Origin) == false)
                {
                    string[] OriginArgs;
                    char[] delim = new char[1];

                    delim[0] = ' ';
                    OriginArgs = Origin.Split(delim, StringSplitOptions.RemoveEmptyEntries);

                    if (OriginArgs.Length == 3)
                    {
                        _Entity.Origin.X = Convert.ToSingle(OriginArgs[0]);
                        _Entity.Origin.Y = Convert.ToSingle(OriginArgs[1]);
                        _Entity.Origin.Z = Convert.ToSingle(OriginArgs[2]);
                    }
                }


                // set the angle
                Angles = ValueForKey(_Entity, "angle");
                if (string.IsNullOrEmpty(Angles) == false)
                {
                    _Entity.Angles.X = 0;
                    _Entity.Angles.Y = 0;
                    _Entity.Angles.Z = Convert.ToSingle(Angles);
                }
            }

            Entities.Add(_Entity);

            return true;
        }

        public void BuildEntities()
        {
            Microsoft.Xna.Framework.Vector4 vec_origin;
            List<CPointLight> PointLights;

            vScript = new Q2BSP.Library.CScript();
            vScript.ParseFromMemory(CQ2BSP.SWorldData.map_entitystring);

            if (Entities != null)
                Entities.Clear();

            Entities = new List<SEntities>();
            PointLights = new List<CPointLight>();

            while (ParseMapEntity() == true)
            {
            }

            CQ2BSP.SWorldData.entities = Entities.ToArray();


            // setup default pointlight
            vec_origin.X = 4096.0f;
            vec_origin.Y = 4096.0f;
            vec_origin.Z = 4096.0f;
            vec_origin.W = 1.0f;
            PointLights.Add(new CPointLight(vec_origin, 20.0f));

            // setup player pointlight
            vec_origin.X = 0.0f;
            vec_origin.Y = 0.0f;
            vec_origin.Z = 0.0f;
            vec_origin.W = 1.0f;
            PointLights.Add(new CPointLight(vec_origin, 300.0f));

            for (int i = 0; i < CQ2BSP.SWorldData.entities.Length; i++)
            {
                if (ValueForKey(CQ2BSP.SWorldData.entities[i], "classname") == "worldspawn")
                {
                    CClient.cl.RefDef.MapName = ValueForKey(CQ2BSP.SWorldData.entities[i], "message");
                    if (string.IsNullOrEmpty(CClient.cl.RefDef.MapName) == true)
                        CClient.cl.RefDef.MapName = "Developed by Jacques Krige";
                }

                // uncomment this to add hardware pointlights
                /*if (ValueForKey(CQ2BSP.SWorldData.entities[i], "classname") == "light")
                {
                    vec_origin.X = CQ2BSP.SWorldData.entities[i].Origin.X;
                    vec_origin.Y = CQ2BSP.SWorldData.entities[i].Origin.Y;
                    vec_origin.Z = CQ2BSP.SWorldData.entities[i].Origin.Z;
                    vec_origin.W = 1.0f;

                    if (string.IsNullOrEmpty(ValueForKey(CQ2BSP.SWorldData.entities[i], "light")) == false)
                    {
                        if (string.IsNullOrEmpty(ValueForKey(CQ2BSP.SWorldData.entities[i], "_color")) == false)
                        {
                            float Red = 1.0f;
                            float Green = 1.0f;
                            float Blue = 1.0f;
                            string LightColor = ValueForKey(CQ2BSP.SWorldData.entities[i], "_color");

                            if (string.IsNullOrEmpty(LightColor) == false)
                            {
                                string[] LightColorArgs;
                                char[] delim = new char[1];

                                delim[0] = ' ';
                                LightColorArgs = LightColor.Split(delim, StringSplitOptions.RemoveEmptyEntries);

                                if (LightColorArgs.Length == 3)
                                {
                                    Red = Convert.ToSingle(LightColorArgs[0]);
                                    Green = Convert.ToSingle(LightColorArgs[1]);
                                    Blue = Convert.ToSingle(LightColorArgs[2]);
                                }
                            }

                            PointLights.Add(new CPointLight(vec_origin, Convert.ToSingle(ValueForKey(CQ2BSP.SWorldData.entities[i], "light")), Red, Green, Blue));
                        }
                        else
                        {
                            PointLights.Add(new CPointLight(vec_origin, Convert.ToSingle(ValueForKey(CQ2BSP.SWorldData.entities[i], "light"))));
                        }
                    }
                    else
                    {
                        PointLights.Add(new CPointLight(vec_origin));
                    }
                }*/

                if (ValueForKey(CQ2BSP.SWorldData.entities[i], "classname") == "info_player_start")
                {
                    CClient.cl.RefDef.ViewOrigin = CQ2BSP.SWorldData.entities[i].Origin;
                    CClient.cl.RefDef.ViewAngles = CQ2BSP.SWorldData.entities[i].Angles;
                    CClient.cl.RefDef.EntityIndex = i;
                }
            }

            CQ2BSP.SWorldData.lights = PointLights.ToArray();
        }


        /// <summary>
        /// Adds keys for the selected entity for its model as well as the model's skin.
        /// This is a "dirty" function to handle logic thats not found within the physical data, but within "gamex86.dll"
        /// </summary>
        /// <param name="inEpairs">Current list of Epairs to scan for valid entities in need of modification</param>
        private void SetupModelMesh(ref List<SEpair> inEpairs)
        {
            string classname;
            string modelpathtris = null;
            string modelpathskin = null;
            SEpair _Epair;

            if (inEpairs == null)
                return;

            classname = ValueForKey(inEpairs, "classname");

            switch (classname)
            {
#region Monsters
                case "monster_soldier_light":
                    modelpathtris = "models/monsters/soldier/tris.md2";
                    modelpathskin = "models/monsters/soldier/skin_lt.pcx";
                    break;

                case "monster_soldier":
                    modelpathtris = "models/monsters/soldier/tris.md2";
                    modelpathskin = "models/monsters/soldier/skin.pcx";
                    break;

                case "monster_soldier_ss":
                    modelpathtris = "models/monsters/soldier/tris.md2";
                    modelpathskin = "models/monsters/soldier/skin_ss.pcx";
                    break;

                case "monster_infantry":
                    modelpathtris = "models/monsters/infantry/tris.md2";
                    modelpathskin = "models/monsters/infantry/skin.pcx";
                    break;

                case "monster_gunner":
                    modelpathtris = "models/monsters/gunner/tris.md2";
                    modelpathskin = "models/monsters/gunner/skin.pcx";
                    break;

                case "monster_flyer":
                    modelpathtris = "models/monsters/flyer/tris.md2";
                    modelpathskin = "models/monsters/flyer/skin.pcx";
                    break;

                case "monster_hover":
                    modelpathtris = "models/monsters/hover/tris.md2";
                    modelpathskin = "models/monsters/hover/skin.pcx";
                    break;

                case "monster_floater":
                    modelpathtris = "models/monsters/float/tris.md2";
                    modelpathskin = "models/monsters/float/skin.pcx";
                    break;

                case "monster_mutant":
                    modelpathtris = "models/monsters/mutant/tris.md2";
                    modelpathskin = "models/monsters/mutant/skin.pcx";
                    break;

                case "monster_parasite":
                    modelpathtris = "models/monsters/parasite/tris.md2";
                    modelpathskin = "models/monsters/parasite/skin.pcx";
                    break;

                case "monster_flipper":
                    modelpathtris = "models/monsters/flipper/tris.md2";
                    modelpathskin = "models/monsters/flipper/skin.pcx";
                    break;

                case "monster_berserk":
                    modelpathtris = "models/monsters/berserk/tris.md2";
                    modelpathskin = "models/monsters/berserk/skin.pcx";
                    break;

                case "monster_gladiator":
                    modelpathtris = "models/monsters/gladiatr/tris.md2";
                    modelpathskin = "models/monsters/gladiatr/skin.pcx";
                    break;

                case "monster_brain":
                    modelpathtris = "models/monsters/brain/tris.md2";
                    modelpathskin = "models/monsters/brain/skin.pcx";
                    break;

                case "monster_chick":
                    modelpathtris = "models/monsters/bitch/tris.md2";
                    modelpathskin = "models/monsters/bitch/skin.pcx";
                    break;

                case "monster_medic":
                    modelpathtris = "models/monsters/medic/tris.md2";
                    modelpathskin = "models/monsters/medic/skin.pcx";
                    break;

                case "monster_tank":
                    modelpathtris = "models/monsters/tank/tris.md2";
                    modelpathskin = "models/monsters/tank/skin.pcx";
                    break;

                case "monster_boss2":
                    modelpathtris = "models/monsters/boss2/tris.md2";
                    modelpathskin = "models/monsters/boss2/skin.pcx";
                    break;

                case "monster_jorg":
                    modelpathtris = "models/monsters/boss3/jorg/tris.md2";
                    modelpathskin = "models/monsters/boss3/jorg/skin.pcx";
                    break;
#endregion

#region Weapons
                case "weapon_shotgun":
                    modelpathtris = "models/weapons/g_shotg/tris.md2";
                    modelpathskin = "models/weapons/g_shotg/skin.pcx";
                    break;

                case "weapon_supershotgun":
                    modelpathtris = "models/weapons/g_shotg2/tris.md2";
                    modelpathskin = "models/weapons/g_shotg2/skin.pcx";
                    break;

                case "weapon_machinegun":
                    modelpathtris = "models/weapons/g_machn/tris.md2";
                    modelpathskin = "models/weapons/g_machn/skin.pcx";
                    break;

                case "weapon_chaingun":
                    modelpathtris = "models/weapons/g_chain/tris.md2";
                    modelpathskin = "models/weapons/g_chain/skin.pcx";
                    break;

                case "weapon_grenadelauncher":
                    modelpathtris = "models/weapons/g_launch/tris.md2";
                    modelpathskin = "models/weapons/g_launch/skin.pcx";
                    break;

                case "weapon_rocketlauncher":
                    modelpathtris = "models/weapons/g_rocket/tris.md2";
                    modelpathskin = "models/weapons/g_rocket/skin.pcx";
                    break;

                case "weapon_hyperblaster":
                    modelpathtris = "models/weapons/g_hyperb/tris.md2";
                    modelpathskin = "models/weapons/g_hyperb/skin.pcx";
                    break;

                case "weapon_railgun":
                    modelpathtris = "models/weapons/g_rail/tris.md2";
                    modelpathskin = "models/weapons/g_rail/skin.pcx";
                    break;

                case "weapon_bfg":
                    modelpathtris = "models/weapons/g_bfg/tris.md2";
                    modelpathskin = "models/weapons/g_bfg/skin.pcx";
                    break;

                case "ammo_shells":
                    modelpathtris = "models/items/ammo/shells/medium/tris.md2";
                    modelpathskin = "models/items/ammo/shells/medium/skin.pcx";
                    break;

                case "ammo_bullets":
                    modelpathtris = "models/items/ammo/bullets/medium/tris.md2";
                    modelpathskin = "models/items/ammo/bullets/medium/skin.pcx";
                    break;

                case "ammo_grenades":
                    modelpathtris = "models/items/ammo/grenades/medium/tris.md2";
                    modelpathskin = "models/items/ammo/grenades/medium/skin.pcx";
                    break;

                case "ammo_rockets":
                    modelpathtris = "models/items/ammo/rockets/medium/tris.md2";
                    modelpathskin = "models/items/ammo/rockets/medium/skin.pcx";
                    break;

                case "ammo_cells":
                    modelpathtris = "models/items/ammo/cells/medium/tris.md2";
                    modelpathskin = "models/items/ammo/cells/medium/skin.pcx";
                    break;

                case "ammo_slugs":
                    modelpathtris = "models/items/ammo/slugs/medium/tris.md2";
                    modelpathskin = "models/items/ammo/slugs/medium/skin.pcx";
                    break;
#endregion

#region Misc
                case "misc_banner":
                    modelpathtris = "models/objects/banner/tris.md2";
                    modelpathskin = "models/objects/banner/skin.pcx";
                    break;

                case "misc_strogg_ship":
                    modelpathtris = "models/ships/strogg1/tris.md2";
                    modelpathskin = "models/ships/strogg1/skin.pcx";
                    break;

                case "misc_viper":
                    modelpathtris = "models/ships/viper/tris.md2";
                    modelpathskin = "models/ships/viper/skin.pcx";
                    break;

                case "misc_bigviper":
                    modelpathtris = "models/ships/bigviper/tris.md2";
                    modelpathskin = "models/ships/bigviper/skin.pcx";
                    break;

                case "misc_satellite_dish":
                    modelpathtris = "models/objects/satellite/tris.md2";
                    modelpathskin = "models/objects/satellite/skin.pcx";
                    break;

                case "misc_explobox":
                    modelpathtris = "models/objects/barrels/tris.md2";
                    modelpathskin = "models/objects/barrels/skin.pcx";
                    break;
#endregion

#region Items
                case "item_adrenaline":
                    modelpathtris = "models/items/adrenal/tris.md2";
                    modelpathskin = "models/items/adrenal/skin.pcx";
                    break;

                case "item_health":
                    modelpathtris = "models/items/healing/medium/tris.md2";
                    modelpathskin = "models/items/healing/medium/skin.pcx";
                    break;

                case "item_health_large":
                    modelpathtris = "models/items/healing/large/tris.md2";
                    modelpathskin = "models/items/healing/large/skin.pcx";
                    break;

                case "item_health_mega":
                    modelpathtris = "models/items/healing/stimpack/tris.md2";
                    modelpathskin = "models/items/healing/stimpack/skin.pcx";
                    break;

                case "item_armor_shard":
                    modelpathtris = "models/items/armor/shard/tris.md2";
                    modelpathskin = "models/items/armor/shard/skin.pcx";
                    break;

                case "item_armor_jacket":
                    modelpathtris = "models/items/armor/jacket/tris.md2";
                    modelpathskin = "models/items/armor/jacket/skin.pcx";
                    break;

                case "item_armor_combat":
                    modelpathtris = "models/items/armor/combat/tris.md2";
                    modelpathskin = "models/items/armor/combat/skin.pcx";
                    break;

                case "item_armor_body":
                    modelpathtris = "models/items/armor/body/tris.md2";
                    modelpathskin = "models/items/armor/body/skin.pcx";
                    break;
#endregion

            }

            if (modelpathtris != null)
            {
                _Epair.Key = "modelpathtris";
                _Epair.Value = modelpathtris;
                inEpairs.Add(_Epair);

                _Epair.Key = "modelpathskin";
                _Epair.Value = modelpathskin;
                inEpairs.Add(_Epair);
            }
        }

        private void SetupModelMeshAnimation(ref SEntities inEntity)
        {
            int idxStart;
            int idxEnd;
            string AnimGroup;

            if (inEntity.Model.numframes == 0)
                return;

            idxStart = -1;
            idxEnd = -1;
            AnimGroup = null;

            for (int i = 0; i < inEntity.Model.ModelMD2.md2.num_frames; i++)
            {
                if (CShared.Com_ToString(inEntity.Model.ModelMD2.aliasframes[i].name).StartsWith("stand") == true)
                {
                    AnimGroup = "stand";
                    break;
                }

                if (CShared.Com_ToString(inEntity.Model.ModelMD2.aliasframes[i].name).StartsWith("wait") == true)
                {
                    AnimGroup = "wait";
                    break;
                }

                if (CShared.Com_ToString(inEntity.Model.ModelMD2.aliasframes[i].name).StartsWith("frame") == true)
                {
                    AnimGroup = "frame";
                    break;
                }
            }

            if (AnimGroup == null)
                return;

            for (int i = 0; i < inEntity.Model.ModelMD2.md2.num_frames; i++)
            {
                if (CShared.Com_ToString(inEntity.Model.ModelMD2.aliasframes[i].name).StartsWith(AnimGroup) == true)
                {
                    if (idxStart == -1)
                        idxStart = idxEnd = i;
                    else
                        idxEnd++;
                }
            }

            if (idxStart < idxEnd)
            {
                inEntity.ModelFrameOffset = CProgram.gQ2Game.gCMain.Rand.Next(idxEnd - idxStart);

                inEntity.ModelFrameSeqStart = idxStart;
                inEntity.ModelFrameSeqEnd = idxEnd;
            }
        }

        public SModel Mod_ForName(string Name, bool Crash)
        {
            int i;
            byte[] header;
            MemoryStream ms;
            SModel _SModel;

            _SModel = new SModel();

            if (string.IsNullOrWhiteSpace(Name) == true | string.IsNullOrEmpty(Name) == true)
            {
                System.Diagnostics.Debug.WriteLine("Mod_ForName: NULL name");
                return _SModel;
            }


            //
            // inline models are grabbed only from worldmodel
            //
            if (Name.StartsWith("*") == true)
            {
                i = Convert.ToInt32(Name.Substring(1));
                if (i < 1 /*|| CQ2BSP.SWorldData == null ||*/ | i >= CQ2BSP.SWorldData.numsubmodels)
                    System.Diagnostics.Debug.WriteLine("bad inline model number");

                return mod_inline[i];
            }


            //
            // search the currently loaded models
            //
            for (i = 1; i < mod_numknown; i++)
            {
                if (string.IsNullOrWhiteSpace(mod_known[i].name) == true | string.IsNullOrEmpty(mod_known[i].name) == true)
                    continue;

                if (mod_known[i].name == Name)
                    return mod_known[i];
            }


            //
            // find a free model slot spot
            //
            for (i = 1; i < mod_numknown; i++)
            {
                if (string.IsNullOrWhiteSpace(mod_known[i].name) == true | string.IsNullOrEmpty(mod_known[i].name) == true)
                    break; // free spot
            }

            if (i == mod_numknown)
            {
                if (mod_numknown == MAX_MOD_KNOWN)
                {
                    System.Diagnostics.Debug.WriteLine("mod_numknown == MAX_MOD_KNOWN");
                    return _SModel;
                }

                mod_numknown++;
            }

            _SModel.name = Name;
            
            
            //
            // load the file
            //
            ms = null;
            if (CProgram.gQ2Game.gCMain.r_usepak == true)
            {
                CProgram.gQ2Game.gCMain.gCFiles.FS_ReadFile(Name, out ms);
            }
            else
            {
                string FilePath = CProgram.gQ2Game.Content.RootDirectory + "\\" + CConfig.GetConfigSTRING("Base Game") + "\\" + Name;
                FileStream _fs = new System.IO.FileStream(FilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                byte[] buf = new byte[_fs.Length];

                _fs.Read(buf, 0, (int)_fs.Length);
                ms = new MemoryStream(buf);

                if (_fs != null)
                    _fs.Close();
            }

            if (ms == null | ms.Length == 0)
            {
                _SModel.name = null;

                if (Crash == true)
                    System.Diagnostics.Debug.WriteLine("Mod_NumForName: " + _SModel.name + " not found");

                return _SModel;
            }


            //
            // fill it in
            //

            header = new byte[4];
            ms.Read(header, 0, 4);
            ms.Seek(0, System.IO.SeekOrigin.Begin);

            // call the apropriate loader
            switch ((int)((header[3] << 24) + (header[2] << 16) + (header[1] << 8) + header[0]))
            {
                case CQ2MD2.IDALIASHEADER:
                    CProgram.gQ2Game.gCMain.gCQ2MD2.Mod_LoadAliasModel(ref _SModel, ms);
                    break;

                //case IDSPRITEHEADER:
                //    loadmodel->extradata = Hunk_Begin (0x10000);
		        //    Mod_LoadSpriteModel (mod, buf);
                //    break;

                case CQ2BSP.IDBSPHEADER:
                    CProgram.gQ2Game.gCMain.gCQ2BSP.Mod_LoadBrushModel(ref _SModel, ms);
                    break;

                default:
                    System.Diagnostics.Debug.WriteLine("Mod_NumForName: unknown fileid for " + _SModel.name);
                    break;
            }

            if (ms != null)
                ms.Close();

            mod_known[i] = _SModel;

            return _SModel;
        }


        /// <summary>
        /// Specifies the model that will be used as the world
        /// </summary>
        /// <param name="model"></param>
        public void R_BeginRegistration(string model)
        {
            string fullname;

            registration_sequence++;
            CMain.r_oldviewcluster = -1;

            fullname = "maps/" + model + ".bsp";

            // explicitly free the old map if different
            // this guarantees that mod_known[0] is the world map
            if (mod_known[0].name != fullname)
            {
                // TODO
                //Mod_Free(&mod_known[0]);
            }

            CQ2BSP.SWorldData = Mod_ForName(fullname, true);
            CMain.r_viewcluster = -1;


            /*char	fullname[MAX_QPATH];
            cvar_t	*flushmap;
            
            registration_sequence++;
            r_oldviewcluster = -1;		// force markleafs
            
            Com_sprintf (fullname, sizeof(fullname), "maps/%s.bsp", model);
            
            // explicitly free the old map if different
            // this guarantees that mod_known[0] is the world map
            flushmap = ri.Cvar_Get ("flushmap", "0", 0);
            if ( strcmp(mod_known[0].name, fullname) || flushmap->value)
                Mod_Free (&mod_known[0]);
            
            r_worldmodel = Mod_ForName(fullname, true);
            r_viewcluster = -1;*/
        }

        public SModel R_RegisterModel(string Name)
        {
            SModel _SModel;
            //dsprite_t	*sprout;

            _SModel = Mod_ForName(Name, false);


            if (_SModel.ModType != EModType.MOD_BAD)
            {
                // register any images used by the models
                if (_SModel.ModType == EModType.MOD_SPRITE)
                {
                    //sprout = (dsprite_t*)mod->extradata;
                    //for (i = 0; i < sprout->numframes; i++)
                    //    mod->skins[i] = GL_FindImage(sprout->frames[i].name, it_sprite);
                }
                else if (_SModel.ModType == EModType.MOD_ALIAS)
                {
                    if (_SModel.ModelMD2.skins == null || _SModel.ModelMD2.skins.Length != _SModel.ModelMD2.md2.num_skins)
                    {
                        _SModel.ModelMD2.skins = new Microsoft.Xna.Framework.Graphics.Texture2D[_SModel.ModelMD2.md2.num_skins];

                        for (int i = 0; i < _SModel.ModelMD2.md2.num_skins; i++)
                        {
                            _SModel.ModelMD2.skins[i] = CProgram.gQ2Game.gCMain.gCImage.LoadSkin(_SModel.ModelMD2.skinnames[i]);
                        }
                    }

                    _SModel.numframes = _SModel.ModelMD2.md2.num_frames;
                }
                else if (_SModel.ModType == EModType.MOD_BRUSH)
                {
                    //for (i = 0; i < mod->numtexinfo; i++)
                    //    mod->texinfo[i].image->registration_sequence = registration_sequence;
                }
            }

            return _SModel;
        }

        public void R_EndRegistration()
        {
            /*int i;
            model_t* mod;

            for (i = 0, mod = mod_known; i < mod_numknown; i++, mod++)
            {
                if (!mod->name[0])
                    continue;
                if (mod->registration_sequence != registration_sequence)
                {	// don't need this model
                    Mod_Free(mod);
                }
            }

            GL_FreeUnusedImages();*/
        }


        // ================================================================
        // 
        // BRUSH MODELS
        // 
        // ================================================================

        // in memory representation

        public struct SMVertex
        {
            public Microsoft.Xna.Framework.Vector3 Position;
        }

        public struct SMModel
        {
            public Microsoft.Xna.Framework.BoundingBox bounds;
            public Microsoft.Xna.Framework.Vector3 origin; // for sounds or lights
            public float radius;
            public int headnode;
            public int visleafs; // not including the solid leaf 0
            public int firstface;
            public int numfaces;
        }

        public const int SIDE_FRONT = 0;
        public const int SIDE_BACK = 1;
        public const int SIDE_ON = 2;

        // surface flags
        [Flags]
        public enum EMSurface : int
        {
            SURF_PLANEBACK = 2,
            SURF_DRAWSKY = 4,
            SURF_DRAWTURB = 0x10,
            SURF_DRAWBACKGROUND = 0x40,
            SURF_UNDERWATER = 0x80
        }

        public struct SMEdge
        {
            public ushort[] v; // size: 2
            public uint cachededgeoffset;
        }

        public struct SMTexInfo
        {
            public Microsoft.Xna.Framework.Vector4[] vecs; // size: 2

            public CQ2BSP.ESurface flags;
            public int numframes;
            public int next; // animation chain (struct mtexinfo_s	*next;)
            public int image;
            public int Width;
            public int Height;
        }

        public struct SPolyVerts
        {
            public int offset;                  // offset in the vertex buffer
            public WorldVertexFormat vertex;    // xyz_position, s1t1, s2t2, xyz_normal
        }

        public struct SGLPoly
        {
            public int next;
            public int chain;
            public int numverts;

            //public int flags; // for SURF_UNDERWATER (not needed anymore?)

            public SPolyVerts[] verts;
        }

        public struct SMSurface
        {
            public int visframe; // should be drawn when node is crossed

            //cShared.SCPlane plane; // cplane_t	*plane;
            public int plane;
            public Microsoft.Xna.Framework.Plane plane2;

            public EMSurface flags;

            public int firstedge; // look up in model->surfedges[], negative numbers
            public int numedges; // are backwards edges

            public short[] texturemins; // size: 2
            public short[] extents; // size: 2

            // lightmap coordinates
            public int light_s;
            public int light_t;

            // lightmap coordinates for dynamic lightmaps
            public int dlight_s;
            public int dlight_t;

            // multiple if warped
            public SGLPoly[] polys;

            public IndexBuffer ibSurface;

            // used to render multiple warped polys quicker
            public int[] ibData;

            public Microsoft.Xna.Framework.BoundingBox bounds;
            public Microsoft.Xna.Framework.Vector3 boundsMid;

            public int[] lightIndex;
            public double[] lightLength;
            //public string TechniqueName;

            public int texturechain; // struct	msurface_s	*texturechain;

            public int texinfo;


            // lighting info
            public int dlightframe;
            public int dlightbits;
            public int lightmaptexturenum;

            public byte[] styles; // size: MAXLIGHTMAPS
            public float[] cached_light; // size: MAXLIGHTMAPS (values currently used in lightmap)
            public int samples; // [numstyles*surfsize]
        }

        public struct SMNode
        {
            // common with leaf
            public int contents; // -1, to differentiate from leafs
            public int visframe; // node needs to be traversed if current

            public Microsoft.Xna.Framework.BoundingBox bounds; // for bounding box culling

            public int parent;

            // node specific
            public int plane;
            public int[] children; // size: 2 (struct mnode_s	*children[2];)

            public ushort firstsurface;
            public ushort numsurfaces;

            // leaf specific
            public int cluster;
            public int area;

            public int firstmarksurface;
            public int nummarksurfaces;
        }

        public struct SMAreaPortal
        {
            public int portalnum;
            public int otherarea;
        }

        public struct SMArea
        {
            public int numareaportals;
            public int firstareaportal;
            public int floodnum;
            public int floodvalid;
        }



        // ------------------------------------------------------
        // STORAGE OF INDEX BUFFER DATA PER-LIGHTMAP, PER-TEXTURE
        // ------------------------------------------------------
        public struct SMIndexBuffer
        {
            public int PrimitiveCount;
            public int[] ibIndices;
            public IndexBuffer ibBuffer;
        }


        // ================================================================
        // 
        // WHOLE MODEL
        // 
        // ================================================================

        public enum EModType
        {
            MOD_BAD,
            MOD_BRUSH,
            MOD_SPRITE,
            MOD_ALIAS
        }

        public struct SModel
        {
            public string name;
            //public string baseName;

            //int			registration_sequence;

            public EModType ModType;
            public int numframes;

            public CQ2MD2.EModelFlags Flags;

            //
            // volume occupied by the model graphics
            //
            public Microsoft.Xna.Framework.Vector3 mins;
            public Microsoft.Xna.Framework.Vector3 maxs;
            public float radius;

            //
            // solid volume for clipping 
            //
            public bool clipbox;
            public Microsoft.Xna.Framework.Vector3 clipmins;
            public Microsoft.Xna.Framework.Vector3 clipmaxs;

            //
            // brush model
            //
            public int firstmodelsurface;
            public int nummodelsurfaces;

            public int numsubmodels;
            public SMModel[] submodels;

            public int numplanes;
            public CShared.SCPlane[] planes;

            public int numleafs; // number of visible leafs, not counting 0

            public int numvertexes;
            public SMVertex[] vertexes;

            public int numedges;
            public SMEdge[] edges;

            public int numnodes;
            public int firstnode;
            public SMNode[] nodes;

            public int numDecisionNodes;

            public int numtexinfo;
            public SMTexInfo[] texinfo;

            public int numsurfaces;
            public SMSurface[] surfaces;

            public int numsurfedges;
            public int[] surfedges;

            public int nummarksurfaces;
            public int[] marksurfaces;

            public int numareas;
            public SMArea[] areas;

            public int numareaportals;
            public SMAreaPortal[] areaportals;

            public CQ2BSP.SDVis vis;
            public byte[] visdata;

            public byte[] lightdata;

            // world vertex buffer
            public VertexBuffer vbWorldSolid;

            // world index buffers [WorldLightmaps, WorldTextures]
            public SMIndexBuffer[,] ibWorldSolid;

            public List<CMain.SChainLightmap> lSChainLightmap;

            // loaded wall textures
            public CImage.STextureWAL[] WorldTextures;

            // loaded lightmaps
            public Texture2D[] WorldLightmaps;

            // skybox vertex buffer
            public VertexBuffer vbSkybox;

            // loaded skybox textures
            public Texture2D[] WorldSkies;

            public string map_entitystring; // MAX_MAP_ENTSTRING
            public SEntities[] entities;

            public CPointLight[] lights;

            public List<CMain.STextureStatic> MarkSurfaceListStatic;
            public List<CMain.STextureDynamic> MarkSurfaceListDynamic;


            //
            // alias model and skins
            //
            public SModelMD2 ModelMD2;


            // for alias models and skins
            //image_t		*skins[MAX_MD2SKINS];

            //int			extradatasize;
            //void		*extradata;
        }

        public struct SModelMD2
        {
            // path information
            public string name;

            // model information
            public CQ2MD2.SMd2 md2;
            public string[] skinnames;
            public CQ2MD2.SSTVert[] st;
            public CQ2MD2.SDTriangle[] triangles;
            public CQ2MD2.SAliasFrameDesc[] aliasframes;
            public byte[] glcmds;

            public IndexBuffer ibModel;
            public VertexBuffer[] vbModel; // size: number of animation frames
            public Texture2D[] skins;

            //public List<float[]> base_xyz;
            //public List<double[]> base_st;
        }


        // ================================================================
        // 
        // ENTITIES
        // 
        // ================================================================

        public struct SEntities
        {
            public Microsoft.Xna.Framework.Vector3 Origin;
            public Microsoft.Xna.Framework.Vector3 Angles;

            //public List<SBspBrush> Brushes;
            //public List<SEpair> Epairs;
            public SEpair[] Epairs;

            // extra data
            public SModel Model;
            public int ModelCurrentSkin;
            public int ModelFrameOffset;
            public int ModelFrameCurrent;

            public int ModelFrameSeqStart;
            public int ModelFrameSeqEnd;
        }

        public struct SEpair
        {
            public string Key;
            public string Value;
        }


        // ================================================================
        // 
        // WORLD VERTEX FORMAT
        // 
        // ================================================================

        public struct WorldVertexFormat
        {
            public Vector3 Position;
            public Vector2 TextureCoordinate;
            public Vector2 LightmapCoordinate;
            public Vector3 Normal;

            public static VertexElement[] VertexElements =
            {
                new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(sizeof(float) * 5, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(sizeof(float) * 7, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            };
        }


        // ================================================================
        // 
        // MD2 MODEL VERTEX FORMAT
        // 
        // ================================================================

        public struct ModelVertexFormat
        {
            public Vector3 Position;
            public Vector2 TextureCoordinate;
            public Vector3 Normal;

            public static VertexElement[] VertexElements =
            {
                new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(sizeof(float) * 5, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            };
        }




    }
}
