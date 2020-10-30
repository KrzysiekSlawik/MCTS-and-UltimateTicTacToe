# MCTS
## Environment
| Language |c#  |
|----------|--|
| where    |self written testing multithreaded program ran on AMD Ryzen 5 4500U |
|number of samples per test | at least 40 games played per parameter value (not including tests ran while developing)

## flat MC and MCTS(silly edition) with UCT selection policy and random simulation policy
### MCTS
 Number of simulations from initial state 22272 / 1s.
 I expected to get at least twice as much. I believe this low number of simulations is caused by both implementation (perhaps some silly mistakes) and language used.
 ### Flat MC
 number of simulations from initial state 323001 / 1s.
 Surprisingly even thou Flat MC didn't get much more simulations it drastically out performed my MCTS. I think flat MC is less dependent on number of simulations thanks to higher exploration
 ```
 ================
TESTS MCTS vs FLAT
MCTS c=1
first won: 67
second won: 133

MCTS c=1.25
first won: 67
second won: 133

MCTS c=1.5
first won: 76
second won: 124

MCTS c=1.75
first won: 77
second won: 123 

================
TESTS MCTS vs FLAT
MCTS c=1.9
first won: 76
second won: 124

MCTS c=2.05
first won: 84
second won: 116

MCTS c=2.2
first won: 76
second won: 124

MCTS c=2.35
first won: 87
second won: 113
```
Both agents won with random player 200 - 0 which proves that both implementations are kind of working... [for some reason at this point I realized how badly I backpropagate)
## flat MC and MCTS with proper backpropagation
I somehow missed that MCTS selection phase should always choose best move from perspective of current players turn. Making this improvement improved win rate by a lot. (previously I'd choose best move from perspective of MCTS player)
 ```
 ================
TESTS MCTS vs FLAT
MCTS c=1.9
first won: 28
second won: 12

MCTS c=2.05
first won: 31
second won: 8

MCTS c=2.2
first won: 29
second won: 11

MCTS c=2.35
first won: 26
second won: 14
```
Smaller batch of tests due to lack of time and obvious change of results.
Changes in backpropagation had no impact on number of simulations in initial state, but increased average effective number of simulations by better predicting opponent's moves. (which allowed using previous branches of tree to be used in next turn)
   ```
   29484
43859
35734
46910
13637
63519
19783
30788
46793
64679
22266
3014
2000
2503
```
In first moves we can see that MCTS foreseen opponent's choices which led to higher  effective number of simulations in second turn than in first. It should be mentioned that in second turn our agent had only 100ms for making his move which is 10 times less than from initial state. Some peaks are even suspicious. (this is not average case so unexpected peaks can happen)
## MCTS with MAST (compared to MC flat)
### without storing MAST knowledge between states
MAST improvement for sure improves MCTS. My results in games vs MC flat:
```
================
TESTS MCTS(MAST) vs FLAT
MCTS c=2.05, e=0.4
first won: 67
second won: 13

MCTS c=2.05, e=0.3
first won: 65
second won: 15

MCTS c=2.05, e=0.2
first won: 73
second won: 7

MCTS c=2.05, e=0.1
first won: 62
second won: 18
```
MAST is perfectly applicable to UTTT as in this game there are some universal favorable positions just like in TTT. (like corners and middles (I think))
### MAST using all previous calculations without decay
Overwhelmingly high number of simulations that are all treated equally makes MAST almost ignore precious knowledge gained in second half of the game, which makes this version perform worse than MASTless MCTS.
```
================
TESTS MCTS(MAST) vs FLAT
MCTS c=2.05, e=0.4
first won: 58
second won: 22

MCTS c=2.05, e=0.3
first won: 58
second won: 22

MCTS c=2.05, e=0.2
first won: 57
second won: 23

MCTS c=2.05, e=0.1
first won: 44
second won: 35
```
### MAST using all previous calculations with decay
Thanks to decay MAST should be able to change it's mind on some of the moves depending on game state. I'm using simple Decay each records weight is decreased 100 times every turn. 100 is totally magic number and should be deducted with trial and error just as with other parameters. 
```
================
TESTS MCTS(MAST) vs FLAT
MCTS c=2.05, e=0.4
first won: 70
second won: 10

MCTS c=2.05, e=0.3
first won: 56
second won: 24

MCTS c=2.05, e=0.2
first won: 69
second won: 11

MCTS c=2.05, e=0.1
first won: 67
second won: 13 
```
Even without fine parameter tuning decay allows MAST to properly use knowledge gained throughout the game.
