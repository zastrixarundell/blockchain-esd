using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json.Nodes;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartContract;
using SmartContract.Controllers;
using Xunit;

namespace SmartContractTests.Api
{
    public class MinerSocketControllerTest
    {
        private readonly MinerSocketController _controller;

        public MinerSocketControllerTest()
        {
            _controller = new MinerSocketController();
        }

        [Fact]
        public void IndexTest()
        {
            var response = (ObjectResult)_controller.Index();

            Assert.NotNull(response.Value);

            var jsonObject = JsonObject.Parse(response.Value.ToString());

            Assert.IsType<Int32>(Convert.ToInt32(jsonObject["miners"].ToString()));
        }

        [Fact]
        public void BroadcastTest()
        {
            var response = _controller.Broadcast() as OkResult;
            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
        }
    }
}