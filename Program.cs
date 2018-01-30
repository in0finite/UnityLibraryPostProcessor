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
				string newInputAssemblyPath = System.IO.Path.Combine( System.IO.Path.GetDirectoryName(inputAssemblyPath), "Assembly-", System.IO.Path.GetFileName(inputAssemblyPath) );
				Console.WriteLine("Copying input assembly (because it's name must start with 'Assembly-') to " + newInputAssemblyPath);
				System.IO.File.Copy( inputAssemblyPath, newInputAssemblyPath, true );

				// invoke unet weaver
				Console.WriteLine("Invoking unet weaver");
				var type = unetWeaverAssembly.GetType("Unity.UNetWeaver.Program", true);
				var method = type.GetMethod("Process");

				// arguments are as following: path to unityengine dll, path to networking dll, current directory,
				// string array of input assemblies, string array of extra assemblies, null (assembly resolver), warning action,
				// error action

				string unityEngineDllPath = System.IO.Path.Combine( unityInstallationPath, "Editor/Data/Managed/UnityEngine.dll" );
				string unityNetworkingDllPath = System.IO.Path.Combine( unityInstallationPath, "Editor/Data/UnityExtensions/Unity/Networking/UnityEngine.Networking.dll");
				Action<string> warningAction = (string msg) => { Console.WriteLine("warning: " + msg); };
				Action<string> errorAction = (string msg) => { Console.WriteLine("error: " + msg); returnValue = 1; };

				Console.WriteLine();

				method.Invoke( null, new object[] { unityEngineDllPath, unityNetworkingDllPath, System.IO.Directory.GetCurrentDirectory(),
					new string[] { newInputAssemblyPath }, new string[0], null, warningAction,
					errorAction } );


				Console.WriteLine();
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

