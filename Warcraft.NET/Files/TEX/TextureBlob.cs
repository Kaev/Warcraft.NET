﻿using System.Collections.Generic;
using System.IO;
using Warcraft.NET.Attribute;
using Warcraft.NET.Extensions;
using Warcraft.NET.Files.TEX.Chunks;
using Warcraft.NET.Files.TEX.Entrys;
using Warcraft.NET.Files.TEX.Flags;

namespace Warcraft.NET.Files.TEX
{
    public class ModelBlob : ChunkedFile
    {
        /// <summary>
        /// Gets or sets the texture blob version.
        /// </summary>
        [ChunkOrder(1)]
        public TXVR Version { get; set; }

        /// <summary>
        /// Gets or sets the blob texture.
        /// </summary>
        [ChunkOrder(2)]
        public TXBT BlobTexture { get; set; }

        /// <summary>
        /// Gets or sets the texture data.
        /// </summary>
        [ChunkOrder(3), ChunkArray]
        public TXMD[] TextureData { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBlob"/> class.
        /// </summary>
        public ModelBlob()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBlob"/> class.
        /// </summary>
        /// <param name="inData">The binary data.</param>
        public ModelBlob(byte[] inData)
        {
            LoadBinaryData(inData);

            using (var ms = new MemoryStream(inData))
            using (var br = new BinaryReader(ms))
            {
                // Get txmd start offset
                br.SeekChunk(TXMD.Signature, true, true);
                long txmdStartOffset = ms.Position -= sizeof(uint);

                foreach (TXBTEntry entry in BlobTexture.Entries)
                {
                    ms.Seek(txmdStartOffset + entry.TXMDOffset, SeekOrigin.Begin);
                    entry.TextureData = br.ReadIFFChunk<TXMD>(false, false);
                }
            }
        }

        /// <summary>
        /// Serializes the current object into a byte array.
        /// </summary>
        /// <returns>The serialized object.</returns>
        public new byte[] Serialize(long offset = 0)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.WriteIFFChunk(Version);

                int txbtStartOffset = (int)ms.Position;
                bw.Seek(BlobTexture.Entries.Count * TXBTEntry.GetSize() + 8, SeekOrigin.Current);

                // We must first write TXMD chunk to get the correct offsets for txbt
                uint txmdStartOffset = (uint)ms.Position;

                foreach (TXBTEntry entry in BlobTexture.Entries)
                {
                    entry.TXMDOffset = (uint)ms.Position - txmdStartOffset;
                    bw.WriteIFFChunk(entry.TextureData);
                }

                // Now we can write txbt with the correct offsets
                bw.Seek(txbtStartOffset, SeekOrigin.Begin);
                bw.WriteIFFChunk(BlobTexture);

                return ms.ToArray();
            }
        }
    }
}
