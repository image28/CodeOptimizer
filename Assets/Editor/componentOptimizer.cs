using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public class componentOptimizer
{

	private static string line;
	private static int bracketCount;
	private static bool bracketRead = false;

	[MenuItem ("Window/Optimize/Optimize Components", false)]
	static void FindAssetsUsingSearchFilter ()
	{
		// Find all code assets
		string[] guids = AssetDatabase.FindAssets ("t:MonoScript");
		Debug.LogError (guids.Length + " Strings found");
		bool inUpdate = false;
		bool inAwake = false;
		bool inStart = false;
		bool inClass = false;
		bool writtenVars = false;
		bool written = false;


		foreach (string guid in guids) {
			int stringCount = 0;
			bracketCount = 0;
			inUpdate = false;
			inAwake = false;
			inStart = false;
			inClass = false;
			written = false;
			bracketRead = false;
			writtenVars = false;
			written = false;
			Debug.Log (AssetDatabase.GUIDToAssetPath (guid));
			if (!AssetDatabase.GUIDToAssetPath (guid).Contains ("Editor")) {
				saveUndoFile (AssetDatabase.GUIDToAssetPath (guid));
				StreamReader input = new StreamReader (AssetDatabase.GUIDToAssetPath (guid));
				StreamWriter output = new StreamWriter ("Assets/temp.txt");
				List<string> newVariables = new List<string> ();
				List<string> newInitializer = new List<string> ();
				int componentCount = 0;


				while ((line = input.ReadLine ()) != null) {
					//Debug.LogError ("Read line " + line);
					bracketRead = false;

					inUpdate = inFunction ("Update(", "Update (", inUpdate, 0);

					if ((inUpdate) && ((line.Contains ("gameObject.GetComponent")) || (line.Contains ("\tGetComponent")) || (line.Contains (" GetComponent")))) {
						Debug.LogError ("String found");
						Regex expression = new Regex (@"GetComponent<.*?>");
						MatchCollection results = expression.Matches (line);

						foreach (Match result in results) {
							Debug.LogError (result);
							string compName = result.Value.Substring (13, result.Value.Length - 14);
							newVariables.Add (compName);
							newInitializer.Add (compName + "Cache" + (newVariables.Count - 1).ToString () + "=GetComponent<" + compName + ">();");
							//Debug.LogError (newVariables [newVariables.Count - 1]);
						}
					}



				}

				input.BaseStream.Seek (0L, SeekOrigin.Begin);
				bracketCount = 0;
				inUpdate = false;
				inAwake = false;
				inStart = false;
				inClass = false;
				written = false;
				bracketRead = false;
				writtenVars = false;
				written = false;

				while ((line = input.ReadLine ()) != null) {
					//Debug.LogError ("Read line " + line);

					bracketRead = false;

					inUpdate = inFunction ("Update(", "Update (", inUpdate, 1);
					inStart = inFunction ("Start(", "Start (", inStart, 1);
					inAwake = inFunction ("Awake(", "Awake (", inAwake, 1);
					inClass = inFunction ("class " + Path.GetFileNameWithoutExtension (AssetDatabase.GUIDToAssetPath (guid)), "class " + Path.GetFileNameWithoutExtension (AssetDatabase.GUIDToAssetPath (guid)), inClass, 0);

					if ((inUpdate) && ((line.Contains ("gameObject.GetComponent")) || (line.Contains ("\tGetComponent")) || (line.Contains (" GetComponent")))) {
						Debug.LogError ("String found");
						Regex expression = new Regex (@"GetComponent<.*?>");
						MatchCollection results = expression.Matches (line);

						foreach (Match result in results) {
							Debug.LogError (result);
							string compName = result.Value.Substring (13, result.Value.Length - 14);
							line = line.Replace ("GetComponent<" + compName + ">()", compName + "Cache" + componentCount);
							line = line.Replace ("GetComponent<" + compName + "> ()", compName + "Cache" + componentCount);
							componentCount++;
						}

					}

					// Write the current line
					output.Write (line);
					output.WriteLine ();

					if ((inClass) && (!writtenVars) && (bracketCount == 1)) {
						output.WriteLine ();
						int componentCount2 = 0;
						foreach (string vars in newVariables) {
							output.Write (vars + " " + vars + "Cache" + componentCount2 + ";");
							output.WriteLine ();
							componentCount2++;
						}

						output.WriteLine ();
						writtenVars = true;
					}

					if ((inStart) && (!written) && (bracketCount == 2)) {
						output.WriteLine ();

						foreach (string vars in newInitializer) {
							output.Write (vars);
							output.WriteLine ();
						}

						output.WriteLine ();
						written = true;
					}

					if ((inAwake) && (!written) && (bracketCount == 2)) {
						output.WriteLine ();

						foreach (string vars in newInitializer) {
							output.Write (vars);
							output.WriteLine ();
						}

						output.WriteLine ();

						written = true;
					}
				}


				output.Close ();
				input.Close ();

				if (written) {
					File.Delete (AssetDatabase.GUIDToAssetPath (guid));
					File.Move ("Assets/temp.txt", AssetDatabase.GUIDToAssetPath (guid));
				} else {
					File.Delete ("Assets/temp.txt");
				}
			}


		}
			
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
