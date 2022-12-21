using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartContract;
using SmartContract.Controllers;
using SmartContract.Services;
using SmartContract.Services.Interfaces;
using Xunit;

namespace SmartContractTests.Api;

public class UserControllerTest : IDisposable
{
    private readonly UserController _controller;
    private IUserService _service;

    public UserControllerTest()
    {
        _controller = new UserController();
        _service = new UserService();
    }

    public void Dispose()
    {
        _controller.Dispose();
    }

    private User GenerateUser(string id = "id", string data = "random data")
    {
        return new User { Id = id, Timestamp = DateTime.Now, Data = data };
    }

    [Fact]
    public void AddUsersToServer()
    {
        var user = GenerateUser();
        var response = (ObjectResult) _controller.Create(user);
        Assert.Equal(201, response.StatusCode);
        
        user = GenerateUser("NewId");
        response = (ObjectResult) _controller.Create(user);
        Assert.Equal(201, response.StatusCode);
    }

    [Fact]
    public void CanNotAddTwoUsersWithSameId()
    {
        var user = GenerateUser();
        _controller.Create(user);
        
        user = GenerateUser();
        var response = (ObjectResult) _controller.Create(user);
        
        Assert.NotEqual(201, response.StatusCode);
    }

    [Fact]
    public void CanListExistingUsers()
    {
        for (var id = 0; id < 5; id++)
        {
            var user = GenerateUser($"UID:{id}");
            _controller.Create(user);

            var response = (ObjectResult)_controller.Index();

            Assert.NotNull(response.Value);
            
            var jsonObject = JObject.Parse(response.Value.ToString());

            List<User> userList = JsonConvert.DeserializeObject<List<User>>(jsonObject["users"].ToString());
            
            Assert.Equal(id + 1, userList.Count);
        }
        
    }
}