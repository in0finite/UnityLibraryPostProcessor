using System;

namespace UnityLibraryPostProcessor
{
	public class Program
	{
		public static int Main (string[] args)
		{

			int returnValue = 0;

			try {
				
				if (args.Length < 2) {
					PrintUsage();
					throw new ArgumentException ("not enough arguments");
				}

				string unityInstallationPath = args [0];
				string inputAssemblyPath = args[1];

				Console.WriteLine("unity installation path: " + unityInstallationPath);
				Console.WriteLine("input assembly path: " + inputAssemblyPath);

				if (!System.IO.Directory.Exists (unityInstallationPath)) {
					throw new ArgumentException ("Specified directory doesn't exist");
				}

				if(!System.IO.File.Exists(inputAssemblyPath)) {
					throw new ArgumentException("Specified assembly doesn't exist");
				}

				// load unet weaver assembly
				string unetWeaverDllPath = System.IO.Path.Combine( unityInstallationPath, "Editor/Data/Managed/Unity.UNetWeaver.dll" );
				Console.WriteLine("Loading unet weaver assembly at: " + unetWeaverDllPath);
				var unetWeaverAssembly = System.Reflection.Assembly.LoadFile( unetWeaverDllPath );
				if(null == unetWeaverAssembly) {
					throw new System.IO.FileLoadException("Failed to load unet weaver assembly");
				}

				// name of input assembly must start with "Assembly-"
				string newInputAssemblyPath = System.IO.Path.Combine( System.IO.Path.GetDirectoryName(inputAssemblyPath), "Assembly-" ) + System.IO.Path.GetFileName(inputAssemblyPath) ;
				string mdbFilePath = inputAssemblyPath + ".mdb" ;
				string newMdbFilePath = System.IO.Path.Combine( System.IO.Path.GetDirectoryName(mdbFilePath), "Assembly-" ) + System.IO.Path.GetFileName(mdbFilePath) ;
				Console.WriteLine("Copying input assembly to " + newInputAssemblyPath);
				System.IO.File.Copy( inputAssemblyPath, newInputAssemblyPath, true );
				Console.WriteLine("Copying mdb file to " + newMdbFilePath);
				System.IO.File.Copy( mdbFilePath, newMdbFilePath, true );

				// invoke unet weaver
				Console.WriteLine("Invoking unet weaver");
				var type = unetWeaverAssembly.GetType("Unity.UNetWeaver.Program", true);
				var method = type.GetMethod("Process");

				// arguments are as following: path to unityengine dll, path to networking dll, current directory,
				// string array of input assemblies, string array of extra assemblies, null (assembly resolver), warning action,
				// error action

				string unityEngineDllPath = System.IO.Path.Combine( unityInstallationPath, "Editor/Data/Managed/UnityEngine.dll" );
				string unityNetworkingDllPath = System.IO.Path.Combine( unityInstallationPath, "Editor/Data/UnityExtensions/Unity/Networking/UnityEngine.Networking.dll");
				int numWarnings = 0, numErrors = 0;
				Action<string> warningAction = (string msg) => { Console.WriteLine("warning: " + msg); numWarnings++; };
				Action<string> errorAction = (string msg) => { Console.WriteLine("error: " + msg); returnValue = 1; numErrors++; };

				Console.WriteLine();

				method.Invoke( null, new object[] { unityEngineDllPath, unityNetworkingDllPath, System.IO.Directory.GetCurrentDirectory(),
					new string[] { newInputAssemblyPath }, new string[0], null, warningAction,
					errorAction } );


				Console.WriteLine();
				Console.WriteLine("Weaver finished, warnings: " + numWarnings + " errors: " + numErrors);

				if(numErrors > 0)
					throw new Exception("Weaver reported some errors. Failed to build assembly.");
				
				Console.WriteLine("Copying assembly");
			//	Console.Read();
				System.IO.File.Copy( newInputAssemblyPath, inputAssemblyPath, true );

				// delete temporary files
				// ignore errors
				Console.WriteLine("Deleting temporary files");
			//	Console.Read();
				try {
					System.IO.File.Delete( newInputAssemblyPath );
					System.IO.File.Delete( newMdbFilePath );
				} catch(Exception ex) {}


				Console.WriteLine("Done");

			} catch(Exception ex) {
				Console.WriteLine (ex.ToString ());
				return 1;
			}


			return returnValue;
		}

		public static string GetUsage() {

			return "usage: path/to/unity/installation/ inputAssemblyName";

		}

		public static void PrintUsage() {
			Console.WriteLine (GetUsage ());
		}

	}
}

