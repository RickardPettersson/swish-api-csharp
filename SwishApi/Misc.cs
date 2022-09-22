﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwishApi
{
    public class Misc
    {
        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
