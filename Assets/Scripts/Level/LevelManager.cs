using Core;
using UnityEngine;

namespace Level
{
    public class LevelManager : MonoBehaviour
    {
        public void EnterLevel()
        {
            GameCore.UI.TransitionFadeOut(() =>
            {
                // todo: load level scene, open level ui or do whatever needed
                GameCore.UI.TransitionFadeIn();
            });
        }

        public void OnUpdate(float deltaTime, float unscaledDeltaTime)
        {
            
        }

        public void OnFixedUpdate()
        {
            
        }
    }
}