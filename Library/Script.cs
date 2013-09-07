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

namespace Q2BSP.Library
{
    public class CScript
    {
        private const int MAX_INCLUDES = 8;
        private const int MAX_TOKEN = 1024;

        //private cCommand vCommand;

        //private string InputData;
        //private SScript[] ScriptStack;
        private List<SScript> Script;
        public int ScriptLine;

        public string Token;
        //private bool ScriptEnd;
        private bool TokenReady; // only qtrue if UnGetToken was just called

        public CScript(/*string Input*/)
        {
            //if (string.IsNullOrEmpty(Input) == true)
            //    InputData = null;
            //else
            //    InputData = Input.Trim();

            //vCommand = new cCommand();

            //ScriptStack = new SScript[MAX_INCLUDES];
            Script = new List<SScript>();
        }

        /*public bool BufferAvailable()
        {
            if (string.IsNullOrEmpty(Script[Script.Count - 1].Buffer) == true)
                return false;
            else
                return true;
        }*/

        // ================
        // AddScriptToStack
        // ================
        private void AddScriptToStack(string FileName)
        {
            SScript _Script;

            if (Script.Count == MAX_INCLUDES - 1)
                CMain.Error(CMain.EErrorParm.ERR_FATAL, "Script file exceeded MAX_INCLUDES");

            _Script.FileName = FileName;
            _Script.Buffer = CCommand.LoadFile(_Script.FileName);

            CMain.Printf("entering " + _Script.FileName + "\n");

            _Script.Line = 1;

            // jkrige - todo
            _Script.StartPosition = 0;
            _Script.EndPosition = _Script.Buffer.Length - 1;
            //script->script_p = script->buffer;
            //script->end_p = script->buffer + size;
            // jkrige - todo

            Script.Add(_Script);
        }

        // ================
        // LoadScriptFile
        // ================
        public string LoadScriptFile(string FileName)
        {
            AddScriptToStack(FileName);

            //ScriptEnd = false;
            TokenReady = false;

            return Script[Script.Count - 1].Buffer;
        }

        // ================
        // ParseFromMemory
        // ================
        public void ParseFromMemory(string buffer)
        {
            SScript _Script;

            if (Script.Count == MAX_INCLUDES)
                CMain.Error(CMain.EErrorParm.ERR_FATAL, "Script file exceeded MAX_INCLUDES");

            _Script.FileName = "memory buffer";
            _Script.Buffer = buffer;

            _Script.Line = 1;

            // jkrige - todo
            _Script.StartPosition = 0;
            _Script.EndPosition = _Script.Buffer.Length - 1;
            //script->script_p = script->buffer;
            //script->end_p = script->buffer + size;
            // jkrige - todo

            Script.Add(_Script);

            //ScriptEnd = false;
            TokenReady = false;
        }

        // ================
        // UnGetToken
        // 
        // Signals that the current token was not used, and should be reported for the next GetToken.
        // Note that
        // 
        // GetToken(qtrue);
        // UnGetToken();
        // GetToken (qfalse);
        // 
        // could cross a line boundary.
        // ================
        public void UnGetToken()
        {
            TokenReady = true;
        }

        public bool EndOfScript(bool CrossLine)
        {
            SScript _Script;

            if (CrossLine == false)
                CMain.Error(CMain.EErrorParm.ERR_FATAL, "Line " + ScriptLine + " is incomplete.");

            if (Script[Script.Count - 1].FileName == "memory buffer")
            {
                //ScriptEnd = true;
                return false;
            }

            _Script = Script[Script.Count - 1];
            _Script.Buffer = "";
            Script[Script.Count - 1] = _Script;

            // jkrige - check : this IF statement
            //if (Script.Count == MAX_INCLUDES)
            //    return false;

            if (Script.Count == 1)
            {
                return false;
            }
            // jkrige - check : this IF statement

            Script.RemoveAt(Script.Count - 1);
            ScriptLine = Script[Script.Count - 1].Line;
            CMain.Printf("returning to " + Script[Script.Count - 1].FileName + "\n");

            return GetToken(CrossLine);
        }

        // ================
        // GetToken
        // ================
        public bool GetToken(bool CrossLine)
        {
            string token_p;
            SScript _Script;

            // is a token already waiting?
            if (TokenReady == true)
            {
                TokenReady = false;
                return true;
            }

            //token_p = "";
            _Script = Script[Script.Count - 1];

            if (_Script.StartPosition >= _Script.EndPosition)
                return EndOfScript(CrossLine);


            //
            // skip space
            //

            skipspace:
            while (_Script.Buffer[_Script.StartPosition] <= 32)
            {
                if (_Script.StartPosition >= _Script.EndPosition)
                {
                    Script[Script.Count - 1] = _Script;
                    return EndOfScript(CrossLine);
                }

                if (_Script.Buffer[_Script.StartPosition++] == '\n')
                {
                    if(CrossLine == false)
                        CMain.Error(CMain.EErrorParm.ERR_FATAL, "Line " + ScriptLine + " is incomplete.");

                    ScriptLine = _Script.Line++;
                }
            }

            if (_Script.StartPosition >= _Script.EndPosition)
            {
                Script[Script.Count - 1] = _Script;
                return EndOfScript(CrossLine);
            }

            // ; # // comments
            if (
                _Script.Buffer[_Script.StartPosition] == ';'
                || _Script.Buffer[_Script.StartPosition] == '#'
                || (_Script.Buffer[_Script.StartPosition] == '/' && _Script.Buffer[_Script.StartPosition + 1] == '/')
                )
            {
                if (CrossLine == false)
                    CMain.Error(CMain.EErrorParm.ERR_FATAL, "Line " + ScriptLine + " is incomplete.");

                while (_Script.Buffer[_Script.StartPosition++] != '\n')
                {
                    if (_Script.StartPosition >= _Script.EndPosition)
                    {
                        Script[Script.Count - 1] = _Script;
                        return EndOfScript(CrossLine);
                    }
                }
                
                ScriptLine = _Script.Line++;
                Script[Script.Count - 1] = _Script;
                goto skipspace;
            }

            // /* */ comments
            if (_Script.Buffer[_Script.StartPosition] == '/' && _Script.Buffer[_Script.StartPosition + 1] == '*')
            {
                if (CrossLine == false)
                    CMain.Error(CMain.EErrorParm.ERR_FATAL, "Line " + ScriptLine + " is incomplete.");

                _Script.StartPosition += 2;

                while (_Script.Buffer[_Script.StartPosition] != '*' && _Script.Buffer[_Script.StartPosition + 1] != '/')
                {
                    if (_Script.Buffer[_Script.StartPosition] == '\n')
                        ScriptLine = _Script.Line++;

                    _Script.StartPosition++;

                    if (_Script.StartPosition >= _Script.EndPosition)
                    {
                        Script[Script.Count - 1] = _Script;
                        return EndOfScript(CrossLine);
                    }
                }

                _Script.StartPosition += 2;
                Script[Script.Count - 1] = _Script;
                goto skipspace;
            }


            //
            // copy token
            //

            token_p = "";
            //token_p = Token;
            //int test = _Script.Position;

            if (_Script.Buffer[_Script.StartPosition] == '"')
            {
                // quoted token
                _Script.StartPosition++;

                while (_Script.Buffer[_Script.StartPosition] != '"')
                {
                    token_p = token_p + _Script.Buffer[_Script.StartPosition++];

                    if (_Script.StartPosition == _Script.EndPosition)
                        break;

                    if (token_p.Length >= MAX_TOKEN)
                        CMain.Error(CMain.EErrorParm.ERR_FATAL, "Token too large on line " + ScriptLine + ".");
                }
                _Script.StartPosition++;
            }
            else
            {
                // regular token
                while (_Script.Buffer[_Script.StartPosition] > 32 && _Script.Buffer[_Script.StartPosition] != ';')
                {
                    token_p = token_p + _Script.Buffer[_Script.StartPosition++];

                    if (_Script.StartPosition == _Script.EndPosition)
                        break;

                    if (token_p.Length >= MAX_TOKEN)
                        CMain.Error(CMain.EErrorParm.ERR_FATAL, "Token too large on line " + ScriptLine + ".");
                }
            }

            Token = token_p;
            Script[Script.Count - 1] = _Script;

            if (Token == "$include")
            {
                GetToken(false);
                AddScriptToStack(Token);

                return GetToken(CrossLine);
            }

            return true;
        }

        // ================
        // TokenAvailable
        // 
        // Returns qtrue if there is another token on the line
        // ================
        public bool TokenAvailable()
        {
            int oldLine;
            bool r;

            oldLine = Script[Script.Count - 1].Line;
            r = GetToken(true);

            if (r == false)
                return false;

            UnGetToken();

            if (oldLine == Script[Script.Count - 1].Line)
                return true;

            return false;
        }


        // =====================================================================

        public void MatchToken(string Match)
        {
            GetToken(true);

            if (Token != Match)
                CMain.Error(CMain.EErrorParm.ERR_FATAL, "MatchToken( \"" + Match + "\" ) failed at line " + ScriptLine + ".");
        }

        /*public void Parse1DMatrix(int x, ref cMath.Vec[] v, int vPosition)
        {
            MatchToken("(");

            if (v != null)
            {
                for (int i = 0; i < x; i++)
                {
                    GetToken(false);
                    v[vPosition + i].Value = Convert.ToSingle(Token);
                }
            }

            MatchToken(")");
        }*/

        /*public void Parse2DMatrix(int y, int x, ref cMath.Vec[] v, int vPosition)
        {
            MatchToken("(");

            if (v != null)
            {
                for (int i = 0; i < y; i++)
                {
                    Parse1DMatrix(x, ref v, vPosition + i * x);
                }
            }

            MatchToken(")");
        }*/

        /*public void Parse3DMatrix(int z, int y, int x, ref cMath.Vec[] v, int vPosition)
        {
            MatchToken("(");

            if (v != null)
            {
                for (int i = 0; i < z; i++)
                {
                    Parse2DMatrix(y, x, ref v, vPosition + i * x * y);
                }
            }

            MatchToken(")");
        }*/

        /*public void Write1DMatrix(System.IO.FileStream FS, int x, ref cMath.Vec[] v, int vPosition)
        {
            System.IO.BinaryWriter BW = new System.IO.BinaryWriter(FS);

            BW.Write("( ");

            if (v != null)
            {
                for (int i = 0; i < x; i++)
                {
                    if (v[vPosition + i].Value == System.Math.Round(v[vPosition + i].Value))
                    {
                        BW.Write(System.Math.Round(v[vPosition + i].Value).ToString() + " ");
                    }
                    else
                    {
                        BW.Write(v[vPosition + i].Value.ToString() + " ");
                    }
                }
            }

            BW.Write(")");
        }*/

        /*public void Write2DMatrix(System.IO.FileStream FS, int y, int x, ref cMath.Vec[] v, int vPosition)
        {
            System.IO.BinaryWriter BW = new System.IO.BinaryWriter(FS);

            BW.Write("( ");

            if (v != null)
            {
                for (int i = 0; i < y; i++)
                {
                    Write1DMatrix(FS, x, ref v, vPosition + i * x);
                    BW.Write(" ");
                }
            }

            BW.Write(")\n");
        }*/

        /*public void Write3DMatrix(System.IO.FileStream FS, int z, int y, int x, ref cMath.Vec[] v, int vPosition)
        {
            System.IO.BinaryWriter BW = new System.IO.BinaryWriter(FS);

            BW.Write("(\n");

            if (v != null)
            {
                for (int i = 0; i < z; i++)
                {
                    Write2DMatrix(FS, y, x, ref v, vPosition + i * x * y);
                }
            }

            BW.Write(")\n");
        }*/

        private struct SScript
        {
            public string FileName;
            public string Buffer;
            public int StartPosition;
            public int EndPosition;
            public int Line;
        }

    }
}
