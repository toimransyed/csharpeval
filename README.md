# Introduction

C# does not include a built-in method to evaluate a string during runtime, like Javascript's eval() or VBScript's Eval().

ExpressionEvaluator is a lightweight, simple and free library capable of parsing and compiling simple to medium complexity C# expressions. It features a Antlr-based tokenizer and parser and generates a LINQ Expression tree which is then compiled into a function.

Applications for an expression parser and compiler are usually in the form of embedded code or user-defined expressions that need to be executed against runtime objects. 

# How can I use this library?

* An [Experimental Music Compiler](http://www.windowsphone.com/en-us/store/app/experimental-music-compiler/370af565-51c8-4cd0-af6a-32d1e9a6690a) for Windows Mobile
* Dynamic data-bound XML templates
* User-defined conditional code stored in configuration files
* Customer-defined queries
* User-defined execution parameters for pluggable controls

If you have downloaded and used this library, I'd like to know about it's usage! Feel free to [let me know about it](https://www.codeplex.com/site/users/contact/rupertavery?OriginalUrl=https://csharpeval.codeplex.com).

# NuGet

Expression Evaluator is now available via [NuGet](https://www.nuget.org/packages/ExpressionEvaluator)

# Getting Started

See **Usage** and **Sample Expressions** under [Documentation](https://csharpeval.codeplex.com/documentation)

# Latest Updates

For more details see [Updates]

# Features

* *Compute*: Arithmetic operators: +- * / % ^
* *Compare*: Relational operators: == != < > <= >=
* *Assign*: Set values with the = operator
* *Test*: Logical Operators: ! & | (bitwise logic) and && || (short circuit logic)
* Expression grouping with parentheses ( )
* Index accessors {"[ ]"}
* Supports *dynamic* objects (including classes that implement *IDynamicMetaObjectProvider* such as *ExpandoObject*)
* Context: Register and access external types and symbols through the *TypeRegistry*
* Strings: (enclosed in '_single quotes_' and string concatenation with +)
* _true_, _false_, _null_ literals
* Declarative typing of numbers using d/f/m/l/u/ul suffixes
* Implicit conversion of numerical expressions
* Member access operator (.) for any valid expression.  Access properties, fields and methods of types, objects and expressions
* Pre-registered default types e.g. bool, int, double, float, char, string
* Supports nested function calls (x.method(y.method(z.method()), y.method2()))

# Donate

Expression Evaluator is 100% free, but if you would like to support the project in any way please do so.

![Donate](https://www.paypalobjects.com/webstatic/en_US/btn/btn_donate_cc_147x47.png)

[Donate](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=rupert.avery@gmail.com&lc=PH&item_name=Expression Evaluator&currency_code=USD&bn=PP-DonationsBF:btn_donateCC_LG.gif:NonHosted)

# Disclaimer

This is *not* a fully C#-compliant compiler, and as such features may be missing and there may be bugs or discrepancies in how the parser and compiler work.  If you wish to use this library in production code do so at your own risk.
