# Microsoft Rules Engine Evaluation

<div align="center">

A small demo project for the Microsoft Rules Engine

</div>

---

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

## General Rules Concepts

The concepts surrounding rules engines are largely common across all implementations. The guiding concept is the separation of business rules from the broader logic of the code, allowing for the rules, and by extension the logic of the system using the rules, to be configured dynamically without necessitating a full redeployment of the system.

The primary components to be considered are:

* The rules file, often a JSON formatted document that contains one or more rules definitions.
* The message / data being evaluated. This originates in the hosting system and forms the basis of the information against which the rules are evaluated.
* The rules engine which is responsible for the actual execution of the process by which the rules are applied to the information.
* The host system which instantiates the rules engine, is responsible for routing data for evaluation, and importantly is responsible for interpreting the results.

## Basic Rules Definition

Simple rules allow for basic binary rules to be executed with a true / false response representing the success or failure of the rule.

The basic rules definition in the Microsoft Rules Engine are in JSON format. The engine does not mandate a particular storage / retrieval approach for the rules, leaving room for a variety of implementations. The basic structure is:

```json
[
    {
        "WorkflowName": "Basic",
        "Rules": [
            {
                "RuleName": "CheckCountry",
                "RuleExpressionType": "LambdaExpression",
                "Expression": "input1.IncorporationCountryCode == \"IE\" || input1.IncorporationCountryCode == \"FR\"",
            }
        ]
    }
]
```

As presented above, the rules file comprises one or more rules gathered under a ```WorkflowName```. This workflow name is used to identify which rules are to be applied by the rules engine.

### Fields

| Field Name             | Description                                                                                                                                                                                                             |
|:-----------------------|:------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **RuleName**           | This is a string field that holds the name for this specific rule, it must be unique within the bounds of the workflow                                                                                                  |
| **SuccessEvent**       | This is an optional string field that is returned if the rule expression evaluates to true                                                                                                                              |
| **ErrorMessage**       | This is an optional string field that holds the error message that will be returned if the rule expression evaluates to false                                                                                           |
| **RuleExpressionType** | This is a string field, at the time of writing the only supported type is `LambdaExpression`                                                                                                                            |
| **Expression**         | This is a string field that holds the actual lambda expression to be evaluated for the rule                                                                                                                             |
| **Actions**            | This optional field allows follow on actions to defined. The trigger for the action can be specified using `OnSuccess` and `OnFailure`. Currently only two actions are supported: `OutputExpression` and `EvaluateRule` |

## Parameters

The Rules Engine supports two types of parameters, global and local. Multiple parameters can be specified in any given file, with locally scoped parameters overriding globally scoped parameters where specified.

Parameters can be used to simplify naming, or to evaluate expressions for later use in a rule expression.

### Global Parameters

Global parameters allow for a variable to be declared using the same expression syntax as the rule definition. This variable is then available for use within the body of every rule expression.

```json
[
    {
        "WorkflowName": "Simple",
        "GlobalParams": [
            {
                "Name": "incorporationYear",
                "Expression": "input1.IncorporationDate.Year"
            }
        ],
        "Rules": [
            {
                "RuleName": "QuickCheckIncorporationAge",
                "SuccessEvent": "Ok",
                "ErrorMessage": "Company does not meet incorporation age requirements",
                "ErrorType": "Error",
                "RuleExpressionType": "LambdaExpression",
                "Expression": "(DateTime.Today.Year - incorporationYear) >= 5"
            }

            ...
```

### Local Parameters

Local parameters allow for a variable to be declared using the same expression syntax as the rule definition. This variable is then available for use within the rule expression within the rule in which it is defined.

```json
{
    "RuleName": "CheckDirectors",
    "LocalParams":[
        {
            "Name": "NumberOfDirectors",
            "Expression": "input1.Directors.Count"
        }
    ],
    "SuccessEvent": "Ok",
    "ErrorMessage": "Company does not have the required number of directors",
    "ErrorType": "Error",
    "RuleExpressionType": "LambdaExpression",
    "Expression": "NumberOfDirectors > 4"
}
```

## Sub Rules and Logical Operators

Rules can contain a collection of sub-rules or child rules that can be optionally combined with logical operators. Currently there are two logical operators supported:

* `Or` which can also be used as `OrElse` for readability
* `And` which can also be used as `AndAlso` for readability

The example below is logically the same as the rule shown in the [Basic Rule Definition](#basic-rules-definition) section above.

```json
{
    "RuleName": "AltCheckCountry",
    "Operator": "Or",
    "ErrorMessage": "Service not available in this country",
    "Rules": [
        {
            "RuleName" : "IsIrish",
            "Expression": "input1.IncorporationCountryCode == \"IE\""
        },
        {
            "RuleName" : "IsFrench",
            "Expression": "input1.IncorporationCountryCode == \"FR\""
        }
    ],
    "Actions": {
        "OnFailure": {
            "Name": "OutputExpression",
            "Context": {
                "Expression": "input1.IncorporationCountryCode"
            }
        }
    }
}
```

## Returning Data

Executing a rules script will cause the engine to evaluate all of the rules, returning the results on completion. A failure of any of the rules _does not_ halt the evaluation of the other rules.

### Results Tree

The output from the execution of a workflow is a results tree, where each rule specified in the workflow has its result reflected as an entry in the results tree. Where a rule has nested rules, they in turn are returned in the `ChildResults` element of the result.

Where no additional information is provided in the rule script, the `IsSuccess` property is set to either `True` or `False` to reflect the result of the evaluation.

### Success Events

Each rule, or sub-rule can specify a `SuccessEvent` which is represented by a string in the rules script. This value is not interpreted in any way by the rules engine, but is only returned if the rule is successfully evaluated.

```json
[
    {
        "WorkflowName": "Simple",
        "Rules": [
            {
                "RuleName": "QuickCheckIncorporationAge",
                "SuccessEvent": "Ok",
                "RuleExpressionType": "LambdaExpression",
                "Expression": "(DateTime.Today.Year - input1.IncorporationDate.Year) >= 5"
            }

            ...
```

How the success event is actually used is entirely at the discretion of the client - the rules engine simply returns whatever value is set.

### Failures

Each rule, or sub-rule can specify an `ErrorMessage` and an `ErrorType`, both are optional. Similar to the `SuccessEvent`, the rules engine does not interpret these values in any way, but only returns them if the rule is not successfully evaluated.

```json
[
    {
        "WorkflowName": "Simple",
        "Rules": [
            {
                "RuleName": "QuickCheckIncorporationAge",
                "ErrorMessage": "Company does not meet incorporation age requirements",
                "ErrorType": "Error",
                "RuleExpressionType": "LambdaExpression",
                "Expression": "(DateTime.Today.Year - input1.IncorporationDate.Year) >= 5"
            }

            ...
```

### Outputs

The third way to receive an output from a rule execution is through the use of a follow on action. This can be specified using either a `OnSuccess` or `OnFailure` trigger, which will determine which action will be executed. The specific action name `OutputExpression` is used to specify the expression that should be evaluated based on the trigger.

```json
{
    "RuleName": "AltCheckCountry",
    "Operator": "Or",
    "ErrorMessage": "Service not available in this country",
    "Rules": [
        {
            "RuleName" : "IsIrish",
            "Expression": "input1.IncorporationCountryCode == \"IE\""
        },
        {
            "RuleName" : "IsFrench",
            "Expression": "input1.IncorporationCountryCode == \"FR\""
        }
    ],
    "Actions": {
        "OnFailure": {
            "Name": "OutputExpression",
            "Context": {
                "Expression": "input1.IncorporationCountryCode"
            }
        }
    }
},
{
    "RuleName": "CreditScore",
    "RuleExpressionType": "LambdaExpression",
    "Expression": "input1.IncorporationCountryCode == \"IE\"",
    "Actions": {
        "OnSuccess" : {
            "Name" : "OutputExpression",
            "Context" : {
                "Expression":"(((DateTime.Today.Year - incorporationYear) * 100m) * 0.66m) % 100m"
            }
        }
    }
}
```

This is the only approach that can be scripted that supports a complex return expression.

## Chaining Rules

Rules can be chained together allowing for more selective execution of rules based on specific criteria. This becomes particularly useful when the rules script is producing more than a simple True/False result. Using actions, a rule can evaluate a follow on rule based on meeting the `OnSuccess` or `OnFailure` triggers.

The triggered rule does **not** need to reside in the same rules file, but where multiple files are being used, it's important to remember to load all of the potential workflows when the engine is initialised.

First, define the follow on rule:

```json
[
    {
        "WorkflowName": "MatureCompanyRules",
        "Rules": [
            {
                "RuleName": "CheckMatureProfitMovingAverage",
                "SuccessEvent": "ok",
                "ErrorMessage": "Company does meet the profitability requirements",
                "ErrorType": "Error",
                "RuleExpressionType": "LambdaExpression",
                "Expression": "ComplexRules.ProfitMovingAverage(input1.AnnualProfit, 5) >= 50000.00m",
                "Actions": {
                    "OnSuccess": {
                        "Name": "OutputExpression",
                        "Context": {
                            "Expression": "ComplexRules.ProfitMovingAverage(input1.AnnualProfit, 5) * 3.0"
                        }
                    }
                }
            }
        ]
    }
]
```

Next, define the rule that will call the follow on rule:

```json
    ...

    {
        "RuleName": "MatureCompanyRule",
        "SuccessEvent": "Ok",
        "ErrorMessage": "Company does not meet incorporation age requirements",
        "RuleExpressionType": "LambdaExpression",
        "Expression": "(DateTime.Today.Year - incorporationYear) >= 10",
        "Actions": {
            "OnSuccess":{
                "Name": "EvaluateRule",
                "Context": {
                    "WorkflowName": "MatureCompanyRules",
                    "RuleName": "CheckMatureProfitMovingAverage"
                }
            }
        }
    }
```

This approach allows for the rules results to be less cluttered. In this example, the `MatureCompanyRules` are only executed, and consequently only appear in the `ResultsTree` if the `MatureCompanyRule` is first matched.

## Complex Rules

In some cases the expression syntax that is available within the body of a rule will not be sufficient. In this case, a custom rule can be encoded and made available to the rules engine for execution within a rule definition.

Establishing a custom rule requires three steps. First define the custom code that holds the logic to be executed:

```csharp
using RulesModel;
using System.Collections.Generic;

namespace CustomRules;

public static class ComplexRules
{
    public static int NumAverages = 4;

    public static decimal ProfitMovingAverage(object profits, int minAverages)
    {
        var ma = new DecimalMovingAverage(NumAverages);
        var result = 0.0m;

        var allProfits = profits.ToProfitArray();

        if (allProfits.Count < minAverages)
            return 0.0m;

        foreach (var v in allProfits.Values)
        {
            result = ma.Update(v);
        }

        return result;
    }
}
```

**Note** You can find the implementation of the `DecimalMovingAverage` in the code.

The class ```ComplexRules``` holds the call to ```ProfitMovingAverage``` that will take a dictionary of &lt;Year,Profit&gt; and the number of entries to use for the moving average.

Because of how the rules engine is moving data around using dynamic types, it can't move complex types between custom calls and the rules engine. Notice how the ```profits``` is passed as an `object` and then cast to a `Dictionary` using a helper function.

Next, the rule must be registered with the rules engine as it is being constructed:

```csharp
    var rulesEngineSettings = new ReSettings { CustomTypes = new Type[] { typeof(ComplexRules) } };

    ...

    var engine = new RulesEngine.RulesEngine(workflowDefinitions.ToArray(), rulesEngineSettings);

    ...
```

The `ReSettings` type can accept multiple custom types for use. Note that for this example implementation, the configuration of custom types is split into two steps: First the custom types are registered with the broker, and second, reading that collection of custom types and configuring the engine to use them.

Finally, the custom type can be used inside a rule expression:

```json
{
    "WorkflowName": "MatureCompanyRules",
    "Rules": [
        {
            "RuleName": "CheckProfitMovingAverage",
            "SuccessEvent": "ok",
            "ErrorMessage": "Company does meet the profitability requirements",
            "ErrorType": "Error",
            "RuleExpressionType": "LambdaExpression",
            "Expression": "ComplexRules.ProfitMovingAverage(input1.AnnualProfit, 5) >= 50000.00m"
        }
    ]
}
```

## Structure of the Code

There are three directories of interest in this project.

## The App Directory

The `App` directory holds the code necessary for running the demo. This directory contains copies of code for [Spinners](https://github.com/Xorcon-ie/dotnet-textspinners) and [TextBars](https://github.com/Xorcon-ie/dotnet-textbars) for the purposes of displaying results (both of these are in public repositories).

In this directory you will find:

* `Program.cs` - the main entry point and has handling for command line flags
* `CustomRules` - a directory where the moving average custom rule can be found
* `Model` - a directory where the data structure used as input can be found
* `Samples` - a directory where the specific handlers for the simple and bulk samples can be found
* `Utils` - a directory where general utilities for the demo can be found

## The Core Directory

The `Core` directory holds the implementation of the `RulesBroker` and supporting code including a handler for reading rules from local storage.

In this directory you will find:

* `Engine` - the directory holding the `RulesBroker`
* `Model` - the directory holding the DTO classes used to transport the results from the rules engine to the client
* `Repository` - the directory holding the classes used to access the local file store
* `Utils` - the directory holding general helper classes

## The Rules Directory

The `Rules` directory holds the rules script, and is referenced in the `BrokerFactory.cs` file. If you move the rules scripts to a different location, don't forget to change it here also.

## Conclusions


This is a simple demonstrator of the [Microsoft Rules Engine](https://www.nuget.org/packages/RulesEngine). It's intended to present some of the major features of the package, and to help with the it's evaluation. Xorcon has no affiliation with Microsoft, and have received no consideration from Microsoft or it's affiliates for this code. I don't consider it industrial strength, or even production ready however you may interpret that phrase; it is provided as-is for reference only.

Feel free to take it, adapt it and use it as you see fit. If you have feedback or comments, I'm always interested to hear at david@xorcon.ie