# BlockchainESD

Blockchain: Elements of Software Development

An example blockchain application for FTN NS.

## How everything works:

In this example we have the smart contract work both as a restful and a websocket server. The restful server is for clients while the websocket server is for miners. 

![Network Topology](https://raw.githubusercontent.com/zastrixarundell/blockchain-esd/master/blockchain.jpeg)

## Clients

Clients are created to just send a JSON body to the server. When the client sends the request it waits a couple of seconds for it to be processed and it has a value returned whether the request was okay or not.

## Miners 

Miners are connected to the websocket server for instant transmission of data. When a client requests information it is distrubuted/broadcasted accross all of the nodes. The miners will then send the information to the server to verify data. If all of the miners agree with the information then it means everything was correct. If one of the miners does not agree it is reprocessed.

On successful processing, the initial sender is saved and rewarded with the currency. Afterwards the blockchain is appended with the transaction.

## Data examples:

    Example data: L9BMZ76TAHIDCZFXP7IH
    Example hash: 000f04a09c3c85d41194fa27f140360946e3c003e26b457ea4908af747e70bf4

    Example data: AJRHT1U53HZWN1XYVZPJ
    Example hash: 00004ef2dead454b2291d41d9811aa83c450a9f1f4d75f746dbb2d24c0d9dbec

    Example data: WDTYPNHQIXDO7BM4TJB0
    Example hash: 00073f3775ca4687305cba27a91e79166dcc8a95abb96a79f250789df18788c3

    Example data: PPVEE9L41ZIXV1UNAYF1
    Example hash: 000aa3e6e3167849f7962b0867aeed52334e8332c35d0f7d0f2dbf2822bb2436

    Example data: 4KYVRYYMU20IY8VQWNK9
    Example hash: 00099fbf4b808cc9b212111b56bd0beb471543bbe224ae3e052683beb786459b

    Example data: 6C6MZQ722MDD3E6SWPGN
    Example hash: 000b7d4e6fbebbc4c07be7268d4d5d636766708aeca744a67443b18077796573

    Example data: 81Z5R6ERD8D10GFIKDL2
    Example hash: 0000c48498c0424864a10d8e567a692587b70afc49cf17ed51b71fbf9b094e77

    Example data: 5W3FSY0S5PL1IY3ZAP9Q
    Example hash: 00027a02177b2e3b506a53b8f538daf58c6342cda1d99936ff3508fc1fcd964b

    Example data: ATQ900MP53HJKJTTO5ED
    Example hash: 000812ec54d6bba02743b8c2f10d97332dbfea6980154d04441fa896fcdc06f3

    Example data: KTEKLQ53JNCZYI9OHATX
    Example hash: 00098a3a978d7c0a6d725d19c788426a1468c3c7e371469d0507fe18e72cc423

## Generating codes via Python

```python
import hashlib
import string
import random

m = hashlib.sha256()

dict = {}

while len(dict) < 10:
	generated = ''.join(random.choices(string.ascii_uppercase + string.digits, k = 20))
	hashed_string = hashlib.sha256(generated.encode('utf-8')).hexdigest()
	if hashed_string[0:3] == "000":
		dict[generated] = hashed_string
		print("Example data: " + generated)
		print("Example hash: " + hashed_string + "\n")
```

## Postman

Under `Postman` there is a JSON file for all of the rest requests. Unfortunately it does not allow exporting websocket connects but you just need to connect to:

    localhost:5067/miners/connect/ws

And the following information needs to be sent to connect to the server:

```json
{
    "topic": "miner",
    "event": "join"
}
```

And to leave the channel:

```json
{
    "topic": "miner",
    "event": "leave"
}
```

Send result from miner to the server after connection:

```json
{
    "topic": "miner",
    "event": "result",
    "data": {
        "data": "asdfasdfasdfasdfsafsfd",
        "user": "string",
        "result": "57aca8c268cf7a8ef33f542479db673c063d8535646f24c56275f32636d298b3"
    }
}
```