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

using System.Net.Torrent.Misc;

namespace System.Net.Torrent.ProtocolExtensions
{
	public class FastExtensions : IProtocolExtension
	{
		public event Action<PeerWireClient, Int32> SuggestPiece;
		public event Action<PeerWireClient, Int32, Int32, Int32> Reject;
		public event Action<PeerWireClient> HaveAll;
		public event Action<PeerWireClient> HaveNone;
		public event Action<PeerWireClient, Int32> AllowedFast;

		public byte[] ByteMask
		{
			get { return new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04}; }
		}

		public byte[] CommandIDs
		{
			get 
			{ 
				return new byte[]
				{
					13,//Suggest Piece
					14,//Have all
					15,//Have none
					16,//Reject Request
					17 //Allowed Fast
				}; 
			}
		}

		public bool OnHandshake(PeerWireClient client)
		{
			return false;
		}

		public bool OnCommand(PeerWireClient client, int commandLength, byte commandId, byte[] payload)
		{
			switch (commandId)
			{
				case 13:
					ProcessSuggest(client, payload);
					break;
				case 14:
					OnHaveAll(client);
					break;
				case 15:
					OnHaveNone(client);
					break;
				case 16:
					ProcessReject(client, payload);
					break;
				case 17:
					ProcessAllowFast(client, payload);
					break;
			}

			return false;
		}

		private void ProcessSuggest(PeerWireClient client, byte[] payload)
		{
			Int32 index = Unpack.Int32(payload, 0, Unpack.Endianness.Big);

			OnSuggest(client, index);
		}

		private void ProcessAllowFast(PeerWireClient client, byte[] payload)
		{
			Int32 index = Unpack.Int32(payload, 0, Unpack.Endianness.Big);

			OnAllowFast(client, index);
		}

		private void ProcessReject(PeerWireClient client, byte[] payload)
		{
			Int32 index = Unpack.Int32(payload, 0, Unpack.Endianness.Big);
			Int32 begin = Unpack.Int32(payload, 4, Unpack.Endianness.Big);
			Int32 length = Unpack.Int32(payload, 8, Unpack.Endianness.Big);

			OnReject(client, index, begin, length);
		}

		private void OnSuggest(PeerWireClient client, Int32 pieceIndex)
		{
			if (SuggestPiece != null)
			{
				SuggestPiece(client, pieceIndex);
			}
		}

		private void OnHaveAll(PeerWireClient client)
		{
			if (HaveAll != null)
			{
				HaveAll(client);
			}
		}

		private void OnHaveNone(PeerWireClient client)
		{
			if (HaveNone != null)
			{
				HaveNone(client);
			}
		}

		private void OnReject(PeerWireClient client, Int32 index, Int32 begin, Int32 length)
		{
			if (Reject != null)
			{
				Reject(client, index, begin, length);
			}
		}

		private void OnAllowFast(PeerWireClient client, Int32 pieceIndex)
		{
			if (AllowedFast != null)
			{
				AllowedFast(client, pieceIndex);
			}
		}
	}
}
