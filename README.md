# DiceRollers
A Unity-based 2D RPG board game featuring a GoDice D20 Bluetooth dice integration that works both locally (via TCP) and online (via AWS cloud).
Built to test integrating GoDice to video game experience and use it in-game logic/events. Game systems built with modularity, scalability, data-driveness and editor-friendliness in mind.
<br/>[Link to companion app's & server's repository, used to integrate the GoDice to Unity.](https://github.com/Pyromaanisorsa/DiceRollerPython).

<img src="gameview.png" alt="Screenshot of the Unity project" width="810"/>
Figure: Screenshot of the Unity project.

## 🕹️ Overview

This project was designed to explore how to integrate physical game element (Bluetooth die) to a digital game world and test how effective and reliable the integration was.
Other goal was to test how the physical element affect the video game experience.

When players roll a connected dice, the roll result can be captured locally or through AWS, depending on the setup:
- Local mode: Game connects directly to a local Python TCP server and the server passes roll values to game.
- Online mode: Roll results are sent to AWS via a Python app, stored in DynamoDB, and retrieved by Unity using HTTP polling (UnityWebRequest).

## 🧱 Architecture Overview
![System Architecture](DiceIntegrationChart.png)
Figure: Data flow between Unity, AWS, and the Bluetooth dice via a Python bridge.

### Local TCP Mode (If dice connected locally)

1. Unity starts a local Python TCP server at runtime (executable).
2. Unity starts TCP-connection to the server.
3. Player sends message to server to connect the nearest GoDice.
4. The server listens for dice state updates from the connected GoDice device.
5. Roll results are streamed to the game instantly whenever the dice state changes, allowing near-zero latency play.
6. Game uses received roll value only when a local roll is currently active and waiting for roll result.

### Cloud AWS Mode (If no dice connected locally)
1. Unity uses AWS API Gateway (HTTP) to access Lambda endpoints:
- requestRoll – creates a roll request entry in DynamoDB (primary key = playerID)

2. Python app connects to Bluetooth dice and sends results to AWS tagged with the player’s ID.
- submitRollResult – used by the Python app to submit the roll outcome

3. Unity polls for while until it receives the final roll value, then applies it in-game.
- checkRollResult – polled by Unity to check if a result is available for roll request

Unity starts polling for result right after request creating was successful. Players have x-amount of time to send the rollValue via Python App before polling timer runs out.

If Unity doesn't receive the result in time in either mode; it will randomly generate number between 1-20 and use that as roll result for game logic.

For more details on how the Python TCP server & app work, check their own [repository](https://github.com/Pyromaanisorsa/DiceRollerPython).

## 🧩 Game Systems
AbilityData Structure
- Component-driven architecture using classes like AbilityFlow, AbilityShape, and AbilityBehaviour.
- New abilities can be added simply by composing components rather than writing new code for each ability.
- Currently each ability is usable by both players and enemy AI.

Level Editor Tools
- Custom Unity Editor window visualizes level tiles and blocking areas.
- Designers can mark blocked tiles interactively, which are then serialized into level data.

EnemyAI
- Very simple currently: either moves closer to player until in range of their single ability or uses their ability to attack.
- Future proofing: enemy's have instance of enemyBehaviourAI class that could be implement to affect their decision making eg. fear or anger value.

Data Structure
- Modular component based (playerCombat -> playerStats -> abilitySlots -> abilityData)
- Manager classes (eg. CombatManager, GridManager) manage big parts of the game, which are singletons so any object can use them.
- Room for new features and improvements eg. confirm/decline feature for moving/using ability, abilityData structure, abilityFlow structure (currently it just decides which tiles can be chosen, but could be used to make more complicated ability targeting modes eg. multi tile target select)
- Also contains unused Node-based dialogue system.

Manager Classes
- RollManager: starts and runs diceRoll events in game logic
- CombatManager: manages turn flow and order, also every action goes through this manager
- GoDiceManager: starts the local TCP-server to connect GoDice locally and communicate with it
- GridManager: manages game board and tiles states, also helps abilities by giving list of targetable tiles based of abilityData parameters + abilityShape component

## 🧩 Running the game
1. Clone the repo and open the Unity project.
2. Press Play in Unity Editor or build a build of the project and run that to start the game.
3. Editor / build will start the server at start with GoDiceManager isntance.
4. Type 3-20 characters long playerID to use ingame and select one of the 3 classes.
5. Connect the dice BEFORE SELECTING DIFFICULTY with the connect button at top left corner of the screen. (if planning to use local dice)
6. Click on one of the 5 difficulty buttons to spawn enemies and start combat.
7. Play the game!

## 🧩 Building / enabling AWS Cloud dice roll option (Companion App required for this OR write simple script to simulate dice rolls to sent to AWS)
1. Create 3 lambda functions (codes in /LambdaCode folder) and create & add to them IAM role(s) to full access to DynamoDB (AmazonDynamoDBFullAccess).
2. Create HTTP API-gateway and create a route for each Lambda function, connect Lambda function trigger to these routes.
3. In Scripts/Managers/RollManager.cs add urls to your requestRoll and checkRollResult gateway routes (rows 48 & 49)
````
    private static string requestRollUrl = "YOUR-REQUESTROLL HTTP GATEWAY URL+ROUTE";
    private static string checkResultUrl = "YOUR-CHECKROLLRESULT HTTP GATEWAY URL+ROUTE";
````
4. In the companion app's app.py file add urls to your submitRollResult gateway route (row 10)
5. ````
   apiurl = "YOUR-SUBMITROLLRESULT HTTP GATEWAY URL+ROUTE"
   ````
6. Create DynamoDB table called RollRequests (if not that name; you have to rename RollRequests in Lambda function code). Make partition key playerID (S).
7. You should now have AWS backend set for sending dice rolls to game via AWS.

## 🧾 License

This project is licensed under the **Creative Commons Attribution–NonCommercial 4.0 International License (CC BY-NC 4.0)**.

You may use and modify this project freely for **personal, educational, or research purposes**, provided that you give credit to the author.  
**Commercial use is strictly prohibited.**

### GoDice Python API Notice
This project includes an executable that uses the **GoDicePythonAPI** by **ParticulaCode.**  
The GoDice API is proprietary and licensed for **personal, academic, and non-commercial use only** under its own license terms.  
See the GoDice API License Agreement for details.
