using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using UnityEngine;
using UnityEditor;

public class stringOptimizer
{

	private static StreamWriter managerOutput;
	private static string line;
	private static int bracketCount;
	private static bool bracketRead = false;

	[MenuItem ("Window/Optimize/Optimize Strings", false)]
	static void FindAssetsUsingSearchFilter ()
	{
		// Find all code assets
		string[] guids = AssetDatabase.FindAssets ("t:MonoScript");
		bool inUpdate;
		bool inFixedUpdate;
		bool inLateUpdate;

		if (File.Exists ("Assets/stringManager.cs")) {
			File.Copy ("Assets/stringManager.cs", "Assets/stringManager.cs" + ".undo");
		}
		managerOutput = new StreamWriter ("Assets/stringManager.cs");

		stringManagerHeadFoot ("Assets/Editor/header.txt");

		Debug.LogError (guids.Length + " Strings found");
		foreach (string guid in guids) {
			int stringCount = 0;

			Debug.Log (AssetDatabase.GUIDToAssetPath (guid));
			if (!AssetDatabase.GUIDToAssetPath (guid).Contains ("Editor")) {
				saveUndoFile (AssetDatabase.GUIDToAssetPath (guid));
				StreamReader input = new StreamReader (AssetDatabase.GUIDToAssetPath (guid));
				StreamWriter output = new StreamWriter ("Assets/temp.txt");
				bracketCount = 0;
				inUpdate = false;
				inLateUpdate = false;
				inFixedUpdate = false;


				while ((line = input.ReadLine ()) != null) {
					Debug.LogError ("Read line " + line);

					bracketRead = false;

					inUpdate = inFunction ("Update(", "Update (", inUpdate, 0);
					inLateUpdate = inFunction ("LateUpdate(", "LateUpdate (", inUpdate, 0);
					inFixedUpdate = inFunction ("FixedUpdate(", "FixedUpdate (", inUpdate, 0);

					if (((inUpdate) || (inFixedUpdate) || (inLateUpdate)) && (line.Contains ("\"") && (!line.Contains ("case") && (!line.Contains ("[ContextMenu"))))) {

						line = line.Replace ("\\\"", "DOUBLESTRING");
						Debug.LogError ("String found");
						string[] temp = line.Split ('"');
						if (temp.Length >= 3) {
							Debug.LogError (temp [0] + temp [1] + temp [2]);

							int count = 1;
							while (count < temp.Length) {
							
	
								Debug.LogError ("String number " + count);
			
								Debug.LogError ("Match");
								// write string to string manager
								managerOutput.Write ("\tpublic string " + Path.GetFileNameWithoutExtension (AssetDatabase.GUIDToAssetPath (guid)) + stringCount + " = \"" + temp [count] + "\";");
								managerOutput.WriteLine ();
								// replace string with stringManager.Instance.classnameStringcount
								string originalString = '"' + temp [count] + '"';
								string replaceString = "stringManager.Instance." + Path.GetFileNameWithoutExtension (AssetDatabase.GUIDToAssetPath (guid)) + stringCount;
			
								line = line.Replace (originalString, replaceString);

								//line.Replace (originalString, replaceString);
								Debug.LogError ("Writing line " + line + " replaced " + originalString + " with " + replaceString);
								
								stringCount++;
						

								count = count + 2;
							}
						}

						line = line.Replace ("DOUBLESTRING", "\\\"");
					}



					// Write the current line
					output.Write (line);
					output.WriteLine ();
				}

				output.Close ();
				input.Close ();

				File.Delete (AssetDatabase.GUIDToAssetPath (guid));
				File.Move ("Assets/temp.txt", AssetDatabase.GUIDToAssetPath (guid));
			}


		}

		stringManagerHeadFoot ("Assets/Editor/footer.txt");
		managerOutput.Close ();
	}

	static void stringManagerHeadFoot (string headerPath)
	{
		string headerLine;
		StreamReader input = new StreamReader (headerPath);

		while ((headerLine = input.ReadLine ()) != null) {
			managerOutput.Write (headerLine);
			managerOutput.WriteLine ();

		}

		input.Close ();
	}

	static bool inFunction (string search1, string search2, bool testCondition, int braceMatch)
	{
		if ((line.Contains (search1)) || (line.Contains (search2))) {
			testCondition = true;
			if ((line.Contains ("{")) && (!bracketRead)) {
				bracketRead = true;
				bracketCount++;
			}
			Debug.LogError ("In Function " + search1);
			Debug.LogError (bracketCount);
		} else {

			if (testCondition) {
				if ((line.Contains ("{")) && (!bracketRead)) {
					bracketRead = true;
					bracketCount++;
					Debug.LogError ("Open function " + bracketCount);
				}

				if ((line.Contains ("}")) && (!bracketRead)) {
					bracketRead = true;
					bracketCount--;
					Debug.LogError ("Close function " + bracketCount);
				}

				if ((bracketCount == braceMatch)) {
					testCondition = false;
					Debug.LogError (bracketCount);
					Debug.LogError ("Exit Function");
				}
			}


		}

		return(testCondition);
	}

	static void saveUndoFile (string path)
	{
		if (File.Exists (path + ".undo")) {
			File.Delete (path + ".undo");
		}
		File.Copy (path, path + ".undo");
	}
}
