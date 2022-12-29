using System;
using Microsoft.AspNetCore.Mvc;
using SmartContract;
using SmartContract.Controllers;
using Xunit;

namespace TestProject1.Api;

public class UserControllerTest
{
    private readonly UserController _controller;

    public UserControllerTest()
    {
        _controller = new UserController();
    }
    
    private User GenerateUser(string id = "id", string data = "random data")
    {
        return new User { Id = id, Timestamp = DateTime.Now, Data = data };
    }

    [Fact]
    public void AddUserToServer()
    {
        var user = GenerateUser();
        var response = (ObjectResult)_controller.Create(user);
        
        Assert.Equal(201, response.StatusCode);

        user = GenerateUser("New ID");
        response = (ObjectResult)_controller.Create(user);
        
        Assert.Equal(201, response.StatusCode);
    }
}