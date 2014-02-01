@echo off
nuget pack ExpressionEvaluator.csproj -Properties Configuration=Debug -OutputDirectory "bin\Debug"
pause
