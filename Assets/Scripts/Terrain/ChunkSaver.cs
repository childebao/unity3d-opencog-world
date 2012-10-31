using System;
using System.IO;
using System.Runtime.Remoting.Channels;
//using System.Runtime.Remoting.Channels.Tcp;
using Ionic.Zip;
//using Remoting;
using UnityEngine;

namespace Irrelevant.Assets.Scripts.Terrain
{
    public class ChunkSaver : MarshalByRefObject
    {
        public void CompressChunk(Chunk chunk)
        {

//            ChannelServices.RegisterChannel(new TcpClientChannel(), false);
//            Tcp.RemoteServer remoteServer =
//                (RemoteServer)
//                Activator.GetObject(typeof (RemoteServer), "tcp://localhost:9998/MineFoundations");
//            remoteServer.SaveChunk(chunk);
            

            //MemoryStream memoryStream = new MemoryStream(chunk.Blocks.Length);
            //for (int z = 0; z < chunk.Blocks.GetUpperBound(2); z++)
            //{
            //    for (int y = 0; y < chunk.Blocks.GetUpperBound(1); y++)
            //    {
            //        for (int x = 0; x < chunk.Blocks.GetUpperBound(0); x++)
            //        {
            //            memoryStream.WriteByte((byte)chunk.Blocks[x, y, z].Type);
            //            memoryStream.WriteByte(chunk.Blocks[x, y, z].LightAmount);
            //        }
            //    }
            //}

            //memoryStream.Seek(0, SeekOrigin.Begin);
            //MemoryStream outputStream = new MemoryStream();

            //using (ZipFile zipFile = new ZipFile())
            //{
            //    zipFile.AddEntry(chunk.ToString(), memoryStream);
            //    zipFile.Save(outputStream);
            //}

            //outputStream.Seek(0, SeekOrigin.Begin);

            //byte[] bytes = outputStream.ToArray();
            //string path = Application.dataPath + @"/chunkzip.zip";

        }
    }
}