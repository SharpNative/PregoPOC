using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Prego.Net
{
    class TcpConnectionListener
    {
        private TcpListener Listener;
        private Boolean IsListening;
        private AsyncCallback ConnectionReqCallback;

        private TcpConnectionManager Manager;
        private TcpConnectionFactory Factory;

        public Boolean isListening
        {
            get
            {
                return this.isListening;
            }
        }


        public TcpConnectionListener(string LocalIp, int Port, TcpConnectionManager Manager)
        {
            IPAddress IP = null;

            if (!IPAddress.TryParse(LocalIp, out IP))
            {
                IP = IPAddress.Loopback;
            }

            this.Listener = new TcpListener(IP, Port);
            this.ConnectionReqCallback = new AsyncCallback(OnRequest);
            this.Factory = new TcpConnectionFactory();
            this.Manager = Manager;
        }

        public void Start()
        {
            if (IsListening)
            {
                return;
            }

            Listener.Start(50);
            IsListening = true;

            WaitForNextConnection();
        }

        public void Stop()
        {
            if (!IsListening)
            {
                return;
            }

            IsListening = false;
            Listener.Stop();
        }

        public void Destroy()
        {
            try
            {
                Stop();

                Listener = null;
                Manager = null;
                Factory = null;
                GC.Collect();
            }
            catch
            {

            }
            finally
            {

            }
        }

        private void WaitForNextConnection()
        {
            if (IsListening)
                Listener.BeginAcceptSocket(ConnectionReqCallback, null);
        }

        private void OnRequest(IAsyncResult iAr)
        {
            try
            {
                Socket Sock = Listener.EndAcceptSocket(iAr);
                

                TcpConnection Connection = Factory.CreateConnection(Sock);

                if (Connection != null)
                {
                    Manager.HandleNewConnection(Connection);
                }
            }

            catch (Exception e)
            {

            }

            finally
            {
                if (IsListening)
                {
                    WaitForNextConnection();
                }
            }
        }
    }
}
