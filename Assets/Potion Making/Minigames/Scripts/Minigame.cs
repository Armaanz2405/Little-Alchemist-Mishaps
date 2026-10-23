using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Nocturne.UI;

namespace Nocturne.PotionMaking.Minigames
{
    public abstract class Minigame : MonoBehaviour
    {
        public UnityEvent<float> onFinished = new UnityEvent<float>();

        public abstract string Title { get; }
        public abstract string Instructions { get; }
        public bool Running { get; private set; }

        protected RectTransform Root { get; private set; }
        private Action<float> callback;
        private bool finishing;
        private bool outOfEnergy;

        public void Begin(Action<float> onDone = null, bool outOfEnergy = false)
        {
            if (Running) return;
            callback = onDone;
            finishing = false;
            this.outOfEnergy = outOfEnergy;
            var canvas = MinigameUI.MakeCanvas(transform, Title, 80);
            Root = MinigameUI.Rect(canvas.transform, "Panel", Vector2.zero, new Vector2(1100, 760), MinigameUI.Panel);
            UIStyle.Box(Root.GetComponent<Image>(), BoxKind.Panel);
            MinigameUI.Label(Root, $"<b>{Title}</b>", new Vector2(0, 330), 46, MinigameUI.Gold);
            MinigameUI.Label(Root, Instructions, new Vector2(0, 278), 24, MinigameUI.Ink);
            if (outOfEnergy) MinigameUI.Label(Root, "Out of energy, this one scores 0%", new Vector2(0, 240), 24, MinigameUI.Bad);
            Build();
            Running = true;
        }

        public void Cancel()
        {
            if (!Running) return;
            Running = false;
            Destroy(Root.parent.gameObject);
        }

        protected abstract void Build();
        protected abstract void Step(float dt);

        private void Update()
        {
            if (Running && !finishing) Step(Time.deltaTime);
        }

        protected void Finish(float score, string result)
        {
            if (finishing) return;
            finishing = true;
            if (outOfEnergy)
            {
                score = 0f;
                result = "Out of energy";
            }
            StartCoroutine(FinishRoutine(Mathf.Clamp01(score), result));
        }

        private IEnumerator FinishRoutine(float score, string result)
        {
            MinigameUI.Label(Root, $"<b>{result}</b>\n<size=24>score {Mathf.RoundToInt(score * 100)}%</size>", new Vector2(0, -320), 36,
                score >= 0.5f ? MinigameUI.Good : MinigameUI.Bad);
            yield return new WaitForSeconds(1.5f);
            Running = false;
            Destroy(Root.parent.gameObject);
            callback?.Invoke(score);
            onFinished.Invoke(score);
        }
    }
}
