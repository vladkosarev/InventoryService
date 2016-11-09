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
let root="./.build/build-"+buildParam+"-server-ui"
let buildDir  = root+"/app-server/"
let testDir   =root+ "/test"
let deployDir = root+"/deploy/"
let nugetWorkingDir =root+ "/packaging-server/"
let allPackageFiles = [(buildDir+"InventoryService.WebUIDeployment.dll",buildDir+"InventoryService.Server.dll")]


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
let projectName="InventoryService.Server"
// Read release notes and version

let BuildFn<'T>= match buildParam with
                  | _       ->MSBuildDebug                  

let BuildVersionType= match buildParam with
                           | _         -> "-pre"

let NugetDeployPath= match nugetDeployPath with
                          | _         -> "-pre"

// version info
let version =
  match buildServer with
  | TeamCity -> (buildVersion+BuildVersionType)
  | _        -> ("1.0.5"+BuildVersionType)

// Targets
Target "Clean" (fun _ -> 
    CleanDirs [root]    
    CreateDir deployDir
    CreateDir testDir
    CreateDir buildDir
    CreateDir nugetWorkingDir
    CreateDir testOutput
)




let serviceReferences  =  !! "./**/*.csproj"

Target "BuildService" (fun _ ->
     MSBuildDebug buildDir "Build" serviceReferences
        |> Log "AppBuild-Output: "
)

//let appReferences  =  !! "./InventoryService.AkkaInMemory/**/*.csproj"

let testReferences = !! "./InventoryService.Tests/*.csproj"



//
//Target "BuildServer" (fun _ ->
//     BuildFn buildDir "Build" appReferences
//        |> Log "AppBuild-Output: "
//)


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
    //CopyFiles nugetWorkingDir [buildDir]

    NuGet (fun p -> 
        { 
          p with
            Authors = authors
            Project = projectName
            Description = description                               
            OutputPath = deployDir
            Summary = description
            WorkingDir = buildDir
            Version = version 
            DependenciesByFramework =[{  
                                      FrameworkVersion  = "net45" 
                                      Dependencies=[
                                                      ("Akka.Cluster", "1.0.8.25-beta")
                                                      ("Akka.Remote", "1.0.8")
                                                      ("Helios", "1.4.2")
                                                      ("Microsoft.Owin.SelfHost", "3.0.1")
                                                      ("Microsoft.Owin.StaticFiles", "3.0.1")
                                                      ("Microsoft.Owin.FileSystems", "3.0.1")
                                                      ("Microsoft.Owin.Diagnostics", "3.0.1")
                                                      ("Microsoft.AspNet.SignalR.SelfHost", "2.2.1")
                                                      ("Microsoft.AspNet.WebApi.Client", "3.0.1") 
                                                      ("Akka.Logger.NLog","1.0.8")
                                                      ("Microsoft.Owin.Host.HttpListener","3.0.1")      
                                     ]    
                                     }]
      })             
            "InventoryService.Server.nuspec"
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
 // ==> "BuildServer"
  ==> "BuildTest"
  ==> "xUnitTest" 
  ==> "BuildService"
  ==> "CreateNuget"
  ==> "RemotePublishNuGet"
  ==> "Deploy"

// start build
RunTargetOrDefault "Deploy"
