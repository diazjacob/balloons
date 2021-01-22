using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public class GameLoader : ManagedMonobehaviour
{
	private static string SAVEFILENAME = "data.json";
	private GameSave _gameSave;

	private void Awake()
	{
		GetAllRefs();
	}

	// Update is called once per frame

	private bool Load()
	{
		bool loadedSave;

		//Testing file paths
		string filePath = Path.Combine(Application.persistentDataPath, SAVEFILENAME);

		if (File.Exists(filePath)) //Populating the loaded save
		{
			//Load the file and cast the Object to a LevelSaveFramework class
			string json = File.ReadAllText(filePath);
			_gameSave = JsonUtility.FromJson<GameSave>(json);

			loadedSave = true;
		}
		else //Creating the new save
		{
			_gameSave = new GameSave();

			
			loadedSave = false;
		}

		//LOAD ALL DATA INTO GAME SCRIPTS HERE IF POSSIBLE





		//--------------------------------------------------

		return loadedSave;
	}

	private bool Save()
	{
		//Populate new save here
		GameSave saveFile = new GameSave();

		//POPULATING



		//---------------------

		//Saving all data into .JSON file
		string filePath = Path.Combine(Application.persistentDataPath, SAVEFILENAME);
		string json = JsonUtility.ToJson(_gameSave, true);

		using (StreamWriter sw = new StreamWriter(filePath))
		{
			sw.Write(json);
			sw.Close();
		}

		return true;
	}
}
