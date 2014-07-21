# QuantConnect.com - QCAlgorithm Interface Class

## 1.0 Introduction

QuantConnect is a powerful flexible online backtesting platform capable of supporting any type of market data and performing time-series analysis. It uses the following " QCALGORITHM " base class to provide all the support tools for building a portfolio, managing your assets and performing trades.

Using this class you can access a powerful cluster of cloud machines which automatically scale to your demands. We provide a 4TB library of data for you to design financial strategies and test how your algorithm performs on historical market data.

To get started:
1. Clone the library and unzip it somewhere on your PC.
2. Open the solution in Visual Studio or Mono Develop (an open sourced C# IDE).
3. Modify the file "Algorithm.cs" files in QuantConnect.Algorithm Project which contains your algorithm code.
4. Compile the entire project, including all the support libraries.

This will output "QuantConnect.Algorithm.dll" which contains your algorithm. This can be uploaded to the QuantConnect cloud by committing it to your personal GIT URL. You may wish to modify the compile output directory to somewhere useful on your computer; such as the root directory of your project code. 

You can access this GIT URL inside the IDE, on the project tab. Firstly you'll need to upload your private SSH key. There is a full tutorial for using GIT inside QuantConnect located here: https://www.quantconnect.com/docs/Tutorials/Backtest-Strategies-Via-GIT

## Usage

The repository has several building blocks: Common, Interface and Algorithm.

* **Common** - Provides the basic security objects for accessing financial data, securities and math libraries. These methods are available all across your algorithm.

* **Interface** - Interaction interface pattern between the Algorithm and the QuantConnect LEAN back-testing engine.

* **Algorithm** - Underlying class handling the logistics of placing orders, event placeholders.

## Extensions

Feel free to extend this class and recommend additions / modifications to the API. We push updates to QC on a nightly basis so approved changes will be live within 48 hours.

## Full API Documentation

See http://www.quantconnect.com/docs for more information.

## License

Provided with the Apache 2.0 license.

## About QuantConnect

QuantConnect is seeking to democratize algorithmic trading through providing powerful tools and free financial data. With our online IDE engineers can design strategies in C#, and backtest them across 15 years of free high resolution financial data. Feel free to reach out to the QC Team -- contact@quantconnect.com
