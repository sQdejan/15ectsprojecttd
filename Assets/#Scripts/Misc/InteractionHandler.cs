using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class InteractionHandler : MonoBehaviour {

#region Variables

	//Publics
	public GameObject arrowTowerPool;
	public GameObject poisonTowerPool;
	public GameObject bombTowerPool;
	public GameObject frostTowerPool;
	public GameObject placementTiles;
	public GameObject backGround;
	public Sprite selectedSpirit;
	public LayerMask layersToHit; //For the unit selection interaction
	public float startGold = 250;

	[HideInInspector]
	public static float curGold = 100;
	[HideInInspector]
	public static int lifesRemaining = 50;
	[HideInInspector]
	public static bool gameOver = false;

	public delegate void GameOver();
	public static GameOver dGameOver; //Used for functions that needs to be called if game is over.

	//Privates 
	private Sprite 		originalSprite;
	private GameObject 	lastTileHit;
	private LayerMask 	tileLayer;
	private Vector3 	placementTilesOriginalPos;
	private bool 		isBuilding = false;
	private bool		towerSelected = false;

	private GameObject curTower;
	private List<Tower> arrowTowers = new List<Tower>();
	private List<Tower> poisonTowers = new List<Tower>();
	private List<Tower> bombTowers = new List<Tower>();
	private List<Tower> frostTowers = new List<Tower>();

	private float arrowTowerCost;
	private float poisonTowerCost;
	private float bombTowerCost;
	private float frostTowerCost;
	
	private GameObject curSelectedTower;
	private string curTargetName = "No Target"; //To display the name of the selected unit

#endregion

	void Start()
	{
		InteractionHandler.curGold = startGold;

		tileLayer = 1 << LayerMask.NameToLayer("Placement");

		placementTilesOriginalPos = placementTiles.transform.position;

		foreach(Transform t in arrowTowerPool.transform) { arrowTowers.Add(t.GetComponent<Tower>()); }
		foreach(Transform t in poisonTowerPool.transform) { poisonTowers.Add(t.GetComponent<Tower>()); }
		foreach(Transform t in bombTowerPool.transform) { bombTowers.Add(t.GetComponent<Tower>()); }
		foreach(Transform t in frostTowerPool.transform) { frostTowers.Add(t.GetComponent<Tower>()); }

		arrowTowerCost = arrowTowers[0].cost;
		poisonTowerCost = poisonTowers[0].cost;
		bombTowerCost = bombTowers[0].cost;
		frostTowerCost = frostTowers[0].cost;
	}

	void Update () 
	{
		if(isBuilding) {
			TowerBuildingInteraction();
		} else {
			UnitSelectionInteraction();
		}
	}
	
	void TowerBuildingInteraction()
	{
		RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0, tileLayer);
		
		//Seeing if mouse has hit any of the placement tiles if so change the material hihihih
		if(hit.collider != null){
			if(lastTileHit) {
				lastTileHit.GetComponent<SpriteRenderer>().sprite = originalSprite;
			}
			
			lastTileHit = hit.collider.gameObject;
			SpriteRenderer tmp = lastTileHit.GetComponent<SpriteRenderer>();
			originalSprite = tmp.sprite;
			tmp.sprite = selectedSpirit;
		} else {
			if(lastTileHit) {
				lastTileHit.GetComponent<SpriteRenderer>().sprite = originalSprite;
				lastTileHit = null;
			}
		}
		
		//If the player does not want to buy the tower
		if(Input.GetKeyDown(KeyCode.Escape)) {
			placementTiles.transform.position = placementTilesOriginalPos;
			isBuilding = false;
			if(curTower) {
				RangeIndicator.selected = false;
				curTower.transform.position = curTower.transform.parent.position;
				curTower.SetActive(false);
				curTower = null;
			}
		}
		
		//Make the current tower follow the mouse and if mouse is clicked build the tower :)
		if(curTower) {
			Vector3 goToPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			goToPos.z = 0;
			curTower.transform.position = goToPos;
			
			//Should probably change this one so that if the player clicks on something and lastObjectHit is null stop the buildPhase and remove tower
			if(Input.GetMouseButtonDown(0)) {
				if(lastTileHit) {
					BuildTower();
				} else {
					placementTiles.transform.position = placementTilesOriginalPos;
					isBuilding = false;
					if(curTower) {
						RangeIndicator.selected = false;
						curTower.transform.position = curTower.transform.parent.position;
						curTower.SetActive(false);
						curTower = null;
					}
				}
			} 
		}
	}

	void BuildTower()
	{
		curTower.transform.position = lastTileHit.transform.position;

		Tower tmpTower = curTower.GetComponent<Tower>();
		tmpTower.available = false;
		tmpTower.tileReplaced = lastTileHit;
		tmpTower.curNetWorth = tmpTower.cost;
		curGold -= tmpTower.cost;
		tmpTower.cost *= 4f;

		RangeIndicator.selected = false;
		lastTileHit.SetActive(false);
		curTower = null;
		placementTiles.transform.position = placementTilesOriginalPos;
		isBuilding = false;
	}

	void UpgradeTower()
	{
		Tower tmpTower = curSelectedTower.GetComponent<Tower>();

		tmpTower.level++;
		tmpTower.damage *= 4f;
		tmpTower.dotDamage *= 4f;
		tmpTower.slow += 0.1f;
		tmpTower.curNetWorth += tmpTower.cost;
		curGold -= tmpTower.cost;
		tmpTower.cost *= 4f;
	}

	//This is used for tower and enemy selection
	void UnitSelectionInteraction()
	{
		if (Input.GetMouseButtonUp(0) && towerSelected) {
			towerSelected = false;
		}

		if(!towerSelected) {
			RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0, layersToHit);

			//Check if mouse has hit anything
			if(hit.collider != null) {
				//If I hit a tower I will need to move the range indicator in place. If I don't hit a tower
				//and range indicator is selected, set selected to false
				if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Tower")) {
					RangeIndicator.SetTarget(hit.collider.transform, hit.collider.GetComponent<Tower>().radius);
					curSelectedTower = hit.collider.gameObject;
					RangeIndicator.selected = true;
				} else {
					RangeIndicator.selected = false;
				}
				
				curTargetName = hit.collider.gameObject.name;
			} else {
				RangeIndicator.selected = false;
				curTargetName = "No Target";
				curSelectedTower = null;
			}
		}

		if(Input.GetMouseButtonUp(0) && curSelectedTower && !towerSelected) {
			towerSelected = true;
		}

	}

#region GUI related

	void OnGUI()
	{
		//Tower building buttons
		if(GUI.Button(new Rect(810, 10, 145, 40), "Arrow Tower")){ if(curGold - arrowTowerCost >= 0) FindNextTower(arrowTowers); }

		if(GUI.Button(new Rect(810, 60, 145, 40), "Poison Tower")){ if(curGold - poisonTowerCost >= 0) FindNextTower(poisonTowers); }

		if(GUI.Button(new Rect(810, 110, 145, 40), "Bomb Tower")){ if(curGold - bombTowerCost >= 0) FindNextTower(bombTowers); }

		if(GUI.Button(new Rect(810, 160, 145, 40), "Frost Tower")){ if(curGold - frostTowerCost >= 0) FindNextTower(frostTowers); }

		//For targerting/information about objects
		GUI.Label(new Rect(810, 210, 100, 20), curTargetName);

		//For economy
		GUI.Label(new Rect(810, 240, 120, 20), "Current Gold: " + (int)curGold);

		//For showing lives left
		GUI.Label(new Rect(810, 270, 120, 25), "Lifes Remaining: " + lifesRemaining);

		if(towerSelected) {
			Vector3 up = Camera.main.WorldToScreenPoint(curSelectedTower.transform.position);
			//I need to minus with Screen.height because the above function gives my coordinates with lower left corner as (0,0)
			//while GUI has top left corner as (0,0).
			int offSetY = 40;
			int width = 100;
			int height = 20;

			if(curSelectedTower.GetComponent<Tower>().level % 4 != 0) {
				if(GUI.Button(new Rect(up.x - width / 2, Screen.height - up.y - offSetY, width, height), "Upgrade Tower")) {
					if(curGold - curSelectedTower.GetComponent<Tower>().cost >= 0) {
						UpgradeTower();
					}
				}
			}

			if(GUI.Button(new Rect(up.x - width / 2, Screen.height - up.y + (offSetY - height), width, height), "Sell Tower")) {
				Tower tmpTower = curSelectedTower.GetComponent<Tower>();
				
				curGold += (float)tmpTower.curNetWorth * 0.75f;
				tmpTower.ResetTower();
				curSelectedTower = null;
				towerSelected = false;
			}
		}
	}

	void FindNextTower(List<Tower> towers)
	{
		for(int i = 0; i < towers.Count; i++) {
			if(towers[i].available) {
				curTower = towers[i].gameObject;
				RangeIndicator.SetTarget(curTower.transform, towers[i].radius);
				RangeIndicator.selected = true;
				curTower.SetActive(true);
				break;
			}
		}

		towerSelected = false;
		placementTiles.transform.position = backGround.transform.position;
		isBuilding = true;
	}

#endregion
}
