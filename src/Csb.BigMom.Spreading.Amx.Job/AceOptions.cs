using Renci.SshNet;
using System;

namespace Csb.BigMom.Spreading.Amx.Job
{
    public class AceOptions
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public int TimeoutSeconds { get; set; }

        public int FailureThreshold { get; set; }

        public int FailureRetryDelaySeconds { get; set; }

        public string Command { get; set; }

        public SshClient CreateSshClient() => new(new ConnectionInfo(Host, Username, new PasswordAuthenticationMethod(Username, Password))
        {
            Timeout = TimeSpan.FromSeconds(TimeoutSeconds),
            ChannelCloseTimeout = TimeSpan.FromSeconds(TimeoutSeconds)
        });
    }
}
