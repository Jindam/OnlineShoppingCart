﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnlineShoppingCart.Helpers
{
    public static class JsonFileReader
    {
        // read json file
        public static List<T> ReadAsync<T>(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                return JsonConvert.DeserializeObject<List<T>>(reader.ReadToEnd());
            }
        }

    }
}
