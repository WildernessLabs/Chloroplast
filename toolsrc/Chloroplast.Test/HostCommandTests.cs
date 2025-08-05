using System.Net;
using System.Net.Sockets;
using Xunit;
using Chloroplast.Tool.Commands;

namespace Chloroplast.Test
{
    public class HostCommandTests
    {
        [Fact]
        public void FindAvailablePort_ReturnsAvailablePort()
        {
            // Arrange
            var hostCommand = new HostCommand();

            // Act
            var port = hostCommand.FindAvailablePort(5000);

            // Assert
            Assert.True(port >= 5000);
            Assert.True(port <= 5100);
            
            // Verify the port is actually available
            using var tcpListener = new TcpListener(IPAddress.Loopback, port);
            tcpListener.Start(); // Should not throw
            tcpListener.Stop();
        }

        [Fact]
        public void FindAvailablePort_WithPortInUse_FindsNextAvailablePort()
        {
            // Arrange
            var hostCommand = new HostCommand();
            var firstListener = new TcpListener(IPAddress.Loopback, 5000);
            firstListener.Start();

            try
            {
                // Act
                var port = hostCommand.FindAvailablePort(5000);

                // Assert
                Assert.Equal(5001, port);
                
                // Verify the port is actually available
                using var tcpListener = new TcpListener(IPAddress.Loopback, port);
                tcpListener.Start(); // Should not throw
                tcpListener.Stop();
            }
            finally
            {
                firstListener.Stop();
            }
        }
    }
}