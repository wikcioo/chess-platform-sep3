# Distributed Chess Application
![Application screenshot](Docs/Screenshots/GamePlay.png?raw=true)

## Intruduction
This project aims to provide students at a university with the opportunity to connect and
participate in chess tournaments, regardless of their location. To fulfill the client’s
request, the system ended up consisting of a Blazor Server providing access to GUI
through a browser, a C# Communication Server handling client connection using SignalR
and REST, a “Stockfish” Server that returns moves imitating chess opponents with
adjustable difficulty through gRPC, and Java Database Access Server for storing user
data and REST API. The result is a responsive and expandable application where users
can play between themselves or practice against an AI and documentation that explores
how to further expand system functionality that is relevant to the client.

## Documentation
* Project and Process Report
* Appendices
* Screenshots

## License
[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
