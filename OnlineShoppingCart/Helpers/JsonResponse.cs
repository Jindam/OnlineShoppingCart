﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace OnlineShoppingCart.Helpers
{
    public enum ServiceResponseStatus
    {
        Success,
        VaslidationError,
        Error
    }
    public class ServiceResponseVM
    {
        public bool Success { get; set; }
        public object Data { get; set; }
        public int count { get; set; }
    } 
}
