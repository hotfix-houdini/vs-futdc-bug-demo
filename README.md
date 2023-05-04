# Purpose
This repo demonstrates a bug with Build Acceleration / Fast-up-to-date-check in Visual Studio which incorrectly copies same-named files (such as appsettings.json) from other projects, into the project that is being fast built. Using the incorrect appsettings.json file from other projects causes issues! Build Acceleration / Fast-up-to-date-check alternates between copying the correct file and the incorreect file, every other fast build (such as every other unit test run). 

# Environment
- Visual Studio 2022 version 17.5 or later.
- NET 6 (likey impacts other targets too, I'm guessing)

# Steps to reproduce
## Quick - Pull this repo
Pull this repo and run the unit test repeatedly, you should see the unit test alternate between passing and failing. Build acceleration is mistakenly giving ProjectA's `appsettings.json` to ProjectB's output folder every other time. 

## Indepth - from scratch
0. Create a new .NET 6 solution
0. Create a new .NET 6 Console App `ProjectA`
    - add `appsettings.json` with 
        ```json 
        {
            "configValue": "From Project A"
        }
        ```
    - ensure that `Copy to Output Directory` is set to `Do not copy` for the moment. 
0. Enable Build Acceleration
    - `Solution -> Add Item -> Directory.Build.Props` with 
        ```xml
        <Project>
            <PropertyGroup>
                <LangVersion>latest</LangVersion>
                <AccelerateBuildsInVisualStudio>true</AccelerateBuildsInVisualStudio>
            </PropertyGroup>
        </Project>
        ```
    - ensure `Tools -> Options -> SDK-Style Projects -> Don't Call MSBuild if a project appears to be up to date` is checked
    - set `Tools -> options -> SDK-Style Projects -> Don't call MSBuild... -> Logging Level` to `Verbose`
0. Create a new .NET 6 nunit project `ProjectB`
    - add `appsettings.json` with 
        ```json
        {
            "configValue": "From Project B"
        }
        ```
    - set `appsettings.json` to have `Copy to Output Directory` to `Copy if Newer` or `Always`
    - install NuGet package `Microsoft.Extensions.Configuration` for `ConfigurationBuilder`
    - install NuGet package `Microsoft.Extensions.Configuration.Json` for `.AddJsonFile(..)`
    - add a test that pulls from the config and asserts that the value is `From Project B`
        ```csharp
        [Test]
        public void ConfigShouldBeFromProjectB()
        {
            // arrange 
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            // act
            var actualConfigValue = config.GetSection("configValue").Value;

            // assert
            Assert.That(actualConfigValue, Is.EqualTo("From Project B"));
        }
        ```
0. Run the test a few times in a row - verify it always succeeds. 
0. Add a project reference so ProjectB references ProjectA
0. Set the `Copy to Output Directory` on `appsettings.json` on `ProjectA` to `Copy if Newer` or `Always`
0. Run the test a few times in a row. Observe that it alternates between passing and failing, and that the value is from `ProjectA`'s config when it fails.
0. Observe in the build output that Build acceleration copied 1 files. Observe that it `Remembering the need to copy file '...\VsFuptdBugDemo\ProjectA\appsettings.json' to '...\VsFuptdBugDemo\ProjectB\bin\Debug\net6.0\appsettings.json'`

Visual studio is alternating between copying the correct `appsettings.json` and the incorrect `appsettings.json`! It shouldn't be copying any files as they're unchanged, let alone one from another project with the same name. 
