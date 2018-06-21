

using System;
using System.Collections.Generic;

namespace Hooks
{
	static class ReferenceLoader
	{
		/*
		 * Referenced libraries are loaded by the .NET runtime when they are used.
		 * 
		 * UnityHook loads THIS library file (HookRegistry.dll) and starts the initialisation procedure
		 * to validate it's contents.
		 * While doing that, all loaded libraries that were referenced during initialisation were recorded
		 * and they will be copied WITH HookRegistry.dll to the game library folder.
		 * 
		 * The result is that the injected code won't throw an exception because of missing library files, since
		 * all code will eventually run from the library folder of the game!
		 * 
		 * The `Load()` method will be called after initialisation of HookRegistry is complete.
		 */

		// Holds the types created by Load().
		// These types are explicitly stored in a list to prevent the compiler optimize away our load calls.
		public static List<Type> ReferenceTypes;

		public static void Load()
		{
			// Put here a type statement for each referenced library, which MUST BE COPIED together 
			// with HookRegistry.dll to the game's library folder.
			// e.g. typeof(String);
			// NOTICE: Do NOT insert types refering to the original game libraries, because it could mess with versioning
			// and produce strange runtime issues!

			ReferenceTypes = new List<Type>()
			{			
				// Reference to PayloadFormat.dll
				//typeof(RestServer),

				// Reference to Google.Protobuf.dll
				//typeof(MessageDescriptor),
			};
		}
	}
}
