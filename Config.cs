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
using System.Configuration;
using System.Text;

namespace Q2BSP
{
    public class CConfig
    {
        /// <summary>
        /// Gets the value of a configuration setting key and attempts to return its value as a boolean
        /// </summary>
        /// <param name="KeyName">The configuration setting key</param>
        /// <returns>Returns configuration setting key value as a boolean</returns>
        public static bool GetConfigBOOL(string KeyName)
        {
            bool KeyValue;

            try
            {
                KeyValue = Convert.ToBoolean(ConfigurationManager.AppSettings[KeyName].ToString());
            }
            catch
            {
                KeyValue = false;
            }

            return KeyValue;
        }

        /// <summary>
        /// Gets the value of a configuration setting key and attempts to return its value as a string
        /// </summary>
        /// <param name="KeyName">The configuration setting key</param>
        /// <returns>Returns configuration setting key value as a string</returns>
        public static string GetConfigSTRING(string KeyName)
        {
            string KeyValue;

            try
            {
                KeyValue = ConfigurationManager.AppSettings[KeyName].ToString();
            }
            catch
            {
                KeyValue = null;
            }

            return KeyValue;
        }

    }
}
