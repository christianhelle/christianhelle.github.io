dotnet build
pwsh ./bin/Debug/net6.0/playwright.ps1 install
dotnet test -- NUnit.NumberOfTestWorkers=5