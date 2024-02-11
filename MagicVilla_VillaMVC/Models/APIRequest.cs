﻿using System.Net;
using static Utility.SD;

namespace MagicVilla_VillaMVC.Models
{
    public class APIRequest
    {
        public ApiType ApiType { get; set; } = ApiType.GET;
        public string Url { get; set; }
        public object Data { get; set; }

    }
}
