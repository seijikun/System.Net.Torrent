﻿/*
Copyright (c) 2013, Darren Horrocks
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this
  list of conditions and the following disclaimer in the documentation and/or
  other materials provided with the distribution.

* Neither the name of Darren Horrocks nor the names of its
  contributors may be used to endorse or promote products derived from
  this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Torrent.Misc;

namespace System.Net.Torrent
{
	public class MagnetLink
	{
		public MagnetLink()
		{
			Trackers = new Collection<string>();
		}

		public String Name { get; set; }
		public byte[] Hash { get; set; }

		public String HashString
		{
			get { return Unpack.Hex(Hash); }
			set { Hash = Pack.Hex(value); }
		}

		public ICollection<String> Trackers { get; set; }

		public static MagnetLink Resolve(String magnetLink)
		{
			IEnumerable<KeyValuePair<String, String>> values = null;

			if (IsMagnetLink(magnetLink))
			{
				values = SplitURLIntoParts(magnetLink.Substring(8));
			}

			if (values == null) return null;

			MagnetLink magnet = new MagnetLink();

			foreach (KeyValuePair<string, string> pair in values)
			{
				if (pair.Key == "xt")
				{
					if (!IsXTValidHash(pair.Value))
					{
						continue;
					}

					magnet.HashString = pair.Value.Substring(9);
				}

				if (pair.Key == "dn")
				{
					magnet.Name = pair.Value;
				}

				if (pair.Key == "tr")
				{
					magnet.Trackers.Add(pair.Value);
				}
			}

			return magnet;
		}

		public static Metadata ResolveToMetadata(String magnetLink)
		{
			return new Metadata(Resolve(magnetLink));
		}

		public static bool IsMagnetLink(String magnetLink)
		{
			return magnetLink.StartsWith("magnet:");
		}

		private static bool IsXTValidHash(String xt)
		{
			return xt.Length == 49 && xt.StartsWith("urn:btih:");
		}

		private static IEnumerable<KeyValuePair<String, String>> SplitURLIntoParts(String magnetLink)
		{
			String[] parts = magnetLink.Split('&');
			ICollection<KeyValuePair<String, String>> values = new Collection<KeyValuePair<string, string>>();

			foreach (String str in parts)
			{
				String[] kv = str.Split('=');
				values.Add(new KeyValuePair<string, string>(kv[0], Uri.UnescapeDataString(kv[1])));
			}

			return values;
		}
	}
}
