using System;
using UnityEngine;
using UnityEngine.UI;

namespace JuiceFresh.States
{
    public class MapState : GameStateBase
    {
        public MapState(LevelManager levelManager) : base(levelManager)
        {
        }

        public override void EnterState()
        {
            if (PlayerPrefs.GetInt("OpenLevelTest") <= 0)
            {
                PlayMapMusic();
                EnableMap(true);

                LevelManager.TriggerOnMapState();
            }
            else
            {
                levelManager.gameStatus = GameState.PrepareGame;
                PlayerPrefs.SetInt("OpenLevelTest", 0);
                PlayerPrefs.Save();
            }
        }

        public override void UpdateState()
        {
            // No specific update logic for map state
        }

        public override void ExitState()
        {
            EnableMap(false);
        }

        private void PlayMapMusic()
        {
            var audioSource = MusicBase.Instance.GetComponent<AudioSource>();
            audioSource.Stop();
            audioSource.loop = true;
            audioSource.clip = MusicBase.Instance.music[0];
            audioSource.Play();
        }

        private void EnableMap(bool enable)
        {
            float aspect = (float)Screen.height / (float)Screen.width;
            aspect = (float)Math.Round(aspect, 2);

            if (enable)
            {
                SetMapCamera(aspect);
            }
            else
            {
                InitScript.DateOfExit = DateTime.Now.ToString();
                SetupGameCamera();
                SetGameCamera(aspect);
            }

            levelManager.gameCamera.GetComponent<MapCamera>().enabled = enable;

            HandleLevelsMapActivation(enable);

            if (enable)
            {
                levelManager.GameField.gameObject.SetActive(false);
            }

            if (!enable)
            {
                levelManager.gameCamera.transform.position = new Vector3(0, 0, -10);
            }

            ClearGameField();
        }

        private void SetMapCamera(float aspect)
        {
            Camera gameCamera = levelManager.gameCamera;
            gameCamera.orthographicSize = 10.25f;

            // Adjust camera orthographic size based on aspect ratio
            if (aspect == 1.6f)
                gameCamera.orthographicSize = 12.2f; //16:10
            else if (aspect == 1.78f)
                gameCamera.orthographicSize = 13.6f; //16:9
            else if (aspect == 1.5f)
                gameCamera.orthographicSize = 11.2f; //3:2
            else if (aspect == 1.33f)
                gameCamera.orthographicSize = 10.25f; //4:3
            else if (aspect == 1.67f)
                gameCamera.orthographicSize = 12.5f; //5:3
            else if (aspect == 2.06f)
                gameCamera.orthographicSize = 15.75f; //2960:1440 S8
            else if (aspect == 2.17f)
                gameCamera.orthographicSize = 16.5f;
            else if (aspect == 2.16f)
                gameCamera.orthographicSize = 16.5f; //iphone x

            gameCamera.GetComponent<MapCamera>().SetPosition(new Vector2(0, gameCamera.transform.position.y));
        }

        private void SetGameCamera(float aspect)
        {
            Camera gameCamera = levelManager.gameCamera;
            if (aspect == 2.06f)
                gameCamera.orthographicSize = 11.5f; //2960:1440 S8
            else if (aspect == 2.17f)
                gameCamera.orthographicSize = 12.26f; //iphone x
            else if (aspect == 2.16f)
                gameCamera.orthographicSize = 12.26f; //iphone x

            GameObject.Find("CanvasGlobal").GetComponent<GraphicRaycaster>().enabled = false;
            GameObject.Find("CanvasGlobal").GetComponent<GraphicRaycaster>().enabled = true;

            var canvas = levelManager.Level.transform.Find("Canvas");
            canvas.GetComponent<GraphicRaycaster>().enabled = false;
            canvas.GetComponent<GraphicRaycaster>().enabled = true;

            var panel = canvas.transform.Find("Panel/Panel").GetComponent<RectTransform>();
            if (aspect == 2.17f)
                panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, 935) + Vector2.down * 100;
        }

        private void SetupGameCamera()
        {
            Camera gameCamera = levelManager.gameCamera;
            float aspect = (float)Screen.height / (float)Screen.width;
            gameCamera.orthographicSize = 10.05f;
            aspect = (float)Math.Round(aspect, 2);
            gameCamera.GetComponent<MapCamera>().SetPosition(new Vector2(0, gameCamera.transform.position.y));
        }

        private void HandleLevelsMapActivation(bool enable)
        {
            GameObject levelsMap = levelManager.LevelsMap;
            GameObject level = levelManager.Level;

            levelsMap.SetActive(!enable); // This seems counterintuitive but matches original code
            levelsMap.SetActive(enable);
            levelsMap.GetComponent<LevelsMap>().Reset();

            foreach (Transform tr in levelsMap.transform)
            {
                if (tr.name != "AvatarManager" && tr.name != "Character")
                    tr.gameObject.SetActive(enable);
                if (tr.name == "Character")
                {
                    tr.GetComponent<SpriteRenderer>().enabled = enable;
                    tr.transform.GetChild(0).gameObject.SetActive(enable);
                }
            }

            level.SetActive(!enable);
        }

        private void ClearGameField()
        {
            foreach (Transform item in levelManager.GameField)
            {
                UnityEngine.Object.Destroy(item.gameObject);
            }
        }
    }
}