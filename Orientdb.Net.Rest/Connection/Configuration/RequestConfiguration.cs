﻿using System;
using System.Collections.Generic;
using Orientdb.Net.Connection.Security;

// ReSharper disable CheckNamespace
namespace Orientdb.Net.Connection
// ReSharper restore CheckNamespace
{
    public class RequestConfiguration : IRequestConfiguration
	{
		public int? RequestTimeout { get; set; }
		public int? ConnectTimeout { get; set; }
		public string ContentType { get; set; }
		public int? MaxRetries { get; set; }
		public Uri ForceNode { get; set; }
		public bool? DisableSniff { get; set; }
		public bool? DisablePing { get; set; }
		public IEnumerable<int> AllowedStatusCodes { get; set; }
        public BasicAuthenticationCredentials BasicAuthenticationCredentials { get; set; }
		public bool EnableHttpPipelining { get; set; }
    
         
    }
}