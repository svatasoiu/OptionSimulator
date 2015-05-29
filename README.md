Option Simulator - Done as a weekend hack to refresh my C# knowledge

Features include
- computing custom payoff functions (that may include pathwise terms like an Arithmetic Asian Call Option)
- changing the stock price simulation model (default is GBM; Heston model doesn't work for now)
- computing typical European call/put options
- simulating correlated stocks (however, no UI for inputting covariances yet)
- changing number of intervals in simulated path
- adjusting number of trials

Packages Used
- MathNet.Numerics.3.7.0
- ncalc.1.3.8
- Oxyplot.(Core|Wpf).2014.1.546

![ScreenShot](/images/new_sample_option_sim.PNG "Example simulation of Arithmetic Asian Call Option")