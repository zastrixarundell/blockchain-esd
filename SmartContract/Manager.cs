using SmartContract.Channels;
using SmartContract.Channels.Bases;
using SmartContract.Services;
using SmartContract.Services.Interfaces;

namespace SmartContract;

public static class Manager
{
    public static readonly ChannelBase MinerChannel = new MinerChannel();
    public static readonly IUserService UserService = new UserService();
}