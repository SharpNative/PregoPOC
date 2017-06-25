using Azione.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using Prego.Net;
using Azione;

namespace Prego.Compositor
{

    class CompositorPacketFS
    {
        private SharedQueue mGeneralQueue;


        public Dictionary<int, PacketFSSession> Sessions = new Dictionary<int, PacketFSSession>();
        private Compositor mComp;

        private Task mListenTask;

        private int mCurrentNum = 0;
        
        public CompositorPacketFS(Compositor comp)
        {
            mComp = comp;

            mGeneralQueue = new SharedQueue(@"Global\comp", 10, true);
            mListenTask = new Task(() => ListenForConnections());
            mListenTask.Start();

            Sessions = new Dictionary<int, PacketFSSession>();
        }

        public void ListenForConnections()
        {
            while(true)
            {
                byte[] buf = mGeneralQueue.Read();
                
                int pid = mCurrentNum++;

                PacketWriter writer = new PacketWriter();
                writer.Write(pid);

                new PacketFSSession(pid, this, mComp);

                mGeneralQueue.Write(writer.GetBytes());

            }

        }

        /// <summary>
        /// Request window ID
        /// </summary>
        /// <param name="windowID"></param>
        /// <param name="session"></param>
        public void RegisterWindowID(int windowID, PacketFSSession session)
        {
            Sessions.Add(windowID, session);
        }
    }
}
