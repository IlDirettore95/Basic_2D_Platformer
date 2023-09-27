using GMDG.NoProduct.Utility;
using System.Collections;
using UnityEngine;
using Event = GMDG.Basic2DPlatformer.System.Event;
using EventManager = GMDG.Basic2DPlatformer.System.EventManager;

namespace GMDG.Basic2DPlatformer.PlayerStatistics
{
    public class Statistics : MonoBehaviour
    {
        [SerializeField] StatisticsData statisticsData;

        private float invincibilityTimer = 0f;

        private SpriteRenderer spriteRenderer;
    
        private int health;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }


        private void OnEnable()
        {
            health = statisticsData.MaxHealth;
            spriteRenderer.color = statisticsData.LifeColors[statisticsData.MaxHealth - 1];

            EventManager.Instance.Subscribe(Event.OnFallDamageTaken, TakeDamage);
            EventManager.Instance.Subscribe(Event.OnTrapHit, TakeDamage);
        }

        private void OnDisable()
        {
            EventManager.Instance.Unsubscribe(Event.OnFallDamageTaken, TakeDamage);
            EventManager.Instance.Unsubscribe(Event.OnTrapHit, TakeDamage);
        }


        // Update is called once per frame
        private void Update()
        {
            if (invincibilityTimer > 0)
            {
                invincibilityTimer -= Time.deltaTime;
            }
        }

        public void TakeDamage(object[] args)
        {
            if (invincibilityTimer > 0) return;

            health -= 1;

            if (health <= 0)
            {
                gameObject.SetActive(false);
                EventManager.Instance.Publish(Event.OnPlayerDeath);
            }
            else
            {
                spriteRenderer.color = statisticsData.LifeColors[health - 1];
                StartCoroutine(SpriteUtility.FlickeringAnimation(spriteRenderer, 0.2f, 0.06f));
                invincibilityTimer = 0.6f;
            }
        }
    }
}
