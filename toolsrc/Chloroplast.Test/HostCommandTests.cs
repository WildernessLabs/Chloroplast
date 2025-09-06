using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Chloroplast.Tool.Commands;
using Chloroplast.Core;

namespace Chloroplast.Test
{
    public class HostCommandTests
    {
        private class MockHostCommand : HostCommand
        {
            private readonly HashSet<int> occupied;
            public MockHostCommand(IEnumerable<int> occupiedPorts)
            {
                occupied = new HashSet<int>(occupiedPorts);
            }
            protected override bool ProbePort(int port) => !occupied.Contains(port);
        }

        [Fact]
        public void FindAvailablePort_SkipsOccupiedPorts()
        {
            var mock = new MockHostCommand(new []{5000,5001,5002});
            var port = mock.FindAvailablePort(5000);
            Assert.Equal(5003, port);
        }

        [Fact]
        public void FindAvailablePort_ReturnsStartIfFree()
        {
            var mock = new MockHostCommand(Array.Empty<int>());
            var port = mock.FindAvailablePort(7000);
            Assert.Equal(7000, port);
        }

        [Fact]
        public void FindAvailablePort_ThrowsWhenRangeExhausted()
        {
            // Occupy full search window of 3 ports for small test by narrowing window via start/end assumptions
            // We'll simulate by occupying start..start+200
            var occupied = Enumerable.Range(8000, 201).ToArray();
            var mock = new MockHostCommand(occupied);
            Assert.Throws<ChloroplastException>(() => mock.FindAvailablePort(8000));
        }
    }
}