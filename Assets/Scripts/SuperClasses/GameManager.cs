using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Linq;
using System.Xml.Linq;

/// <summary>
/// Handles the progression logic of levels in the game
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Static instance of the game manager
    /// </summary>
    public static GameManager instance = null;
    
    /// <summary>
    /// current level teh player is on
    /// </summary>
    private int level = 1;
    
    /// <summary>
    /// Whether the level is complete
    /// </summary>
   // private bool levelComplete = false;

    /// <summary>
    /// The end chest of the current level
    /// </summary>
   // private GameObject endChest = null;
    /// <summary>
    /// The recipes required to win the level
    /// </summary>
    public List<Recipe> winCondition = new List<Recipe>();

    /// <summary>
    /// The amount of each recipe required to end the level
    /// </summary>
    public List<float> endRequirementAmount =  new List<float>();
    /// <summary>
    ///  Grab the level document containing all level data
    /// </summary>
    private static IEnumerable<XElement> xmlData;
    /// <summary>
    /// Local instance of the recipe class
    /// </summary>
    public static Recipes recipes = new Recipes();

    /// <summary>
    /// Location of the streaming objects folder when compiled
    /// </summary>
    public string dataPath;


    private void Awake()
    {
        //Check if instance already exists
        if (instance == null)
        {
            //if not, set instance to this
            instance = this;
        }
        else if (instance != this)
        {
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
        }
        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
        dataPath = Application.dataPath;
        recipes.GenerateRecipeBook();
        //generate the first levels data and load it
        GenerateLevelData();
        SceneManager.LoadScene(level);

    }

    /// <summary>
    /// Load the XML data containing level requirements
    /// </summary>
    private void GenerateLevelData()
    {
        //Load the level data from the XML file
        xmlData = from all in XElement.Load(dataPath+"/StreamingAssets/Data/Levels.xml").Elements("level")
                  select all;
        //wipe the level details
        winCondition = new List<Recipe>();
        endRequirementAmount = new List<float>();
        //Foreach xml tag in the xml file
        foreach (XElement levelRequirement in xmlData)
        {
            //if the levelID is equal to the requested level
            if(int.Parse(levelRequirement.Element("levelID").Value.Trim()) == level)
            {
                //query the file for the recipe requirements
                IEnumerable<XElement> requirementsQuery = levelRequirement.Descendants()
                    .Where(requiremet => requiremet.Name.LocalName == ("recipeRequirements")).Elements("recipeRequirement"); ;
                //foreach the level requirement in the query
                foreach (var recipeRequirment in requirementsQuery)
                {
                    //load level requirement data into memory
                    string recipeName = recipeRequirment.Element("recipe").Value.Trim();
                    Debug.Log(level);
                    Debug.Log(recipeName);
                    Recipe recipeToAdd = recipes.FindRecipeByName(recipeName);
                    float amountNeeded = float.Parse(recipeRequirment.Element("amountNeeded").Value.Trim());
                    winCondition.Add(recipeToAdd);
                    endRequirementAmount.Add(amountNeeded);
                }
            }
        }
    }

    /// <summary>
    /// Advance the game level when completed
    /// </summary>
    public void AdvanceLevel()
    {
        //If the level is less than  the current max level, increase level count
        if (level < 2)
        {
            level++;
        }else
        {
            //otherwise, loop back to start
            level = 1;
        }
        //Gather level data related specific to the level.
        GenerateLevelData();
        //load the level at the specified ID
        SceneManager.LoadScene(level);
    }
}
