﻿using Warcraft.NET.Files.Interfaces;
using System.IO;
using Warcraft.NET.Files.WDT.Entrys.BfA;

namespace Warcraft.NET.Files.WDT.Chunks.BfA
{
    /// <summary>
    /// MAID Chunk - Contains file ids for map files
    /// </summary>
    public class MAID : IIFFChunk, IBinarySerializable
    {
        /// <summary>
        /// Holds the binary chunk signature.
        /// </summary>
        public const string Signature = "MAID";

        public MAIDEntry[,] Entrys = new MAIDEntry[64,64];

        /// <summary>
        /// Initializes a new instance of the <see cref="MAID"/> class.
        /// </summary>
        public MAID()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MAID"/> class.
        /// </summary>
        /// <param name="inData">ExtendedData.</param>
        public MAID(byte[] inData)
        {
            LoadBinaryData(inData);
        }

        /// <inheritdoc/>
        public void LoadBinaryData(byte[] inData)
        {
            using (var ms = new MemoryStream(inData))
            using (var br = new BinaryReader(ms))
            {
                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        Entrys[x, y] = new MAIDEntry(br.ReadBytes(MAIDEntry.GetSize()));
                    }
                }
            }
        }

        /// <inheritdoc/>
        public string GetSignature()
        {
            return Signature;
        }

        /// <inheritdoc/>
        public uint GetSize()
        {
            return (uint)Serialize().Length;
        }

        /// <inheritdoc/>
        public byte[] Serialize(long offset = 0)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        bw.Write(Entrys[x, y].Serialize());
                    }
                }

                return ms.ToArray();
            }
        }
    }
}