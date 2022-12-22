# BlockchainESD

An example blockchain application for FTN NS.

## How everything works:

In this example we have the smart contract work both as a restful and a websocket server. The restful server is for clients while the websocket server is for miners. 

## Clients

Clients are created to just send a JSON body to the server. When the client sends the request it waits a couple of seconds for it to be processed and it has a value returned whether the request was okay or not.

## Miners 

Miners are connected to the websocket server for instant transmission of data. When a client requests information it is distrubuted/broadcasted accross all of the nodes. The miners will then send the information to the server to verify data. If all of the miners agree with the information then it means everything was correct. If one of the miners does not agree it is reprocessed.

On successful processing, the initial sender is saved and rewarded with the currency. Afterwards the blockchain is appended with the transaction.

## How everything looks:

![Network Topology](https://raw.githubusercontent.com/zastrixarundell/blockchain-esd/master/blockchain.jpeg)