//http://fsharp.github.io/FAKE/apidocs/fake-filehelper.html
#I "packages/FAKE/tools"
#r "packages/FAKE/tools/FakeLib.dll" // include Fake lib
#r "System.Xml.Linq"
#r "packages/ConfigJson/lib/ConfigJsonNET.dll"

open System
open System.IO
open Fake.TaskRunnerHelper
open Fake
open Fake.FileUtils
open System.Xml.Linq
open System.Collections.Generic
open Fake.Testing

let buildParam = getBuildParamOrDefault  "buildType" "release" 
// Directories
let root="./.build/build-"+buildParam
let buildDir  = root+"/app/"
let testDir   =root+ "/test"
let deployDir = root+"/deploy/"
let nugetWorkingDir =root+ "/packaging/"
let allPackageFiles = [(buildDir+"InventoryService.Messages.dll")]


let nugetDeployPath = getBuildParamOrDefault  "nugetDeployPath" deployDir 

//let testOutput = FullName (root+ "-TestResults")
let testOutput = FullName "./.build/TestResults"
 



//--------------------------------------------------------------------------------
// Information about the project for Nuget and Assembly info files
//--------------------------------------------------------------------------------

let product = "InventoryService"
let authors = [ "Officialcomminity" ]
let copyright = "Copyright © Officialcomminity 2016"
let company = "Officialcomminity"
let description = "Prototyping some things for real-time inventory service"
let tags = ["InventoryService";"occ";]
let projectName="InventoryService.Messages"
// Read release notes and version

let BuildFn<'T>= match buildParam with
                  | "debug" -> MSBuildDebug
                  | _       ->MSBuildRelease


                  

let BuildVersionType= match buildParam with
                           | "release" -> ""
                           | _         -> "-"+buildParam

let NugetDeployPath= match nugetDeployPath with
                           | "release" -> ""
                           | _         -> "-"+buildParam

// version info
let version =
  match buildServer with
  | TeamCity -> (buildVersion+BuildVersionType)
  | _        -> ("0.1.2"+BuildVersionType)

// Targets
Target "Clean" (fun _ -> 
    CleanDirs [root]    
    CreateDir deployDir
    CreateDir testDir
    CreateDir buildDir
    CreateDir nugetWorkingDir
    CreateDir testOutput
)




let serviceReferences  =  !! "./InventoryService.ServiceDeployment/**/*.csproj"

Target "BuildService" (fun _ ->
     MSBuildDebug buildDir "Build" serviceReferences
        |> Log "AppBuild-Output: "
)

let appReferences  =  !! "./InventoryService.Messages/**/*.csproj"

let testReferences = !! "./InventoryService.Tests/*.csproj"




Target "BuildMessages" (fun _ ->
     BuildFn buildDir "Build" appReferences
        |> Log "AppBuild-Output: "
)


Target "BuildTest" (fun _ ->
     BuildFn testDir "Build" testReferences
        |> Log "TestBuild-Output: "
)

Target "xUnitTest" (fun _ ->  
    let xunitTestAssemblies = !! (testDir + "/InventoryService.Tests.dll")

    let xunitToolPath = findToolInSubPath "xunit.console.exe" "packages/FAKE/xunit.runner.console/tools"
    
    printfn "Using XUnit runner: %s" xunitToolPath
    let runSingleAssembly assembly =
        let assemblyName = Path.GetFileNameWithoutExtension(assembly)
        xUnit2
            (fun p -> { p with XmlOutputPath = Some (testOutput + @"\" + assemblyName + "_xunit_"+buildParam+".xml"); HtmlOutputPath = Some (testOutput + @"\" + assemblyName + "_xunit_"+buildParam+".HTML"); ToolPath = xunitToolPath; TimeOut = System.TimeSpan.FromMinutes 30.0; Parallel = ParallelMode.NoParallelization }) 
            (Seq.singleton assembly)

    xunitTestAssemblies |> Seq.iter (runSingleAssembly)
 
)

Target "CreateNuget" (fun _ ->
    // Copy all the package files into a package folder
    CopyFiles nugetWorkingDir allPackageFiles

    NuGet (fun p -> 
        { 
          p with
            Authors = authors
            Project = projectName
            Description = description                               
            OutputPath = deployDir
            Summary = description
            WorkingDir = nugetWorkingDir
            Version = version 
         })             
            "InventoryService.Messages.nuspec"
)

Target "Deploy" (fun _ ->
    !! (buildDir + "/**/*.*") 
        -- "*.zip" 
        |> Zip buildDir (deployDir + "InventoryService." + version + ".zip")
)



Target "RemotePublishNuGet" (fun _ ->     
    !! (deployDir + "*.nupkg") 
      |> Copy NugetDeployPath
)

// Build order
"Clean"
  ==> "BuildMessages"
  ==> "BuildTest"
  ==> "xUnitTest" 
  ==> "BuildService"
  ==> "CreateNuget"
  ==> "RemotePublishNuGet"
  ==> "Deploy"

// start build
RunTargetOrDefault "Deploy"
